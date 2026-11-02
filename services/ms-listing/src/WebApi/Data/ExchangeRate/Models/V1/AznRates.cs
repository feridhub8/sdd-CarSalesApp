namespace WebApi.Data.ExchangeRate.Models.V1;

// How many AZN one unit of each currency is worth, for comparing prices.
public sealed record AznRates(decimal Usd, decimal Eur);
