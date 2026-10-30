# Spec Delta

## ADDED Requirements

### Requirement: Forward listing requests unchanged
The gateway SHALL forward every request whose path is `/api/v{version}/Listing` or starts with `/api/v{version}/Listing/` to `ms-listing`, without changing the method, path, query string, body, or headers, and return the service's response unchanged.

#### Scenario: Paged list through the gateway
- **WHEN** a client lists listings with `?page=2&pageSize=10` through the gateway
- **THEN** `ms-listing` receives the same path and query string and the client receives its response unchanged

#### Scenario: Owner request through the gateway
- **WHEN** a signed-in user creates a listing through the gateway with a bearer token
- **THEN** `ms-listing` receives the `Authorization` header and responds with 201 Created
