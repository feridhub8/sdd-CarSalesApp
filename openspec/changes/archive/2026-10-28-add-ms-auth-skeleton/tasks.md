# Tasks

## 1. Prerequisites

- [x] 1.1 Install the .NET 10 SDK and verify `dotnet --list-sdks` lists a `10.0.x` version
- [x] 1.2 Add root `global.json` with `sdk.version` `10.0.403` and `rollForward` `latestFeature`, and verify `dotnet --version` in the repository root prints `10.0.403`

## 2. Web API project

- [x] 2.1 Create `services/ms-auth/src/WebApi/` with `dotnet new web -n WebApi -f net10.0` and verify `WebApi.csproj` targets `net10.0` with no `PackageReference`
- [x] 2.2 Enable `Nullable`, `ImplicitUsings`, and `TreatWarningsAsErrors` in `WebApi.csproj` and verify the three properties are present
- [x] 2.3 Remove the sample `Hello World!` endpoint from `Program.cs` and verify only `CreateBuilder` → `Build` → `Run` remain
- [x] 2.4 In `Properties/launchSettings.json`, remove the `https` profile and set the `http` profile to `http://localhost:5001`, `ASPNETCORE_ENVIRONMENT=Local`, and `launchBrowser: false`, and verify it is the only profile
- [x] 2.5 Add `appsettings.Local.json` with the same content as the template's `appsettings.Development.json` and verify `appsettings.json` and `appsettings.Development.json` are unchanged from the template

## 3. Solutions

- [x] 3.1 Create `services/ms-auth/Auth.slnx`, add `WebApi.csproj`, and verify `dotnet build services/ms-auth/Auth.slnx` succeeds with 0 warnings
- [x] 3.2 Create root `CarSales.slnx`, add `WebApi.csproj` under a `services/ms-auth` solution folder, and verify `dotnet build CarSales.slnx` succeeds with 0 warnings

## 4. Service files

- [x] 4.1 Add root `.editorconfig` (4-space indent, file-scoped namespaces) and verify `dotnet format CarSales.slnx --verify-no-changes` passes
- [x] 4.2 Add root `.vscode/tasks.json` (`build ms-auth`), `.vscode/launch.json` (`ms-auth (Local)` reading the `http` profile from `launchSettings.json`, plus an `All services (Local)` compound), and `.vscode/settings.json`, and verify the build task builds, the program path exists, and the compound and `preLaunchTask` names resolve
- [x] 4.3 Add `services/ms-auth/README.md` with build and run commands and verify each documented command runs as written

## 5. Verification

- [x] 5.1 Run `dotnet run --project services/ms-auth/src/WebApi` and verify the log shows `Hosting environment: Local`, then stop it
- [x] 5.2 Verify the files under `services/ms-auth/` match the proposal (no Docker files, no `Extensions/`, `Middlewares/`, `Exceptions/`) and `git status` shows no `bin/` or `obj/`

## Workflow follow-up

- Review the result, commit it with approval, then archive the change with `/opsx:archive`.
