# ms-listing

Listing service. For now it holds the catalog that listings will refer to: car brands, their models, and cities. Anyone can read the catalog; only admins can change it.

## Requirements

- .NET 10 SDK (pinned by `global.json` in the repository root)
- The local tools: `dotnet tool restore` in the repository root (installs `dotnet-ef`)
- The infrastructure running and Vault unsealed (see [`infrastructure/README.md`](../../infrastructure/README.md)):

  ```bash
  docker compose -f infrastructure/docker-compose.yml up -d
  docker compose -f infrastructure/docker-compose.yml run --rm vault-unseal
  ```

- On a new machine, the one-time setup from the infrastructure README:
  - `LISTING_DB_PASSWORD` / `LISTING_DB_PASSWORD_DEV` in `infrastructure/postgres/.env`.
  - `MS_LISTING_CLIENT_SECRET` / `MS_LISTING_CLIENT_SECRET_DEV` in `infrastructure/keycloak/.env`.
  - The `ms-listing` lines of the Vault setup.
- `services/ms-listing/.env` with the Vault AppRole credentials (copy `.env.example`):

  ```
  VAULT_ROLE_ID=...
  VAULT_SECRET_ID=...
  ```

At startup ms-listing signs in to Vault and reads its connection string (`ConnectionStrings:Default`) and its Keycloak client secret. Nothing about the database is in `appsettings*.json`. Each environment has its own Vault path: `ms-listingLocal`, `ms-listingDev`, `ms-listing`.

## Database and migrations

| Environment | Database / role | Realm | Vault path |
|---|---|---|---|
| Local | `listingDev` | `carsalesDev` | `ms-listingLocal` |
| Development | `listingDev` | `carsalesDev` | `ms-listingDev` |
| Production | `listing` | `carsales` | `ms-listing` |

Tables use `snake_case` names. Every entity has `id` (UUID v7), `created_at`, `updated_at`, `is_deleted`, and `deleted_at`. Deletes are soft: rows stay and are hidden from normal reads.

Names compare ignoring letter case (a nondeterministic collation). `name = 'bmw'` finds `BMW`, but PostgreSQL does not allow `LIKE` on these columns; use `name collate "C" ilike 'b%'` in hand-written SQL.

From `services/ms-listing/`:

```bash
# Apply migrations, then exit
dotnet run --project src/WebApi -- --migrate

# Create a migration after changing entities or configurations
dotnet ef migrations add <Name> --project src/WebApi --output-dir Persistence/Migrations
```

The app does not migrate on startup. In Docker a one-shot `ms-listing-migrate` container runs `--migrate` before the service starts.

## Build

From `services/ms-listing/`:

```bash
dotnet build Listing.slnx
```

## Run

From `services/ms-listing/`:

```bash
dotnet run --project src/WebApi
```

The app starts in the `Local` environment on `http://localhost:5002`. Swagger UI is at `http://localhost:5002/swagger`. Stop it with `Ctrl+C`.

## Endpoints

`Brand`, `Model`, and `City` each have the same actions under `api/v1/<Entity>`. Errors are returned as ProblemDetails.

| Action | Route | Who | Success | Errors |
|---|---|---|---|---|
| List | `GET` | anyone | `200` non-deleted entries, sorted by name | |
| By id | `GET {id}` | anyone | `200` | `404` missing or deleted |
| List with deleted | `GET WithDeleted` | admin | `200` all entries with `isDeleted`, `deletedAt` | `401`, `403` |
| By id with deleted | `GET WithDeleted/{id}` | admin | `200` | `401`, `403`, `404` |
| Create | `POST Create` | admin | `201` entry | `400`, `401`, `403`, `409` |
| Update | `PUT Update/{id}` | admin | `200` entry | `400`, `401`, `403`, `404`, `409` |
| Delete | `DELETE Delete/{id}` | admin | `204` (soft delete) | `401`, `403`, `404`, `409` (brand with models) |
| Restore | `PATCH Restore/{id}` | admin | `204` | `401`, `403`, `404`, `409` |

Bodies and responses:

| Entity | Create / Update body | Response |
|---|---|---|
| `Brand`, `City` | `name` | `id`, `name` |
| `Model` | `brandId`, `name` | `id`, `name`, `brandId`, `brandName` |

Rules:

- `name`: required, at most 100 characters; surrounding spaces are trimmed.
- Brand and city names are unique, and model names are unique within their brand. All three ignore letter case and count only non-deleted entries, so a deleted entry's name can be used again. A duplicate gives `409`.
- `Model` lists take an optional `?brandId=` filter.
- A model needs an existing, non-deleted brand; otherwise `400` on `brandId`.
- A brand with non-deleted models cannot be deleted (`409`). Delete its models first.
- Restore gives `409` when the entry is not deleted, when its name is now taken, or, for a model, when its brand is deleted.

### Admin token

Admin actions need an access token from `ms-auth` for a user with the `admin` realm role. The role is given in the Keycloak console; see "Admin role" in the infrastructure README. Sign in again after the role is given, because older tokens do not carry it. A signed-out token is rejected at once (`401`).

### Example against Local

```bash
AUTH=http://localhost:5001/api/v1/Auth
API=http://localhost:5002/api/v1

TOKEN=$(curl -s -X POST $AUTH/Login -H "X-Client-Type: mobile" -H "Content-Type: application/json" \
  -d '{"login":"<admin user>","password":"<password>"}' | python3 -c "import json,sys;print(json.load(sys.stdin)['accessToken'])")

curl -s -X POST $API/Brand/Create -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"name":"Volkswagen"}'
# copy the brand id
curl -s -X POST $API/Model/Create -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"brandId":"<brand id>","name":"Golf"}'

curl -s $API/Brand
curl -s "$API/Model?brandId=<brand id>"
curl -s $API/City
```

## Docker

Requires the `carsales_network` network and `services/ms-listing/.env` (passed to the containers with `env_file`). From `services/ms-listing/`:

```bash
# Development: database listingDev, realm carsalesDev, Vault path ms-listingDev
docker compose -f docker-compose.dev.yml up -d --build
docker compose -f docker-compose.dev.yml down

# Production: database listing, realm carsales, Vault path ms-listing
docker compose up -d --build
docker compose down
```

Each `up` first runs `ms-listing-migrate` (the same image with `--migrate`), which applies pending migrations and exits; `ms-listing` starts only after it exits with 0. If it fails, read its logs with `docker compose logs ms-listing-migrate`.

In Docker, ms-listing publishes no host port. Clients reach it through [`api-gateway`](../../api-gateway/README.md) on `http://localhost:6080` (Development) or `http://localhost:7080` (Production), on the same paths, so the examples above work with `API=http://localhost:6080/api/v1` or `API=http://localhost:7080/api/v1`. It can be scaled, while migrations still run once: `docker compose up -d --scale ms-listing=2` from the repository root.

**Temporary direct access in Development** (for example, to open Swagger): uncomment the `ports` block in `docker-compose.dev.yml` and run `up` again. ms-listing is then at `http://localhost:6002/swagger`. Comment the block back before committing; while it is active, `--scale` does not work.

Health check: `GET /health`.
