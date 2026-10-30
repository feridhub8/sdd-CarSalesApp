# Proposal

## Why

A car listing must name its make, model, and city. If these are free text, the same make ends up spelled many ways ("Mercedes", "mercedes-benz", "Mersedes") and filtering breaks. The catalog of brands, models, and cities has to exist before listings can reference it. This change also brings the project's first database, migrations, and admin role. Proving them on a simple domain keeps the listings change focused on listings.

## What Changes

- **New service `ms-listing`** (Local `5002`; no host port in Docker, with a commented `6002` block in the Development compose file like `ms-auth`).
  - It has its own PostgreSQL databases: `listing` (Production) and `listingDev` (Development), each with its own role.
  - It uses EF Core.
- **Catalog (`Brand`, `Model`, `City`):**
  - Anyone can list them and read one by id, without signing in. Deleted entries are hidden.
  - Lists are sorted by name and not paged. A model list can be filtered by brand.
  - Admins can create, update, delete (soft), and restore entries, and can list and read entries including deleted ones.
  - **Rules:**
    - Brand and city names are unique, ignoring letter case.
    - Model names are unique within their brand.
    - A brand with active models cannot be deleted.
    - A model needs an active brand.
- **Admin role:** an `admin` realm role in both realms. Requests to admin endpoints without a token get `401`, and requests without the role get `403`. A user is given the role in the Keycloak console.
- **Token checks:** like `ms-auth`, `ms-listing` validates the JWT and introspects it, so a signed-out token stops working at once.
  - For this, `ms-listing` gets its own Keycloak client.
  - Tokens issued at login also carry `ms-listing` as an audience.
- **Infrastructure:**
  - `postgres-setup` creates the two roles and databases.
  - `keycloak-setup` makes sure the `admin` role, the `ms-listing` client, and the audience exist in both realms.
  - Vault gets an `ms-listing` policy, AppRole, and secrets (database password, client secret).
- **Migrations:** the same `ms-listing` image runs with `--migrate`, applies migrations, and exits.
  - In Docker, a one-shot `ms-listing-migrate` service runs it, and `ms-listing` starts only after it succeeds.
  - In Local, `dotnet run -- --migrate` does the same.
- **Gateway:** `/api/v{version}/Brand/**`, `/Model/**`, and `/City/**` are forwarded to `ms-listing`.
- **`openspec/config.yaml`:** records the data access choice (EF Core with Npgsql, `snake_case` names, migrations in a one-shot container).

## Capabilities

### New Capabilities
- `listing-catalog`: reading and managing the brands, models, and cities that listings refer to. It covers public reads, admin-only writes, soft delete and restore, viewing deleted entries, and the uniqueness and dependency rules.

### Modified Capabilities
- `api-gateway`: forwards catalog requests to `ms-listing`, as it does auth requests to `ms-auth`.

## Impact

- **New:** `services/ms-listing/` with EF Core 10, Npgsql, `EFCore.NamingConventions`, and the same libraries as `ms-auth` (MediatR 12.5.0, Mapster, FluentValidation, Asp.Versioning, Swashbuckle, JwtBearer, VaultSharp, DotNetEnv). A local `dotnet-ef` tool manifest is added in the repository root.
- **Infrastructure:**
  - `postgres/setup/databases.sql`, `postgres/.env.example`, and the `postgres-setup` command.
  - `keycloak/setup/realms.sh` and `keycloak/.env.example`, with the `ms-listing` client secrets.
  - A new `vault/config/policies/ms-listing.hcl`.
  - `infrastructure/README.md`, with the setup steps for a new service.
- **Repository root:** `CarSales.slnx`, both root compose files, `.vscode`, `README.md`, and `openspec/config.yaml`.
- **api-gateway:** three routes and one cluster in `appsettings.json`, and the Local address in `appsettings.Local.json`. The README route table is updated.
- **One manual step for the user:** sign in to Vault once with their userpass admin account inside the Vault container.
  - The assistant generates the new secrets into the local `.env` files without printing them.
  - It runs the Vault commands with that sign-in, writes the AppRole ids into `services/ms-listing/.env`, and revokes the token afterwards.
  - It gives the developer's own `carsalesDev` user the `admin` role.
- **Later work:** listings (`add-listings`), listing features, images, generations/modifications, moderation, VIP, search.
