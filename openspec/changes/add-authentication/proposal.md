# Proposal

## Why

CarSales has no way to identify users. ms-auth is an empty service. Every later feature that depends on who the caller is, such as listing a car for sale, needs users who can register, sign in, keep their session alive, and sign out. The project decisions say Keycloak comes when authentication is requested and Vault comes with the first secret. This change brings both, together with the PostgreSQL server that Keycloak needs.

## What Changes

- Add the `infrastructure/` folder with three tools, each started with docker compose:
  - `postgres`: the shared PostgreSQL server. It holds the development and production databases (development names end with `Dev`) and, for now, the `keycloak` database.
  - `keycloak`: one instance on its own database. It has the realms `carsales` (production) and `carsalesDev` (development), imported on first start, each with an `ms-auth` client.
  - `vault`: persistent secret store. It keeps the `ms-auth` client secret per environment. It is initialized once in the UI (5 keys, threshold 3) and unsealed by hand after each start, the same way on a PC and on a server. The keys are downloaded from the UI as a JSON file outside the project, on the Desktop by default. Services sign in with AppRole, people with userpass. The root token is revoked after setup.
- Root `docker-compose.yml` and `docker-compose.dev.yml` include the infrastructure.
- ms-auth gains five endpoints under `api/v1/Auth`: register, login, refresh, logout, and me (the current user). ms-auth acts as a facade over Keycloak.
- ms-auth signs in to Vault with AppRole and loads its secrets at startup. `VAULT_ROLE_ID` and `VAULT_SECRET_ID` are the only values in `services/ms-auth/.env`, loaded with DotNetEnv.
- ms-auth gets the base pieces that the global rules require for a working API: API versioning, MediatR request flow, FluentValidation, Mapster, global exception middleware, Swagger (Local and Development only), and a `/health` endpoint.
- READMEs document first-time setup: the network, Vault init and seeding, and the ms-auth `.env`.

## Capabilities

### New Capabilities
- `authentication`: user registration, login with email and password, token refresh, logout, and reading the current user's profile.

### Modified Capabilities
(none)

## Impact

- **New folders:** `infrastructure/` (postgres, keycloak, vault).
- **ms-auth:** new code under `src/WebApi` (Data/Auth, Exceptions, Extensions, Middlewares), new NuGet packages, `appsettings.*` per environment, `.env.example`, compose files gain `env_file`.
- **Root:** compose files include the infrastructure; README gains an infrastructure section.
- **Host ports:** PostgreSQL `5432`, Keycloak `8080`, Vault `8200`.
- **Dependencies:** MediatR 12.5.0, Mapster 10.0.13, FluentValidation, Asp.Versioning, Swashbuckle, JwtBearer, VaultSharp, DotNetEnv; images `postgres:17`, `quay.io/keycloak/keycloak:26.4`, `hashicorp/vault:2.2.1`.
