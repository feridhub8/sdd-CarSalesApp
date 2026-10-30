using WebApi.Data.Listing.Enums.V1;
using WebApi.Data.Shared.Entities.V1.Base;

namespace WebApi.Data.Listing.Entities.V1;

public sealed class Listing : BaseEntity
{
    public const int VinLength = 17;
    public const int DescriptionMaxLength = 3000;

    // The Keycloak user (token "sub") who published the listing; users live in Keycloak, so there is no foreign key.
    public Guid OwnerId { get; set; }

    public Guid ModelId { get; set; }

    public Model.Entities.V1.Model Model { get; set; } = null!;

    public Guid CityId { get; set; }

    public City.Entities.V1.City City { get; set; } = null!;

    public int Year { get; set; }

    public int Mileage { get; set; }

    public MileageUnit MileageUnit { get; set; }

    // Litres with one decimal; empty for electric cars.
    public decimal? EngineVolume { get; set; }

    public int Horsepower { get; set; }

    public int Seats { get; set; }

    public string? Vin { get; set; }

    public BodyType BodyType { get; set; }

    public FuelType FuelType { get; set; }

    public Transmission Transmission { get; set; }

    public Drivetrain Drivetrain { get; set; }

    public Color Color { get; set; }

    public Market Market { get; set; }

    public decimal Price { get; set; }

    public Currency Currency { get; set; }

    public string? Description { get; set; }

    public bool IsCredit { get; set; }

    public bool IsBarter { get; set; }

    public bool IsPainted { get; set; }

    public bool IsDamaged { get; set; }
}
