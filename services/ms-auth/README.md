# ms-auth

Authentication service. Currently an empty project that only starts; features come in later changes.

## Requirements

- .NET 10 SDK (pinned by `global.json` in the repository root)

## Build

From `services/ms-auth/`:

```bash
dotnet build Auth.slnx
```

## Run

From `services/ms-auth/`:

```bash
dotnet run --project src/WebApi
```

The app starts in the `Local` environment on `http://localhost:5001`. Stop it with `Ctrl+C`.
