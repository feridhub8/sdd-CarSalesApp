using WebApi.Data.Listing.Enums.V1;

namespace WebApi.Data.Listing.Models.V1;

// A checked search. Price bounds are in PriceCurrency; mileage bounds are in kilometres.
public sealed record ListingSearch
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public Guid? BrandId { get; init; }

    public Guid? ModelId { get; init; }

    public Guid? CityId { get; init; }

    public int? YearFrom { get; init; }

    public int? YearTo { get; init; }

    public int? MileageFrom { get; init; }

    public int? MileageTo { get; init; }

    public decimal? EngineFrom { get; init; }

    public decimal? EngineTo { get; init; }

    public decimal? PriceFrom { get; init; }

    public decimal? PriceTo { get; init; }

    public Currency PriceCurrency { get; init; }

    public List<BodyType> BodyType { get; init; } = [];

    public List<FuelType> FuelType { get; init; } = [];

    public List<Transmission> Transmission { get; init; } = [];

    public List<Drivetrain> Drivetrain { get; init; } = [];

    public List<Color> Color { get; init; } = [];

    public List<Market> Market { get; init; } = [];

    public bool IsCredit { get; init; }

    public bool IsBarter { get; init; }

    public bool NotPainted { get; init; }

    public bool NotDamaged { get; init; }

    public bool HasImages { get; init; }

    public ListingSort Sort { get; init; }
}
