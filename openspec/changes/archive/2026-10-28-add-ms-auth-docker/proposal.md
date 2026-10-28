# Proposal

## Why

`ms-auth` runs only with `dotnet run` today. The `Development` and `Production` environments are defined as running from Docker Compose, so the service needs container support before any further service or infrastructure joins it.

## What Changes

- Add `services/ms-auth/Dockerfile` (multi-stage, .NET 10, non-root), built with the repository root as build context.
- Add root `.dockerignore`, shared by every image built from the repository root.
- Add `services/ms-auth/docker-compose.yml` (`Production`, host port `7001`) and `services/ms-auth/docker-compose.dev.yml` (`Development`, host port `6001`), both mapping to container port `8080`, without fixed container names, and attached to the external network `carsales_network`. `Local` keeps port `5001`.
- Add root `docker-compose.yml` and `docker-compose.dev.yml` that include the service compose files, so the whole system starts from the repository root.
- Add root `README.md` stating that the external network `carsales_network` is required and how to create it, plus how to run the system with Docker.
- Extend `services/ms-auth/README.md` with the Docker commands for the service.

Out of scope: infrastructure tools, the API gateway, health checks, `.env` files, and `appsettings.Production.json` (`Production` uses `appsettings.json`). Global rule updates in `sdd-global-rules` are handled separately.

## Capabilities

### New Capabilities

None. This change adds container support and no system behavior, so it declares `skip_specs: true`.

### Modified Capabilities

None.

## Impact

- New files: `services/ms-auth/Dockerfile`, `services/ms-auth/docker-compose.yml`, `services/ms-auth/docker-compose.dev.yml`, root `.dockerignore`, `docker-compose.yml`, `docker-compose.dev.yml`, and `README.md`.
- Changed file: `services/ms-auth/README.md`.
- Running with Docker requires Docker with Compose and the `carsales_network` network, on the developer machine and on the Production server.
- `Development` runs only on the developer machine; `Production` is the only deployed environment and never runs at the same time as `Development` on one machine.
