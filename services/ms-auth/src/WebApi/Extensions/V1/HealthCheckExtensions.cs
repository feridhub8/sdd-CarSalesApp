namespace WebApi.Extensions.V1;

public static class HealthCheckExtensions
{
    public static void InitHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
    }

    public static void MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
    }
}
