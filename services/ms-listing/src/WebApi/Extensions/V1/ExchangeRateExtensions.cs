using WebApi.Data.ExchangeRate.Models.V1;
using WebApi.Data.ExchangeRate.Services.V1;

namespace WebApi.Extensions.V1;

public static class ExchangeRateExtensions
{
    private const int MaxResponseBytes = 1024 * 1024;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

    public static void InitExchangeRates(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExchangeRateOptions>(configuration.GetSection(ExchangeRateOptions.SectionName));

        services.AddHttpClient(ExchangeRateOptions.HttpClientName, client =>
            {
                client.Timeout = RequestTimeout;
                client.MaxResponseContentBufferSize = MaxResponseBytes;
            })
            // A redirect means the file was not found: the site sends those to an HTML page.
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false });

        services.AddHostedService<ExchangeRateRefreshService>();
    }
}
