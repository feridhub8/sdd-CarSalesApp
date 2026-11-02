# Design

## Context

- `GET Listing` pages non-deleted listings, newest first, through `ListingEfCoreRepository.GetPageAsync`, which `Mine` and `WithDeleted` share.
- Listings store `price` with `currency` (`AZN`, `USD`, `EUR`) and `mileage` with `mileage_unit` (`Km`, `Mi`). Real Development data already mixes both.
- Existing indexes: `(created_at, id)`, `model_id`, `city_id`, and the owner index.
- Enums are sent by name and stored as strings; JSON rejects numbers, but query string binding of an enum would accept them.
- Requirements are in the three delta specs. Motivation is in proposal.md.

## Goals / Non-Goals

**Goals:**
- Filters and sorts that hold up with hundreds of thousands of listings, with no new infrastructure.
- Prices compared fairly, with nobody maintaining rates.
- Existing callers of `GET Listing` see no change.

**Non-Goals:**
- Free text search, per-filter result counts, saved searches, and showing converted prices.
- Filters on `Mine` and `WithDeleted`.

## Decisions

### 1. PostgreSQL, not Elasticsearch

Chosen by the user. Searching cars is mostly structured filters, which PostgreSQL does well, without a second copy of the data to keep in step. The API hides the engine, so moving later does not change the site.

### 2. No new indexes for now

Measured on a throwaway PostgreSQL 17 with the real schema and 200,000 listings (5% deleted), 600 models, 40 cities:

| Query | Page | Count |
|---|---|---|
| Newest, no filter | 0.2 ms | 37 ms |
| Page 400, newest | 8 ms | 38 ms |
| Brand | 0.9 ms | 20 ms |
| Model | 1.3 ms | 1.2 ms |
| City and year | 0.7 ms | 7 ms |
| Fuel, body, transmission | 34 ms | 17 ms |
| Converted price range | 0.3 ms | 27 ms |
| Cheapest, converted, no filter | 51 ms | 37 ms |
| Cheapest, within a brand | 20 ms | 19 ms |
| Mileage range in km | 0.9 ms | 25 ms |
| Year, descending | 46 ms | 37 ms |
| Has images | 0.3 ms | 63 ms |

The slowest search costs about 90 ms in total. Each index slows writes and can serve only some filter combinations, so none is added. This was the raw SQL shape, on warm caches; task 2.2 measures the real EF queries. The time grows about in step with the table, so these numbers are a reason to look again at about a million listings.

### 3. Conversion happens in the query, with no stored column

```
price_in_azn = CASE currency WHEN AZN THEN price WHEN USD THEN price * usd ELSE price * eur END
```

A stored `price_azn` would allow an index, but every listing would need rewriting when a rate changes, several times a week for EUR. Computing in the query means a new rate applies at once and no listing row changes. The price row above shows the cost is acceptable.

A price filter given in USD or EUR converts its bounds to AZN first, so all comparison happens in AZN. Mileage works the same way with a fixed factor of 1.609.

### 4. Exchange rate source

Checked by hand on Monday 2026-11-02:

| Date asked | File returned | USD | EUR |
|---|---|---|---|
| 02.11.2026, today | 02.11.2026 | 1.7 | 1.9 |
| 01.11.2026, Sunday | 30.10.2026 | 1.7 | 1.9019 |
| 31.10.2026, Saturday | 30.10.2026 | 1.7 | 1.9019 |
| 03.11.2026, tomorrow | 02.11.2026 | 1.7 | 1.9 |
| 25.12.2030, far future | 02.11.2026 | 1.7 | 1.9 |
| 99.99.2026, invalid | `302` to HTML | | |

So asking for today always gives the latest working day, and the `Date` attribute says which. The site is reachable from a container on `carsales_network`. It is a public file, not a documented API, so it may change, which is why the service must survive bad answers.

Rates are stored as `exchange_rates`: `currency` (primary key), `rate_to_azn`, `rate_date`, `updated_at`. No soft delete applies, so it is not a `BaseEntity`.

### 5. Fetching

`ExchangeRateRefreshService`, a `BackgroundService`, fetches on start and every 6 hours. Six hours rather than daily: a service started just before the bank publishes would otherwise stay a day behind, and a repeat fetch costs one small request. `--migrate` never reaches it, since the app exits before hosted services start.

Rules for a usable answer: parses as XML with DTD processing prohibited, has both USD and EUR with `Nominal` 1, and each rate is positive and within 0.1 to 100, which catches a wrong column without guessing at real market moves. The request has a 15-second timeout and a size limit. Anything else logs a warning and changes nothing; a failure never stops the service.

Writes are one upsert per currency (`INSERT ... ON CONFLICT DO UPDATE`), so replicas fetching at once cannot conflict.

Fallback in `appsettings.json`: `ExchangeRates:Fallback` with USD 1.7 and EUR 1.9, used only when the table has no row. The table is read on each search: two rows by primary key, cheap enough not to need a cache.

Names: `IExchangeRateRepository` and `ExchangeRateEfCoreRepository` for the table; `IExchangeRateSourceRepository` and `ExchangeRateCbarRepository` for the bank; `IExchangeRateService` and `ExchangeRateService`, all in a new `ExchangeRate` feature. `ExchangeRateExtensions.InitExchangeRates` registers the options, the HTTP client, and the hosted service. The source sits behind an interface so a different one can replace it.

### 6. API shape

`GET Listing` gets a `[FromQuery] SearchListingsDto`, wrapped by `GetAllListingsQuery`. `GetPageAsync` stays for `Mine` and `WithDeleted`, and a new `SearchAsync` serves the public list, so those two cannot gain filters by accident.

- **Fixed values are bound as strings** and checked by the validator with `Enum.TryParse` plus a rule that rejects digits, so `fuelType=1` gives 400 like it does in JSON.
- **Repeatable values** use `List<string>`, with at most 20 values each, so a long query string cannot build a huge `IN`.
- **Sort tie-break:** every sort ends with `id`, so pages stay stable. Newest keeps `(created_at, id)` to use its index.
- **Unchanged behavior:** no filter and no sort produce the same query as today, plus reading the rates only when a price is involved.

### 7. Website

- **State lives in the address.** The panel reads and writes `useSearchParams`; there is no separate filter state to fall out of step, so reload, sharing, and back all work.
- **Bad values:** the page keeps only values it knows before calling the API, so a hand-edited link cannot cause a 400 page.
- **Debounce:** number fields wait 500 ms. Selects and checkboxes apply at once. Filtering uses `replace` for typing and `push` otherwise, so back does not step through each digit.
- **A rejected range** is checked in the browser and shown beside the fields, without a request.
- **Layout:** a side column at `lg` and above, and a collapsible block with an active-count badge below. Multi-value filters are checkbox groups, which work on phones.
- **Requests:** normal use stays far below the 300 per minute API limit. The existing 429 message covers the rest.

## Risks / Trade-offs

- **The bank's file changes or disappears:** rates freeze at the last good values, silently for users. → A warning is logged every 6 hours; searching still works.
- **Slightly stale rates:** a converted comparison can be up to a working day old. → Acceptable for browsing, and the page says rates are used for comparison.
- **A converted filter differs from the shown price:** a 15,000 USD car appears for "up to 25,500 AZN" and may not the next day. → Accepted; the seller's price stays what is shown.
- **Sorting by converted price cannot use an index:** it reads every matching row. → Measured at about 50 ms at 200,000 listings. Revisit around a million; a stored column would then be the answer.
- **Deep pages:** `OFFSET` grows with the page number. → Capped at page 500, measured at 8 ms for page 400.
- **`totalCount` on every search:** about 40 ms at this size. → Accepted; it is the largest fixed cost, and an estimate could replace it later.
- **Outgoing traffic:** `ms-listing` now calls the internet. → Only this one address, with a timeout. A locked-down server must allow it, or it stays on the last rates.

## Migration Plan

Additive. `AddExchangeRates` creates the table through the usual migration, and the first fetch fills it. Rollback: the extra parameters can be ignored and the table can stay.
