namespace WebApi.Data.ExchangeRate.Models.V1;

public sealed class ExchangeRateOptions
{
    public const string SectionName = "ExchangeRates";
    public const string HttpClientName = "ExchangeRates";

    // {0} is the date as dd.MM.yyyy.
    public string SourceUrl { get; init; } = string.Empty;

    public int RefreshHours { get; init; }

    // Used only while no rate has ever been stored.
    public AznRates Fallback { get; init; } = new(0, 0);
}
