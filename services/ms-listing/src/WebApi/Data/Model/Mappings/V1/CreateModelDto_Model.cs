using Mapster;
using WebApi.Data.Model.Dtos.V1;

namespace WebApi.Data.Model.Mappings.V1;

public sealed class CreateModelDto_Model : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateModelDto, Entities.V1.Model>()
            .Ignore(destination => destination.Brand)
            .Map(destination => destination.Name, source => source.Name.Trim());
    }
}
