# Proposal

## Why

The global rules now fix the names of the core extensions and the exception handler (`microservices.md`, Program.cs and file naming). `ms-listing` follows them; `ms-auth` still uses the older names. One naming scheme across services keeps them easy to read side by side and keeps the rules honest.

## What Changes

- **Vault:** `SecretExtensions.InitSecretsAsync` becomes `VaultExtensions.InitVaultAsync`.
- **Exception handler:** `ExceptionHandlingMiddleware` becomes `GlobalExceptionHandler`, in `Middlewares/V1`.
- **Keycloak registration** is split three ways:
  - `RepositoryExtensions.InitRepositories` registers the repositories (`ITokenRepository`, `IUserRepository`, still singletons).
  - `ServiceExtensions.InitServices` registers the services (`ITokenService`, `IUserService`, still scoped).
  - `KeycloakExtensions.InitKeycloak` keeps only the Keycloak options and the named `HttpClient`, as an area with its own configuration.
- `Program.cs` calls `InitRepositories` and `InitServices` directly.
- No behavior change: same endpoints, responses, and lifetimes.

## Capabilities

### New Capabilities
(none)

### Modified Capabilities
(none). This is a rename with no spec-level behavior change, so the change sets `skip_specs: true`.

## Impact

- **ms-auth only:**
  - `Extensions/V1`: `SecretExtensions.cs` becomes `VaultExtensions.cs`, `KeycloakExtensions.cs` is trimmed, and `RepositoryExtensions.cs` and `ServiceExtensions.cs` are new.
  - `Middlewares/V1/ExceptionHandlingMiddleware.cs` becomes `GlobalExceptionHandler.cs`.
  - `Program.cs` is updated.
- No configuration, infrastructure, or documentation changes. The READMEs do not name these types.
