using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OAuthClientViloc
{
	public class TokenPersistingService : ITokenPersistingService
	{
		private readonly string path;

		public TokenPersistingService(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("'path' cannot be null or empty.");

			this.path = path;
		}

		public async Task StoreAsync(AuthTokenApiModel authToken)
		{
			try
			{
				var json = JsonConvert.SerializeObject(authToken);
				var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(json));
				using (FileStream fs = new FileStream(path, FileMode.Create))
				using (StreamWriter sw = new StreamWriter(fs))
				{
					await sw.WriteAsync(encoded);
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task<AuthTokenApiModel> LoadAsync()
		{
			try
			{
				if (File.Exists(path))
				{
					string fileContent = "";
					using (FileStream fs = new FileStream(path, FileMode.Open))
					using (StreamReader sr = new StreamReader(fs))
					{
						fileContent = await sr.ReadToEndAsync();
					}
					var bytes = Convert.FromBase64String(fileContent);
					var json = Encoding.Unicode.GetString(bytes);
					return JsonConvert.DeserializeObject<AuthTokenApiModel>(json);
				}
				else
					return null;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
