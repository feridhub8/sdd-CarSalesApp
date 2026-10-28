# CarSales

Car sales platform built as .NET 10 microservices.

## Services

| Service | Folder |
|---|---|
| ms-auth | `services/ms-auth/` |

## Ports

The first digit is the environment, the last digits are the service number.

| Environment | Runs with | Port range | ms-auth |
|---|---|---|---|
| Local | `dotnet run` / IDE | `5xxx` | `5001` |
| Development | Docker, `docker-compose.dev.yml` | `6xxx` | `6001` |
| Production | Docker, `docker-compose.yml` | `7xxx` | `7001` |

## Docker

### Prerequisite: `carsales_network`

Every container joins the external Docker network `carsales_network`. It must exist on every machine that runs the containers, including the Production server. Create it once:

```bash
docker network create carsales_network
```

### Run

From the repository root:

```bash
# Development
docker compose -f docker-compose.dev.yml up -d --build
docker compose -f docker-compose.dev.yml down

# Production
docker compose up -d --build
docker compose down
```

Development and Production are not run at the same time on one machine.
