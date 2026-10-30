# Proposal

## Why

The catalog (brands, models, cities) is in place, but there is still nothing to sell. Listings are the core of CarSales. Signed-in users publish their cars, anyone browses them, and admins can take listings down. Listings belong to the users who create them, so the platform also needs its first ownership rule. Until now only admins could write.

## What Changes

- **Listings in `ms-listing`** (`/api/v1/Listing`), built on the catalog.
  - **Each listing has:**
    - A model, which also gives the brand, and a city.
    - Year, mileage and mileage unit, engine volume in litres, horsepower, seats, and an optional VIN.
    - Body type, fuel type, transmission, drive type, color, and market.
    - Price and currency, an optional description, and credit, barter, painted, and damaged flags.
  - Technical lists are fixed enums, sent as strings (for example `"fuelType": "Petrol"`).
  - A listing is visible as soon as it is created. There is no moderation yet.
- **Who can do what:**
  - **Anyone** lists listings (paged) and reads one.
  - **A signed-in user:**
    - creates listings; the owner is taken from the token, never from the body,
    - updates and deletes their own listings,
    - lists their own listings (`Mine`).
  - **An admin** deletes and restores any listing and reads deleted ones. An admin cannot edit another user's listing.
  - Another user's listing gives `403` on update or delete.
- **Paging:**
  - `?page=&pageSize=` returns `{ items, page, pageSize, totalCount }`, newest first.
  - Page size is 20 by default, at most 100. Page numbers go up to 500.
- **Catalog protection:** a model or city with non-deleted listings cannot be deleted (`409`), as a brand with models cannot.
- **Gateway:** `/api/v{version}/Listing/**` is forwarded to `ms-listing`.
- **Global rule (`sdd-global-rules/microservices.md`):** "write actions require the admin role" gains the ownership exception. The owner creates, updates, and deletes their own resource; admins can still delete and restore any of them and read `WithDeleted`.

## Capabilities

### New Capabilities
- `listings`: publishing, browsing, editing, and removing car listings. It covers ownership and admin rights, field rules, paging, the owner's own listings, soft delete, and restore.

### Modified Capabilities
- `listing-catalog`: the soft delete requirement also refuses to delete a model or city that non-deleted listings use.
- `api-gateway`: forwards listing requests to `ms-listing`.

## Impact

- **ms-listing:**
  - A new `Data/Listing` feature with entity, enums, DTOs, commands, queries, validators, handlers, mappings, repository, service, and controller.
  - A migration that adds `listings`.
  - Model and city delete checks.
  - String enums in JSON.
  - A shared paging model.
  - README.
- **api-gateway:** one route in `appsettings.json` and the README route table.
- **Global rules:** one bullet in `microservices.md`, committed in `sdd-global-rules`.
- **No infrastructure change:** same database, Vault secrets, and Keycloak setup.
- **Later work:** search and filters (Elasticsearch, cursor paging), features (equipment), images, moderation and status, VIP, seller phone.
