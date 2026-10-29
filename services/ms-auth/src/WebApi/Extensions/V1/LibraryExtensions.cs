using System.Text.Json;
using FluentValidation;
using Mapster;

namespace WebApi.Extensions.V1;

public static class LibraryExtensions
{
    public static void InitLibraries(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());

        // Validators check the command (x => x.Dto.Email); report the field as the client sent it ("email").
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) =>
            member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);
        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) =>
            member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);
        services.AddValidatorsFromAssemblyContaining<Program>();

        TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
        TypeAdapterConfig.GlobalSettings.Compile();
        services.AddMapster();
    }
}
