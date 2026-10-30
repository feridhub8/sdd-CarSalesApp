# CarSales

Car sales platform built as .NET 10 microservices.

## Services

| Service | Folder |
|---|---|
| api-gateway | `api-gateway/` |
| ms-auth | `services/ms-auth/` |
| ms-listing | `services/ms-listing/` |

Clients use only `api-gateway`. It forwards each request to the service that owns it (see [`api-gateway/README.md`](api-gateway/README.md)).

## Infrastructure

Shared by every environment. Setup, daily use, and backups are in [`infrastructure/README.md`](infrastructure/README.md).

| Tool | Host port |
|---|---|
| PostgreSQL | `5432` |
| Keycloak | `8080` |
| Vault | `8200` |

Vault is sealed after every start. Services cannot read their secrets until it is unsealed:

```bash
docker compose -f infrastructure/docker-compose.yml run --rm vault-unseal
```

Local runs (`dotnet run` / IDE) need only the infrastructure:

```bash
docker compose -f infrastructure/docker-compose.yml up -d
docker compose -f infrastructure/docker-compose.yml run --rm vault-unseal
```

## Ports

The first digit is the environment, the last digits are the service number. `api-gateway` is number `80`.

| Environment | Runs with | api-gateway | ms-auth | ms-listing |
|---|---|---|---|---|
| Local | `dotnet run` / IDE | `5080` | `5001` | `5002` |
| Development | Docker, `docker-compose.dev.yml` | `6080` | no host port | no host port |
| Production | Docker, `docker-compose.yml` | `7080` | no host port | no host port |

In Docker, services publish no host port: clients reach them only through the gateway, and services can be scaled with `--scale`. Local runs reach every service directly as well.

## Docker

### Prerequisite: `carsales_network`

Every container joins the external Docker network `carsales_network`. It must exist on every machine that runs the containers, including the Production server. Create it once:

```bash
docker network create carsales_network
```

### Run

From the repository root. These start the infrastructure, the gateway, and the services together; unseal Vault after `up`. Clients then use `http://localhost:6080` (Development) or `http://localhost:7080` (Production):

```bash
# Development
docker compose -f docker-compose.dev.yml up -d --build
docker compose -f docker-compose.dev.yml run --rm vault-unseal
docker compose -f docker-compose.dev.yml down

# Production
docker compose up -d --build
docker compose run --rm vault-unseal
docker compose down
```

Development and Production are not run at the same time on one machine. `down` also stops the infrastructure; its data is kept.

A service with a database applies its migrations first: a one-shot `<service>-migrate` container (for example `ms-listing-migrate`) runs and exits, and the service starts only after it succeeds. Locally, run `dotnet run --project src/WebApi -- --migrate` in the service folder (see its README).
