# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Tolerant resolver

- [x] 1.1 Check that `DnsDestinationResolver` and `ResolvedDestinationCollection` can be constructed from outside YARP (public constructors), and choose the registration in design §2 (result: the resolver type is internal, so the registration is wrapped); add `ServiceDiscovery/V1/TolerantDnsDestinationResolver.cs` and register it in `ReverseProxyExtensions`; verify the build passes with no warnings
- [x] 1.2 Verify in Local: `localhost` always resolves, so point the `ms-listing` cluster at a host that does not exist (environment override) and start the gateway; `/health` gives 200, an auth request is forwarded, a catalog request gives 503 within 5 seconds, the log warns about the unresolved host every 10 seconds, and nothing crashes; with both services up, the auth and catalog scenarios through the gateway still pass. Recovery when the host appears later is verified in Docker (2.1)

## 2. Docker and docs

- [x] 2.1 Verify in Development and Production from a stopped stack: `up -d --build` brings the gateway up before `ms-listing` finishes migrating, catalog requests succeed within 30 seconds with no manual restart, auth requests work throughout, and with `--scale ms-listing=2` both replicas still receive requests; remove test data and containers afterwards
- [x] 2.2 Remove the "Known issue" block from the root `README.md`, describe the startup behavior in `api-gateway/README.md` (a missing service gives 503 and is picked up within about 10 seconds), and run `graphify update .`; verify the docs match the observed behavior and graphify finishes without errors

## Workflow follow-up

- Commit after group 1 and after group 2, each with the message shown for approval.
- Archive with `/opsx:archive` after review.
