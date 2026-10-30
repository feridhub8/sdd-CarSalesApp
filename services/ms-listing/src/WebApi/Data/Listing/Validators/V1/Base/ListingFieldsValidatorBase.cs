using System.Text.RegularExpressions;
using FluentValidation;
using WebApi.Data.Listing.Commands.V1.Base;
using WebApi.Data.Listing.Enums.V1;

namespace WebApi.Data.Listing.Validators.V1.Base;

// Field rules shared by create and update. Rules that need the database (model and city exist) live in the service.
public abstract class ListingFieldsValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : IListingFieldsCommand
{
    private const string VinPattern = "^[A-HJ-NPR-Z0-9]{17}$";

    protected ListingFieldsValidatorBase()
    {
        RuleFor(command => command.Dto.ModelId).NotEmpty();
        RuleFor(command => command.Dto.CityId).NotEmpty();

        RuleFor(command => command.Dto.Year).InclusiveBetween(1900, DateTime.UtcNow.Year + 1);
        RuleFor(command => command.Dto.Mileage).InclusiveBetween(0, 2_000_000);
        RuleFor(command => command.Dto.MileageUnit).NotNull().IsInEnum();
        RuleFor(command => command.Dto.Horsepower).InclusiveBetween(1, 2_000);
        RuleFor(command => command.Dto.Seats).InclusiveBetween(1, 60);
        RuleFor(command => command.Dto.Price).InclusiveBetween(1m, 100_000_000m).PrecisionScale(12, 2, ignoreTrailingZeros: true);
        RuleFor(command => command.Dto.Currency).NotNull().IsInEnum();

        RuleFor(command => command.Dto.BodyType).NotNull().IsInEnum();
        RuleFor(command => command.Dto.FuelType).NotNull().IsInEnum();
        RuleFor(command => command.Dto.Transmission).NotNull().IsInEnum();
        RuleFor(command => command.Dto.Drivetrain).NotNull().IsInEnum();
        RuleFor(command => command.Dto.Color).NotNull().IsInEnum();
        RuleFor(command => command.Dto.Market).NotNull().IsInEnum();

        // Litres with one decimal; electric cars have no engine volume.
        RuleFor(command => command.Dto.EngineVolume)
            .Null()
            .WithMessage("'{PropertyName}' must be empty for electric cars.")
            .When(command => command.Dto.FuelType == FuelType.Electric);
        RuleFor(command => command.Dto.EngineVolume)
            .NotNull()
            .InclusiveBetween(0.1m, 10.0m)
            .PrecisionScale(3, 1, ignoreTrailingZeros: true)
            .When(command => command.Dto.FuelType is not null && command.Dto.FuelType != FuelType.Electric);

        RuleFor(command => command.Dto.Vin)
            .Matches(VinPattern, RegexOptions.IgnoreCase)
            .WithMessage("'{PropertyName}' must be 17 letters and digits without I, O, or Q.")
            .When(command => command.Dto.Vin is not null);

        RuleFor(command => command.Dto.Description).MaximumLength(Entities.V1.Listing.DescriptionMaxLength);
    }
}
