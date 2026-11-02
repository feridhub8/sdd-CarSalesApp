# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Keycloak

- [x] 1.1 Add `infrastructure/keycloak/setup/user-profile.json` and apply it for both realms in `realms.sh` (design §5), first comparing it with the live profiles of `carsales` and `carsalesDev`; verify `keycloak-setup` succeeds twice, both realms list `phone`, existing accounts still sign in, and their fields and roles are unchanged
- [x] 1.2 Update `infrastructure/README.md` (what `keycloak-setup` owns, comparing the file when upgrading Keycloak); verify it matches the script

## 2. API

- [x] 2.1 Add `Attributes` to `KeycloakUserRepresentation`, `phone` to `GetUserOkResponseDto`, `PhoneRuleExtensions`, and `IUserRepository.UpdatePhoneAsync` as read-then-write (design §4); verify the build has no warnings and `GET Me` returns `phone: null` for an existing test account
- [x] 2.2 Add `PUT Auth/Me/Phone` and `GET Auth/Sellers/{id}` with their commands, queries, validators, handlers, DTOs, and mappings (design §3, §6, §7); verify every scenario of `specs/authentication` in Local, including that after a change the email, names, roles, and password stay and sign-in works, that the seller response holds only two fields, and that the token holds no number; remove test users afterwards
- [x] 2.3 Update `services/ms-auth/README.md` (endpoints, number format, the read-then-write rule); verify the documented commands work as written; rerun the existing auth suite

## 3. Website

- [ ] 3.1 Add the contact block to the listing page (design §8); verify with Playwright the `Seller contact on a listing` scenarios at both widths, including that nothing is requested before pressing, and read the screenshots
- [ ] 3.2 Add `/profile`, the header link, and the number field in the new-listing form with its saving order; verify with Playwright the `Contact number when publishing` and `Profile page` scenarios; update the existing website suites for the new field

## 4. Whole system in Docker

- [ ] 4.1 Run all website suites, the full walk, and the auth suite in Development (`6080`) and then Production (`7080`), including `ms-auth` really stopped; remove test data and users, leaving the user's own data, and bring Development back up; run `graphify update .`; verify `git status` shows no secrets

## Workflow follow-up

- Commit after each group, with the message shown for approval.
- Archive with `/opsx:archive` (specs are synced automatically).
- Next changes to explore: search and filters, rate limiting on public endpoints.
