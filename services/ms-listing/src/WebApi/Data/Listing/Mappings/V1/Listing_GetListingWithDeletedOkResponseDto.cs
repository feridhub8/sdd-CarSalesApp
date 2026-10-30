using Mapster;
using WebApi.Data.Listing.Dtos.V1;

namespace WebApi.Data.Listing.Mappings.V1;

public sealed class Listing_GetListingWithDeletedOkResponseDto : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Entities.V1.Listing, GetListingWithDeletedOkResponseDto>()
            .Map(destination => destination.BrandId, source => source.Model.BrandId)
            .Map(destination => destination.BrandName, source => source.Model.Brand.Name)
            .Map(destination => destination.ModelName, source => source.Model.Name)
            .Map(destination => destination.CityName, source => source.City.Name);
    }
}
