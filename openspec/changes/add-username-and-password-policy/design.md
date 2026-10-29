# Design

## Context

- `add-authentication` set up the realms with `registrationEmailAsUsername: true` and `passwordPolicy: length(8)`.
- Realms are imported with `IGNORE_EXISTING`, so editing the realm files changes only fresh installs. The running `carsales` and `carsalesDev` realms keep their old settings.
- ms-auth sends the email as the Keycloak username, and login takes `email`.
- Requirements are in `specs/authentication/spec.md`.

Keycloak 26.4 behavior, checked in a throwaway container:

| Case | Keycloak response |
|---|---|
| Duplicate username, any case | `409 {"errorMessage":"User exists with same username"}` |
| Duplicate email | `409 {"errorMessage":"User exists with same email"}` |
| Password breaks the policy | `400 {"error":"invalidPassword…","error_description":"…"}`. The user is **not** created |
| Uppercase and lowercase checks | Unicode-aware, so `Ə` counts as uppercase |
| Stored username | Lowercase |
| Login | Works with the email when `loginWithEmailAllowed` is on |
| `kcadm.sh update realms/<realm> -s key=value` | Updates a running realm |

## Goals / Non-Goals

**Goals:**
- Every Keycloak, whether new or existing, on a PC or a server, ends up with the same realm settings without manual steps.
- The client sees one clear 400 per broken rule, and the same rules hold inside Keycloak.

**Non-Goals:**
- Changing the username of existing accounts. Only test accounts exist.
- Refresh token in a cookie. This was deferred until the client type is known.
- Other password rules, such as history, blocklist, or expiry.

## Decisions

### 1. `keycloak-setup`: realm settings applied on every start

- **Where:** a one-shot service in `infrastructure/keycloak/docker-compose.yml`. It uses the Keycloak image itself, which ships `kcadm.sh` and `bash`, and runs `setup/realms.sh`, mounted read-only.
- **What the script does:**
  1. Wait until Keycloak answers.
  2. Sign in as the bootstrap admin from `keycloak/.env`.
  3. For `carsales` and `carsalesDev`, run `kcadm.sh update realms/<realm>` with:
     - `registrationEmailAsUsername=false`
     - `loginWithEmailAllowed=true`
     - `duplicateEmailsAllowed=false`
     - `passwordPolicy="length(8) and upperCase(1) and lowerCase(1) and digits(1) and specialChars(1)"`
  4. Exit 0.
- `restart: "no"`, and `depends_on: keycloak` with `service_started`. The script itself waits until Keycloak is ready.
- The setup is idempotent: running it again sets the same values.
- The realm JSON files get the same values, so a fresh import is already correct and the setup only confirms it.
  - The script is the source of truth for these keys. Changing one of them in the console is undone on the next `up`.
- Alternatives:
  - Manual commands in the README: rejected, easy to forget on a new machine or server.
  - Recreating the realms: rejected, it deletes users.

### 2. Username rules

- FluentValidation: `^[a-zA-Z0-9._-]{3,30}$`.
- `@` is not allowed, so a login value is never ambiguous between a username and an email.
- Keycloak lowercases usernames and compares them case-insensitively. ms-auth sends the value as typed, and the responses show Keycloak's lowercase form, because the user is read back after creation.

### 3. Password rules in two places

- FluentValidation (`RegisterUserValidator`) checks the following, with one message per broken rule:
  - minimum length 8
  - `\p{Lu}`: an uppercase letter
  - `\p{Ll}`: a lowercase letter
  - `\p{Nd}`: a digit
  - `[^\p{L}\p{Nd}]`: a symbol
- Unicode classes match Keycloak's checks. ASCII-only classes would reject passwords that Keycloak accepts, or accept passwords that Keycloak rejects.
- Keycloak's policy is the backstop. If its rules ever differ from the validator's, a `400 invalidPassword…` from the admin API is turned into a `ValidationException` on `password` carrying Keycloak's description. The client still gets a 400, never a 500.

### 4. Conflict messages

`UserKeycloakRepository` reads the 409 `errorMessage`:
- `…same username` → `ConflictException("An account with this username already exists.")`
- `…same email` → the existing email message
- Anything else → a generic conflict message

### 5. API changes

| Item | Change |
|---|---|
| `RegisterUserDto` | Adds `Username`. The mapping to `KeycloakUserRepresentation` sends `Username` instead of the email |
| `LoginUserDto` | `Email` → `Login`. It goes to Keycloak as `username`, which accepts the email too. Validator: `Login` and `Password` not empty |
| `GetUserOkResponseDto` | Adds `Username`. The Mapster mapping picks it up by name |
| `ITokenService.LoginAsync` / `ITokenRepository.GetByPasswordAsync` | Parameter renamed `email` → `login` |

## Risks / Trade-offs

- [`keycloak-setup` signs in as the bootstrap admin, which Keycloak labels "temporary". If you replace or delete that admin, the setup fails] → Its error names the variables to check. A dedicated setup account can come later if needed.
- [The realm settings live in two places, the JSON files and `realms.sh`] → Both are in the same folder and change in the same change. The script wins on every start.
- [**BREAKING:** login takes `login` instead of `email`] → There are no clients yet. The README and Swagger show the new field.
- [Users created before this change have their email as the username] → Only test users exist. They can still sign in with that value.

## Migration Plan

1. Update the realm files and add `keycloak-setup`.
2. Run `up`. The setup updates both running realms.
3. Deploy ms-auth.

Rollback: revert the commit. The realms keep the new settings until the old values are set again by hand or with a reverted setup script.
