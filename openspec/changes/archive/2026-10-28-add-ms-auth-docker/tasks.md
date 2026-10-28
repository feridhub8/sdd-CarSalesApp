# Tasks

## 1. Prerequisites

- [x] 1.1 Check whether the `carsales_network` Docker network exists, create it with `docker network create carsales_network` if it is missing, and verify `docker network inspect carsales_network` succeeds

## 2. Image

- [x] 2.1 Add root `.dockerignore` (`**/bin/`, `**/obj/`, `.git/`, `.vs/`, `.vscode/`, `.idea/`, `**/.env`, `graphify-out/`, `openspec/`, `.claude/`, `**/.DS_Store`) and verify the build context in 2.2 contains no `bin/` or `obj/` folders
- [x] 2.2 Add `services/ms-auth/Dockerfile` (`sdk:10.0` build stage that copies `global.json` and `WebApi.csproj`, restores, copies sources, publishes; `aspnet:10.0` final stage with `USER $APP_UID`, `EXPOSE 8080`) and verify `docker build -f services/ms-auth/Dockerfile .` succeeds from the repository root
- [x] 2.3 Run the built image and verify `docker exec <container> whoami` prints `app` and the log shows `Now listening on: http://[::]:8080`; then remove the container

## 3. Service compose files

- [x] 3.1 Add `services/ms-auth/docker-compose.yml` (`name: carsales`, no `container_name`, `7001:8080`, `ASPNETCORE_ENVIRONMENT=Production`, external `carsales_network`) and verify `docker compose up -d --build` in `services/ms-auth/` starts `carsales-ms-auth-1`, its log shows `Hosting environment: Production`, and `curl -s -o /dev/null -w '%{http_code}' http://localhost:7001` returns `404`; then run `docker compose down`
- [x] 3.2 Add `services/ms-auth/docker-compose.dev.yml` (`name: carsales`, no `container_name`, `6001:8080`, `ASPNETCORE_ENVIRONMENT=Development`, external `carsales_network`) and verify `docker compose -f docker-compose.dev.yml up -d --build` shows `Hosting environment: Development` and port `6001` returns `404`; then bring it down

## 4. Root compose files

- [x] 4.1 Add root `docker-compose.yml` (`name: carsales`, `include` of `services/ms-auth/docker-compose.yml`) and verify `docker compose up -d --build` from the root starts `carsales-ms-auth-1` in `Production` on port `7001`; then bring it down
- [x] 4.2 Add root `docker-compose.dev.yml` (`name: carsales`, `include` of `services/ms-auth/docker-compose.dev.yml`) and verify `docker compose -f docker-compose.dev.yml up -d --build` from the root starts `carsales-ms-auth-1` in `Development` on port `6001`; then bring it down
- [x] 4.3 Start `Development` from the root, then run `docker compose -f docker-compose.dev.yml up -d` in `services/ms-auth/`, and verify the second command reuses the running container instead of creating a second one; then bring it down
- [x] 4.4 Start `Development` from the root and `dotnet run` for `ms-auth` at the same time, and verify ports `6001` and `5001` both answer; then stop both

## 5. Documentation

- [x] 5.1 Add root `README.md` with the `carsales_network` prerequisite (developer machine and Production server) and `docker network create carsales_network`, the port scheme (`5xxx` Local, `6xxx` Development, `7xxx` Production), and the root commands for `Production` and `Development`, and verify each documented command runs as written
- [x] 5.2 Add a Docker section to `services/ms-auth/README.md` with the service-level compose commands and ports `6001` / `7001`, and verify each documented command runs as written

## 6. Verification

- [x] 6.1 Verify `dotnet build CarSales.slnx` still succeeds with 0 warnings and no containers from this change are left running (`docker ps --filter name=carsales-`)

## Workflow follow-up

- Review the result, commit it with approval, update `sdd-global-rules` (repository-root build context, root `.dockerignore` with optional `Dockerfile.dockerignore` override, root `docker-compose.dev.yml`, external network check and README note, no fixed container names, host port scheme), then archive the change with `/opsx:archive`.
