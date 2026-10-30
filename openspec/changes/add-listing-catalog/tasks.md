# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Infrastructure

- [x] 1.1 Add the `listing` and `"listingDev"` roles and databases to `postgres/setup/databases.sql` (idempotent, owned by their role) and revoke `CONNECT` from `PUBLIC` on `listing`, `"listingDev"`, and `keycloak`, `LISTING_DB_PASSWORD` / `LISTING_DB_PASSWORD_DEV` to `postgres/.env.example`, and pass them to `postgres-setup` with the empty-value guard; generate both passwords into the local `postgres/.env` without printing them; verify after `up -d` that `postgres-setup` exits 0, both databases exist with their owners, each role (including `keycloak`) can connect only to its own database, Keycloak still works, and a second `up` changes nothing
- [x] 1.2 Extend `keycloak/setup/realms.sh` to ensure, in both realms, the `admin` realm role, the confidential `ms-listing` client (no flows, secret from `MS_LISTING_CLIENT_SECRET` / `MS_LISTING_CLIENT_SECRET_DEV`, updated if it exists), and the `ms-listing-audience` mapper on `ms-auth`; add the two variables to `keycloak/.env.example` and generate them into the local `keycloak/.env` without printing them; verify `keycloak-setup` exits 0 twice in a row, a login token through `ms-auth` carries `aud` with `ms-listing`, and introspecting it with the `ms-listing` credentials returns `active: true`
- [x] 1.3 Add `vault/config/policies/ms-listing.hcl` (read `carsales/data/ms-listing`, `ms-listingDev`, and `ms-listingLocal`) and the setup steps for `ms-listing` to `infrastructure/README.md` (policy, AppRole, `kv put` with `ConnectionStrings:Default` and `Keycloak:ClientSecret` per environment, connection strings shown only as placeholders, the `.env` values, giving a user the `admin` role in the console and with kcadm); after the user signs in once with `vault login -method=userpass username=<admin user>` inside the Vault container, run the steps with that token, write the role id and secret id into `services/ms-listing/.env` without printing them, then revoke the token (`vault token revoke -self`); verify the AppRole can read its three secrets and nothing else
- [x] 1.4 Give the developer's own `carsalesDev` user the `admin` role with kcadm (bootstrap admin from `keycloak/.env`); verify a fresh login token for that user carries `admin` in `realm_access.roles`
- [x] 1.5 Record the data access choice in `openspec/config.yaml` (EF Core with Npgsql, `snake_case` names, migrations run by a one-shot `<service>-migrate` container with `--migrate`); verify `openspec validate add-listing-catalog` passes

## 2. ms-listing skeleton (Local)

- [x] 2.1 Create `services/ms-listing/` (`src/WebApi/WebApi.csproj` with the packages from design §3 and the `ms-auth` libraries, `Listing.slnx`, `.env.example`, `README.md` stub), add it to `CarSales.slnx`, add `.config/dotnet-tools.json` with `dotnet-ef`, and add `build ms-listing` / `ms-listing (Local)` to `.vscode` and the "All services" compound; verify `dotnet build CarSales.slnx` and `dotnet tool restore` succeed
- [x] 2.2 Add `Extensions/V1` (`VaultExtensions.InitVaultAsync`, Library, ApiVersioning, Swagger, HealthCheck), `GlobalExceptionHandler` with `NotFoundException`, `appsettings*.json` (Keycloak and Vault only; no connection string), and `launchSettings.json` on `5002`; verify the app starts in Local, reads its Vault secret, and `/health` gives 200
- [x] 2.3 Add `BaseEntity`, `ApplicationDbContext` (snake_case, `case_insensitive` collation, soft-delete query filter, timestamps in `SaveChangesAsync`), `ApplicationDbContextFactory`, `DatabaseExtensions` (connection string from Vault `ConnectionStrings:Default`), and the `--migrate` mode in `Program.cs`; create the initial migration; verify `dotnet run -- --migrate` applies it to `listingDev` and exits 0, a second run applies nothing, and the app still starts normally without the flag
- [x] 2.4 Add `AuthenticationExtensions` (JwtBearer, audience `ms-listing`, introspection with the `ms-listing` client, `realm_access` roles as `role` claims) and the `Roles` constants; verify with a temporary admin-only probe endpoint that no token gives 401, a token without `admin` gives 403, a token with `admin` gives 200, and a signed-out token gives 401; remove the probe afterwards

## 3. Brand

- [x] 3.1 Add the `Brand` entity, `BrandConfigurations` (collation on `name`, partial unique index), and its migration; verify `--migrate` creates `brands` with the index `WHERE is_deleted = false` and the collation
- [x] 3.2 Add the brand commands, queries, validators, handlers, mappings, DTOs, `IBrandService`/`BrandService`, `IBrandRepository`/`BrandEfCoreRepository`, and `BrandController` with every action in design §2; verify the build and that Swagger shows the eight actions with their response types
- [x] 3.3 Verify the brand scenarios of `specs/listing-catalog` in Local: public reads and 404s, 401/403 on admin actions, name rules (empty, too long, trimmed, `bmw` vs `BMW` → 409, reuse of a deleted name → 201), update, delete (204, hidden afterwards), restore (204, name taken → 409, not deleted → 409, unknown → 404), and the `WithDeleted` reads

## 4. Model

- [x] 4.1 Add the `Model` entity (`brand_id` FK, restrict), `ModelConfigurations` (unique `(brand_id, name)` where not deleted), its migration, and the full model slice with `?brandId=` on both lists and `brandName` in responses; verify the build and the Swagger actions
- [x] 4.2 Add the brand rule that a brand with non-deleted models cannot be deleted; verify the model scenarios in Local: create under an active brand (201), missing or deleted brand (400 `brandId`), same name under another brand (201), duplicate under the same brand (409), update including a brand change, `?brandId=` filtering, delete and restore (brand deleted → 409), and deleting a brand with active models (409) then without (204)

## 5. City

- [x] 5.1 Add the `City` entity, `CityConfigurations`, its migration, and the full city slice; verify the city scenarios in Local (the same set as brand)
- [x] 5.2 Write `services/ms-listing/README.md` (purpose, requirements including the `.env` values and the Vault and Keycloak steps, migrations in Local, run, endpoints table with auth per action, how to get an admin token, curl examples); verify the documented Local commands work as written

## 6. Docker and gateway

- [ ] 6.1 Add `services/ms-listing/Dockerfile` and both compose files (`ms-listing-migrate` with `--migrate`, `ms-listing` depending on its successful completion, no host port, a commented `6002` block in Development), and include them in the root compose files; verify `docker compose config` for the root and service files
- [ ] 6.2 Add the `brand`, `model`, and `city` routes and the `ms-listing` cluster to the gateway settings (Local `http://localhost:5002`), and the routes to `api-gateway/README.md`; verify in Local through `5080` that a public list, a `?brandId=` list, and an admin create reach `ms-listing` unchanged
- [ ] 6.3 Verify Development and then Production through the gateway (`6080` / `7080`): the migrate container exits 0 before `ms-listing` starts and is a no-op on the second `up`; public reads, an admin create, update, delete, and restore work against `listingDev` / `listing` with `carsalesDev` / `carsales` admin tokens; a Development admin token is rejected in Production; `--scale ms-listing=2` serves requests from both replicas while the migration ran once; remove the test data and users afterwards
- [ ] 6.4 Update the root `README.md` (services table, ports, the migrate step) and the `ms-listing` README Docker section; run `graphify update .`; verify the documented commands work and graphify finishes without errors

## Workflow follow-up

- Commit after group 1 (infrastructure), after group 2 (skeleton), after groups 3–5 (catalog), and after group 6 (Docker and gateway), each with the message shown for approval.
- Archive the change with `/opsx:archive` after review.
