# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Rule and shared pieces

- [x] 1.1 Update the Controllers bullet in `~/Desktop/sdd-global-rules/microservices.md` with the ownership exception and `GET Mine` (design §8); verify with `git diff` that only that bullet changed
- [x] 1.2 Add `ForbiddenException` (403 in `GlobalExceptionHandler`), `ICurrentUserService`/`CurrentUserService` (`Id` from `sub`, `IsAdmin`; registered in `ServiceExtensions` with `AddHttpContextAccessor`), `PagedResponse<T>`, `PagingRuleExtensions`, and `JsonStringEnumConverter(allowIntegerValues: false)` in `ApiVersioningExtensions`; verify the build passes with no warnings and the existing Brand, Model, and City scenarios still pass

## 2. Listing data

- [x] 2.1 Add the eight enums in `Data/Listing/Enums/V1`, the `Listing` entity (with `Model` and `City` navigations), `ListingConfigurations` (string enums, column types, FKs with restrict, the two partial indexes), the `Listings` DbSet, and the `AddListings` migration; verify `--migrate` creates `listings` with the indexes from design §3

## 3. Listing slice

- [x] 3.1 Add DTOs, commands, queries (paged list, by id, `Mine`, `WithDeleted` list and by id), validators (field rules, engine volume rule, VIN, paging), mappings (names of brand, model, and city), `IListingRepository`/`ListingEfCoreRepository`, `IListingService`/`ListingService` (ownership and admin checks, model and city checks, restore conflicts), handlers, and `ListingController`; register them in `RepositoryExtensions` and `ServiceExtensions`; verify the build and that Swagger shows the nine actions with string enums
- [x] 3.2 Verify every scenario of `specs/listings` in Local: public reads and 404, paging (default, second page, totals, out of range), create with the owner taken from the token, no token, field rules (each range, enums, VIN, engine volume for electric and petrol, missing or deleted model or city), owner update and delete, another user and admin update (403), another user delete (403), admin delete, `Mine`, `WithDeleted`, restore (204, owner 403, conflicts); remove the test data and users afterwards

## 4. Catalog protection

- [x] 4.1 Add `HasActiveListingsAsync` to the model and city repositories and the 409 checks to `ModelService.DeleteAsync` and `CityService.DeleteAsync`; verify a model or city used by a listing cannot be deleted, can be after the listing is deleted, and the existing catalog scenarios still pass
- [x] 4.2 Update `services/ms-listing/README.md` (listing endpoints with who can call them, fields and rules, enums, paging, ownership, the catalog delete rule, curl examples for create, list, and `Mine`); verify the documented commands work as written

## 5. Gateway and Docker

- [x] 5.1 Add the `listing` route to the gateway settings and README; verify in Development and then Production through the gateway: paged list with query string, create with a bearer token, owner update, `Mine`, admin restore; remove test data and containers afterwards; run `graphify update .`

## Workflow follow-up

- Commit group 1 (in `sdd-global-rules` and CarSales), then after groups 2–3, after group 4, and after group 5, each with the message shown for approval.
- Archive with `/opsx:archive` (specs are synced automatically).
