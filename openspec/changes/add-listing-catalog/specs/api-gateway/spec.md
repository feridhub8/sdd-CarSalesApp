# Spec Delta

## ADDED Requirements

### Requirement: Forward catalog requests unchanged
The gateway SHALL forward every request whose path starts with `/api/v{version}/Brand/`, `/api/v{version}/Model/`, or `/api/v{version}/City/`, or is exactly one of those paths without a trailing segment, to `ms-listing`. It MUST NOT change the method, path, query string, body, or headers, and it MUST return the service's status code, headers, and body unchanged.

#### Scenario: Public catalog read through the gateway
- **WHEN** a client lists brands through the gateway
- **THEN** the client receives the same status code and body that `ms-listing` returns

#### Scenario: Query string reaches the service
- **WHEN** a client lists models with `?brandId=<id>` through the gateway
- **THEN** `ms-listing` receives the same path and query string and returns only that brand's models

#### Scenario: Admin request through the gateway
- **WHEN** an admin creates a city through the gateway with a bearer token
- **THEN** `ms-listing` receives the `Authorization` header and responds with 201 Created
