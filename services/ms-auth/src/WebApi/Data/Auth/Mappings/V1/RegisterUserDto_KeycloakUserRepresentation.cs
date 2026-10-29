using Mapster;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Models.V1;

namespace WebApi.Data.Auth.Mappings.V1;

public sealed class RegisterUserDto_KeycloakUserRepresentation : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterUserDto, KeycloakUserRepresentation>()
            .Ignore(destination => destination.Id!)
            .Map(destination => destination.Username, source => source.Email)
            .Map(destination => destination.Enabled, _ => true)
            .Map(destination => destination.Credentials, source => new List<KeycloakCredentialRepresentation>
            {
                new() { Type = KeycloakCredentialRepresentation.PasswordType, Value = source.Password, Temporary = false }
            });
    }
}
