using Microsoft.OpenApi;

namespace WebApi.Extensions.V1;

public static class SwaggerExtensions
{
    private const string BearerScheme = "Bearer";

    public static void InitSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "ms-auth", Version = "v1" });

            options.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Access token from POST /api/v1/Auth/Login"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerScheme, document)] = []
            });
        });
    }

    public static void UseSwaggerInLocalAndDevelopment(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Local"))
        {
            return;
        }

        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
