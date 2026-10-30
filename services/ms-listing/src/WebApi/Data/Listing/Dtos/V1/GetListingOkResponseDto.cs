using WebApi.Data.Listing.Enums.V1;

namespace WebApi.Data.Listing.Dtos.V1;

public sealed record GetListingOkResponseDto
{
    public Guid Id { get; init; }

    public Guid OwnerId { get; init; }

    public Guid BrandId { get; init; }

    public string BrandName { get; init; } = string.Empty;

    public Guid ModelId { get; init; }

    public string ModelName { get; init; } = string.Empty;

    public Guid CityId { get; init; }

    public string CityName { get; init; } = string.Empty;

    public int Year { get; init; }

    public int Mileage { get; init; }

    public MileageUnit MileageUnit { get; init; }

    public decimal? EngineVolume { get; init; }

    public int Horsepower { get; init; }

    public int Seats { get; init; }

    public string? Vin { get; init; }

    public BodyType BodyType { get; init; }

    public FuelType FuelType { get; init; }

    public Transmission Transmission { get; init; }

    public Drivetrain Drivetrain { get; init; }

    public Color Color { get; init; }

    public Market Market { get; init; }

    public decimal Price { get; init; }

    public Currency Currency { get; init; }

    public string? Description { get; init; }

    public bool IsCredit { get; init; }

    public bool IsBarter { get; init; }

    public bool IsPainted { get; init; }

    public bool IsDamaged { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
