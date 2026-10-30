# Design

## Context

- `ms-listing` has the catalog features `Brand`, `Model`, and `City` (one feature folder each).
- **Shared code it already has:**
  - `BaseEntity`: v7 id, timestamps, soft delete.
  - `ApplicationDbContext`: `snake_case`, a soft delete query filter, and timestamps in `SaveChangesAsync`.
  - JWT and introspection, with `realm_access` roles as `role` claims and `Roles.Admin`.
  - `GlobalExceptionHandler`: 400, 401, 404, 409, 500.
  - `RepositoryExtensions` and `ServiceExtensions`.
  - The connection string from Vault.
- **Not there yet:** a current-user abstraction, a 403 mapping, paging, and enums in JSON.
- **Name collision:** inside `Data/*`, a bare `Listing` resolves to the namespace `WebApi.Data.Listing`. It is written `Entities.V1.Listing` inside its feature, as with `Brand`.
- Requirements are in `specs/listings`, `specs/listing-catalog`, and `specs/api-gateway`. Motivation is in proposal.md.

## Goals / Non-Goals

**Goals:**
- An ownership model that later user-owned resources reuse unchanged. Favourites and messages are examples.
- Paging that the search change can replace without breaking the response shape.

**Non-Goals:**
- Search, filters, and sorting options.
- Images and features.
- Moderation states.
- The seller's phone.
- Rate limiting against spam.

## Decisions

### 1. Endpoints

| Action | Route (`api/v1/Listing`) | Who | Success |
|---|---|---|---|
| List | `GET ?page=&pageSize=` | anyone | 200 page |
| By id | `GET {id:guid}` | anyone | 200 / 404 |
| Own list | `GET Mine?page=&pageSize=` | signed in | 200 page |
| List with deleted | `GET WithDeleted?page=&pageSize=` | admin | 200 page |
| By id with deleted | `GET WithDeleted/{id:guid}` | admin | 200 / 404 |
| Create | `POST Create` | signed in | 201 listing |
| Update | `PUT Update/{id:guid}` | owner | 200 listing / 403 / 404 |
| Delete | `DELETE Delete/{id:guid}` | owner or admin | 204 / 403 / 404 |
| Restore | `PATCH Restore/{id:guid}` | admin | 204 / 404 / 409 |

- `Mine` is the one action beyond the global list. It is the owner's view that the ownership rule implies.
- **Authorization is split across two layers:**
  - Attributes handle who may call at all: `[Authorize]`, or `[Authorize(Roles = Roles.Admin)]` for `WithDeleted` and Restore.
  - The service handles whose listing it is, because only the service knows the owner.

### 2. Current user and 403

- `Data/Shared/Services/V1/Base/ICurrentUserService` and `CurrentUserService` (scoped, registered in `ServiceExtensions`) read `HttpContext.User` through `IHttpContextAccessor`.
  - `Id` is `sub` parsed as a `Guid`. A missing or invalid value throws `UnauthorizedException`.
  - `IsAdmin` is `IsInRole(Roles.Admin)`.
- Services get the user from this service instead of commands carrying a user id. Commands stay "wrap the request DTO", as the global rule says, and handlers cannot forget to pass the user.
- A new `ForbiddenException` maps to 403 in `GlobalExceptionHandler`.
- **Ownership checks in `ListingService`:**
  - Update requires `listing.OwnerId == user.Id`. An admin is not exempt from this.
  - Delete requires the owner or `IsAdmin`.
  - A listing that is missing or deleted gives 404 before any ownership check, so callers cannot probe for other users' deleted listings.

### 3. Data model

```
listings
  id uuid PK | owner_id uuid (index) | model_id uuid FK -> models | city_id uuid FK -> cities   (restrict)
  year int | mileage int | mileage_unit varchar(2) | engine_volume numeric(3,1) NULL | horsepower int | seats int
  vin varchar(17) NULL | body_type, fuel_type, transmission, drive_type, color, market varchar(20)
  price numeric(12,2) | currency varchar(3) | description varchar(3000) NULL
  is_credit, is_barter, is_painted, is_damaged bool | created_at, updated_at, is_deleted, deleted_at
  index ix_listings_created_at_id (created_at DESC, id DESC) WHERE is_deleted = false   -- public paging
  index ix_listings_owner_id_created_at (owner_id, created_at DESC) WHERE is_deleted = false  -- Mine
```

- **Enums** live in `Data/Listing/Enums/V1`: `BodyType`, `FuelType`, `Transmission`, `DriveType`, `Color`, `Market`, `Currency`, `MileageUnit`. They are stored as their names with `HasConversion<string>()`.
  - Readable in SQL.
  - Adding a value needs no data change.
  - Reordering cannot corrupt rows.
- **Engine volume** is `decimal?` in litres, `numeric(3,1)`.
- **Price** is `decimal`, `numeric(12,2)`.
- **Navigations:** `Listing` → `Model` (→ `Brand`) and → `City`. Reads `Include` them for the names in responses.

### 4. Enums in JSON

- `ApiVersioningExtensions`, where `AddControllers` is, adds `JsonStringEnumConverter(allowIntegerValues: false)`.
  - Clients send and receive `"Petrol"`.
  - Numbers and unknown names fail model binding, which gives 400 through `[ApiController]`.
  - Swagger shows the names.
- Validators also check `IsInEnum()` as a second guard.

### 5. Paging

- `Data/Shared/Models/V1/PagedResponse<T>` contains `Items`, `Page`, `PageSize`, and `TotalCount`.
- Paged queries take `Page` and `PageSize` from `[FromQuery]` with defaults 1 and 20.
- `Data/Shared/Validators/V1/PagingRuleExtensions` enforces `Page` 1–500 and `PageSize` 1–100, so every paged validator uses the same rule.
- The repository runs `CountAsync`, then `OrderByDescending(CreatedAt).ThenByDescending(Id).Skip().Take()`.
  - The `Id` tie-break keeps the order stable for listings created in the same instant.
- The 500-page cap bounds the deepest `OFFSET` at 49,900 rows.
- The response shape is kept when search moves to Elasticsearch. A cursor variant can be added beside it.

### 6. Validation

- `CreateListingValidator` and `UpdateListingValidator` share the same field rules through a listing rule set.
- The VIN regex is `^[A-HJ-NPR-Z0-9]{17}$`, case-insensitive. The VIN is stored upper-case.
- **Engine volume:**
  - It must be empty when `FuelType == Electric`.
  - Otherwise it is required, from 0.1 to 10.0, with at most one decimal.
- The year's upper bound is computed at validation time: `DateTime.UtcNow.Year + 1`.
- **Rules that need the database live in the service, as in the catalog:**
  - The model or city must exist and not be deleted. Otherwise a `ValidationException` names `modelId` or `cityId`.
  - Restore conflicts throw `ConflictException`.

### 7. Catalog protection

- `IModelRepository.HasActiveListingsAsync` and `ICityRepository.HasActiveListingsAsync` query `Listings`, where the query filter skips deleted rows.
- `ModelService.DeleteAsync` and `CityService.DeleteAsync` throw `ConflictException` when either is true.
- A brand is already protected through its models.

### 8. Global rule

In `microservices.md`, Controllers, "Write actions and `WithDeleted` actions require the admin role." becomes:

> Write actions and `WithDeleted` actions require the admin role, unless the resource is owned by a user: then any signed-in user creates it, the owner updates and deletes it, and admins may still delete and restore any of them and read `WithDeleted`. A list of the caller's own resources is `GET Mine`.

### 9. Gateway

`appsettings.json` gets a `listing` route (`/api/v{version}/Listing/{**rest}`) to the existing `ms-listing` cluster. No code changes.

## Risks / Trade-offs

- [`totalCount` costs a second query on every page] → It is cheap with the partial index at this scale. Search will replace it.
- [Offset pages shift when listings are added while paging] → This is accepted for now. The cursor variant comes with search.
- [An admin cannot fix a typo in someone's listing] → This is intended: admins moderate by removing listings, not by editing them. A later moderation change can add a rejection reason.
- [Deleting a model or city is blocked once listings use it] → This is intended. To retire an entry, its listings are moved or deleted first.

## Migration Plan

1. The global rule and the shared pieces: 403, current user, paging, and string enums.
2. The listing entity, enums, configuration, and migration.
3. The listing slice, verified in Local.
4. Catalog protection.
5. The gateway route, then Development and Production through the gateway, and docs.

Rollback: revert the commits. The migration's `Down` drops `listings`.
