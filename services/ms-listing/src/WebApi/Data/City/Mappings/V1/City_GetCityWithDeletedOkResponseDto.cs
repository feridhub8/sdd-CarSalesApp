using Mapster;
using WebApi.Data.City.Dtos.V1;

namespace WebApi.Data.City.Mappings.V1;

public sealed class City_GetCityWithDeletedOkResponseDto : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Entities.V1.City, GetCityWithDeletedOkResponseDto>();
    }
}
