using WebApi.Data.ExchangeRate.Models.V1;

namespace WebApi.Data.ExchangeRate.Repositories.V1.Base;

public interface IExchangeRateSourceRepository
{
    // Throws when the source cannot be reached or its answer is unusable.
    Task<SourceRates> GetLatestAsync(CancellationToken ct = default);
}
