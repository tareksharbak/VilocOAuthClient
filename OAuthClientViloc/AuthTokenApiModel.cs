using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OAuthClientViloc
{
	[Serializable]
	public class AuthTokenApiModel
	{
		public AuthTokenApiModel()
		{
			if (IssuedOn == null)
				IssuedOn = DateTime.UtcNow;
		}

		[JsonProperty(PropertyName = "access_token")]
		public string AccessToken { get; set; }

		[JsonProperty(PropertyName = "token_type")]
		public string TokenType { get; set; }

		[JsonProperty(PropertyName = "expires_in")]
		public int ExpiresIn { get; set; }

		[JsonProperty(PropertyName = "refresh_token")]
		public string RefreshToken { get; set; }

		[JsonProperty(PropertyName = "as:client_id")]
		public string ClientId { get; set; }

		[JsonProperty(PropertyName = "username")]
		public string Username { get; set; }

		public DateTime? IssuedOn { get; set; }
		public DateTime? ExpiresOn { get => IssuedOn.HasValue ? IssuedOn.Value.AddSeconds(ExpiresIn) : (DateTime?)null; }
	}
}
