# Design

## Context

- Today `Login` and `Refresh` return both tokens in the body, and `Refresh` and `Logout` take `{ refreshToken }` in the body.
- The handlers, services, and repositories only see a refresh token string. They do not care where it came from.
- Keycloak realms are configured by the realm files and by `keycloak-setup`, which applies the project's settings on every start.
- Keycloak 26.4 rotation behavior, checked in a throwaway container with `revokeRefreshToken=true` and `refreshTokenMaxReuse=0`:
  - Exchanging R1 returns R2.
  - Sending R1 again returns `400 invalid_grant` ("Maximum allowed refresh token reuse exceeded").
  - After that, R2 also returns 400, and the login's access token introspects as `active: false`.
  - The existing translation turns that `invalid_grant` into 401.
- Requirements are in `specs/authentication/spec.md`.

## Goals / Non-Goals

**Goals:**
- A browser never holds the refresh token where JavaScript can read it.
- Mobile keeps the body-based flow.
- A stolen refresh token stops working as soon as either side reuses it.

**Non-Goals:**
- CORS for a frontend on another origin. That comes with the web frontend change.
- Separate Keycloak clients or token lifetimes per platform.
- Payments, step-up authentication, and changes to `Register` or `Me`.

## Decisions

### 1. Transport lives only in the controller

```
 AuthController  -- reads X-Client-Type, picks the source and the sink of the refresh token
      |             web:    cookie in / cookie out      mobile: body in / body out
      v
 mediator --> handler --> validator --> service --> repository   (unchanged, string in / string out)
```

- **Client type:**
  - `X-Client-Type: mobile` (case-insensitive) means mobile. Anything else, or no header, means web.
  - A `ClientType` enum (`Web`, `Mobile`) is added in `Data/Auth/Enums/V1`.
  - The header is a `[FromHeader]` parameter on Login, Refresh, and Logout, so Swagger shows it as an optional field.
- **Isolation rule:** web reads only the cookie, and mobile reads only the body.
  - The controller never falls back from one source to the other.
  - This is what keeps the cookie's token away from injected JavaScript. If a mobile request read the cookie, a script could ask for the token in the body.
  - A dedicated scenario covers it.
- **Body on Refresh and Logout:**
  - `[FromBody(EmptyBodyBehavior = Allow)] RefreshTokenDto? dto`, so web requests may send no body.
  - The controller builds the command's DTO from the chosen source. The command, validator, and handler stay as they are.
- **Missing token:**
  - Web with no cookie throws `UnauthorizedException`, which gives 401.
  - Mobile with an empty body token goes through the validator, which gives 400.
- **Response:**
  - `GetTokenOkResponseDto.RefreshToken` becomes `string?` with `[JsonIgnore(Condition = WhenWritingNull)]`.
  - For web, the controller writes the cookie and returns the DTO with `RefreshToken = null`.

Every endpoint works in both modes:

| Endpoint | Web (no header) | Mobile (`X-Client-Type: mobile`) |
|---|---|---|
| `Login` | body `{login, password}` → body `{accessToken, expiresIn, refreshExpiresIn, tokenType}` + cookie | same body in → all tokens in the body, no cookie |
| `Refresh` | no body, token from the cookie → new access token in the body + new cookie; no cookie → 401; invalid → 401 and cookie deleted | body `{refreshToken}` → all tokens in the body; empty → 400; invalid → 401 |
| `Logout` | no body, token from the cookie → 204, cookie deleted; no cookie or invalid → 401, cookie deleted | body `{refreshToken}` → 204; empty → 400; invalid → 401 |
| `Register`, `Me` | unchanged | unchanged |

### 2. Cookie

| Attribute | Value | Why |
|---|---|---|
| Name | `refresh_token` | |
| `HttpOnly` | yes | JavaScript cannot read it |
| `SameSite` | `Strict` | Not sent on cross-site requests, which blocks CSRF |
| `Path` | `/api/v1/Auth` | Sent only to the auth endpoints |
| `Max-Age` | `refreshExpiresIn` from the token response | Expires with the token |
| `Secure` | yes, in every environment | Sent only over HTTPS, or to `http://localhost` |

- The attributes are fixed, not configurable. They live as constants in one place (`RefreshTokenCookie` in `Data/Auth/Models/V1`), which builds the `CookieOptions` for writing and deleting.
- Chrome, Edge, and Firefox accept `Secure` cookies on `http://localhost`, so Local, Development, and Production on the PC work with them.
  - Safari does not accept them on plain `http://localhost`. The README notes that the web cookie flow needs one of the other browsers, or HTTPS, when testing locally.
- **Clearing:**
  - Web logout always deletes the cookie (same name and path), whether the token was valid or not, so a dead cookie never lingers.
  - A web refresh that answers 401 also deletes it.

### 3. Rotation

- `keycloak-setup` adds `revokeRefreshToken=true` and `refreshTokenMaxReuse=0` to both realms. The realm files get the same values.
- ms-auth needs no code for this: a reused token comes back as `invalid_grant`, which is already turned into 401.
- **Consequence for the web frontend:** two tabs refreshing at once send the same cookie, and the second request fails and ends the login. The frontend must send a single refresh at a time and share the result. This is noted in the README for the frontend work.

## Risks / Trade-offs

- [The isolation rule is the whole protection. A later "convenience" fallback from body to cookie would silently undo it] → The rule is a spec scenario and part of the test script. The controller has a comment that names it.
- [A mobile app that forgets the header gets a cookie it probably ignores. The user then loses the session after 5 minutes] → The README documents the header. The mobile app should add it in one HTTP interceptor.
- [**BREAKING (web):** the login and refresh responses lose `refreshToken`, and refresh and logout take no body] → There are no clients yet. The README and Swagger show both modes.
- [Rotation plus parallel refreshes from several tabs ends the login] → A frontend rule, documented.
- [Safari rejects `Secure` cookies on `http://localhost`, so the web flow cannot be tested there over plain HTTP] → It is documented in the README. Use Chrome, Edge, or Firefox locally. Real deployments use HTTPS.
- [`SameSite=Strict` requires the web frontend and the API on the same site, e.g. `carsales.az` and `api.carsales.az`] → It is planned with the frontend and the gateway. Different sites would need `SameSite=None` and CSRF tokens, which would be a later change.

## Migration Plan

1. Group 1 adds the rotation settings, and `keycloak-setup` applies them to the running realms on the next `up`.
2. Group 2 deploys ms-auth.

Existing refresh tokens issued before rotation stay valid until they expire. Rollback means reverting the commits and running `keycloak-setup` with the old values.
