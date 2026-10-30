# ms-listing

Listing service: car listings and the catalog they refer to (brands, models, cities). Anyone can browse; signed-in users publish and manage their own listings; admins manage the catalog and can remove and restore any listing.

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

## Catalog endpoints

`Brand`, `Model`, and `City` each have the same actions under `api/v1/<Entity>`. Errors are returned as ProblemDetails.

| Action | Route | Who | Success | Errors |
|---|---|---|---|---|
| List | `GET` | anyone | `200` non-deleted entries, sorted by name | |
| By id | `GET {id}` | anyone | `200` | `404` missing or deleted |
| List with deleted | `GET WithDeleted` | admin | `200` all entries with `isDeleted`, `deletedAt` | `401`, `403` |
| By id with deleted | `GET WithDeleted/{id}` | admin | `200` | `401`, `403`, `404` |
| Create | `POST Create` | admin | `201` entry | `400`, `401`, `403`, `409` |
| Update | `PUT Update/{id}` | admin | `200` entry | `400`, `401`, `403`, `404`, `409` |
| Delete | `DELETE Delete/{id}` | admin | `204` (soft delete) | `401`, `403`, `404`, `409` (brand with models; model or city with listings) |
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
- A model or city used by non-deleted listings cannot be deleted (`409`). Delete or move those listings first.
- Restore gives `409` when the entry is not deleted, when its name is now taken, or, for a model, when its brand is deleted.

## Listing endpoints

All under `api/v1/Listing`. A listing belongs to the user who created it (the token's subject).

| Action | Route | Who | Success | Errors |
|---|---|---|---|---|
| List | `GET ?page=&pageSize=` | anyone | `200` page of non-deleted listings, newest first | `400` paging |
| By id | `GET {id}` | anyone | `200` | `404` missing or deleted |
| Own list | `GET Mine?page=&pageSize=` | signed in | `200` page of the caller's listings | `400`, `401` |
| List with deleted | `GET WithDeleted?page=&pageSize=` | admin | `200` page with `isDeleted`, `deletedAt` | `400`, `401`, `403` |
| By id with deleted | `GET WithDeleted/{id}` | admin | `200` | `401`, `403`, `404` |
| Create | `POST Create` | signed in | `201` listing | `400`, `401` |
| Update | `PUT Update/{id}` | owner | `200` listing | `400`, `401`, `403`, `404` |
| Delete | `DELETE Delete/{id}` | owner or admin | `204` (soft delete) | `401`, `403`, `404` |
| Restore | `PATCH Restore/{id}` | admin | `204` | `401`, `403`, `404`, `409` |

- **Ownership:** the owner comes from the access token; an `ownerId` in the body is ignored. Only the owner can update a listing (an admin gets `403`); the owner or an admin can delete it; only an admin can restore it. A missing or deleted listing gives `404` before any ownership check.
- **Paging:** `page` 1–500 (default 1), `pageSize` 1–100 (default 20). The response is `{ items, page, pageSize, totalCount }`, newest first.
- **Restore** gives `409` when the listing is not deleted or when its model or city is deleted.

Body (create and update):

| Field | Rule |
|---|---|
| `modelId`, `cityId` | an existing, non-deleted model and city (otherwise `400` on that field) |
| `year` | 1900 to next year |
| `mileage`, `mileageUnit` | 0–2,000,000; `Km` or `Mi` |
| `engineVolume` | litres with one decimal, 0.1–10.0; must be empty when `fuelType` is `Electric` |
| `horsepower` | 1–2,000 |
| `seats` | 1–60 |
| `vin` | optional; 17 letters and digits without `I`, `O`, `Q`; stored upper-case |
| `price`, `currency` | 1–100,000,000; `AZN`, `USD`, or `EUR` |
| `description` | optional, at most 3,000 characters; trimmed |
| `isCredit`, `isBarter`, `isPainted`, `isDamaged` | `true` / `false` |
| `bodyType` | `Sedan`, `Hatchback`, `Liftback`, `Wagon`, `Suv`, `Crossover`, `Coupe`, `Convertible`, `Minivan`, `Pickup`, `Van` |
| `fuelType` | `Petrol`, `Diesel`, `Gas`, `Hybrid`, `PluginHybrid`, `Electric` |
| `transmission` | `Manual`, `Automatic`, `Robotic`, `Cvt` |
| `drivetrain` | `Front`, `Rear`, `AllWheel` |
| `color` | `White`, `Black`, `Silver`, `Gray`, `Red`, `Blue`, `DarkBlue`, `Green`, `Yellow`, `Orange`, `Brown`, `Beige`, `Gold`, `Purple`, `Other` |
| `market` | `OfficialDealer`, `Europe`, `Usa`, `Korea`, `Japan`, `Gcc`, `Russia`, `China`, `Other` |

Enums are sent and returned as their names; numbers and unknown names give `400`. Each broken rule gives `400` naming its field.

The response has every field plus `id`, `ownerId`, `brandId`, `brandName`, `modelName`, `cityName`, `createdAt`, and `updatedAt`.

## Tokens

Admin actions need an access token from `ms-auth` for a user with the `admin` realm role. The role is given in the Keycloak console; see "Admin role" in the infrastructure README. Sign in again after the role is given, because older tokens do not carry it. A signed-out token is rejected at once (`401`).

## Example against Local

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

# Listings: any signed-in user (USER_TOKEN obtained like TOKEN above)
curl -s -X POST $API/Listing/Create -H "Authorization: Bearer $USER_TOKEN" -H "Content-Type: application/json" -d '{
  "modelId":"<model id>","cityId":"<city id>","year":2018,"mileage":85000,"mileageUnit":"Km","engineVolume":2.0,
  "horsepower":184,"seats":5,"bodyType":"Sedan","fuelType":"Petrol","transmission":"Automatic","drivetrain":"Rear",
  "color":"Black","market":"OfficialDealer","price":32500,"currency":"USD","description":"Well kept.",
  "isCredit":true,"isBarter":false,"isPainted":false,"isDamaged":false}'

curl -s "$API/Listing?page=1&pageSize=20"
curl -s "$API/Listing/Mine" -H "Authorization: Bearer $USER_TOKEN"
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
