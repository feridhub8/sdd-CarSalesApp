# api-gateway

The single entry point for CarSales clients. A YARP reverse proxy that forwards each request to the service that owns it, unchanged, and spreads load across that service's replicas. It does not check tokens; services do.

## Routes

| Path | Service |
|---|---|
| `/api/v{version}/Auth/**` | `ms-auth` |

Anything else gets `404` from the gateway. Paths are not rewritten, so a service is called on the same path the client used (the refresh token cookie's `Path=/api/v1/Auth` keeps working). Controller names must therefore be unique across services.

`GET /health` reports the gateway itself, whatever the state of the services. A service that cannot be reached gives `502` or `503`.

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

The app starts in the `Local` environment on `http://localhost:5000` and forwards to `ms-auth` on `http://localhost:5001`. In VS Code, **All services (Local)** starts the gateway and every service together. Stop it with `Ctrl+C`.

```bash
curl -i -X POST http://localhost:5000/api/v1/Auth/Login -H "Content-Type: application/json" \
  -d '{"login":"jane","password":"Secret123!"}'
```

**macOS:** AirPlay Receiver listens on ports `5000` and `7000` when it is turned on. If the gateway cannot bind, turn it off in System Settings → General → AirDrop & Handoff.
