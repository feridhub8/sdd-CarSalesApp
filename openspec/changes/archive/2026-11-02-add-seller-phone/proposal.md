# Proposal

## Why

A buyer can read a listing but has no way to reach the seller: listing responses carry only `ownerId`, and no phone number exists anywhere in the system. This is the main gap left on the website.

## What Changes

- An account can hold one phone number, an Azerbaijani number stored as `+994XXXXXXXXX`. It is kept in Keycloak and is not part of tokens.
- **`ms-auth`:** `GET Auth/Me` returns `phone`; `PUT Auth/Me/Phone` changes it; a new public `GET Auth/Sellers/{id}` returns only the seller's first name and phone.
- **Keycloak:** both realms declare the phone attribute, optional, visible to admins only.
- **Website, listing page:** a "show number" button fetches the seller's name and number when pressed, without signing in. On a phone the number starts a call.
- **Website, new listing:** the form asks for the contact number, filled in from the account, and saves it to the account. A person who only browses is never asked.
- **Website, profile:** a new `/profile` page shows the account and changes the number.
- Registration does not change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `authentication`: the current user profile includes the phone, an owner can change it, and anyone can read a seller's contact details.
- `web-frontend`: seller contact on the listing page, the number in the new-listing form, and a profile page.

## Impact

- **`services/ms-auth`:** user representation, Keycloak repository, new commands, query, DTOs, and controller actions; `README.md`.
- **`infrastructure/keycloak`:** a user profile file applied by `keycloak-setup` on every start; `infrastructure/README.md`.
- **`web/`:** listing page, new-listing form, new profile page, and session state.
- **No change:** `ms-listing`, gateway routes (`Auth/**` already goes to `ms-auth`), and registration.
- **Out of scope, kept for later:** rate limiting on the public endpoint, foreign numbers, confirming a number by SMS, removing a number, and showing the number per listing.
