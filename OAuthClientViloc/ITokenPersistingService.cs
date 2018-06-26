using System.Threading.Tasks;

namespace OAuthClientViloc
{
	public interface ITokenPersistingService
	{
		Task<AuthTokenApiModel> LoadAsync();
		Task StoreAsync(AuthTokenApiModel authToken);
	}
}