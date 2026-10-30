# Design

## Context

- `ReverseProxyExtensions` registers YARP 2.3 with `AddDnsDestinationResolver` (10-second refresh, IPv4 only), a 3-second connect timeout, and routes from configuration.
- YARP calls the registered `IDestinationResolver` once per cluster while loading configuration.
  - The DNS resolver throws when a host does not resolve.
  - On the initial load, that exception escapes `MapReverseProxy` in `Program.cs`.
  - The process then stops answering without exiting. This was observed in `add-listing-catalog`, where `ms-listing` waits for `ms-listing-migrate`.
- **Behavior at runtime is already correct:** a name that stops resolving after startup only empties that cluster (502/503).
- `DnsDestinationResolverOptions` has no setting to tolerate unresolved hosts. It offers only `RefreshPeriod` and `AddressFamily`.
- Requirements are in `specs/api-gateway/spec.md`.

## Goals / Non-Goals

**Goals:**
- Startup never depends on any service being up.
- An unresolved cluster recovers on the existing refresh cycle.

**Non-Goals:**
- Active health checks.
- Making the gateway exit on other startup errors.
- A Docker healthcheck. Docker does not restart unhealthy containers outside Swarm, so it would not help here.

## Decisions

### 1. Wrap YARP's DNS resolver instead of replacing it

```
YARP config load ──> TolerantDnsDestinationResolver
                       ├─ try   DnsDestinationResolver.ResolveDestinationsAsync  ──> resolved IPs + its change token
                       └─ catch (not cancellation): log warning with the host
                                 ──> no destinations + change token that fires after RefreshPeriod
```

- `TolerantDnsDestinationResolver : IDestinationResolver` takes the inner DNS resolver (as `IDestinationResolver`) and the `DnsDestinationResolverOptions` monitor.
  - **Success:** it returns the inner result unchanged, so IP expansion, refresh, and RoundRobin behave exactly as now.
  - **Failure:** it returns an empty destination collection with a `CancellationChangeToken` that fires after `RefreshPeriod`. YARP re-resolves when the token fires, so a missing host is retried every 10 seconds.
  - Cancellation exceptions are not caught, so shutdown still works.
- **Alternatives rejected:**
  - Writing a whole DNS resolver duplicates YARP's code, IP expansion, and host header handling.
  - `depends_on` from the gateway to every service breaks running a compose file on its own. It also does not cover a service that is down when the gateway restarts.
  - A plain destination address without DNS expansion loses load spreading across replicas.
- **One cluster at a time:** YARP resolves per cluster, so only the cluster with the unknown host is emptied. Each cluster has one destination host today. If a cluster ever lists several hosts, one unresolved host empties that whole cluster until the next refresh. That is acceptable, and the warning names the host.

### 2. Registration

- **Checked at implementation:** `DnsDestinationResolver` is `internal` in YARP 2.3, so it cannot be named or registered directly. `ResolvedDestinationCollection` has a public constructor `(destinations, changeToken)`.
- `AddDnsDestinationResolver(...)` stays as it is (10-second refresh, IPv4). It registers YARP's no-op resolver (`TryAdd`) and then the DNS resolver as `IDestinationResolver`.
- `ReverseProxyExtensions.WrapDnsResolver` then:
  - takes the implementation type of the last `IDestinationResolver` registration (the DNS resolver),
  - removes all `IDestinationResolver` registrations,
  - registers a singleton factory that builds that type with `ActivatorUtilities.CreateInstance` and wraps it in `TolerantDnsDestinationResolver`.
- The wrapper reads the retry period from `IOptionsMonitor<DnsDestinationResolverOptions>` (public), with 10 seconds as a fallback.

### 3. Location in the gateway

- The global rules give the gateway no `Data/`, and the class is not an extension.
- It goes in `ServiceDiscovery/V1/TolerantDnsDestinationResolver.cs`, versioned like the other folders, with namespace `WebApi.ServiceDiscovery.V1`. The folder is named after YARP's own `ServiceDiscovery` namespace.

## Risks / Trade-offs

- [A misspelled service name no longer stops the gateway] → It is logged as a warning on every refresh, naming the host, and its routes answer 503. Testing a new route through the gateway shows it at once.
- [Up to 10 seconds before a newly started service is used] → This is within the spec's 30 seconds and is the same delay as scaling today.
- [A YARP upgrade changes the resolver API] → `Yarp.ReverseProxy` is pinned (2.3.0). The wrapper depends only on public contracts (`IDestinationResolver`, `ResolvedDestinationCollection`, `DnsDestinationResolverOptions`) and on `AddDnsDestinationResolver` registering its resolver last; the registration code fails at startup with a clear message if it does not.

## Migration Plan

Deploy the new gateway image. No configuration or service changes. Rollback: revert the commit; the documented manual restart works again as the workaround.
