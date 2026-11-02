# Design

## Context

- No phone exists anywhere: not on listings, accounts, or in Keycloak's user profile. Services do not call each other, and Kafka is not installed.
- `ms-auth` already has a Keycloak service account with `manage-users` and `view-users`, and `IUserRepository.GetByIdAsync`.
- Listing responses already publish `ownerId`.
- The gateway already sends `Auth/**` to `ms-auth`.
- Requirements are in `specs/authentication` and `specs/web-frontend`. Motivation is in proposal.md.

## Goals / Non-Goals

**Goals:**
- A number changes in one place and applies to every listing at once.
- A person who only browses is never asked for personal data.
- `ms-listing` and the service boundaries stay as they are.

**Non-Goals:**
- Rate limiting on the public endpoint, SMS confirmation, foreign numbers, and removing a number.
- A rule in `ms-listing` that a listing needs a number: it cannot see accounts.

## Decisions

### 1. The number lives on the account

Chosen by the user over a number per listing. It costs a profile page and an update API, but one change reaches every listing.

### 2. The browser joins listing and seller

The listing page reads the listing from `ms-listing` and, when the button is pressed, the seller from `ms-auth` by `ownerId`.

- **Alternative, `ms-listing` calls `ms-auth`:** needs a service token and makes listing reads depend on `ms-auth`.
- **Alternative, Kafka copy:** survives `ms-auth` being down, but brings the first broker, events, and stale data for one field.

Pressing a button also means a page view makes no call to Keycloak, and `ms-auth` being down never affects a listing.

### 3. Public seller endpoint

`GET api/v1/Auth/Sellers/{id:guid}` returns `{ firstName, phone }` through `GetSellerOkResponseDto`, so a new account field cannot leak by accident. The last name, email, and username stay private. Unknown and disabled users give 404, and an account without a number gives 200 with `phone: null`.

Anyone with an id can read that number. Ids are public only for users who have listings, and those numbers are shown on their listing pages anyway. The endpoint has no rate limit, so numbers can be harvested; this is recorded as a risk and a later change.

### 4. Updating Keycloak: read the whole user, change one attribute, write it all back

**This is the rule the implementation must not break.** Tested on a throwaway Keycloak 26.4.7:

| Update | Result |
|---|---|
| `PUT` with only `attributes` | **Email, first name, and last name are erased**, and sign-in then fails with "Account is not fully set up" |
| `GET`, change `attributes.phone`, `PUT` the whole body | All fields stay, sign-in works, roles stay |

So `UpdatePhoneAsync` reads the user as a `JsonObject` rather than `KeycloakUserRepresentation`, which holds only some fields, and sets `attributes.phone`, keeping any other attribute. Serializing the typed class would drop fields such as `emailVerified`.

A scenario in `specs/authentication` covers this, and task 2.2 checks it against the real realm.

### 5. Keycloak user profile

Without a declaration Keycloak silently drops the attribute (tested: `204`, nothing stored). `infrastructure/keycloak/setup/user-profile.json` holds Keycloak's default profile plus:

- `phone`, not required, so accounts without it keep signing in. A required attribute would block them.
- Permissions `view: [admin]`, `edit: [admin]`: users cannot change it through Keycloak's own account screens, and only `ms-auth` writes it.
- A pattern of `^\+994[0-9]{9}$`, as a backstop to the API's check.

`realms.sh` applies it with `kcadm.sh update users/profile -f` for both realms on every start. Tested: this works and can be repeated. The script therefore owns this configuration, like its other realm settings. Task 1.1 compares the file with the live profiles first, so a setting made there is not lost unnoticed.

The number is not added to tokens: no mapper exists for it (tested).

### 6. Number format

`PhoneRuleExtensions` in `ms-auth` normalizes before checking:
- removes spaces, dashes, and brackets;
- `0` plus 9 digits, `994` plus 9 digits, and `+994` plus 9 digits all become `+994` plus 9 digits;
- anything else is invalid.

Normalizing on the server means every client stores the same form. Operator codes are not checked, since a new code should not need a release. The site shows `+994 50 123 45 67`.

The phone does not appear in logs, since it is personal data. The unhandled exception handler logs the method and path only, and the seller path holds an id, not a number.

### 7. API shape

Follows the existing `Auth` feature:

| Kind | Name |
|---|---|
| Command, DTO | `UpdateUserPhoneCommand`, `UpdateUserPhoneDto` |
| Query, DTO | `GetSellerByIdQuery`, `GetSellerOkResponseDto` |
| Handlers, validators | named after them |
| Repository | `IUserRepository.UpdatePhoneAsync` |
| Representation | `KeycloakUserRepresentation.Attributes` for reads |

`PUT Auth/Me/Phone` responds with the updated profile. `GetSellerById` uses its own service method, since `GetByIdAsync` turns a missing user into 401 for `Me`, and a missing seller must be 404.

### 8. Website

- **Listing page:** a contact block in the side card with a "Nömrəni göstər" button. The query is `enabled: false` until pressed. No number, 404, and failures each have their message.
- **Session state:** `AuthContext` gets a way to replace the user after the number changes, so the form and profile page agree without a reload.
- **Profile page:** `/profile`, behind `RequireAuth`; the username in the header links to it.
- **New-listing form:** one editable number field, filled in from the account. This replaces the read-only number with a link to the profile suggested during exploration, since following a link would lose what was typed.

Saving order: check locally, save the number if it changed, create the listing, upload images. The number goes first because a rejected number must not leave a listing behind. A number saved just before a failed create stays on the account, which is harmless.

## Risks / Trade-offs

- **Harvesting numbers:** the public endpoint has no rate limit, and the button stops only casual scraping. → Accepted for now; rate limiting is planned as a later change.
- **Personal data in public:** a seller's number becomes public by publishing a listing. → The form's hint says it appears on all of their listings.
- **`keycloak-setup` overwrites the profile:** a change made in the console is undone on the next start. → As for other settings the script owns; documented in the README.
- **Listings without a number:** older listings, and those made by API clients, show that no contact is available until the owner adds a number. → Accepted; adding one fixes all of their listings at once.
- **Two people changing one account at once:** read-then-write can lose the earlier write. → Only the owner changes it, and only this one field is written by the service.
- **A new Keycloak version may add profile defaults:** the file would then overwrite them. → Compare it when upgrading Keycloak; noted in the README.

## Migration Plan

Additive. `keycloak-setup` applies the profile on the next start, with no change to existing users. Rollback: remove the endpoints and the page. The attribute can stay, since nothing reads it.
