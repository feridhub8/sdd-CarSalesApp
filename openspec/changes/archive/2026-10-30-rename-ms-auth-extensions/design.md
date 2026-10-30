# Design

## Context

- `ms-auth` registers its Keycloak options, named `HttpClient`, repositories, and services in one `KeycloakExtensions.InitKeycloak`. It loads Vault secrets with `SecretExtensions.InitSecretsAsync` and maps exceptions in `ExceptionHandlingMiddleware`.
- The global rules now require four things:
  - `VaultExtensions.InitVaultAsync`.
  - `GlobalExceptionHandler`.
  - `RepositoryExtensions.InitRepositories` and `ServiceExtensions.InitServices` called from `Program.cs`, with no repository or service registered elsewhere.
  - `<Area>Extensions` for anything with its own configuration.

## Goals / Non-Goals

**Goals:** match the global names; keep behavior identical.

**Non-Goals:** changing how the handler works (it stays a middleware); changing lifetimes; shared code between services.

## Decisions

- **`KeycloakExtensions` keeps the Keycloak options and the named `HttpClient`.** They are configuration the repositories depend on, so they stay in their own area class. Only the repository and service registrations move.
- **Lifetimes stay as they are.**
  - Repositories remain singletons, because `UserKeycloakRepository` caches the service-account token across requests. The comment moves with them.
  - Services remain scoped.
- **Order in `Program.cs`:** `InitVaultAsync`, `InitLibraries`, `InitApiVersioning`, `InitKeycloak`, `InitRepositories`, `InitServices`, `InitAuthentication`, `InitSwagger`, `InitHealthChecks`. `InitAuthentication` resolves `ITokenRepository` at request time, so registration order does not matter for it.
- Files are moved with `git mv`, so history follows them.

## Risks / Trade-offs

- [A missed registration surfaces only at runtime] → The build plus the full auth scenario run catch it: login, refresh in both modes, logout, `Me`, and register.
