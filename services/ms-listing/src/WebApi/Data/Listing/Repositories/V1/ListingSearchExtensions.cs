using System.Linq.Expressions;
using WebApi.Data.ExchangeRate.Models.V1;
using WebApi.Data.Listing.Enums.V1;
using WebApi.Data.Listing.Models.V1;

namespace WebApi.Data.Listing.Repositories.V1;

public static class ListingSearchExtensions
{
    private const decimal KilometresPerMile = 1.609m;

    private static readonly Expression<Func<Entities.V1.Listing, decimal>> MileageInKm = listing =>
        listing.MileageUnit == MileageUnit.Mi ? listing.Mileage * KilometresPerMile : listing.Mileage;

    public static IQueryable<Entities.V1.Listing> Matching(this IQueryable<Entities.V1.Listing> listings, ListingSearch search, AznRates rates)
    {
        var priceInAzn = PriceInAzn(rates);

        return listings
            .WhereIf(search.BrandId is not null, listing => listing.Model.BrandId == search.BrandId)
            .WhereIf(search.ModelId is not null, listing => listing.ModelId == search.ModelId)
            .WhereIf(search.CityId is not null, listing => listing.CityId == search.CityId)
            .WhereIf(search.YearFrom is not null, listing => listing.Year >= search.YearFrom)
            .WhereIf(search.YearTo is not null, listing => listing.Year <= search.YearTo)
            .WhereIf(search.EngineFrom is not null, listing => listing.EngineVolume >= search.EngineFrom)
            .WhereIf(search.EngineTo is not null, listing => listing.EngineVolume <= search.EngineTo)
            .WhereIf(search.BodyType.Count > 0, listing => search.BodyType.Contains(listing.BodyType))
            .WhereIf(search.FuelType.Count > 0, listing => search.FuelType.Contains(listing.FuelType))
            .WhereIf(search.Transmission.Count > 0, listing => search.Transmission.Contains(listing.Transmission))
            .WhereIf(search.Drivetrain.Count > 0, listing => search.Drivetrain.Contains(listing.Drivetrain))
            .WhereIf(search.Color.Count > 0, listing => search.Color.Contains(listing.Color))
            .WhereIf(search.Market.Count > 0, listing => search.Market.Contains(listing.Market))
            .WhereIf(search.IsCredit, listing => listing.IsCredit)
            .WhereIf(search.IsBarter, listing => listing.IsBarter)
            .WhereIf(search.NotPainted, listing => !listing.IsPainted)
            .WhereIf(search.NotDamaged, listing => !listing.IsDamaged)
            .WhereIf(search.HasImages, listing => listing.Images.Any())
            .WhereAtLeast(MileageInKm, search.MileageFrom)
            .WhereAtMost(MileageInKm, search.MileageTo)
            .WhereAtLeast(priceInAzn, ToAzn(search.PriceFrom, search.PriceCurrency, rates))
            .WhereAtMost(priceInAzn, ToAzn(search.PriceTo, search.PriceCurrency, rates));
    }

    // Every sort ends with the id, so pages stay stable for equal values.
    public static IQueryable<Entities.V1.Listing> SortedBy(this IQueryable<Entities.V1.Listing> listings, ListingSort sort, AznRates rates)
    {
        return sort switch
        {
            ListingSort.PriceAsc => listings.OrderBy(PriceInAzn(rates)).ThenByDescending(listing => listing.Id),
            ListingSort.PriceDesc => listings.OrderByDescending(PriceInAzn(rates)).ThenByDescending(listing => listing.Id),
            ListingSort.YearDesc => listings.OrderByDescending(listing => listing.Year).ThenByDescending(listing => listing.Id),
            ListingSort.MileageAsc => listings.OrderBy(MileageInKm).ThenByDescending(listing => listing.Id),
            _ => listings.NewestFirst()
        };
    }

    // The id breaks ties for listings created at the same instant.
    public static IQueryable<Entities.V1.Listing> NewestFirst(this IQueryable<Entities.V1.Listing> listings)
    {
        return listings.OrderByDescending(listing => listing.CreatedAt).ThenByDescending(listing => listing.Id);
    }

    private static Expression<Func<Entities.V1.Listing, decimal>> PriceInAzn(AznRates rates)
    {
        return listing => listing.Currency == Currency.AZN
            ? listing.Price
            : listing.Currency == Currency.USD ? listing.Price * rates.Usd : listing.Price * rates.Eur;
    }

    private static decimal? ToAzn(decimal? amount, Currency currency, AznRates rates)
    {
        return currency switch
        {
            Currency.USD => amount * rates.Usd,
            Currency.EUR => amount * rates.Eur,
            _ => amount
        };
    }

    private static IQueryable<Entities.V1.Listing> WhereIf(
        this IQueryable<Entities.V1.Listing> listings, bool condition, Expression<Func<Entities.V1.Listing, bool>> predicate)
    {
        return condition ? listings.Where(predicate) : listings;
    }

    private static IQueryable<Entities.V1.Listing> WhereAtLeast(
        this IQueryable<Entities.V1.Listing> listings, Expression<Func<Entities.V1.Listing, decimal>> value, decimal? bound)
    {
        return bound is null
            ? listings
            : listings.Where(Expression.Lambda<Func<Entities.V1.Listing, bool>>(Expression.GreaterThanOrEqual(value.Body, Expression.Constant(bound.Value)), value.Parameters));
    }

    private static IQueryable<Entities.V1.Listing> WhereAtMost(
        this IQueryable<Entities.V1.Listing> listings, Expression<Func<Entities.V1.Listing, decimal>> value, decimal? bound)
    {
        return bound is null
            ? listings
            : listings.Where(Expression.Lambda<Func<Entities.V1.Listing, bool>>(Expression.LessThanOrEqual(value.Body, Expression.Constant(bound.Value)), value.Parameters));
    }
}
