using WebApi.Data.ExchangeRate.Models.V1;

namespace WebApi.Data.ExchangeRate.Services.V1.Base;

public interface IExchangeRateService
{
    // Fetches and stores the latest rates. Throws when they cannot be fetched, leaving the stored ones as they are.
    Task RefreshAsync(CancellationToken ct = default);

    Task<AznRates> GetRatesAsync(CancellationToken ct = default);
}
