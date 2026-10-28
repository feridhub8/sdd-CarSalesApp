# Tasks

Apply one group at a time (design §8): stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. PostgreSQL

- [x] 1.1 Create `infrastructure/postgres/` with `docker-compose.yml` (`name: carsales`, `postgres:17`, port `5432`, volume `postgres_data`, `env_file: .env`, healthcheck `pg_isready`, network `carsales_network` external) and `.env.example`; verify `docker compose config` passes in that folder
- [x] 1.2 Add `setup/databases.sql` (idempotent role and database `keycloak` via `\gexec`, password set on every run) and the one-shot `postgres-setup` service (after `postgres` is healthy, `env_file` also loads `../keycloak/.env` for `KC_DB_PASSWORD`); verify two `up` runs in a row both exit 0 and `psql -l` lists `keycloak` owned by `keycloak`
- [x] 1.3 Create `infrastructure/docker-compose.yml` (`name: carsales`) including the postgres compose file; verify `docker compose -f infrastructure/docker-compose.yml up -d` starts `carsales-postgres-1` as healthy

## 2. Keycloak

- [ ] 2.1 Create `infrastructure/keycloak/docker-compose.yml` (`keycloak:26.4`, `start-dev --import-realm`, DB at `postgres:5432/keycloak`, bootstrap admin, `KC_HOSTNAME`, `KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true`, port `8080`, `realms/` mounted read-only, `restart: unless-stopped`) and `.env.example`; include it in `infrastructure/docker-compose.yml`; verify the admin console opens on `http://localhost:8080`
- [ ] 2.2 Write `realms/carsales-realm.json` and `realms/carsalesDev-realm.json` with the realm settings, the `ms-auth` client, the service account roles, and the audience mapper from design §3, with secret placeholders; verify after a fresh start (`down -v`, then `up`) that both realms exist, a `client_credentials` request with each secret from `keycloak/.env` returns 200, and a token fetched from inside the network (`keycloak:8080`) has issuer `http://localhost:8080/realms/<realm>`
- [ ] 2.3 Restart Keycloak with the realms already present; verify the log shows the import skipped them and a change made in the console is kept

## 3. Vault

- [ ] 3.1 Create `infrastructure/vault/` with `config/vault.hcl` (file storage, TCP listener without TLS, UI), `config/policies/admin.hcl`, `config/policies/ms-auth.hcl` (read `carsales/data/ms-auth` and `carsales/data/ms-authDev`), `docker-compose.yml` (`vault:2.2.1` server, port `8200`, volume `vault_data`), and `.env.example` (`VAULT_KEYS_FILE=~/Desktop/carsales-vault-keys.json`); include it in `infrastructure/docker-compose.yml`; verify the UI on `http://localhost:8200` shows the initialize screen on a fresh volume
- [ ] 3.2 Initialize Vault in the UI (5 shares, threshold 3) and download the keys to `~/Desktop/carsales-vault-keys.json`; add the manual service `vault-unseal` (mounts the file read-only, feeds `keys_base64` until unsealed) under `profiles: [manual]`; verify `up -d` does not start it and, after a Vault restart, `run --rm vault-unseal` leaves Vault unsealed
- [ ] 3.3 Do the one-time setup with the root token from the JSON file: KV v2 at `carsales`, both policies, `userpass` with an admin user, `approle` with the role `ms-auth` (role id and secret id into `services/ms-auth/.env`), the secret `Keycloak:ClientSecret` at `carsales/ms-auth` and `carsales/ms-authDev`, then revoke the root token; verify the admin user can sign in to the UI, the root token is rejected, an AppRole login as `ms-auth` can read both paths but not write, and after a restart plus `vault-unseal` everything is still there
- [ ] 3.4 Write `infrastructure/README.md` (start/stop, ports, UI init and key download, unsealing after every start in the UI or with `vault-unseal`, first-time setup for all three tools, emergency `generate-root`, each machine having its own Vault, and the production export commands); verify by following it from a clean `down -v` to a working state

## 4. Root wiring

- [ ] 4.1 Add `infrastructure/docker-compose.yml` to `include:` in the root `docker-compose.yml` and `docker-compose.dev.yml`; verify `docker compose config --services` from the root lists `postgres`, `postgres-setup`, `keycloak`, `vault`, `ms-auth` for both files
- [ ] 4.2 Update the root `README.md` with an infrastructure section (tools, ports, link to `infrastructure/README.md`, Local needs the infrastructure running); verify the commands it lists run as written

## 5. ms-auth foundation

- [ ] 5.1 Add the NuGet packages from design §7 to `WebApi.csproj`; verify `dotnet build` passes with 0 warnings
- [ ] 5.2 Add `appsettings.json`, `appsettings.Development.json`, and `appsettings.Local.json` values from design §6, plus `services/ms-auth/.env.example` with `VAULT_ROLE_ID=` and `VAULT_SECRET_ID=`; verify `.env` is git-ignored and `.env.example` is not
- [ ] 5.3 Add `Extensions/V1/SecretExtensions` (DotNetEnv, AppRole login, KV read into configuration, fail-fast messages) and call it first in `Program.cs`; verify that Local startup reads `Keycloak:ClientSecret` without logging it, and fails with a clear message for empty values, a wrong secret id, and a sealed Vault (the message names `vault-unseal`)
- [ ] 5.4 Add `LibraryExtensions`, `ApiExtensions` (controllers and API versioning), `SwaggerExtensions` (Bearer scheme, Local and Development only), `HealthCheckExtensions`, `Exceptions/V1` (`ConflictException`, `UnauthorizedException`), and `Middlewares/V1/ExceptionHandlingMiddleware`; verify `/health` returns 200 and `/swagger` opens in Local
- [ ] 5.5 Add `env_file: .env` to `services/ms-auth/docker-compose.yml` and `docker-compose.dev.yml`; verify the Development container starts, reads its secret from `ms-authDev`, and returns 200 on `http://localhost:6001/health`

## 6. Authentication endpoints

- [ ] 6.1 Add `Models/V1` (`KeycloakOptions`, `VaultOptions`, Keycloak token and user representations), `KeycloakExtensions` (options and typed HTTP clients), and the repositories `UserKeycloakRepository` (service account token cached until 30 s before expiry) and `TokenKeycloakRepository` with their `Base/` interfaces and error translation; verify the build passes
- [ ] 6.2 Add `UserService` and `TokenService` with their `Base/` interfaces, plus the DTOs and the Mapster mappings from design §5 (one `IRegister` per file); verify the build passes and `TypeAdapterConfig.Compile()` succeeds at startup
- [ ] 6.3 Implement register (command, validator, handler, `POST Register`); verify the spec scenarios: 201 with the user, 409 on a duplicate email, 400 on invalid input, user visible in the `carsalesDev` realm
- [ ] 6.4 Implement login (command, validator, handler, `POST Login`); verify the spec scenarios: 200 with tokens, 401 with the same message for a wrong password or unknown email, 400 on missing fields
- [ ] 6.5 Implement refresh (command, validator, handler, `POST Refresh`); verify the spec scenarios: 200 with new tokens, 401 on a malformed or expired token
- [ ] 6.6 Implement logout (command, validator, handler, `POST Logout`); verify the spec scenarios: 204, a later refresh with the same token gives 401, and logging out with an invalid token gives 401
- [ ] 6.7 Add `AuthenticationExtensions` (JwtBearer with the introspection session check in `OnTokenValidated`) and implement me (query, validator, handler, `GET Me` with `[Authorize]`); verify the spec scenarios: 200 with the profile, 401 without a token, 401 with a `carsales` realm token against Local, and 401 with an unexpired access token right after logout
- [ ] 6.8 Update `services/ms-auth/README.md` (endpoints, `.env` setup, needs the infrastructure running and Vault unsealed); verify the documented curl commands work as written

## 7. Integration check

- [ ] 7.1 From a clean state, run all five endpoints in each environment: Local (`5001`, `carsalesDev`), Development container (`6001`, `carsalesDev`), and Production container (`7001`, `carsales`); verify a user registered in Production cannot log in through Local and the reverse
- [ ] 7.2 Run `graphify update .`; verify it finishes without errors

## Workflow follow-up

- Commit after each group, with the message shown for approval.
- Archive the change with `/opsx:archive` after review.
