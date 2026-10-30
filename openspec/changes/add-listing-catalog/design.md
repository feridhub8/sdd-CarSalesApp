# Design

## Context

- **No database yet.** `ms-auth` has no database. PostgreSQL holds only Keycloak's database, which `postgres-setup` creates from `databases.sql` with a password read from `keycloak/.env`.
- **`ms-auth` sets the patterns to copy:**
  - Vault AppRole sign-in (`VaultExtensions`).
  - JwtBearer with introspection in `OnTokenValidated`.
  - Exception middleware with ProblemDetails (named `GlobalExceptionHandler` in `ms-listing`).
  - `Init<Area>()` extensions.
  - Services never share code, so `ms-listing` copies what it needs.
- **Keycloak:**
  - Realm files are imported with `IGNORE_EXISTING`, so a change to a realm file never reaches realms that already exist.
  - `keycloak-setup` (`realms.sh`, kcadm) runs on every start and is the place that changes running realms.
  - Tokens from login are issued to the `ms-auth` client. An audience mapper adds `aud: ms-auth`.
  - No realm roles exist yet.
- **Vault:** the root token is revoked. Configuration is done by a person signed in with a userpass admin account.
- **Gateway:** picks the service by the controller segment of the path, one route per controller (`add-api-gateway`).
- Requirements are in `specs/listing-catalog/spec.md` and `specs/api-gateway/spec.md`. Motivation is in proposal.md.

## Goals / Non-Goals

**Goals:**
- A reusable data access setup: base entity, soft delete, timestamps, naming, migrations. `add-listings` builds on it unchanged.
- Admin authorization that works the same way for every later service.
- The catalog rules in the spec, enforced by the database where possible, not only by code.

**Non-Goals:**
- Paging, search, and seed data.
- Generations, modifications, and features.
- A UI for admins. They use Swagger in Local or any HTTP client through the gateway.
- Removing deleted rows for good.

## Decisions

### 1. Service layout: one feature per entity

```
services/ms-listing/src/WebApi/
├── Data/
│   ├── Brand/                (Model/ and City/ have the same shape)
│   │   ├── Commands/V1/      CreateBrandCommand, UpdateBrandCommand, DeleteBrandCommand, RestoreBrandCommand
│   │   ├── Controllers/V1/   BrandController
│   │   ├── Dtos/V1/          CreateBrandDto, UpdateBrandDto, GetBrandOkResponseDto, GetBrandWithDeletedOkResponseDto
│   │   ├── Entities/V1/      Brand
│   │   ├── Handlers/V1/  Mappings/V1/  Queries/V1/  Validators/V1/
│   │   ├── Repositories/V1/  Base/IBrandRepository, BrandEfCoreRepository
│   │   └── Services/V1/      Base/IBrandService, BrandService
│   ├── Model/
│   ├── City/
│   └── Shared/
│       ├── Entities/V1/Base/BaseEntity.cs
│       ├── Models/V1/        VaultOptions, KeycloakOptions, KeycloakIntrospectionResponse, Roles
│       ├── Repositories/V1/  Base/ITokenRepository, TokenKeycloakRepository
│       └── Validators/V1/    NameRuleExtensions
├── Exceptions/V1/            ConflictException, NotFoundException, UnauthorizedException
├── Extensions/V1/            Vault, Library, Database, Repository, Service, ApiVersioning, Authentication, Swagger, HealthCheck
├── Middlewares/V1/           GlobalExceptionHandler
├── Persistence/
│   ├── Configurations/       BrandConfigurations, ModelConfigurations, CityConfigurations
│   ├── Migrations/
│   ├── ApplicationDbContext.cs
│   └── ApplicationDbContextFactory.cs   (design time)
└── Program.cs
```

- **One feature per entity, singular, as the global rules say.** Each feature keeps only its own files.
- **Name collision:** the feature namespace `WebApi.Data.Brand` and the entity class `Brand` share a name.
  - Inside any `WebApi.Data.*` namespace, a bare `Brand` resolves to the namespace (CS0118), so the entity is written qualified there.
  - Inside its own feature, that is `Entities.V1.Brand`.
  - From another feature, it is `Brand.Entities.V1.Brand`, for example a model handler reading its brand.
  - Outside `Data/`, as in `Persistence/` (DbContext, configurations), the bare name works with a `using`.
  - The same applies to `Model` and `City`.
  - This was the user's choice over plural feature folders or an `Entity` suffix on class names, because it keeps both the folder rule and the class names unchanged.
- **Registration:** `RepositoryExtensions.InitRepositories()` registers every repository and `ServiceExtensions.InitServices()` every service. Both are called from `Program.cs`, so adding an entity means adding a line to each.
- **File naming follows the global table.** `WithDeleted` goes before the kind word: `GetAllBrandsWithDeletedQuery`, `GetBrandByIdWithDeletedQuery`, `GetBrandWithDeletedOkResponseDto`. The global rule says "suffix" without fixing the position. This position keeps the names readable and is used consistently.

### 2. Endpoints

| Action | Route (`api/v1/<Entity>`) | Auth | Success |
|---|---|---|---|
| list | `GET` (`Model`: `?brandId=`) | none | 200 list |
| by id | `GET {id:guid}` | none | 200 / 404 |
| list with deleted | `GET WithDeleted` (`Model`: `?brandId=`) | admin | 200 list |
| by id with deleted | `GET WithDeleted/{id:guid}` | admin | 200 / 404 |
| create | `POST Create` | admin | 201 entry |
| update | `PUT Update/{id:guid}` | admin | 200 entry |
| delete | `DELETE Delete/{id:guid}` | admin | 204 |
| restore | `PATCH Restore/{id:guid}` | admin | 204 |

- The `:guid` constraint keeps `WithDeleted` from being read as an id.
- Body DTOs:
  - `Create`/`Update` for brand and city: `{ name }`.
  - `Create`/`Update` for model: `{ brandId, name }`.
- Response DTOs:
  - Normal: `{ id, name }`, and for models `{ id, name, brandId, brandName }`.
  - `WithDeleted` adds `isDeleted` and `deletedAt`.
- Actions follow the global controller rules. Actions with a body return `ActionResult<T>`, actions without one return `IActionResult`, and every action declares `[ProducesResponseType]` and takes `ct`.

### 3. Data model and EF Core

```
brands                      models                              cities
+--------------------+      +--------------------------+       +--------------------+
| id uuid PK         |1    *| id uuid PK               |       | id uuid PK         |
| name text (ci)     |------| brand_id uuid FK -> brands       | name text (ci)     |
| created_at  timestamptz   | name text (ci)           |       | created_at ...     |
| updated_at  timestamptz?  | created_at / updated_at  |       | updated_at ...     |
| is_deleted  bool          | is_deleted / deleted_at  |       | is_deleted ...     |
| deleted_at  timestamptz?  +--------------------------+       | deleted_at ...     |
+--------------------+                                         +--------------------+
unique (name) where not is_deleted     unique (brand_id, name) where not is_deleted     unique (name) where not is_deleted
```

- **`BaseEntity`** has `Id`, `CreatedAt`, `UpdatedAt?`, `IsDeleted`, and `DeletedAt?`.
  - `Id` is `Guid.CreateVersion7()`, set in code. It is time-ordered, so inserts stay index-friendly, and ids do not reveal counts.
  - Timestamps are UTC `timestamptz`.
  - `ApplicationDbContext.SaveChangesAsync` sets `CreatedAt` on insert, `UpdatedAt` on update, and `DeletedAt` when `IsDeleted` turns true. It clears `DeletedAt` on restore.
- **Soft delete:**
  - A global query filter `!IsDeleted` on every entity makes normal reads skip deleted rows without each query remembering to.
  - `WithDeleted` reads and restore use `IgnoreQueryFilters()`.
  - Deletes set `IsDeleted`, never `Remove()`.
- **Case-insensitive uniqueness in the database:**
  - Names use a nondeterministic ICU collation, `case_insensitive` (`und-u-ks-level2`), declared with `HasCollation` and `UseCollation`.
  - Partial unique indexes use `HasFilter("is_deleted = false")`.
  - `bmw` and `BMW` then collide in the index itself. A deleted entry's name is free to reuse, and the database stays correct even if two admins create the same name at once.
  - Handlers check first to give a clean 409. A unique violation (`23505`) from a race is also mapped to 409 by the repository.
  - The `postgres:17` image is built with ICU.
- **Naming:**
  - `EFCore.NamingConventions` with `UseSnakeCaseNamingConvention()` gives tables `brands` and columns `brand_id`, `created_at`.
  - The alternative was quoted PascalCase identifiers (`"BrandId"`), which every hand-written SQL query would have to quote.
- **Foreign key:** `models.brand_id → brands.id` with `DeleteBehavior.Restrict`. Rows are never physically deleted, and a brand delete is guarded by the active-models rule (409).
- **Packages:**
  - `Microsoft.EntityFrameworkCore` 10.0.x, `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.x, and `EFCore.NamingConventions` 10.0.x, at the latest patch when applied.
  - `Microsoft.EntityFrameworkCore.Design` with `PrivateAssets=all`, for the tooling.
  - `dotnet-ef` as a local tool in `.config/dotnet-tools.json`, so every machine uses the same version (`dotnet tool restore`).
- **Design-time factory:** `ApplicationDbContextFactory` (`IDesignTimeDbContextFactory`) uses a placeholder connection string. `dotnet ef migrations add` then works without Vault or a running database. Creating a migration never connects.

### 4. Connection string and secrets

- **The whole connection string lives in Vault**, as `ConnectionStrings:Default`, together with `Keycloak:ClientSecret`. `appsettings*.json` has no connection string at all. This is a user decision: no part of how to reach a database is kept in the repository.
- **One Vault path per environment:** `carsales/ms-listing` (Production), `carsales/ms-listingDev` (Development), and `carsales/ms-listingLocal` (Local).
  - Local and Development use the same database but reach it by different hosts: the host machine versus the Docker network. So they cannot share one connection string.
  - `appsettings.Local.json` points `Vault:SecretPath` to `ms-listingLocal`.
- `DatabaseExtensions` reads `ConnectionStrings:Default` from configuration after `InitVaultAsync` has loaded it, and fails at startup with a clear message when it is missing.
- **The password exists twice:**
  - In `infrastructure/postgres/.env` (`LISTING_DB_PASSWORD`, `LISTING_DB_PASSWORD_DEV`), because `postgres-setup` creates and syncs the roles from it.
  - In Vault, for the service.
  - This is the same arrangement as `KC_DB_PASSWORD` and the `ms-auth` client secret.
- **`postgres-setup`:**
  - Its command passes `-v listing_password=... -v listing_dev_password=...`, with the same empty-value guard as `KC_DB_PASSWORD`.
  - `databases.sql` creates the roles `listing` and `"listingDev"` and the databases `listing` and `"listingDev"`, each owned by its role, idempotently with `\gexec`.
  - It revokes `CONNECT` from `PUBLIC` on both listing databases and on `keycloak`, so each role reaches only its own database.
  - In particular, a leaked `listingDev` password, which is used on developer machines, cannot connect to `listing` or `keycloak`.

### 5. Migrations: `--migrate` mode of the same image

```
docker compose up
  postgres --healthy--> postgres-setup (roles, databases) --exit 0-->
  ms-listing-migrate:  dotnet WebApi.dll --migrate  --exit 0-->  ms-listing (xN replicas)
```

- **`Program.cs`:**
  - It loads secrets and registers services as usual.
  - If `args` contains `--migrate`, it builds the app, runs `Database.MigrateAsync(ct)` in a scope, logs the applied migrations, and returns without starting Kestrel.
  - A failure exits non-zero.
- **Compose (`ms-listing` service files):**
  - `ms-listing-migrate` has the same `build`, so the second build is a cache hit, plus `env_file`, environment, `command: ["--migrate"]`, and `restart: "no"`.
  - `ms-listing` has `depends_on: { ms-listing-migrate: { condition: service_completed_successfully } }`.
  - The migrate service has no `depends_on: postgres-setup`, because a service compose file must also run on its own. With the root compose, `postgres-setup` normally finishes long before.
  - If the database is not ready, the migrate container fails and is retried on the next `up`.
- **Why this over the alternatives:**
  - With `Database.Migrate()` at app startup, several replicas race on every start.
  - A separate EF bundle would need its own way to get the password from Vault.
  - This mode reuses `InitVaultAsync` as is.
- **Local:**
  - `dotnet run --project src/WebApi -- --migrate` applies migrations, and then `dotnet run --project src/WebApi` runs the app.
  - New migrations are created with `dotnet ef migrations add <Name> --project src/WebApi --output-dir Persistence/Migrations`.

### 6. Authentication and the admin role

- **Keycloak, ensured by `realms.sh` for both realms** on every start, and created only when missing:
  - **Realm role `admin`:** `kcadm get roles/admin` succeeds, or `kcadm create roles -s name=admin`.
  - **Client `ms-listing`:**
    - Confidential. Standard flow, direct access grants, implicit flow, and service accounts are all disabled. It exists only to introspect tokens.
    - Its secret comes from `MS_LISTING_CLIENT_SECRET` / `MS_LISTING_CLIENT_SECRET_DEV` in `keycloak/.env`.
    - If it exists, its secret is updated to the `.env` value, so `.env` stays the source.
  - **An audience mapper `ms-listing-audience` on the `ms-auth` client** (`included.client.audience: ms-listing`), so access tokens from login carry `aud: [ms-auth, ms-listing]`.
  - Realm files are not changed. `realms.sh` handles both fresh and existing realms, so there is a single source for these settings.
- **ms-listing JwtBearer** is the same as `ms-auth`:
  - Authority is the realm, Audience is `ms-listing`, and `MapInboundClaims = false`.
  - `OnTokenValidated` introspects with the `ms-listing` client credentials. `active: false` fails authentication.
  - `ITokenRepository` (`TokenKeycloakRepository`) uses a named `HttpClient`.
- **Roles:**
  - Keycloak puts realm roles in the `realm_access` JSON claim (`{"roles":["admin",...]}`).
  - After a successful introspection, `OnTokenValidated` adds one `role` claim per realm role to the identity, and the identity's role claim type is `role`.
  - Controllers use `[Authorize(Roles = Roles.Admin)]`, with constants in `Data/Shared/Models/V1/Roles`.
  - Unauthenticated requests get 401 and authenticated requests without the role get 403 (ASP.NET defaults).
- **Who is admin:**
  - Assigned by hand in the Keycloak console: realm → Users → user → Role mapping → `admin`.
  - The infrastructure README shows the kcadm equivalent.
  - The user signs in again to get a token with the role.

### 7. Errors

- The middleware maps exceptions to responses:
  - `ValidationException` → 400.
  - `NotFoundException` → 404 (new in this service).
  - `ConflictException` → 409.
  - `UnauthorizedException` → 401.
  - Anything else → 500.
- **Rule checks that need the database live in handlers or services, not in validators.** Validators stay synchronous and DB-free:
  - Model brand existence throws a `ValidationException` naming `brandId`.
  - Name taken throws `ConflictException`.
  - Brand has active models throws `ConflictException`.
  - Restore conflicts throw `ConflictException`.

### 8. Gateway

- `appsettings.json` gets three routes:
  - `brand` (`/api/v{version}/Brand/{**rest}`).
  - `model`.
  - `city`.
- All three go to a new cluster `ms-listing` (`RoundRobin`, `http://ms-listing:8080`). `appsettings.Local.json` points it to `http://localhost:5002`.
- The catch-all also matches the bare path (`/api/v1/Brand`), so list requests are covered.
- No code change. The DNS resolver and connect timeout apply to the new cluster as configured.

## Risks / Trade-offs

- [Nondeterministic collations do not support `LIKE` in PostgreSQL 17] → Not needed now, because catalog lists are not searched. Search on names, when it comes, uses `ILIKE` on a deterministic expression or a separate search column.
- [Two places hold each database password (`postgres/.env` and Vault)] → This is the existing pattern. The infrastructure README lists both places in the setup and rotation steps.
- [`keycloak-setup` now also writes clients and mappers] → Every step checks first and only creates or updates its own objects. A failure exits non-zero and is visible in `docker compose ps`.
- [The audience on every login token grows with each service] → Acceptable for a handful of services. A client scope per service can replace the mappers if it grows.
- [A migration that fails halfway leaves `ms-listing` not started] → Intended: the app never runs against a schema it does not expect. The migrate container's logs show the error.
- [Roles in a token are fixed until the token expires] → A user who loses `admin` keeps it for up to the access token lifetime (5 minutes by default). Acceptable for an admin role.

## Migration Plan

1. **Infrastructure:**
   - Database roles and databases, Keycloak role, client, and mapper.
   - Vault policy, AppRole, and secrets.
   - `config.yaml`.
   - The user adds the new `.env` values and runs the Vault commands.
2. **`ms-listing` skeleton** with the base entity, `DbContext`, `--migrate`, and auth, verified in Local.
3. **`Brand`, then `Model`, then `City`**, each with its migration and verification in Local.
4. **Docker:** `ms-listing` and `ms-listing-migrate` compose files, root includes, gateway routes, and docs. Verified in Development and Production through the gateway.

**Rollback:**
- Revert the commits and remove the `ms-listing` containers.
- The databases, roles, Keycloak objects, and Vault entries can stay, or be dropped by hand. Nothing else depends on them.
