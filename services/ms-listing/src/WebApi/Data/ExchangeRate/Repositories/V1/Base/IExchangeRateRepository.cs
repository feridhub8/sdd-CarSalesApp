using WebApi.Data.Listing.Enums.V1;

namespace WebApi.Data.ExchangeRate.Repositories.V1.Base;

public interface IExchangeRateRepository
{
    Task<List<Entities.V1.ExchangeRate>> GetAllAsync(CancellationToken ct = default);

    // Inserts or replaces the row of a currency, safely when replicas write at the same time.
    Task SaveAsync(Currency currency, decimal rateToAzn, DateOnly rateDate, CancellationToken ct = default);
}
