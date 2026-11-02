using System.Linq.Expressions;
using FluentValidation;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Enums.V1;
using WebApi.Data.Shared.Validators.V1;

namespace WebApi.Data.Listing.Validators.V1;

public sealed class GetAllListingsValidator : AbstractValidator<SearchListingsDto>
{
    private const int MaxValuesPerFilter = 20;
    private const string UnknownValue = "'{PropertyName}' has an unknown value.";

    // Far above any real price, and low enough that converting it cannot overflow.
    private const decimal MaxPriceBound = 1_000_000_000_000m;

    public GetAllListingsValidator()
    {
        RuleFor(search => search.Page).PageNumber();
        RuleFor(search => search.PageSize).PageSize();

        RuleFor(search => search.YearFrom).GreaterThanOrEqualTo(0);
        RuleFor(search => search.YearTo).GreaterThanOrEqualTo(0);
        RuleFor(search => search.YearFrom).LessThanOrEqualTo(search => search.YearTo).When(search => search.YearTo is not null);

        RuleFor(search => search.MileageFrom).GreaterThanOrEqualTo(0);
        RuleFor(search => search.MileageTo).GreaterThanOrEqualTo(0);
        RuleFor(search => search.MileageFrom).LessThanOrEqualTo(search => search.MileageTo).When(search => search.MileageTo is not null);

        RuleFor(search => search.EngineFrom).GreaterThanOrEqualTo(0);
        RuleFor(search => search.EngineTo).GreaterThanOrEqualTo(0);
        RuleFor(search => search.EngineFrom).LessThanOrEqualTo(search => search.EngineTo).When(search => search.EngineTo is not null);

        RuleFor(search => search.PriceFrom).InclusiveBetween(0, MaxPriceBound);
        RuleFor(search => search.PriceTo).InclusiveBetween(0, MaxPriceBound);
        RuleFor(search => search.PriceFrom).LessThanOrEqualTo(search => search.PriceTo).When(search => search.PriceTo is not null);

        RuleFor(search => search.PriceCurrency).Must(IsName<Currency>).When(search => search.PriceCurrency is not null).WithMessage(UnknownValue);
        RuleFor(search => search.Sort).Must(IsName<ListingSort>).When(search => search.Sort is not null).WithMessage(UnknownValue);

        this.RuleForValues<BodyType>(search => search.BodyType);
        this.RuleForValues<FuelType>(search => search.FuelType);
        this.RuleForValues<Transmission>(search => search.Transmission);
        this.RuleForValues<Drivetrain>(search => search.Drivetrain);
        this.RuleForValues<Color>(search => search.Color);
        this.RuleForValues<Market>(search => search.Market);
    }

    // The limit keeps a long query string from building a huge IN.
    private void RuleForValues<TEnum>(Expression<Func<SearchListingsDto, List<string>>> values)
        where TEnum : struct, Enum
    {
        RuleFor(values)
            .Must(list => list.Count <= MaxValuesPerFilter)
            .WithMessage($"'{{PropertyName}}' can have at most {MaxValuesPerFilter} values.")
            .Must(list => list.All(IsName<TEnum>))
            .WithMessage(UnknownValue);
    }

    // Exact names only: Enum.TryParse would also accept "1" and "Sedan,Suv".
    private static bool IsName<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        return Enum.GetNames<TEnum>().Contains(value, StringComparer.OrdinalIgnoreCase);
    }
}
