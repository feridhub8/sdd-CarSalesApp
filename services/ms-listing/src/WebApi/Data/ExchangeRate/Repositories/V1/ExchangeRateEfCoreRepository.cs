using Microsoft.EntityFrameworkCore;
using WebApi.Data.ExchangeRate.Repositories.V1.Base;
using WebApi.Data.Listing.Enums.V1;
using WebApi.Persistence;

namespace WebApi.Data.ExchangeRate.Repositories.V1;

public sealed class ExchangeRateEfCoreRepository : IExchangeRateRepository
{
    private readonly ApplicationDbContext context;

    public ExchangeRateEfCoreRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public Task<List<Entities.V1.ExchangeRate>> GetAllAsync(CancellationToken ct = default)
    {
        return this.context.ExchangeRates.AsNoTracking().ToListAsync(ct);
    }

    public async Task SaveAsync(Currency currency, decimal rateToAzn, DateOnly rateDate, CancellationToken ct = default)
    {
        await this.context.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO exchange_rates (currency, rate_to_azn, rate_date, updated_at)
            VALUES ({currency.ToString()}, {rateToAzn}, {rateDate}, {DateTime.UtcNow})
            ON CONFLICT (currency) DO UPDATE
            SET rate_to_azn = EXCLUDED.rate_to_azn, rate_date = EXCLUDED.rate_date, updated_at = EXCLUDED.updated_at
            """,
            ct);
    }
}
