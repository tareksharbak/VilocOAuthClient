using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OAuthClientViloc
{
	public class ApiAccess
	{
		private readonly string authBaseUri;
		private readonly string baseUri;
		private readonly string clientId;
		private readonly string clientSecret;

		private AuthTokenApiModel token;
		private readonly ITokenPersistingService persistingService;

		public ApiAccess(string authBaseUri, string baseUri, string clientId, string clientSecret, ITokenPersistingService persistingService)
		{
			this.authBaseUri = authBaseUri;
			this.baseUri = baseUri;
			this.clientId = clientId;
			this.clientSecret = clientSecret;

			this.persistingService = persistingService;
			
			token = persistingService?.LoadAsync().Result;
		}

		public async Task<ApiResult> RegisterUserAsync(string token, string username, string password)
		{
			var userModel = new UserApiModel()
			{
				UserName = username,
				Password = password,
				ConfirmPassword = password,
				Token = token
			};
			return await PostJsonAsync(authBaseUri, "api/Account/Register", userModel, null);
		}

		public async Task LoginAsync(string username, string password)
		{
			var parameters = new Dictionary<string, string> { };
			parameters.Add("grant_type", "password");
			parameters.Add("username", username);
			parameters.Add("password", password);
			parameters.Add("client_id", clientId);
			parameters.Add("client_secret", clientSecret);

			var response = await PostUrlEncodedContentAsync(authBaseUri, "api/Account/Login", parameters);

			if (response.IsSuccessful)
			{
				var newToken = JsonConvert.DeserializeObject<AuthTokenApiModel>(response.Content);
				token = newToken;

				await persistingService?.StoreAsync(token);
			}
			else
				throw new UnauthorizedAccessException(response.Content);
		}

		public async Task<bool> HealthCheckAsync()
		{
			var result = await GetAsync(baseUri, "api/Health", null);
			return result.IsSuccessful;
		}

		public async Task<bool> HealthCheckAuthorizedAsync()
		{
			await CheckTokenValidityAsync();

			var result = await GetAsync(baseUri, "api/Health/Authorized", token.AccessToken);
			return result.IsSuccessful;
		}

		private async Task CheckTokenValidityAsync()
		{
			if (token == null)
				throw new UnauthorizedAccessException();

			if (token.ExpiresOn < DateTime.UtcNow)
				await RefreshTokenAsync();
		}

		private async Task RefreshTokenAsync()
		{
			if (token == null)
				throw new UnauthorizedAccessException();

			var parameters = new Dictionary<string, string> { };
			parameters.Add("grant_type", "refresh_token");
			parameters.Add("refresh_token", token.RefreshToken);
			parameters.Add("client_id", clientId);
			parameters.Add("client_secret", clientSecret);

			var response = await PostUrlEncodedContentAsync(authBaseUri, "api/Account/Login", parameters);

			if (response.IsSuccessful)
			{
				token = JsonConvert.DeserializeObject<AuthTokenApiModel>(response.Content);

				await persistingService?.StoreAsync(token);
			}
			else
			{
				token = null;
				throw new UnauthorizedAccessException(response.Content);
			}
		}

		private async Task<ApiResult> GetAsync(string baseUri, string subUri, string accessToken)
		{
			ApiResult apiResult = null;
			using (var httpClient = new HttpClient())
			{
				httpClient.BaseAddress = new Uri(baseUri);

				if (!String.IsNullOrWhiteSpace(accessToken))
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				var response = await httpClient.GetAsync(subUri);

				apiResult = new ApiResult(response);
			}
			return apiResult;
		}

		private async Task<ApiResult> PostUrlEncodedContentAsync(string baseUri, string subUri, Dictionary<string, string> parameters)
		{
			ApiResult apiResult = null;
			using (var httpClient = new HttpClient())
			{
				httpClient.BaseAddress = new Uri(baseUri);
				var content = new FormUrlEncodedContent(parameters);

				var response = await httpClient.PostAsync(subUri, content);

				apiResult = new ApiResult(response);
			}
			return apiResult;
		}

		private async Task<ApiResult> PostJsonAsync<T>(string baseUri, string subUri, T content, string accessToken)
		{
			ApiResult apiResult = null;
			using (var httpClient = new HttpClient())
			{
				httpClient.BaseAddress = new Uri(baseUri);

				if (!String.IsNullOrWhiteSpace(accessToken))
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				var dataAsString = JsonConvert.SerializeObject(content);
				var httpContent = new StringContent(dataAsString);

				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				var response = await httpClient.PostAsync(subUri, httpContent);

				apiResult = new ApiResult(response);
			}

			return apiResult;
		}
	}

	public class ApiResult
	{
		public ApiResult(HttpResponseMessage response)
		{
			Content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			IsSuccessful = response.IsSuccessStatusCode;
			StatusCode = response.StatusCode;
		}

		public bool IsSuccessful { get; set; }
		public string Content { get; set; }
		public HttpStatusCode StatusCode { get; set; }
	}
}
