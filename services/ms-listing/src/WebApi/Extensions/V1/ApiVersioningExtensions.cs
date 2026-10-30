using Asp.Versioning;

namespace WebApi.Extensions.V1;

public static class ApiVersioningExtensions
{
    public static void InitApiVersioning(this IServiceCollection services)
    {
        // Required fields are checked by FluentValidation, not by the nullable annotations.
        services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
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
