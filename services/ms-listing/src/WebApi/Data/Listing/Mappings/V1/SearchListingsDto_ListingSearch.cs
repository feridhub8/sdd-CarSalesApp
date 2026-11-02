using Mapster;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Enums.V1;
using WebApi.Data.Listing.Models.V1;

namespace WebApi.Data.Listing.Mappings.V1;

public sealed class SearchListingsDto_ListingSearch : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SearchListingsDto, ListingSearch>()
            .Map(destination => destination.PriceCurrency, source => Parse(source.PriceCurrency, Currency.AZN))
            .Map(destination => destination.Sort, source => Parse(source.Sort, ListingSort.Newest))
            .Map(destination => destination.BodyType, source => ParseAll<BodyType>(source.BodyType))
            .Map(destination => destination.FuelType, source => ParseAll<FuelType>(source.FuelType))
            .Map(destination => destination.Transmission, source => ParseAll<Transmission>(source.Transmission))
            .Map(destination => destination.Drivetrain, source => ParseAll<Drivetrain>(source.Drivetrain))
            .Map(destination => destination.Color, source => ParseAll<Color>(source.Color))
            .Map(destination => destination.Market, source => ParseAll<Market>(source.Market));
    }

    public static TEnum Parse<TEnum>(string? value, TEnum fallback)
        where TEnum : struct, Enum
    {
        return value is null ? fallback : Enum.Parse<TEnum>(value, ignoreCase: true);
    }

    public static List<TEnum> ParseAll<TEnum>(List<string> values)
        where TEnum : struct, Enum
    {
        return values.Select(value => Enum.Parse<TEnum>(value, ignoreCase: true)).ToList();
    }
}
