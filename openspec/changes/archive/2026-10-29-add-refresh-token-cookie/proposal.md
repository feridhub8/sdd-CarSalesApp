# Proposal

## Why

The web and mobile clients will be built in parallel, and paid VIP listings make a stolen session worth real money. Today the refresh token is returned in the response body. A browser has to keep it where JavaScript can read it, so an XSS bug could carry the session away and keep it alive for hours. Refresh tokens can also be reused until they expire, so a stolen one goes unnoticed.

## What Changes

- **Hybrid token transport** on the same endpoints, chosen per request by the `X-Client-Type` header.
  - **Web (default, no header):**
    - Login and refresh return the access token in the body and set the refresh token as an `HttpOnly`, `Secure`, `SameSite=Strict` cookie scoped to `/api/v1/Auth`.
    - Refresh and logout read the refresh token only from that cookie.
    - Logout clears the cookie.
  - **Mobile (`X-Client-Type: mobile`):** both tokens are in the body, as today, and refresh and logout read the token only from the body.
  - Each mode ignores the other mode's source. In particular, a mobile request never uses the cookie, so the cookie's token can never reach JavaScript.
- **BREAKING (web):** login and refresh responses no longer include `refreshToken`, and refresh and logout take no body. There are no clients yet.
- **Refresh token rotation:** each refresh token works once.
  - Reusing an already-exchanged token gives 401.
  - It also ends the login it came from, so its newer refresh token and its access token stop working too.
  - It is enabled in both realms through the realm files and `keycloak-setup`.
- `Register` and `Me` do not change.

## Capabilities

### New Capabilities
(none)

### Modified Capabilities
- `authentication`:
  - Login, token refresh, and logout gain web (cookie) and mobile (body) behavior, keeping the two modes apart.
  - Token refresh adds rotation with reuse detection.

## Impact

- **Infrastructure:** realm files and `keycloak/setup/realms.sh` gain the rotation settings.
- **ms-auth:**
  - `AuthController` handles the transport.
  - A client type enum and a cookie constants class are added.
  - The token response DTO omits `refreshToken` for web, and the refresh and logout DTOs make it optional.
  - The cookie attributes are fixed in code, the same in every environment.
  - The README is updated.
- **Later work:** the web frontend change must enable CORS with credentials if it runs on another origin. The frontend must also send only one refresh at a time, because rotation makes parallel refreshes from several tabs fail.
