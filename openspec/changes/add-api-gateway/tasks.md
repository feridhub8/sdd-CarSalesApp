# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Rules and project context

- [x] 1.1 In `~/Desktop/sdd-global-rules/microservices.md`, replace the Docker host port wording so only `api-gateway` publishes a host port in Development and Production (service number `0`) and Local keeps per-service ports; add the `api-gateway` layout rule under Repositories (service root layout, `<Name>` `ApiGateway`, no `Data/`, counts as a service in the solution, root compose files, and `.vscode`); verify the two edits read correctly and nothing else in the file changed (`git diff`)
- [x] 1.2 Add `docker-compose.dev.yml` to the repository list in `openspec/config.yaml`; verify `openspec validate add-api-gateway` still passes

## 2. Gateway project (Local)

- [x] 2.1 Create `api-gateway/src/WebApi/WebApi.csproj` (net10.0, Nullable, ImplicitUsings, TreatWarningsAsErrors, `Yarp.ReverseProxy` 2.3.0), `ApiGateway.slnx`, and add the project to `CarSales.slnx` under `/api-gateway/`; verify `dotnet build CarSales.slnx` passes
- [x] 2.2 Add `Extensions/V1/ReverseProxyExtensions` (config-loaded proxy, DNS destination resolver with a 10-second refresh and IPv4 only), `Extensions/V1/HealthCheckExtensions` (`/health`), and a short `Program.cs`; add `appsettings.json` (route `/api/v{version}/Auth/{**rest}` → cluster `ms-auth`, RoundRobin, destination `http://ms-auth:8080`), `appsettings.Development.json`, `appsettings.Local.json` (destination `http://localhost:5001`), and `Properties/launchSettings.json` (`http://localhost:5000`, Local, no browser); verify the build passes
- [x] 2.3 Add `build api-gateway` to `.vscode/tasks.json`, `api-gateway (Local)` to `.vscode/launch.json`, and the gateway to the "All services (Local)" compound; verify the compound starts both apps
- [x] 2.4 Verify in Local with `ms-auth` on `5001` and the gateway on `5000`: through `5000`, web and mobile Login, Refresh (cookie keeps `Path=/api/v1/Auth` and its attributes), Me with a bearer token, and Logout behave as when calling `5001` directly; a path is forwarded with its query unchanged; `/api/v1/Unknown` gives 404; with `ms-auth` stopped, an auth path gives 502 or 503 and `/health` gives 200; after `ms-auth` restarts, requests succeed again within 30 seconds without restarting the gateway; remove the test users afterwards
- [x] 2.5 Write `api-gateway/README.md` with the purpose, the route table, how to add a service route and cluster, the Local run, and the macOS AirPlay note for `5000`/`7000`; verify the documented Local commands work as written

## 3. Docker

- [ ] 3.1 Add `api-gateway/Dockerfile` (repository root as context, like `ms-auth`) and `docker-compose.yml` / `docker-compose.dev.yml` (`name: carsales`, `7000:8080` / `6000:8080`, environment, `carsales_network`, no `depends_on`); include both in the root compose files; verify `docker compose config` succeeds for the root and gateway compose files
- [ ] 3.2 Remove `ports` from the `ms-auth` Production compose file; in the Development compose file, comment it out with a one-line note that uncommenting it gives temporary direct access (Swagger included) and must not be committed; verify `docker compose config` shows no published port for `ms-auth` in either file, and that uncommenting the block makes Swagger reachable on `6001`
- [ ] 3.3 Verify Development (`docker-compose.dev.yml`) and then Production (`docker-compose.yml`): `6001` / `7001` refuse connections; through `6000` / `7000` the web and mobile flows, Me, and Logout pass against `carsalesDev` / `carsales`; `/api/v1/Unknown` gives 404; `/health` gives 200; remove the test users afterwards
- [ ] 3.4 Verify scaling in Production: with `--scale ms-auth=3`, the gateway logs show consecutive requests going to different replica IPs; after scaling to 1 and back to 3, within 30 seconds the gateway uses only the running replicas; with `ms-auth` stopped, an auth path gives 502 or 503 and the gateway recovers when it starts again
- [ ] 3.5 Update the docs for Docker: the gateway README Docker section; the root `README.md` (gateway in the services table, ports table with the gateway row and no Docker ports for services, Docker section pointing clients at `6000` / `7000`); the `ms-auth` README Docker section, a note that Docker access goes through the gateway, and how to enable the commented Development port temporarily; verify the documented commands work as written

## 4. Integration check

- [ ] 4.1 Run `graphify update .`; verify it finishes without errors

## Workflow follow-up

- Commit group 1 in `sdd-global-rules` and in CarSales, then commit after group 2 and after groups 3–4, each with the message shown for approval.
- Archive the change with `/opsx:archive` after review.
