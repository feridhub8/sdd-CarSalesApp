using Mapster;
using WebApi.Data.City.Dtos.V1;

namespace WebApi.Data.City.Mappings.V1;

public sealed class CreateCityDto_City : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCityDto, Entities.V1.City>()
            .Map(destination => destination.Name, source => source.Name.Trim());
    }
}
