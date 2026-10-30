using Mapster;
using WebApi.Data.Brand.Dtos.V1;

namespace WebApi.Data.Brand.Mappings.V1;

public sealed class Brand_GetBrandWithDeletedOkResponseDto : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Entities.V1.Brand, GetBrandWithDeletedOkResponseDto>();
    }
}
