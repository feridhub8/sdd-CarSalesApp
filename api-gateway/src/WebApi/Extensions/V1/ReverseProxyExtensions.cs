using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using WebApi.ServiceDiscovery.V1;
using Yarp.ReverseProxy.ServiceDiscovery;

namespace WebApi.Extensions.V1;

public static class ReverseProxyExtensions
{
    private const string SectionName = "ReverseProxy";
    private static readonly TimeSpan DnsRefreshPeriod = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(3);

    public static void InitReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection(SectionName))
            // A stopped replica's IP stays listed until the next DNS refresh and never answers,
            // so fail fast instead of YARP's 15-second default.
            .ConfigureHttpClient((_, handler) => handler.ConnectTimeout = ConnectTimeout)
            // Expands each destination host into one destination per IP, so a scaled service gets
            // load on every replica, and re-resolves periodically to follow replicas as they come and go.
            .AddDnsDestinationResolver(options =>
            {
                options.RefreshPeriod = DnsRefreshPeriod;
                options.AddressFamily = AddressFamily.InterNetwork;
            });

        WrapDnsResolver(services);
    }

    // YARP's DNS resolver type is internal, so wrap whatever AddDnsDestinationResolver registered last.
    private static void WrapDnsResolver(IServiceCollection services)
    {
        var dnsResolverType = services.Last(descriptor => descriptor.ServiceType == typeof(IDestinationResolver)).ImplementationType
            ?? throw new InvalidOperationException("AddDnsDestinationResolver did not register a resolver type.");

        services.RemoveAll<IDestinationResolver>();
        services.AddSingleton<IDestinationResolver>(provider => new TolerantDnsDestinationResolver(
            (IDestinationResolver)ActivatorUtilities.CreateInstance(provider, dnsResolverType),
            provider.GetRequiredService<IOptionsMonitor<DnsDestinationResolverOptions>>(),
            provider.GetRequiredService<ILogger<TolerantDnsDestinationResolver>>()));
    }
}
