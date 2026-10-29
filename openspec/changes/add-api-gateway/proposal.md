# Proposal

## Why

Clients call each service on its own host port (`ms-auth` on `6001` and `7001`). Every new service would add another address for clients to know. A service that publishes a host port also cannot run several replicas: `docker compose up --scale` fails on the port clash. The project already chose YARP for `api-gateway/` as the single entry point. Now is the right time to add it, before a second service exists and before clients are written.

## What Changes

- **New `api-gateway`** (YARP) in `api-gateway/`, built like a service: `src/WebApi/`, `ApiGateway.slnx`, `Dockerfile`, two compose files, and a README. It has no `Data/`, no secrets, and no `.env`.
  - **Routes:** `/api/v{version}/Auth/**` is forwarded to `ms-auth` unchanged: path, query, headers, cookies, and responses. There is no prefix rewriting, so the refresh token cookie path `/api/v1/Auth` keeps working. Other paths get `404` from the gateway.
  - **No token checks:** the gateway forwards tokens untouched, and services keep validating them.
  - **Replicas:** the gateway resolves a service name to every replica and spreads requests across them. It refreshes the list so scaled-up or removed replicas are picked up.
  - **Ports:** service number `0`, so `5000` (Local), `6000` (Development), and `7000` (Production).
  - `GET /health` reports the gateway itself.
- **BREAKING (Development and Production):** microservices publish no host ports. `6001` and `7001` are removed from `ms-auth`, and clients reach it through `6000` and `7000`. Local runs keep reaching services directly (`5001`). There are no clients yet.
- **Consequence:** Swagger stays enabled in Development but has no host port by default.
  - The `ms-auth` Development compose file keeps the port as a commented block. Uncommenting it gives temporary direct access, Swagger included, for debugging.
  - Exposing Swagger through the gateway is later work.
- **Local:** the gateway's Local settings point to `http://localhost:5001`. VS Code gets an `api-gateway (Local)` configuration, and it joins the "All services" compound.
- **Global rules (`sdd-global-rules`):**
  - The host port rule changes: in Development and Production only `api-gateway` publishes a port, using service number `0`.
  - A new rule describes the `api-gateway` layout.
- **`openspec/config.yaml`:** the repository list gains `docker-compose.dev.yml`, which already exists.

## Capabilities

### New Capabilities
- `api-gateway`: the single client entry point. It covers which requests are forwarded to which service, forwarding without changes, `404` for unknown paths, load across replicas, behavior when a service is down, and the gateway health check.

### Modified Capabilities
(none). The `authentication` endpoints, paths, and cookie attributes do not change; only the address clients use in Docker does.

## Impact

- **New:** `api-gateway/` with the `Yarp.ReverseProxy` 2.3.0 package.
- **Repository root:** `CarSales.slnx`, `docker-compose.yml`, `docker-compose.dev.yml`, `.vscode/tasks.json`, `.vscode/launch.json`, and `README.md` (services and ports tables, Docker section).
- **ms-auth:** both compose files lose `ports`. The README Docker section and examples point to the gateway.
- **Global rules:** `microservices.md` gets the port rule change and the gateway layout rule. These are committed in the `sdd-global-rules` repository.
- **Testing:** Development and Production scenarios run against `6000` and `7000` instead of `6001` and `7001`.
- **Later work:** CORS (needed with a web frontend), rate limiting, Swagger through the gateway, and TLS termination at the gateway. The refresh cookie is always `Secure`, so a real server needs HTTPS.
