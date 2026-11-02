using Microsoft.Extensions.Options;
using WebApi.Data.ExchangeRate.Models.V1;
using WebApi.Data.ExchangeRate.Repositories.V1.Base;
using WebApi.Data.ExchangeRate.Services.V1.Base;
using WebApi.Data.Listing.Enums.V1;

namespace WebApi.Data.ExchangeRate.Services.V1;

public sealed class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository rateRepository;
    private readonly IExchangeRateSourceRepository sourceRepository;
    private readonly ExchangeRateOptions options;

    public ExchangeRateService(
        IExchangeRateRepository rateRepository,
        IExchangeRateSourceRepository sourceRepository,
        IOptions<ExchangeRateOptions> options)
    {
        this.rateRepository = rateRepository;
        this.sourceRepository = sourceRepository;
        this.options = options.Value;
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        // Both rates are read and checked before either is stored, so a bad answer changes nothing.
        var rates = await this.sourceRepository.GetLatestAsync(ct);

        await this.rateRepository.SaveAsync(Currency.USD, rates.Usd, rates.Date, ct);
        await this.rateRepository.SaveAsync(Currency.EUR, rates.Eur, rates.Date, ct);
    }

    public async Task<AznRates> GetRatesAsync(CancellationToken ct = default)
    {
        var stored = await this.rateRepository.GetAllAsync(ct);

        return new AznRates(
            stored.SingleOrDefault(rate => rate.Currency == Currency.USD)?.RateToAzn ?? this.options.Fallback.Usd,
            stored.SingleOrDefault(rate => rate.Currency == Currency.EUR)?.RateToAzn ?? this.options.Fallback.Eur);
    }
}
