using FluentValidation;
using Mapster;

namespace WebApi.Extensions.V1;

public static class LibraryExtensions
{
    public static void InitLibraries(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());
        services.AddValidatorsFromAssemblyContaining<Program>();

        TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
        services.AddMapster();
    }
}
