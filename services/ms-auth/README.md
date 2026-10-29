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
| `POST Register` | `username`, `email`, `password`, `firstName`, `lastName` | `201` user (`id`, `username`, `email`, `firstName`, `lastName`), no tokens | `400` invalid fields, `409` username or email exists |
| `POST Login` | `login` (username or email), `password` | `200` `accessToken`, `expiresIn`, `refreshExpiresIn`, `tokenType`; web: refresh token cookie, mobile: `refreshToken` in the body | `400` missing fields, `401` wrong login or password |
| `POST Refresh` | web: no body; mobile: `refreshToken` | `200` new tokens, delivered like Login | web: `401` no or invalid cookie (cookie cleared); mobile: `400` no token, `401` invalid token |
| `POST Logout` | web: no body; mobile: `refreshToken` | `204`; the login ends, its refresh and access tokens stop working; web: cookie cleared | web: `401` no or invalid cookie (cookie cleared); mobile: `400` no token, `401` invalid token |
| `GET Me` | header `Authorization: Bearer <accessToken>` | `200` user | `401` missing, invalid, expired, signed-out, or other-environment token |

Rules:

- `username`: 3–30 characters of letters, digits, `.`, `_` or `-`; case-insensitive (stored in lowercase).
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
# Development: http://localhost:6001, realm carsalesDev
docker compose -f docker-compose.dev.yml up -d --build
docker compose -f docker-compose.dev.yml down

# Production: http://localhost:7001, realm carsales
docker compose up -d --build
docker compose down
```

Health check: `GET /health`.
