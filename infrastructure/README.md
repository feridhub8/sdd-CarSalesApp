# Infrastructure

Shared tools for every CarSales environment. Each tool has its own folder with a `docker-compose.yml` and a `.env.example`; this folder's `docker-compose.yml` includes them all.

| Tool | Container | Host port | Notes |
|---|---|---|---|
| PostgreSQL 17 | `carsales-postgres-1` | `5432` | One server for development and production databases. Development names end with `Dev`. |
| Keycloak 26.4 | `carsales-keycloak-1` | `8080` | One instance. Realms: `carsales` (production), `carsalesDev` (development). |
| Vault 2.2.1 | `carsales-vault-1` | `8200` | Secrets. Sealed after every start. |

Two one-shot services run on every start and exit. Both are safe to run again and again:

- `postgres-setup` creates any missing roles and databases (`postgres/setup/databases.sql`).
- `keycloak-setup` applies the realm settings the project owns to `carsales` and `carsalesDev` (`keycloak/setup/realms.sh`): username separate from email, login with username or email, unique emails, the password policy (8+ characters with an uppercase letter, a lowercase letter, a digit, and a symbol), and refresh token rotation (each refresh token works once; reusing one ends that login).
  - Realm import only creates missing realms, so this is how existing realms get new settings.
  - These keys are owned by the script: a change made to them in the admin console is undone on the next start.
  - It signs in as the bootstrap admin from `keycloak/.env`. If that admin is replaced or deleted, the setup fails until the variables point to a working admin.

## Daily use

From the repository root:

```bash
docker compose -f infrastructure/docker-compose.yml up -d
docker compose -f infrastructure/docker-compose.yml run --rm vault-unseal
```

Vault is sealed after every start. Unseal it with the second command, or in the UI at `http://localhost:8200` by pasting 3 of the keys. Services cannot read their secrets until Vault is unsealed.

Stop everything (data is kept):

```bash
docker compose -f infrastructure/docker-compose.yml down
```

`down -v` also deletes the volumes: every database, every Keycloak user, and all Vault data. After that, Vault must be initialized again and the old keys file no longer works.

## First-time setup on a machine

Every machine has its own data and its own Vault. Nothing below is in git; a fresh clone starts empty and gets new keys.

### 1. Network and `.env` files

```bash
docker network create carsales_network
```

Copy each `.env.example` to `.env` in the same folder and fill in the empty values:

| File | Values |
|---|---|
| `postgres/.env` | `POSTGRES_PASSWORD`: superuser password. It is applied only when the volume is created; changing it later needs `down -v`. `LISTING_DB_PASSWORD`, `LISTING_DB_PASSWORD_DEV`: passwords of the `listing` and `listingDev` database roles (`postgres-setup` syncs them on every start; Vault holds the same values for `ms-listing`). |
| `keycloak/.env` | `KC_DB_PASSWORD`: password of the `keycloak` database role, kept only here (`postgres-setup` reads it too and syncs it on every start). `KC_BOOTSTRAP_ADMIN_PASSWORD`: Keycloak admin. `MS_AUTH_CLIENT_SECRET`, `MS_AUTH_CLIENT_SECRET_DEV`: the `ms-auth` client secret in each realm, used only on the first import. `MS_LISTING_CLIENT_SECRET`, `MS_LISTING_CLIENT_SECRET_DEV`: the `ms-listing` client secret in each realm (`keycloak-setup` syncs them on every start; Vault holds the same values). |
| `vault/.env` | `VAULT_KEYS_FILE`: where the Vault keys file is. The default is `~/Desktop/carsales-vault-keys.json`. |

Any long random string works for the secrets, for example `openssl rand -hex 32`.

### 2. Start

```bash
docker compose -f infrastructure/docker-compose.yml up -d
```

Keycloak imports both realms on its first start. Its admin console is at `http://localhost:8080`.

### 3. Initialize Vault and keep the keys

1. Open `http://localhost:8200`.
2. Set **Key shares** to `5` and **Key threshold** to `3`, then select **Initialize**.
3. Select **Download keys** and save the file as `~/Desktop/carsales-vault-keys.json`. The browser may name it `vault-cluster-vault_<date>.txt`; it is JSON, so only rename it.
4. Unseal:

   ```bash
   docker compose -f infrastructure/docker-compose.yml run --rm vault-unseal
   ```

The keys are shown only once. **If the keys file is lost, the Vault data cannot be opened again.** Keep a copy somewhere other than this machine.

### 4. Configure Vault

Open a shell in the Vault container and sign in with the root token from the keys file (`root_token`):

```bash
docker compose -f infrastructure/docker-compose.yml exec vault sh
export VAULT_TOKEN=<root_token>
```

Inside that shell:

```sh
# Secrets engine and policies
vault secrets enable -path=carsales kv-v2
vault policy write admin /vault/policies/admin.hcl
vault policy write ms-auth /vault/policies/ms-auth.hcl
vault policy write ms-listing /vault/policies/ms-listing.hcl

# Your admin login for the UI and CLI
vault auth enable userpass
vault write auth/userpass/users/<your-name> policies=admin password=<your-password>

# ms-auth signs in with AppRole
vault auth enable approle
vault write auth/approle/role/ms-auth token_policies=ms-auth token_ttl=1h token_max_ttl=4h secret_id_ttl=0
vault read -field=role_id auth/approle/role/ms-auth/role-id
vault write -f -field=secret_id auth/approle/role/ms-auth/secret-id

# ms-auth secrets: the client secrets from keycloak/.env
vault kv put -mount=carsales ms-auth "Keycloak:ClientSecret=<MS_AUTH_CLIENT_SECRET>"
vault kv put -mount=carsales ms-authDev "Keycloak:ClientSecret=<MS_AUTH_CLIENT_SECRET_DEV>"

# ms-listing signs in with AppRole
vault write auth/approle/role/ms-listing token_policies=ms-listing token_ttl=1h token_max_ttl=4h secret_id_ttl=0
vault read -field=role_id auth/approle/role/ms-listing/role-id
vault write -f -field=secret_id auth/approle/role/ms-listing/secret-id

# ms-listing secrets: the full connection string of each environment and the client secrets from keycloak/.env.
# Connection strings use the Npgsql format Host=...;Port=...;Database=...;Username=...;Password=..., with the
# database, role, and password that postgres-setup creates from postgres/.env. Local runs on the host and
# Development in Docker reach the same database by different hosts, so each has its own path.
vault kv put -mount=carsales ms-listing "ConnectionStrings:Default=<Production connection string>" "Keycloak:ClientSecret=<MS_LISTING_CLIENT_SECRET>"
vault kv put -mount=carsales ms-listingDev "ConnectionStrings:Default=<Development connection string>" "Keycloak:ClientSecret=<MS_LISTING_CLIENT_SECRET_DEV>"
vault kv put -mount=carsales ms-listingLocal "ConnectionStrings:Default=<Local connection string>" "Keycloak:ClientSecret=<MS_LISTING_CLIENT_SECRET_DEV>"
```

Put each service's role id and secret id into its `.env` (`services/ms-auth/.env`, `services/ms-listing/.env`):

```
VAULT_ROLE_ID=<role_id>
VAULT_SECRET_ID=<secret_id>
```

Sign in to the UI with **Method: Username** to check your admin login. Then revoke the root token and leave the shell:

```sh
vault token revoke -self
exit
```

From now on you sign in with your username. The `root_token` in the keys file no longer works; the unseal keys still do.

### 5. Admin role

`keycloak-setup` creates the `admin` realm role in both realms. Give it to a user in the console (`http://localhost:8080`, bootstrap admin): realm → **Users** → the user → **Role mapping** → **Assign role** → `admin`. Or with kcadm:

```bash
docker compose -f infrastructure/docker-compose.yml exec keycloak bash -c '
  /opt/keycloak/bin/kcadm.sh config credentials --config /tmp/kcadm.config --server http://localhost:8080 \
    --realm master --user "$KC_BOOTSTRAP_ADMIN_USERNAME" --password "$KC_BOOTSTRAP_ADMIN_PASSWORD"
  /opt/keycloak/bin/kcadm.sh add-roles --config /tmp/kcadm.config -r carsalesDev --uusername <username> --rolename admin'
```

The role is in the user's next login token; tokens issued before keep their old roles until they expire.

## Adding a service to an existing setup

After the root token is revoked, a new service is added with your admin login. Example for `ms-listing` (already included in the first-time steps above):

```bash
docker compose -f infrastructure/docker-compose.yml exec vault vault login -method=userpass username=<your-name>
docker compose -f infrastructure/docker-compose.yml exec vault sh
```

Inside the shell, run the service's lines from section 4 (`policy write`, `approle/role`, `role-id`, `secret-id`, `kv put`), write the ids into the service's `.env`, then end the session with `vault token revoke -self`.

## Emergency: new root token

If every admin login is lost, create a temporary root token from 3 unseal keys (`keys_base64` in the keys file). This works without a token because `vault/config/vault.hcl` allows it.

```bash
docker compose -f infrastructure/docker-compose.yml exec vault sh
```

```sh
vault operator generate-root -init                         # prints Nonce and OTP
vault operator generate-root -nonce=<nonce> <unseal-key-1>
vault operator generate-root -nonce=<nonce> <unseal-key-2>
vault operator generate-root -nonce=<nonce> <unseal-key-3>   # prints Encoded Token
vault operator generate-root -decode=<encoded-token> -otp=<otp>
```

Use the printed token to fix the problem, then revoke it with `VAULT_TOKEN=<token> vault token revoke -self`.

## Moving production to a server

Production data lives on this machine until it moves. Development data (`Dev` names, the `carsalesDev` realm, `ms-authDev` secrets) stays behind.

1. Export the `carsales` realm with its users:

   ```bash
   docker compose -f infrastructure/docker-compose.yml run --rm \
     -v ./infrastructure/keycloak-export:/opt/keycloak/data/export \
     keycloak export --dir /opt/keycloak/data/export --realm carsales --users realm_file
   ```

   The export lands in `infrastructure/keycloak-export/carsales-realm.json`. It contains users and secrets; never commit it.

2. Dump each production service database (names without `Dev`):

   ```bash
   docker compose -f infrastructure/docker-compose.yml exec postgres \
     pg_dump -U postgres -Fc <database> > <database>.dump
   ```

3. On the server: follow the first-time setup, set `KC_HOSTNAME` in `keycloak/.env` to the server's URL, import the realm file instead of `realms/carsales-realm.json`, restore the dumps with `pg_restore`, and write fresh secrets into the server's Vault.
