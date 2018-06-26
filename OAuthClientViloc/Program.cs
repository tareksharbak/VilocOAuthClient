using System;
using System.IO;
using System.Threading.Tasks;

namespace OAuthClientViloc
{
	class Program
	{
		//Fill here your ClientId, ClientSecret, AuthBaseUri, and BaseUri
		private const string AUTH_BASE_URI = "";
		private const string BASE_URI = "";
		private const string CLIENT_ID = "";
		private const string CLIENT_SECRET = "";

		private static readonly string TOKEN_FILE_PATH = Path.Combine(Directory.GetCurrentDirectory(), "token.cfg");

		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			RunAsync().Wait();
		}

		static async Task RunAsync()
		{
			ApiAccess apiAccess = new ApiAccess(AUTH_BASE_URI, BASE_URI, CLIENT_ID, CLIENT_SECRET, new TokenPersistingService(TOKEN_FILE_PATH));
			while (true)
			{
				try
				{
					var healthNotAuthorizedResult = await apiAccess.HealthCheckAsync();
					Console.WriteLine($"Health check without authorization returned: {healthNotAuthorizedResult}");

					var healthAuthorizedResult = await apiAccess.HealthCheckAuthorizedAsync();
					Console.WriteLine($"Health check with authorization returned: {healthAuthorizedResult}");
				}
				catch (UnauthorizedAccessException)
				{
					Console.WriteLine("Authentication required.");
					Console.Write("Press 'R' to register a new user, or 'L' to login with an existing user: ");
					var choice = Console.ReadLine().ToUpper().Trim();

					try
					{
						switch (choice)
						{
							case "R":
								await RegisterAsync(apiAccess);
								break;
							case "L":
								await LoginAsync(apiAccess);
								break;
							default:
								Console.WriteLine("Your choice is invalid.");
								break;
						}
					}
					catch (UnauthorizedAccessException ex)
					{
						Console.WriteLine(ex.Message);
					}
				}
				Console.Write("Press any button to run again.");
				Console.ReadLine();
			}
		}

		private static async Task LoginAsync(ApiAccess apiAccess)
		{
			var username = "";
			var password = "";
			Console.Write("Username: ");
			username = Console.ReadLine();

			Console.Write("Password: ");
			password = GetMaskedPassword();

			await apiAccess.LoginAsync(username, password);
		}

		private static async Task RegisterAsync(ApiAccess apiAccess)
		{
			var username = "";
			var password = "";
			var token = "";
			Console.Write("Username: ");
			username = Console.ReadLine();

			Console.Write("Password: ");
			password = Console.ReadLine();

			Console.Write("Invitation Token: ");
			token = Console.ReadLine();

			var result = await apiAccess.RegisterUserAsync(token, username, password);

			if (result.IsSuccessful)
				await apiAccess.LoginAsync(username, password);
			else
				Console.WriteLine($"Registration failed: {result.Content}");
		}

		private static string GetMaskedPassword()
		{
			ConsoleKeyInfo key;
			string password = "";
			do
			{
				key = Console.ReadKey(true);
				if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
				{
					password += key.KeyChar;
					Console.Write("*");
				}
				else
				{
					if (key.Key == ConsoleKey.Backspace && password.Length > 0)
					{
						password = password.Substring(0, (password.Length - 1));
						Console.Write("\b \b");
					}
				}
			}
			while (key.Key != ConsoleKey.Enter);
			Console.WriteLine();

			return password;
		}
	}
}
