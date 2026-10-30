using Mapster;
using WebApi.Data.Model.Dtos.V1;

namespace WebApi.Data.Model.Mappings.V1;

public sealed class Model_GetModelWithDeletedOkResponseDto : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Entities.V1.Model, GetModelWithDeletedOkResponseDto>()
            .Map(destination => destination.BrandName, source => source.Brand.Name);
    }
}
