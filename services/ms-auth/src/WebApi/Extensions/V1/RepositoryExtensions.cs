using WebApi.Data.Auth.Repositories.V1;
using WebApi.Data.Auth.Repositories.V1.Base;

namespace WebApi.Extensions.V1;

public static class RepositoryExtensions
{
    public static void InitRepositories(this IServiceCollection services)
    {
        // Singletons: the user repository caches the service account token across requests.
        services.AddSingleton<ITokenRepository, TokenKeycloakRepository>();
        services.AddSingleton<IUserRepository, UserKeycloakRepository>();
    }
}
