# Design

## Context

- `ms-auth` is the only service. It publishes `6001:8080` (Development) and `7001:8080` (Production), and listens on `5001` in Local.
- All containers join the external network `carsales_network`. Compose names the service `ms-auth`, so Docker DNS resolves `ms-auth` to every replica's IP.
- Development and Production never run at the same time on one machine (root README). The name `ms-auth` therefore always points to one environment.
- `ms-auth` validates tokens itself. It checks JWTs against Keycloak's fixed issuer and introspects on every authorized call. The refresh cookie is scoped to `Path=/api/v1/Auth`.
- The global rules say `api-gateway` is the single entry point. They say nothing about its layout, and the host port rule assumes every service publishes a port.
- Requirements are in `specs/api-gateway/spec.md`. Motivation is in proposal.md.

## Goals / Non-Goals

**Goals:**
- One address per environment for clients, with services hidden behind it in Docker.
- Scaling a service with `--scale` works and spreads load without extra configuration.
- Adding a service later means adding one route and one cluster to the gateway settings.

**Non-Goals:**
- Authentication or authorization at the gateway.
- CORS, rate limiting, response caching, request size limits, and Swagger aggregation.
- TLS. The gateway speaks HTTP like the services do today.
- Active health checks against destinations. Passive failure handling is enough for now.

## Decisions

### 1. YARP with routes in configuration

```
 client --> api-gateway (YARP)
              Routes:   auth  { Match.Path = /api/v{version}/Auth/{**rest} } --> cluster ms-auth
              Clusters: ms-auth { Destinations.main = http://ms-auth:8080 }  (Local: http://localhost:5001)
                          |  DNS destination resolver: "ms-auth" --> 172.x.0.5, 172.x.0.6, 172.x.0.7
                          v
                     replicas, RoundRobin
```

- `Yarp.ReverseProxy` 2.3.0, set up with `AddReverseProxy().LoadFromConfig(configuration.GetSection("ReverseProxy"))`.
- Routes and clusters live in `appsettings.json`, and `appsettings.Local.json` overrides only the destination address.
  - A new service then needs only configuration, and environment differences stay in the settings files as they do for `ms-auth`.
  - The alternative was routes in code. It needs a rebuild to change an address and splits configuration across code and settings.
- The gateway has no `Data/`, controllers, MediatR, Vault, or `.env`.
  - `Program.cs` calls `InitReverseProxy(configuration)` and `InitHealthChecks()`, then `MapReverseProxy()` and `MapHealthCheckEndpoints()`.
  - Both live in `Extensions/V1` (`ReverseProxyExtensions`, `HealthCheckExtensions`), following the existing pattern.

### 2. Paths are forwarded unchanged

- **Route match:** `/api/v{version}/Auth/{**rest}`, with no transforms. ASP.NET routing is case-insensitive, so `/api/v1/auth/login` matches too, as it does on `ms-auth` today.
- **Why unchanged paths:**
  - The refresh cookie's `Path=/api/v1/Auth` stays correct, because the browser sees the same path at the gateway as at the service.
  - Swagger documents and curl examples keep the same paths.
- **Rejected alternative:** a per-service prefix (`/auth/api/v1/Auth/...` with `PathRemovePrefix`). It would force the cookie path to change and make the service aware of the gateway's prefix.
- **Constraint this adds:** controller names must be unique across services, because the controller segment picks the service. They are already per-domain (`Auth`, later `Listing`, ...).
- **Default YARP header behavior is kept:**
  - Request and response headers, cookies, and `Set-Cookie` are copied.
  - The incoming `Host` is replaced by the destination host.
  - `X-Forwarded-For`, `X-Forwarded-Proto`, `X-Forwarded-Host`, and `X-Forwarded-Prefix` are added.
  - `ms-auth` does not read the `X-Forwarded-*` headers today. That is fine, because it does not use the client IP or build absolute URLs.

### 3. No token checks at the gateway

- Tokens pass through untouched.
- Validating them at the gateway too would double the Keycloak introspection calls per request and duplicate `ms-auth`'s configuration (issuer, audience, realm per environment).
- A coarse check at the gateway can be added later as its own change if a reason appears, such as shedding unauthenticated traffic before it reaches services.

### 4. Ports and host exposure

| | Local | Development | Production |
|---|---|---|---|
| `api-gateway` | `5080` (`launchSettings.json`) | `6080:8080` | `7080:8080` |
| `ms-auth` | `5001` (`launchSettings.json`) | no host port | no host port |

- The gateway takes service number `80`, which fits the existing rule: the first digit is the environment, the last digits are the service number.
  - `80` reads as the HTTP entry point.
  - `0` was tried first and rejected. Chrome and Firefox block `6000` as an unsafe port (X11), so a Development web frontend could never call it. macOS AirPlay Receiver also listens on `5000` and `7000`.
  - `5080`, `6080`, and `7080` are on neither list.
- Removing the service ports in both Docker environments is the user's decision. Development and Production then behave the same, and direct access to a service is a Local-only debugging path.
- **Consequences:**
  - `--scale ms-auth=N` works in both Docker environments.
  - Swagger in Development stays registered but cannot be reached from the host until Swagger is exposed through the gateway (Non-Goal).
- **Temporary direct access in Development:** `ms-auth/docker-compose.dev.yml` keeps the port as a commented block (`# ports: - "6001:8080"`) with a one-line note.
  - Uncommenting it gives direct access, including Swagger, for a debugging session.
  - It is commented back before committing.
  - While it is active, `--scale` does not work in Development.
  - Production has no such block.
- The gateway's compose files have no `depends_on: ms-auth`. A service compose file can be run on its own, and the DNS resolver keeps retrying until the service appears (decision 5).

### 5. Load across replicas: DNS destination resolver + RoundRobin

- **Problem:** with one destination `http://ms-auth:8080`, `HttpClient` resolves the name once per new connection and reuses pooled connections. With 3 replicas, traffic can stay pinned to the replica whose IP was resolved first.
- **Solution:** `AddDnsDestinationResolver()` turns each destination into one destination per resolved IP and refreshes the list periodically.
  - The refresh period is 10 seconds, under the 30-second bound in the spec.
  - `AddressFamily` is limited to IPv4, so that `localhost` in Local does not double into `127.0.0.1` and `::1`.
  - YARP keeps the original host name as the `Host` header for the resolved destinations.
- The cluster uses `LoadBalancingPolicy: RoundRobin`. The default `PowerOfTwoChoices` also spreads load, but RoundRobin is easy to verify: consecutive requests visibly hit different replicas in the gateway's logs.
- **Rejected alternative:** shortening `PooledConnectionLifetime` alone. It only spreads load by chance at reconnect time and gives YARP no per-replica view for balancing or passive health.
- **Connect timeout of 3 seconds** (`ConfigureHttpClient`, `SocketsHttpHandler.ConnectTimeout`), down from YARP's default of 15 seconds.
  - A stopped container's IP stays in the destination list until the next refresh, up to 10 seconds.
  - Nothing answers on that IP, so the connection attempt does not fail fast, it times out. With the default, a request waited 15 seconds and got 504 (measured).
  - Inside the Docker network a connection takes milliseconds, so 3 seconds still leaves a wide margin.
- **Down services (measured):**
  - A request to a stale IP gets 504 after the connect timeout.
  - Once the name no longer resolves, YARP answers 502, or 503 when no destination is left.
  - All are within the spec's 5 seconds. After the service returns, the next refresh, within 10 seconds, restores the destinations.

### 6. Layout and repository wiring

```
api-gateway/
├── src/WebApi/
│   ├── Extensions/V1/        ReverseProxyExtensions.cs, HealthCheckExtensions.cs
│   ├── Properties/launchSettings.json   (http://localhost:5080, Local, no browser)
│   ├── appsettings.json      routes + cluster ms-auth -> http://ms-auth:8080
│   ├── appsettings.Development.json     (empty object; Docker settings are the base)
│   ├── appsettings.Local.json           ms-auth destination -> http://localhost:5001
│   ├── Program.cs
│   └── WebApi.csproj         Nullable, ImplicitUsings, TreatWarningsAsErrors
├── ApiGateway.slnx
├── Dockerfile                repository root as build context, like ms-auth
├── docker-compose.yml        name: carsales, 7080:8080, Production
├── docker-compose.dev.yml    name: carsales, 6080:8080, Development
└── README.md
```

- `<Name>` is `ApiGateway`, the folder name in PascalCase. No `.env.example`, because there are no secrets.
- **Repository root:**
  - `CarSales.slnx` gains `/api-gateway/` with the project.
  - Both root compose files include the gateway compose file.
  - `.vscode/tasks.json` gets `build api-gateway`.
  - `.vscode/launch.json` gets `api-gateway (Local)`, which reads `launchSettings.json`, and the "All services (Local)" compound includes it.

### 7. Global rule changes (`sdd-global-rules/microservices.md`)

- **Docker, host ports line:** replace "`Development` and `Production` map their port to `8080` in the compose files" with:
  - In `Development` and `Production` only `api-gateway` publishes a host port, using service number `80`.
  - Microservices publish none.
  - `Local` keeps per-service ports in `launchSettings.json`.
- **Repositories:** add that `api-gateway` follows the service root layout, its `<Name>` is `ApiGateway`, and its `WebApi/` has no `Data/`. It counts as a service wherever the rules list services: solution, root compose files, `.vscode`.
- These are committed in the `sdd-global-rules` repository, separately from CarSales, as with the earlier rule changes.

## Risks / Trade-offs

- [Every new service needs a gateway route] → The route and cluster are added in the same change as the service. The gateway README shows the two-entry pattern.
- [Development Swagger is unreachable] → Use Swagger in Local, or temporarily uncomment the port in `ms-auth/docker-compose.dev.yml` (decision 4). Swagger through the gateway is later work and restores it for good.
- [Up to 10 seconds of stale replicas after scaling down or stopping] → Requests that land on a removed replica get 504 after the 3-second connect timeout until the next refresh. That is acceptable at this stage; active health checks can be added later to drop dead replicas sooner.
- [The refresh cookie is `Secure` while the gateway is HTTP] → Browsers accept it on `localhost`. A real server needs TLS at the gateway first (later work, noted in the proposal).
- [Existing test scripts target `6001` and `7001`] → Verification in this change runs the same scenarios against `6080` and `7080`.

## Migration Plan

1. Global rules and `config.yaml` first. They describe the target state.
2. Gateway project, verified in Local against `ms-auth` on `5001`.
3. Docker: gateway compose files, root includes, and removal of `ms-auth` host ports. Verified in Development and Production, including scaling and a stopped service.
4. Documentation.

Rollback: revert the commits. Restoring `ports` in the `ms-auth` compose files brings back direct access.
