# Proposal

## Why

The repository has no code yet. `ms-auth` is the first service, and it needs an empty project that follows the agreed service layout before any feature work starts, so that later changes only add features.

## What Changes

- Add root `global.json` that pins the .NET SDK to 10.0.x.
- Add root solution `CarSales.slnx`.
- Add root `.editorconfig` and root `.vscode/` (launch, settings, tasks) with an `ms-auth` build task, an `ms-auth` launch configuration, and an `All services (Local)` compound.
- Add `services/ms-auth/` with `Auth.slnx` and `README.md`.
- Add `services/ms-auth/src/WebApi/` from the empty web template with `WebApi.csproj` (.NET 10, no package references), `Program.cs`, `Properties/launchSettings.json`, and `appsettings.json` / `appsettings.Development.json`, plus a new `appsettings.Local.json`.
- Template default settings are kept, except that launch uses a single `http` profile on port 5001 that runs in the `Local` environment and does not open a browser.
- `Program.cs` contains only the code needed to start the app: it builds and runs the host, registers no services, and maps no endpoints.

Out of scope: Docker files, root `docker-compose.yml`, `Extensions/`, `Middlewares/`, `Exceptions/`, libraries, database, endpoints, and authentication. Each comes in a later change when needed.

## Capabilities

### New Capabilities

None. This change adds an empty project skeleton and no system behavior, so it declares `skip_specs: true`.

### Modified Capabilities

None.

## Impact

- New files under `services/ms-auth/` and new root `global.json` and `CarSales.slnx`.
- Building and running the project requires the .NET 10 SDK on the developer machine.
- No APIs, dependencies, or infrastructure change.
