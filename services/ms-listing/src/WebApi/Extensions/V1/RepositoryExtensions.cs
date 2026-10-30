using WebApi.Data.Brand.Repositories.V1;
using WebApi.Data.Brand.Repositories.V1.Base;
using WebApi.Data.City.Repositories.V1;
using WebApi.Data.City.Repositories.V1.Base;
using WebApi.Data.Model.Repositories.V1;
using WebApi.Data.Model.Repositories.V1.Base;
using WebApi.Data.Shared.Repositories.V1;
using WebApi.Data.Shared.Repositories.V1.Base;

namespace WebApi.Extensions.V1;

public static class RepositoryExtensions
{
    public static void InitRepositories(this IServiceCollection services)
    {
        // Scoped, like the DbContext the EF Core repositories use.
        services.AddScoped<IBrandRepository, BrandEfCoreRepository>();
        services.AddScoped<IModelRepository, ModelEfCoreRepository>();
        services.AddScoped<ICityRepository, CityEfCoreRepository>();

        // Stateless and called from token validation on every authorized request.
        services.AddSingleton<ITokenRepository, TokenKeycloakRepository>();
    }
}
