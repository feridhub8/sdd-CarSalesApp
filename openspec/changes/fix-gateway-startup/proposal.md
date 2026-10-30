# Proposal

## Why

The gateway stops working when it starts before a service it forwards to is reachable. Found while testing `add-listing-catalog`: `ms-listing` waits for its migration container, so its name is not yet in Docker DNS when the gateway starts.

YARP's DNS destination resolver then fails the whole initial configuration load, and the gateway process hangs: it stops answering but does not exit, so Docker never restarts it. Today the only remedy is a manual `docker compose restart api-gateway`.

This breaks the `api-gateway` requirement that the gateway keeps running while a service is unavailable. On a server, any late or crashed service would take down the single entry point.

## What Changes

- **The gateway starts when a service is not reachable yet.** Every cluster whose host does resolve works at once. A cluster whose host does not resolve has no destinations, so its routes answer `503` within 5 seconds.
- **The unresolved host is retried on the same 10-second refresh**, so the cluster starts forwarding within 30 seconds of the service appearing, without restarting the gateway.
- Each failed resolution is logged as a warning that names the host, so a misspelled service name is visible.
- **Docs:** the "Known issue" and its manual restart are removed from the root README. The gateway README states the startup behavior.

## Capabilities

### New Capabilities
(none)

### Modified Capabilities
- `api-gateway`: the "Service unavailable" requirement now also covers a service that is not reachable yet when the gateway starts.

## Impact

- **api-gateway:** a small resolver class that wraps YARP's DNS resolver, and its registration in `ReverseProxyExtensions`. No new packages. No configuration change.
- **Services:** unchanged.
- **Docs:** root `README.md` and `api-gateway/README.md`.
