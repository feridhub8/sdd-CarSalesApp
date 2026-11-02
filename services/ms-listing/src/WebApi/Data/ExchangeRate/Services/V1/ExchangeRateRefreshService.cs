using Microsoft.Extensions.Options;
using WebApi.Data.ExchangeRate.Models.V1;
using WebApi.Data.ExchangeRate.Services.V1.Base;

namespace WebApi.Data.ExchangeRate.Services.V1;

// Keeps the stored rates current: on start, then every few hours. A service started just before the bank
// publishes would stay a day behind with a daily fetch, and a repeat costs one small request.
public sealed class ExchangeRateRefreshService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<ExchangeRateRefreshService> logger;
    private readonly TimeSpan period;

    public ExchangeRateRefreshService(IServiceScopeFactory scopeFactory, IOptions<ExchangeRateOptions> options, ILogger<ExchangeRateRefreshService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.period = TimeSpan.FromHours(options.Value.RefreshHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(this.period);

        do
        {
            await this.RefreshAsync(stoppingToken);
        }
        while (await WaitAsync(timer, stoppingToken));
    }

    // Never fails: without new rates, searches keep using the last ones.
    private async Task RefreshAsync(CancellationToken ct)
    {
        try
        {
            using var scope = this.scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IExchangeRateService>().RefreshAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // The service is stopping.
        }
        catch (Exception exception)
        {
            this.logger.LogWarning(exception, "Could not refresh exchange rates; searches keep using the last ones.");
        }
    }

    private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
