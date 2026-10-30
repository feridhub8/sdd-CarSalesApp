using WebApi.Data.Auth.Services.V1;
using WebApi.Data.Auth.Services.V1.Base;

namespace WebApi.Extensions.V1;

public static class ServiceExtensions
{
    public static void InitServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
    }
}
