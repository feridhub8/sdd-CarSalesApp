# Spec Delta

## MODIFIED Requirements

### Requirement: Service unavailable
When the service that owns a route cannot be reached, the gateway SHALL respond within 5 seconds with 502 Bad Gateway, 503 Service Unavailable, or 504 Gateway Timeout. It SHALL keep running and resume forwarding once the service is back. This SHALL also hold when the service is not reachable yet when the gateway starts: the gateway MUST start, serve every other route, and pick the service up without a restart.

#### Scenario: Service stopped
- **WHEN** `ms-auth` is stopped and a client sends a request to an auth path through the gateway
- **THEN** the gateway responds with 502, 503, or 504 within 5 seconds

#### Scenario: Service comes back
- **WHEN** `ms-auth` is started again after being stopped
- **THEN** within 30 seconds requests through the gateway reach it again, without restarting the gateway

#### Scenario: Service missing when the gateway starts
- **WHEN** the gateway starts while `ms-listing` is not running and `ms-auth` is
- **THEN** the gateway answers `GET /health` with 200 OK, forwards auth requests to `ms-auth`, and answers catalog requests with 503 within 5 seconds
- **AND** within 30 seconds after `ms-listing` starts, catalog requests through the gateway reach it, without restarting the gateway
