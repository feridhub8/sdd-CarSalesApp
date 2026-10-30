using Mapster;
using WebApi.Data.Listing.Dtos.V1;

namespace WebApi.Data.Listing.Mappings.V1;

public sealed class CreateListingDto_Listing : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // The VIN is stored upper-case; a blank description is stored as none.
        config.NewConfig<CreateListingDto, Entities.V1.Listing>()
            .Ignore(destination => destination.Model)
            .Ignore(destination => destination.City)
            .Map(destination => destination.Vin, source => source.Vin == null ? null : source.Vin.ToUpperInvariant())
            .Map(destination => destination.Description, source => string.IsNullOrWhiteSpace(source.Description) ? null : source.Description.Trim());
    }
}
