# Design

## Context

The repository contains only OpenSpec and Graphify setup. See proposal.md for scope. The developer machine has the .NET 10 SDK (10.0.403) alongside .NET 8, and a trusted HTTPS development certificate.

## Goals / Non-Goals

**Goals:**
- The skeleton builds and starts from both `Auth.slnx` and `CarSales.slnx`.
- Running it locally uses the `Local` environment.

**Non-Goals:**
- Any HTTP endpoint, including health checks and Swagger.
- Container support.

## Decisions

**Minimal `Program.cs` instead of an empty file.**
An empty `Program.cs` has no entry point, so a Web SDK project fails to build (`CS5001`). The file contains only `WebApplication.CreateBuilder(args)`, `Build()`, and `Run()`. This keeps the project buildable while registering nothing.
Alternative: a truly empty file. Rejected because the skeleton would not build until the next change.

**`.slnx` solution format.**
Both solutions use `.slnx`, as the rules require for .NET 10 projects. It is the default of .NET 10's `dotnet new sln`, plain XML, and produces far fewer merge conflicts than `.sln`.
Alternative: classic `.sln` via `--format sln`. Rejected because the rules reserve it for projects on older .NET versions.

**Pin the SDK with a root `global.json`.**
`global.json` sets `sdk.version` to `10.0.403` with `rollForward: latestFeature`. The CLI then uses 10.0.403 or a newer 10.0 SDK, never 11, even when a newer SDK is installed. This matters because `TreatWarningsAsErrors` turns new warnings from a newer SDK into build failures.
Alternative: no pin, letting the CLI pick the newest installed SDK. Rejected because an unrelated SDK upgrade could break the build.

**Root solution groups services in solution folders.**
`CarSales.slnx` references `services/ms-auth/src/WebApi/WebApi.csproj` under a `services/ms-auth` solution folder, so later services and `api-gateway` slot in beside it. `Auth.slnx` references the same project for working on the service alone.

**Editor and IDE files live in the repository root.**
`.editorconfig` and `.vscode/` are in the repository root, as the rules require for a monorepo. One `.editorconfig` keeps the style identical across services, and VS Code only reads `.vscode/` from the folder it opens, which is the repository root. `.vscode/` has one build task and one launch configuration per service, plus an `All services (Local)` compound that starts every service with one F5. Launch configurations read port and environment from the service's `launchSettings.json` (`launchSettingsProfile`), so those values live in one place.
Alternative: per-service files. Rejected because a per-service `.editorconfig` with `root = true` hides the shared one, and a per-service `.vscode/` is ignored when the repository is opened at its root.

**Generate with `dotnet new web` and keep its defaults.**
The project is created from the empty web template and its default settings are kept. The only code removed is the sample `Hello World!` endpoint, since the app must only start.
Alternative: `dotnet new webapi`. Rejected because it adds OpenAPI packages and a sample controller.

**One `http` launch profile on port 5001.**
The template's `https` profile is removed: services sit behind the gateway and do not need HTTPS locally. The `http` profile uses the fixed port `5001` instead of the template's random one, runs in `Local`, matching the `Local` environment definition, and sets `launchBrowser` to `false`.
Alternative: keep the template's `http` and `https` profiles. Rejected because HTTPS is not needed and random ports are hard to remember across services.

**`appsettings.Local.json` mirrors the template's Development settings.**
`appsettings.json` and `appsettings.Development.json` stay as the template generates them. `appsettings.Local.json` is added with the same content as `appsettings.Development.json`, so `Local` starts from the same defaults.

## Risks / Trade-offs

- [Template output differs between SDK versions] → Tasks verify the final file list against the proposal instead of trusting the template.
- [A project added to the service later is put in only one of the two solutions] → Accepted trade-off; every future change that adds a project includes a task to add it to both `Auth.slnx` and `CarSales.slnx`.
- [A machine has only a 10.0 SDK older than 10.0.403] → The CLI stops with a message naming the required version; install the latest 10.0 SDK.
- [Tools older than .NET SDK 9.0.200 or older IDEs cannot open `.slnx`] → The local machine uses SDK 10; any future CI uses a .NET 10 image.
