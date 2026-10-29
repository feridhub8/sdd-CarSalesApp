# Design

## Context

- ms-auth is an empty .NET 10 Web API: `Program.cs` only builds and runs the app. It is dockerized; Local runs on `5001`, Development on `6001`, Production on `7001`.
- There is no `infrastructure/` folder yet. Every container joins the external network `carsales_network`.
- Project decisions:
  - One PostgreSQL server holds both development and production databases.
  - Development resources on a shared instance end with `Dev`.
  - There is one Keycloak instance, and environments are separated by realms.
  - Services sign in to Vault with AppRole. The role id and secret id are kept in `.env` and loaded with DotNetEnv.
  - People sign in to Vault with userpass. The root token is revoked after setup.
  - Production may later move to a server, with or without its data.
- Requirements are in `specs/authentication/spec.md`.

## Goals / Non-Goals

**Goals:**
- `docker compose` starts PostgreSQL, Keycloak, and Vault. After the one-time setup, the only manual step is unsealing Vault after a start.
- A PC and a server behave the same way. Only `.env` values differ.
- ms-auth works the same with `dotnet run`, the IDE, and Docker. Only the hosts in `appsettings.*` differ.
- Production data on the PC stays separate from development data and can be moved to a server later.

**Non-Goals:**
- Roles, admin endpoints, email verification, password reset, and social login.
- The API gateway. Clients call ms-auth directly for now.
- HTTPS and the production-mode Keycloak setup for a real server. These come with the server move.
- A database for ms-auth itself. Users live in Keycloak.

## Decisions

### 1. Infrastructure layout

```
infrastructure/
+-- docker-compose.yml        name: carsales; include: postgres, keycloak, vault
+-- README.md                 first-time setup, daily start, unseal, backups
+-- postgres/
|   +-- docker-compose.yml    postgres:17 (port 5432, volume postgres_data) + postgres-setup
|   +-- .env.example          POSTGRES_USER, POSTGRES_PASSWORD
|   +-- setup/databases.sql   idempotent: roles and databases
+-- keycloak/
|   +-- docker-compose.yml    keycloak:26.4, start-dev --import-realm, port 8080
|   +-- .env.example          KC_DB_PASSWORD, bootstrap admin, KC_HOSTNAME, client secrets
|   +-- realms/carsales-realm.json
|   +-- realms/carsalesDev-realm.json
+-- vault/
    +-- docker-compose.yml    vault:2.2.1 (port 8200, volume vault_data) + manual vault-unseal
    +-- .env.example          VAULT_KEYS_FILE (default ~/Desktop/carsales-vault-keys.json)
    +-- config/vault.hcl      file storage, listener without TLS, UI on
    +-- config/policies/admin.hcl
    +-- config/policies/ms-auth.hcl

~/Desktop/carsales-vault-keys.json   outside the project, downloaded from the UI at init
```

- Each tool has one compose file with no dev/prod split, because the instance is shared by all environments.
- The root `docker-compose.yml` and `docker-compose.dev.yml` both include `infrastructure/docker-compose.yml`. Local development starts only the infrastructure, with `docker compose -f infrastructure/docker-compose.yml up -d`.
- Compose names the containers `carsales-postgres-1`, `carsales-keycloak-1`, and `carsales-vault-1`. Other services reach them by service name: `postgres`, `keycloak`, `vault`.
- Image tags are exact: `postgres:17`, `quay.io/keycloak/keycloak:26.4`, `hashicorp/vault:2.2.1`.
- Keycloak has no `depends_on` on postgres, because the compose files must also work alone. It uses `restart: unless-stopped` and retries until its database exists.
- Host ports are the defaults: `5432`, `8080`, `8200`. Only one local project that uses these tools runs at a time.

### 2. PostgreSQL: idempotent setup on every start

- The one-shot service `postgres-setup` runs on every `up`, after `postgres` is healthy. It uses the same image and runs `setup/databases.sql` with `psql`.
- The script creates a role or database only when it is missing, using `\gexec`. It sets the role password on every run, so the server always matches `.env`.
  - Rejected alternative: `/docker-entrypoint-initdb.d`. It runs only on an empty volume, so databases added later would never be created on an existing server.
- For now, the script creates the role and database `keycloak`. Later services add their `<name>` and `<name>Dev` databases to the same script. Names are quoted so the `Dev` suffix keeps its case.
- The Keycloak database password exists only in `keycloak/.env` as `KC_DB_PASSWORD`. `postgres-setup` loads that file as an extra `env_file` and passes the value to `psql` as a variable, so the password is never duplicated.

### 3. Keycloak: one instance, two realms, imported from files

- `carsales` is the production realm and `carsalesDev` the development realm. Both are defined in JSON and imported with `--import-realm`.
  - The import strategy is `IGNORE_EXISTING`, so it never overwrites a realm that exists, including migrated data.
  - File names must be `<realm>-realm.json`; Keycloak refuses to start otherwise. This was verified on 26.4.
- Each realm has the confidential client `ms-auth`, configured as follows:
  - Direct access grants are on, for login with email and password.
  - A service account holds the `realm-management` roles `manage-users` and `view-users`, for registration and user lookup.
  - Standard browser flow is off.
  - An audience mapper puts `ms-auth` into the access token's `aud` claim.
- Realm settings: email as the username, no duplicate emails, password policy `length(8)`, and default token lifetimes (access 5 minutes, session idle 30 minutes).
- Client secrets are not committed. The realm files contain `${MS_AUTH_CLIENT_SECRET}` and `${MS_AUTH_CLIENT_SECRET_DEV}`, which Keycloak fills from `keycloak/.env` during import. This was verified on 26.4: the secret from the environment works, and the literal placeholder does not.
- **Fixed issuer:** `KC_HOSTNAME=http://localhost:8080` and `KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true`.
  - Every token has the same issuer, whether Keycloak is reached as `localhost` (Local) or `keycloak` (inside Docker).
  - Backchannel URLs (token, certs, introspection) follow the host that was used, so containers still call `keycloak:8080`. This was verified on 26.4.
  - On a server, only `KC_HOSTNAME` in `.env` changes.
- Keycloak runs `start-dev` (plain HTTP) on the PC. Production mode needs TLS, which belongs to the server move.

### 4. Vault: persistent storage, manual unseal from key files, AppRole and userpass

- Vault runs in server mode with `file` storage on the volume `vault_data`. Dev mode was rejected because it keeps everything in memory.
- A PC and a server behave the same way. After every start Vault is sealed, and you unseal it with one command.
- **Init in the UI, once.** On the first visit to `http://localhost:8200`, Vault asks to be initialized.
  - You choose 5 key shares and threshold 3.
  - The UI shows the 5 unseal keys and the root token only once. You save them with "Download keys" as a JSON file, `~/Desktop/carsales-vault-keys.json`.
  - The file is outside the project, so it can never be committed.
- **Unseal after every start**, either way:
  - In the UI, by pasting 3 of the keys.
  - With `docker compose -f infrastructure/docker-compose.yml run --rm vault-unseal`. This one-shot service runs only on demand (`profiles: [manual]`). It mounts the JSON file read-only, takes the keys from `keys_base64`, and feeds them to `vault operator unseal` until Vault reports unsealed. It uses the Vault image itself, so no extra tools are needed and the command is the same on every OS.
- `VAULT_KEYS_FILE` in `vault/.env` points to the JSON file and defaults to the Desktop path. Compose expands `~` to the user's home folder on every OS. On a server it can point anywhere.
- **Who signs in how:**
  - **Services: AppRole.** ms-auth logs in at startup with `VAULT_ROLE_ID` and `VAULT_SECRET_ID` from `services/ms-auth/.env`, gets a short-lived token (1 hour), and reads its secrets. There is no long-lived token that can expire unnoticed. The role `ms-auth` has only the `ms-auth` policy, which can read `carsales/data/ms-auth` and `carsales/data/ms-authDev`.
  - **People: userpass.** An admin user with the `admin` policy signs in to the UI (`http://localhost:8200`) or the CLI with a username and password.
  - **Root token: revoked after setup.** It can do everything and never expires, so it is not kept. In an emergency, such as a lost admin password, `vault operator generate-root` with 3 unseal keys creates a temporary one. You revoke it again after the fix.
    - Vault 2.x requires a token for `generate-root` by default. This would leave no way back once every admin login is lost.
    - `vault.hcl` therefore sets `enable_unauthenticated_access = ["generate-root"]`. The cost is that anyone who can reach port 8200 can start or cancel a generation attempt, but no one can finish it without 3 unseal keys.
- One-time setup after init, documented in `infrastructure/README.md` and run with the root token from the JSON file:
  1. Enable a KV v2 engine at `carsales`.
  2. Write the policies `admin` and `ms-auth`.
  3. Enable `userpass` and create your admin user.
  4. Enable `approle`, create the role `ms-auth`, and read its role id and a secret id into `services/ms-auth/.env`.
  5. Write the secrets.
  6. Revoke the root token. The copy in the JSON file then no longer works.
- The JSON file is the one thing that must not be lost. Its keys open Vault and can recreate a root token.
- Secret paths follow the `Dev` rule: `carsales/ms-auth` for production and `carsales/ms-authDev` for development. Key names are configuration keys (`Keycloak:ClientSecret`), so they map straight into `IConfiguration`.
- Every machine has its own Vault. Keys and data live in that machine's volume and key file, not in git. A fresh clone initializes Vault again and gets new keys.

### 5. ms-auth as a facade over Keycloak

```
client --> AuthController --> mediator --> Handler --> validator --> Service --> Repository --> Keycloak

POST api/v1/Auth/Register --> RegisterUserCommand ---> UserService.Register  --> UserKeycloakRepository  (admin API)
POST api/v1/Auth/Login    --> LoginUserCommand    ---> TokenService.Login    --> TokenKeycloakRepository (password grant)
POST api/v1/Auth/Refresh  --> RefreshTokenCommand ---> TokenService.Refresh  --> TokenKeycloakRepository (refresh_token grant)
POST api/v1/Auth/Logout   --> LogoutUserCommand   ---> TokenService.Logout   --> TokenKeycloakRepository (logout endpoint)
GET  api/v1/Auth/Me       --> GetUserByIdQuery(sub) -> UserService.GetById   --> UserKeycloakRepository  (admin API)
```

- Feature folder `Data/Auth`, laid out as the global rules require.
- Keycloak's JSON shapes live in `Models/V1`, along with `KeycloakOptions` and `VaultOptions`. Mapster maps them to and from the DTOs. Each mapping is one `IRegister` class in its own file, named `<Source>_<Destination>`.
- Each repository uses a typed `HttpClient` against `Keycloak:BaseUrl`.
- The admin API repository gets a service account token through `client_credentials`. It caches the token until 30 seconds before it expires.
- Registration sends the user and the password credential in a single admin API call. The new user's id is read from the `Location` header.
- Keycloak errors are translated in the repositories:
  - `409` → `ConflictException`
  - `401` or `400 invalid_grant` → `UnauthorizedException`
- `ExceptionHandlingMiddleware` turns these into 409 and 401 responses. FluentValidation's `ValidationException` becomes 400 with a ProblemDetails error list.
- **Me:**
  - `[Authorize]` uses JwtBearer with authority `{BaseUrl}/realms/{Realm}` and audience `ms-auth`.
  - The issuer check uses the fixed issuer from discovery, so a token from another realm gives 401.
  - `MapInboundClaims = false`, so `sub` is read as is.
  - **Session check:** after local validation, JwtBearer's `OnTokenValidated` calls Keycloak token introspection. If the session has ended (`active: false`), the request fails with 401. This way an access token stops working right after logout, not 5 minutes later.
  - The handler loads the user from Keycloak by `sub`, so the profile is always current. If the user no longer exists, the response is 401.
- `RequireHttpsMetadata` comes from configuration and is false on the PC.

### 6. Configuration per environment

| Key | appsettings.json (Production) | Development | Local |
|---|---|---|---|
| `Keycloak:BaseUrl` | `http://keycloak:8080` | (base) | `http://localhost:8080` |
| `Keycloak:Realm` | `carsales` | `carsalesDev` | `carsalesDev` |
| `Keycloak:ClientId` | `ms-auth` | (base) | (base) |
| `Vault:Address` | `http://vault:8200` | (base) | `http://localhost:8200` |
| `Vault:SecretPath` | `ms-auth` | `ms-authDev` | `ms-authDev` |

- `Keycloak:ClientSecret` comes only from Vault.
- **Secrets at startup:** `InitSecrets` runs first in `Program.cs`. It calls `Env.TraversePath().Load()`, so `services/ms-auth/.env` is found from the service folder or from `src/WebApi`. Then it logs in with AppRole (`VAULT_ROLE_ID`, `VAULT_SECRET_ID`), reads the KV secret, and adds it to configuration.
  - The app fails fast with a clear message when the credentials are missing or wrong, or when Vault is sealed (the message names `vault-unseal`) or unreachable.
  - In Docker, the compose files pass `.env` with `env_file`. `.dockerignore` already keeps `.env` out of the image.

### 7. Libraries and versions

| Package | Version | Note |
|---|---|---|
| MediatR | 12.5.0 | last free release, never 13+ |
| Mapster + Mapster.DependencyInjection | 10.0.13 | MIT, no known advisories |
| FluentValidation.DependencyInjectionExtensions | 12.2.0 | |
| Asp.Versioning.Mvc + .ApiExplorer | 10.2.1 | |
| Swashbuckle.AspNetCore | 10.3.1 | Bearer scheme so `Me` can be tried in Swagger |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.14 | |
| VaultSharp | 1.17.5.1 | AppRole login, KV v2 |
| DotNetEnv | 3.2.0 | |

- Mapster replaces AutoMapper. AutoMapper 14.0.0 has a high-severity advisory (GHSA-rvv3-g6hj-g44x) that fails the build under `TreatWarningsAsErrors`, and versions 15 and later need a commercial license.
- `Program.cs` stays short. Setup goes through `Init<Area>()` classes in `Extensions/V1`:
  - `SecretExtensions`
  - `LibraryExtensions` (MediatR, Mapster, FluentValidation)
  - `ApiExtensions` (controllers, versioning)
  - `KeycloakExtensions` (options, typed clients)
  - `AuthenticationExtensions` (JwtBearer, session check)
  - `SwaggerExtensions` (Local and Development only)
  - `HealthCheckExtensions` (`/health`)

### 8. Applying in stages

The change is large, so `/opsx:apply` runs one task group at a time and stops after each one:

```
group done --> I report what was built and how to test it --> you test
          --> commit (message shown for approval) --> next group
```

- Each group ends in a working, verifiable state:
  - Groups 1–4 need no ms-auth code.
  - Group 5 runs without the endpoints.
  - Group 6 adds them one by one.
- A problem is found in the group that caused it, not several groups later.

## Risks / Trade-offs

- [The password grant is deprecated in OAuth 2.1] → It is the only way for ms-auth to own the login endpoint. Keycloak still supports it. A browser flow can be added later without changing these endpoints.
- [Future services that validate tokens locally will still accept an access token for up to 5 minutes after logout. Only `Me` checks the session] → The lifetime is short. Those services can use the same introspection check when they need it.
- [Introspection adds one Keycloak call to every `Me` request] → This is acceptable at this scale. Only `Me` pays for it.
- [Vault is sealed after every restart, so ms-auth fails at startup until it is unsealed] → The error message says to run `vault-unseal`. The READMEs list it right after the start command.

## Migration Plan

- First run on a machine:
  1. Create `carsales_network`.
  2. Copy the `.env.example` files and fill them in.
  3. Start the infrastructure.
  4. Initialize Vault in the UI, download the keys, and do the Vault setup once.
  5. Unseal Vault after each start.
  6. Run ms-auth.
- Moving production to a server later:
  - Export the `carsales` realm with its users (`kc.sh export --realm carsales --users realm_file`).
  - Move service databases without the `Dev` suffix with `pg_dump`/`pg_restore`.
  - Set `KC_HOSTNAME` to the server URL.
  - Initialize and set up Vault on the server, and write fresh secrets.
- Rollback: `docker compose down` stops everything. `down -v` also removes the volumes and every user.
