# Proposal

## Why

Accounts are identified only by email, and passwords only need 8 characters. Users should pick a username they can sign in with, and passwords should be strong enough to resist guessing.

## What Changes

- Registration takes a `username` as well. It is 3–30 characters (letters, digits, `.`, `_`, `-`), unique, and case-insensitive. A taken username gives 409, like a taken email.
- Passwords need at least 8 characters with at least one uppercase letter, one lowercase letter, one digit, and one symbol.
  - ms-auth checks this and answers 400.
  - Keycloak enforces the same policy on its side.
- **BREAKING:** login takes `login` instead of `email`. The value may be the username or the email. There are no clients yet.
- The register and `Me` responses include `username`.
- Realm files: the email is no longer the username, and the new password policy applies.
- A new one-shot `keycloak-setup` service applies these realm settings on every start, like `postgres-setup`.
  - Realm import skips realms that already exist, so without it running Keycloaks would never get the new settings.

## Capabilities

### New Capabilities
(none)

### Modified Capabilities
- `authentication`: registration takes a username and enforces the password policy, login accepts a username or an email, and the current user profile includes the username.

## Impact

- **Infrastructure:** `infrastructure/keycloak/` gets updated realm files, a `setup/` script, and the `keycloak-setup` service. `infrastructure/README.md` is updated.
- **ms-auth:** register DTO and validator, login DTO and validator, the user DTO, the user representation and its mappings, and the conflict handling in `UserKeycloakRepository`. The README is updated.
- **Existing users:** none outside tests. Users created earlier keep their email as the username.
