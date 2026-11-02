# Proposal

## Why

Buyers can only page through all listings, newest first. They cannot look for a brand, a city, a price range, or a body type, which makes the site hard to use once it holds more than a few pages of cars.

## What Changes

- **`GET Listing`** takes optional filters: brand, model, city, year, price, mileage, engine volume, body type, fuel, transmission, drivetrain, color, market, credit, barter, not painted, not damaged, and has images. It also takes a sort: newest, price, year, or mileage. Calls without them behave as today.
- **Prices compare across currencies:** filters and sorting convert every price at the Central Bank of Azerbaijan's rate. The price shown stays as the seller wrote it.
- **Exchange rates:** `ms-listing` fetches rates from the Central Bank by itself and keeps them in its database, so nobody maintains them by hand. If the bank cannot be reached, it uses the last rates it has.
- **Mileage compares in kilometres,** converting listings written in miles. The mileage shown does not change.
- **Website:** a filter panel and a sort control on the home page. Filters live in the address, so a search can be reloaded and shared.
- No free text search in this version, and no new infrastructure.

## Capabilities

### New Capabilities

- `listing-search`: filtering and sorting listings, and the exchange rates that make prices comparable.

### Modified Capabilities

- `listings`: `Paged lists` no longer always sorts newest first.
- `web-frontend`: a filter panel and sort control.

## Impact

- **`services/ms-listing`:** search parameters on `GET Listing`, an `ExchangeRate` feature with a table, migration, a Central Bank client, and a background service; `README.md`.
- **`web/`:** home page filters and sort.
- **External dependency:** `https://www.cbar.az/currencies/<date>.xml`, a public file, not a documented API.
- **No change:** gateway, `ms-auth`, infrastructure, `Mine`, and `WithDeleted`.
- **Out of scope, kept for later:** free text search, Elasticsearch, result counts per filter value, saved searches, and showing prices converted.
