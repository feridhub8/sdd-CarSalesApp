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

| Endpoint | Body | Success | Errors |
|---|---|---|---|
| `POST Register` | `username`, `email`, `password`, `firstName`, `lastName` | `201` user (`id`, `username`, `email`, `firstName`, `lastName`), no tokens | `400` invalid fields, `409` username or email exists |
| `POST Login` | `login` (username or email), `password` | `200` `accessToken`, `refreshToken`, `expiresIn`, `refreshExpiresIn`, `tokenType` | `400` missing fields, `401` wrong login or password |
| `POST Refresh` | `refreshToken` | `200` new token pair | `400` missing field, `401` invalid, expired, or signed-out token |
| `POST Logout` | `refreshToken` | `204`; the session ends, its refresh and access tokens stop working | `400` missing field, `401` invalid, expired, or signed-out token |
| `GET Me` | header `Authorization: Bearer <accessToken>` | `200` user | `401` missing, invalid, expired, signed-out, or other-environment token |

Rules:

- `username`: 3–30 characters of letters, digits, `.`, `_` or `-`; case-insensitive (stored in lowercase).
- `password`: at least 8 characters with an uppercase letter, a lowercase letter, a digit, and a symbol. Each broken rule is listed in the `400` response. Keycloak enforces the same policy.

Example against Local:

```bash
BASE=http://localhost:5001/api/v1/Auth

curl -i -X POST $BASE/Register -H "Content-Type: application/json" \
  -d '{"username":"jane","email":"jane@example.com","password":"Secret123!","firstName":"Jane","lastName":"Doe"}'

curl -s -X POST $BASE/Login -H "Content-Type: application/json" \
  -d '{"login":"jane","password":"Secret123!"}'
# copy accessToken and refreshToken from the response

curl -s $BASE/Me -H "Authorization: Bearer <accessToken>"

curl -s -X POST $BASE/Refresh -H "Content-Type: application/json" -d '{"refreshToken":"<refreshToken>"}'

curl -i -X POST $BASE/Logout -H "Content-Type: application/json" -d '{"refreshToken":"<refreshToken>"}'
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
