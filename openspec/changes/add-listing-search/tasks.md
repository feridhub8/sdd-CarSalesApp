# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Exchange rates

- [x] 1.1 Add the `ExchangeRate` feature: entity, configuration, `AddExchangeRates` migration, repositories, service, options with fallback rates, and registration (design §4, §5); verify `--migrate` creates the table and the build has no warnings
- [x] 1.2 Add `ExchangeRateRefreshService`; verify the `Exchange rates` scenarios in Local: real rates arrive and match the bank, a closed port and an unusable answer (bad XML, zero rate, missing currency, wrong nominal, DTD) leave the rates unchanged with a warning, an empty table uses the fallback, two replicas keep one row per currency, and the service starts when the bank is down

## 2. Search API

- [x] 2.1 Add `SearchListingsDto`, validation, `SearchAsync` with filters, conversion, and sorts (design §3, §6); verify the build has no warnings and that `GET Listing` without parameters returns what it did, with `Mine` and `WithDeleted` unchanged
- [x] 2.2 Verify every scenario of `specs/listing-search` and the `listings` delta in Local against test data, including exact conversion cases and bounds; rerun the existing listing suite; measure the real EF queries on a throwaway database with 200,000 listings and record any large difference from design §2; remove test data
- [x] 2.3 Update `services/ms-listing/README.md` (parameters, conversion, rates and where they come from); verify the documented commands work as written

## 3. Website

- [x] 3.1 Add the filter panel, sort control, result count, address handling, reset, and phone layout (design §7); verify with Playwright every scenario of the `web-frontend` delta at both widths, and read the screenshots; update the browsing suite; run all website suites with raised limits

## 4. Whole system in Docker

- [ ] 4.1 Run all suites and the full walk in Development and then Production with raised limits, confirming real rates arrive in both databases and `ms-listing` has no errors; remove test data and users, leaving the user's own data, and bring Development back up with default limits; run `graphify update .`; verify `git status` shows no secrets

## Workflow follow-up

- Commit after each group, with the message shown for approval.
- Archive with `/opsx:archive` (specs are synced automatically).
- Backlog: free text search, listing features, moderation, VIP and payments.
