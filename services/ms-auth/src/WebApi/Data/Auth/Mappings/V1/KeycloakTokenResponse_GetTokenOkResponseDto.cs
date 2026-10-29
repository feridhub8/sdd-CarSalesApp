using Mapster;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Models.V1;

namespace WebApi.Data.Auth.Mappings.V1;

public sealed class KeycloakTokenResponse_GetTokenOkResponseDto : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<KeycloakTokenResponse, GetTokenOkResponseDto>();
    }
}
