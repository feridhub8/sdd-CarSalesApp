using Mapster;
using WebApi.Data.Brand.Dtos.V1;

namespace WebApi.Data.Brand.Mappings.V1;

public sealed class CreateBrandDto_Brand : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateBrandDto, Entities.V1.Brand>()
            .Map(destination => destination.Name, source => source.Name.Trim());
    }
}
