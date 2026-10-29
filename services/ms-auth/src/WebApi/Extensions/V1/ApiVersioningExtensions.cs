using Asp.Versioning;

namespace WebApi.Extensions.V1;

public static class ApiVersioningExtensions
{
    public static void InitApiVersioning(this IServiceCollection services)
    {
        services.AddControllers();

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}
