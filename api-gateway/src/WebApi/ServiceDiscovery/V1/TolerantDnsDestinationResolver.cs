using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.ServiceDiscovery;

namespace WebApi.ServiceDiscovery.V1;

// YARP's DNS resolver throws when a host does not resolve, and on the initial load that stops the gateway.
// This wrapper turns a failed resolution into an empty cluster (its routes answer 503) and retries it after
// the refresh period, so a service that starts after the gateway is picked up without a restart.
public sealed class TolerantDnsDestinationResolver : IDestinationResolver
{
    private static readonly TimeSpan FallbackRetryPeriod = TimeSpan.FromSeconds(10);

    private readonly IDestinationResolver dnsResolver;
    private readonly IOptionsMonitor<DnsDestinationResolverOptions> options;
    private readonly ILogger<TolerantDnsDestinationResolver> logger;

    public TolerantDnsDestinationResolver(
        IDestinationResolver dnsResolver,
        IOptionsMonitor<DnsDestinationResolverOptions> options,
        ILogger<TolerantDnsDestinationResolver> logger)
    {
        this.dnsResolver = dnsResolver;
        this.options = options;
        this.logger = logger;
    }

    public async ValueTask<ResolvedDestinationCollection> ResolveDestinationsAsync(
        IReadOnlyDictionary<string, DestinationConfig> destinations, CancellationToken ct)
    {
        try
        {
            return await this.dnsResolver.ResolveDestinationsAsync(destinations, ct);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var retryPeriod = this.options.CurrentValue.RefreshPeriod ?? FallbackRetryPeriod;
            this.logger.LogWarning(
                exception,
                "Could not resolve {Addresses}; their routes answer 503 until it resolves. Retrying in {RetryPeriod}.",
                string.Join(", ", destinations.Values.Select(destination => destination.Address)),
                retryPeriod);

            var retry = new CancellationTokenSource(retryPeriod);
            return new ResolvedDestinationCollection(new Dictionary<string, DestinationConfig>(), new CancellationChangeToken(retry.Token));
        }
    }
}
