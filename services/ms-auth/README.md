# ms-auth

Authentication service. A facade over Keycloak: users register, sign in, refresh their session, sign out, and read their own profile. Users live in Keycloak; ms-auth has no database.

## Requirements

- .NET 10 SDK (pinned by `global.json` in the repository root)
- The infrastructure running and Vault unsealed (see [`infrastructure/README.md`](../../infrastructure/README.md)):

  ```bash
  docker compose -f infrastructure/docker-compose.yml up -d
  docker compose -f infrastructure/docker-compose.yml run --rm vault-unseal
  ```

- `services/ms-auth/.env` with the Vault AppRole credentials (copy `.env.example`; the values come from the Vault setup in the infrastructure README):

  ```
  VAULT_ROLE_ID=...
  VAULT_SECRET_ID=...
  ```

At startup ms-auth signs in to Vault and reads the Keycloak client secret. It stops with a clear message if `.env` is missing, the credentials are wrong, or Vault is sealed or down.

## Build

From `services/ms-auth/`:

```bash
dotnet build Auth.slnx
```

## Run

From `services/ms-auth/`:

```bash
dotnet run --project src/WebApi
```

The app starts in the `Local` environment on `http://localhost:5001` and uses the `carsalesDev` realm. Swagger UI is at `http://localhost:5001/swagger`; after login, paste the `accessToken` into **Authorize** to call `Me`. Stop it with `Ctrl+C`.

## Endpoints

All under `api/v1/Auth`. Errors are returned as ProblemDetails.

### Web and mobile clients

The same endpoints serve both. The `X-Client-Type` header picks how the refresh token travels:

| | Web (no header, default) | Mobile (`X-Client-Type: mobile`) |
|---|---|---|
| Refresh token | `HttpOnly` cookie `refresh_token` (`Secure`, `SameSite=Strict`, `Path=/api/v1/Auth`), set by Login and Refresh | In the response body |
| Refresh / Logout send it | Nothing to do: the browser sends the cookie; no body | In the body: `{"refreshToken": "..."}` |
| Access token | Body (keep it in memory only) | Body |

Each mode uses only its own source: a web request never reads a body token, and a mobile request never reads the cookie. This keeps the cookie's token out of reach of JavaScript.

| Endpoint | Request | Success | Errors |
|---|---|---|---|
| `POST Register` | `username`, `email`, `password`, `firstName`, `lastName` | `201` user (`id`, `username`, `email`, `firstName`, `lastName`, `phone`), no tokens | `400` invalid fields, `409` username or email exists |
| `POST Login` | `login` (username or email), `password` | `200` `accessToken`, `expiresIn`, `refreshExpiresIn`, `tokenType`; web: refresh token cookie, mobile: `refreshToken` in the body | `400` missing fields, `401` wrong login or password |
| `POST Refresh` | web: no body; mobile: `refreshToken` | `200` new tokens, delivered like Login | web: `401` no or invalid cookie (cookie cleared); mobile: `400` no token, `401` invalid token |
| `POST Logout` | web: no body; mobile: `refreshToken` | `204`; the login ends, its refresh and access tokens stop working; web: cookie cleared | web: `401` no or invalid cookie (cookie cleared); mobile: `400` no token, `401` invalid token |
| `GET Me` | header `Authorization: Bearer <accessToken>` | `200` user | `401` missing, invalid, expired, signed-out, or other-environment token |
| `PUT Me/Phone` | header `Authorization: Bearer <accessToken>`, `phone` | `200` user | `400` invalid number, `401` as for `Me` |
| `GET Sellers/{id}` | none, public | `200` `firstName`, `phone` | `404` unknown or disabled user |

Rules:

- `username`: 3–30 characters of letters, digits, `.`, `_` or `-`; case-insensitive (stored in lowercase).
- `phone`: an Azerbaijani number, optional on an account and `null` until set. Spaces, dashes, and brackets are ignored; `050 123 45 67`, `994501234567`, and `+994501234567` are all stored as `+994501234567`. It lives in Keycloak as the `phone` attribute (declared in `infrastructure/keycloak/setup/user-profile.json`) and is not in tokens.
- **`GET Sellers/{id}` is public:** anyone with a user id can read that first name and number. `ownerId` on a listing gives that id. It returns nothing else about the account.
- **Changing Keycloak users:** Keycloak's `PUT` replaces the whole user. Fields left out, such as the email and names, are erased and the user can no longer sign in. Always read the user, change the one field, and send the whole body back, as `UpdatePhoneAsync` does.
- `password`: at least 8 characters with an uppercase letter, a lowercase letter, a digit, and a symbol. Each broken rule is listed in the `400` response. Keycloak enforces the same policy.
- **Refresh tokens work once (rotation).** Every refresh returns a new one. Sending an already-used refresh token gives `401` and ends that login, including its newer tokens. A web frontend must therefore send only one refresh at a time (for example, one shared refresh promise across tabs); parallel refreshes from several tabs end the login.
- **Safari and `http://localhost`:** the cookie is always `Secure`. Chrome, Edge, and Firefox accept it on `http://localhost`; Safari does not, so test the web flow locally with one of the others (or over HTTPS).

### Example against Local

Web (curl's cookie jar stands in for the browser):

```bash
BASE=http://localhost:5001/api/v1/Auth

curl -i -X POST $BASE/Register -H "Content-Type: application/json" \
  -d '{"username":"jane","email":"jane@example.com","password":"Secret123!","firstName":"Jane","lastName":"Doe"}'

curl -s -c jar.txt -X POST $BASE/Login -H "Content-Type: application/json" \
  -d '{"login":"jane","password":"Secret123!"}'
# copy accessToken; the refresh token is now in jar.txt

curl -s $BASE/Me -H "Authorization: Bearer <accessToken>"
curl -s -X PUT $BASE/Me/Phone -H "Authorization: Bearer <accessToken>" -H "Content-Type: application/json" -d '{"phone":"050 123 45 67"}'
curl -s $BASE/Sellers/<user id>

curl -s -b jar.txt -c jar.txt -X POST $BASE/Refresh

curl -i -b jar.txt -c jar.txt -X POST $BASE/Logout
```

Mobile:

```bash
curl -s -X POST $BASE/Login -H "X-Client-Type: mobile" -H "Content-Type: application/json" \
  -d '{"login":"jane","password":"Secret123!"}'
# copy accessToken and refreshToken

curl -s -X POST $BASE/Refresh -H "X-Client-Type: mobile" -H "Content-Type: application/json" \
  -d '{"refreshToken":"<refreshToken>"}'
# use the new refreshToken from this response; the old one no longer works

curl -i -X POST $BASE/Logout -H "X-Client-Type: mobile" -H "Content-Type: application/json" \
  -d '{"refreshToken":"<refreshToken>"}'
```

## Docker

Requires the `carsales_network` network and `services/ms-auth/.env` (passed to the container with `env_file`). From `services/ms-auth/`:

```bash
# Development: realm carsalesDev
docker compose -f docker-compose.dev.yml up -d --build
docker compose -f docker-compose.dev.yml down

# Production: realm carsales
docker compose up -d --build
docker compose down
```

In Docker, ms-auth publishes no host port. Clients reach it through [`api-gateway`](../../api-gateway/README.md) on `http://localhost:6080` (Development) or `http://localhost:7080` (Production), on the same paths, so the examples above work with `BASE=http://localhost:6080/api/v1/Auth` or `BASE=http://localhost:7080/api/v1/Auth`. Without a host port, ms-auth can be scaled: `docker compose up -d --scale ms-auth=3` from the repository root.

**Temporary direct access in Development** (for example, to open Swagger): uncomment the `ports` block in `docker-compose.dev.yml` and run `up` again. ms-auth is then at `http://localhost:6001` and Swagger at `http://localhost:6001/swagger`. Comment the block back before committing; while it is active, `--scale` does not work.

Health check: `GET /health`.
