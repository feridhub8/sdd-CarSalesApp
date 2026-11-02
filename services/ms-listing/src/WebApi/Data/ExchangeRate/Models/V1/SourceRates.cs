namespace WebApi.Data.ExchangeRate.Models.V1;

public sealed record SourceRates(decimal Usd, decimal Eur, DateOnly Date);
