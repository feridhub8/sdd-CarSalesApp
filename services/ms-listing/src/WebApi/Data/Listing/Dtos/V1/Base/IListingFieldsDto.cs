using WebApi.Data.Listing.Enums.V1;

namespace WebApi.Data.Listing.Dtos.V1.Base;

// The editable fields shared by create and update, so both are validated by the same rules.
// Enums are nullable here so a missing value is reported instead of silently becoming the first member.
public interface IListingFieldsDto
{
    Guid ModelId { get; }

    Guid CityId { get; }

    int Year { get; }

    int Mileage { get; }

    MileageUnit? MileageUnit { get; }

    decimal? EngineVolume { get; }

    int Horsepower { get; }

    int Seats { get; }

    string? Vin { get; }

    BodyType? BodyType { get; }

    FuelType? FuelType { get; }

    Transmission? Transmission { get; }

    Drivetrain? Drivetrain { get; }

    Color? Color { get; }

    Market? Market { get; }

    decimal Price { get; }

    Currency? Currency { get; }

    string? Description { get; }

    bool IsCredit { get; }

    bool IsBarter { get; }

    bool IsPainted { get; }

    bool IsDamaged { get; }
}
