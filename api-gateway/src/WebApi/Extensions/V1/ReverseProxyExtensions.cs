using System.Net.Sockets;

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
    }
}
