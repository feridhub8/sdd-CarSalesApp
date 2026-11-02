# api-gateway

The single entry point for CarSales clients. A YARP reverse proxy that forwards each request to the service that owns it, unchanged, and spreads load across that service's replicas. It does not check tokens; services do.

## Routes

| Path | Service |
|---|---|
| `/api/v{version}/Auth/**` | `ms-auth` |
| `/api/v{version}/Brand/**` | `ms-listing` |
| `/api/v{version}/Model/**` | `ms-listing` |
| `/api/v{version}/City/**` | `ms-listing` |
| `/api/v{version}/Listing/**` | `ms-listing` |
| `/api/v{version}/ListingImage/**` | `ms-listing` |
| `/listing-images/**` (Production), `/listing-images-dev/**` (Development, Local) | MinIO, `GET` and `HEAD` only |
| Any other path | `web`, `GET` and `HEAD` only |

The file routes let anyone read an image by its URL without a token. Any other method on them gets `405` from the gateway, and MinIO itself refuses to list the files. Each environment serves only its own path, set in `appsettings.Development.json` and `appsettings.Local.json`.

An unknown path under `/api/` gets `404` from the gateway and never reaches the website. Paths are not rewritten, so a service is called on the same path the client used (the refresh token cookie's `Path=/api/v1/Auth` keeps working). Controller names must therefore be unique across services.

`GET /health` reports the gateway itself, whatever the state of the services. A service, the website, or MinIO that cannot be reached gives `502`, `503`, or `504` within a few seconds (connect timeout 3 seconds).

The gateway does not depend on any service being up when it starts. A service whose name does not resolve yet (for example while it waits for its migration) gets `503`, and the gateway logs a warning naming the host. It retries every 10 seconds and starts forwarding as soon as the service appears, without a restart.

## Request limits

The gateway limits API requests per client IP, in `RateLimiting` in `src/WebApi/appsettings.json`:

| Policy | Default | Paths |
|---|---|---|
| `login` | 10 per minute | `Auth/Login` |
| `register` | 5 per 10 minutes | `Auth/Register` |
| `seller` | 30 per minute | `Auth/Sellers/**` |
| `api` | 300 per minute | every other API path |

- A request over a limit gets `429` with `Retry-After` and reaches no service.
- The website, image files, and `/health` are not limited. A route gets a limit through `RateLimiterPolicy` in its settings.
- Counters are in memory: they reset on restart, and each gateway replica counts on its own.
- **Only the connection's IP counts.** `X-Forwarded-For` is ignored, since a client could send a new value each time. A load balancer in front of the gateway would make every client one IP, so it needs trusted proxy settings added with it.
- **Docker Desktop shows every client as one IP** (`192.168.65.1`), so on a Mac all clients share the limits in Development and Production. If testing hits `429`, wait for `Retry-After`, or raise a limit for the moment with an environment variable such as `RateLimiting__Login__PermitLimit=100`.

## Adding a service

Add a route and a cluster to `src/WebApi/appsettings.json`, and the Local address to `src/WebApi/appsettings.Local.json`:

```jsonc
// appsettings.json, under "ReverseProxy"
"Routes": {
  "listing": {
    "ClusterId": "ms-listing",
    "Match": { "Path": "/api/v{version}/Listing/{**rest}" }
  }
},
"Clusters": {
  "ms-listing": {
    "LoadBalancingPolicy": "RoundRobin",
    "Destinations": { "main": { "Address": "http://ms-listing:8080" } }
  }
}

// appsettings.Local.json, under "ReverseProxy"
"Clusters": {
  "ms-listing": {
    "Destinations": { "main": { "Address": "http://localhost:5002" } }
  }
}
```

The destination host is resolved to every replica's IP and re-resolved every 10 seconds, so `docker compose up --scale ms-<name>=N` needs no gateway change.

## Requirements

- .NET 10 SDK (pinned by `global.json` in the repository root)
- The services it forwards to. The gateway itself has no secrets and no `.env`.

## Build

From `api-gateway/`:

```bash
dotnet build ApiGateway.slnx
```

## Run

Start `ms-auth` first (see [`services/ms-auth/README.md`](../services/ms-auth/README.md)), then from `api-gateway/`:

```bash
dotnet run --project src/WebApi
```

The app starts in the `Local` environment on `http://localhost:5080` and forwards to `ms-auth` on `http://localhost:5001`. In VS Code, **All services (Local)** starts the gateway and every service together. Stop it with `Ctrl+C`.

```bash
curl -i -X POST http://localhost:5080/api/v1/Auth/Login -H "Content-Type: application/json" \
  -d '{"login":"jane","password":"Secret123!"}'
```

## Docker

Requires the `carsales_network` network. The root compose files start the gateway together with the infrastructure and every service (see the root [`README.md`](../README.md)). On its own, from `api-gateway/`:

```bash
# Development: http://localhost:6080
docker compose -f docker-compose.dev.yml up -d --build
docker compose -f docker-compose.dev.yml down

# Production: http://localhost:7080
docker compose up -d --build
docker compose down
```

In Docker the gateway is the only container clients can reach: services publish no host port. To run several replicas of a service, scale it; the gateway picks the replicas up within 10 seconds:

```bash
docker compose up -d --scale ms-auth=3   # from the repository root
```
