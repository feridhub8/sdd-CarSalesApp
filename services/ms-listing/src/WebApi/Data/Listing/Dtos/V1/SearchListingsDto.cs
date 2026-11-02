using WebApi.Data.Shared.Validators.V1;

namespace WebApi.Data.Listing.Dtos.V1;

// Fixed values are strings: binding a query string to an enum would accept numbers, which JSON rejects.
public sealed record SearchListingsDto
{
    public int Page { get; init; } = PagingRuleExtensions.DefaultPage;

    public int PageSize { get; init; } = PagingRuleExtensions.DefaultPageSize;

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

    public string? PriceCurrency { get; init; }

    public List<string> BodyType { get; init; } = [];

    public List<string> FuelType { get; init; } = [];

    public List<string> Transmission { get; init; } = [];

    public List<string> Drivetrain { get; init; } = [];

    public List<string> Color { get; init; } = [];

    public List<string> Market { get; init; } = [];

    public bool IsCredit { get; init; }

    public bool IsBarter { get; init; }

    public bool NotPainted { get; init; }

    public bool NotDamaged { get; init; }

    public bool HasImages { get; init; }

    public string? Sort { get; init; }
}
