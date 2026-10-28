# Design

## Context

`ms-auth` is an empty .NET 10 Web API (see the archived `add-ms-auth-skeleton`) that runs locally on port `5001`. The repository root holds `global.json` (SDK `10.0.403`, `latestFeature`) and `CarSales.slnx`. Docker Desktop with Compose v5 is available; no `carsales_network` network exists yet. See proposal.md for scope.

Verified before implementation:
- `mcr.microsoft.com/dotnet/sdk:10.0` ships SDK `10.0.403`, which satisfies `global.json`.
- Compose accepts a top-level `name` inside files listed under `include:`; the root and the service files both resolve to project `carsales`.
- Host ports `5001`, `6001`, and `7001` are free on the developer machine.

`Development` runs only on the developer machine. `Production` is the only deployed environment; it can also run on the developer machine, but never at the same time as `Development`.

## Goals / Non-Goals

**Goals:**
- `ms-auth` builds into an image and runs from its own compose files and from the root compose files.
- `Production` and `Development` containers start in the matching `ASPNETCORE_ENVIRONMENT`.
- `Local` (`dotnet run`) and `Development` (Docker) can run at the same time.

**Non-Goals:**
- Container health checks; there is no endpoint to probe yet.
- Image publishing to a registry and the deployment itself.

## Decisions

**Repository root as build context.**
Every image is built with the repository root as context and `-f services/<service>/Dockerfile`. Shared projects added later (for example event contracts) sit outside the service folder and must be reachable from the build.
Alternative: the service folder as context. Rejected because shared projects could not be copied into the image.

**One root `.dockerignore`.**
With the repository root as context, Docker reads `.dockerignore` from the root. It excludes build output (`bin/`, `obj/`), VCS and IDE folders, `.env` files, `graphify-out/`, `openspec/`, `.claude/`, and macOS `.DS_Store` files. A service that needs different rules adds `services/<service>/Dockerfile.dockerignore`, which replaces the root file for that build rather than extending it.

**Multi-stage Dockerfile, restore before copying sources.**
The `build` stage uses `mcr.microsoft.com/dotnet/sdk:10.0`: it copies `global.json` and `WebApi.csproj` first, runs `dotnet restore`, then copies the rest of `src/WebApi/` and runs `dotnet publish -c Release`. Restoring before the source copy keeps the restore layer cached until the project file changes. The `final` stage uses `mcr.microsoft.com/dotnet/aspnet:10.0`, runs as the image's built-in non-root user (`USER $APP_UID`), exposes `8080` (the image default), and starts `dotnet WebApi.dll`.
Copying `global.json` keeps the container build on the same SDK rules as local builds.
Alternative: a single-stage image from the SDK. Rejected because it ships the SDK and is several times larger.

**Host port per environment.**
The first digit of the host port is the environment and the last digits are the service number: `Local` `5001`, `Development` `6001`, `Production` `7001`. The next service uses `5002` / `6002` / `7002`. `Local` and `Development` can run side by side, and a port tells which environment answered.
Alternative: one port for every environment. Rejected because `dotnet run` and the `Development` container could not run together.

**No fixed container names.**
Compose names containers `<project>-<service>-<n>` (`carsales-ms-auth-1`), so a service can later run several replicas with `--scale`.
Alternative: `container_name: carsales_ms-auth`. Rejected because a fixed name allows only one container per service.

**One compose project name: `carsales`.**
Every compose file sets `name: carsales`, so the root files and the service files form one Compose project. Running from the root and then from a service folder reuses the same containers. `Development` and `Production` never run at the same time on one machine, so they share the project name and the network; switching between them recreates the service.

**Service compose files.**
`docker-compose.yml` and `docker-compose.dev.yml` define the `ms-auth` service with `build.context: ../..` and `build.dockerfile: services/ms-auth/Dockerfile`, `restart: unless-stopped`, ports `7001:8080` and `6001:8080`, and `ASPNETCORE_ENVIRONMENT` `Production` and `Development`. Both attach to `carsales_network`, declared `external: true`. No `env_file` is set because the service has no `.env` yet.

**Root compose files include the service files.**
Root `docker-compose.yml` and `docker-compose.dev.yml` set `name: carsales` and list the service compose files under `include:`. A new service adds one line to each. `docker compose up -d --build` starts `Production`; `docker compose -f docker-compose.dev.yml up -d --build` starts `Development`.

**External network is created once, documented in the root README.**
`carsales_network` stays external. Root `README.md` lists it as a prerequisite on the developer machine and on the Production server, with `docker network create carsales_network`. Before running Docker commands, the assistant checks whether the network exists and creates it when it is missing.

## Risks / Trade-offs

- [`--scale` fails while a service publishes a host port, because one host port binds to one container] → Accepted until the API gateway arrives; services behind the gateway drop their host ports and are reached as `ms-<name>:8080` on `carsales_network`, where Docker balances requests across replicas.
