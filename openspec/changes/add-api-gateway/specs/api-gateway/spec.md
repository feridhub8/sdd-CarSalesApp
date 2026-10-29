# Spec Delta

## Purpose

Gives clients one address for every CarSales service: the gateway forwards each request to the service that owns it and spreads load across that service's replicas.

## ADDED Requirements

### Requirement: Single entry point
In Development and Production, clients SHALL reach microservices only through the gateway, on port `6000` and `7000` respectively. Microservices MUST NOT be reachable from the host there. In Local, the gateway SHALL listen on `5000`, and services SHALL stay reachable directly on their own ports.

#### Scenario: Service port closed in Docker
- **WHEN** the Development or Production stack is running and a client connects to `ms-auth` on a host port (`6001` or `7001`)
- **THEN** the connection is refused
- **AND** the same request sent to the gateway (`6000` or `7000`) reaches `ms-auth`

#### Scenario: Local runs reach services directly
- **WHEN** the gateway and `ms-auth` run locally
- **THEN** a request to the gateway on `5000` reaches `ms-auth`
- **AND** a request to `ms-auth` directly on `5001` also succeeds

### Requirement: Forward auth requests unchanged
The gateway SHALL forward every request whose path starts with `/api/v{version}/Auth/` to `ms-auth`. It MUST NOT change the method, path, query string, body, `Authorization`, `Cookie`, or `X-Client-Type` headers, and it MUST return the service's status code, headers (including `Set-Cookie`), and body unchanged.

#### Scenario: Login through the gateway
- **WHEN** a client sends a valid `POST /api/v1/Auth/Login` to the gateway
- **THEN** the client receives the same status code and body that `ms-auth` returns

#### Scenario: Refresh token cookie passes both ways
- **WHEN** a web client logs in through the gateway and then calls `POST /api/v1/Auth/Refresh` through the gateway with the cookie it received
- **THEN** the login response carries the `refresh_token` cookie with `Path=/api/v1/Auth` and its other attributes unchanged
- **AND** the refresh succeeds with 200 OK and a new cookie

#### Scenario: Headers reach the service
- **WHEN** a client calls `GET /api/v1/Auth/Me` with a bearer token, or a mobile client calls Login with `X-Client-Type: mobile`, through the gateway
- **THEN** `ms-auth` receives the header and responds as it does when called directly

#### Scenario: Path and query are not rewritten
- **WHEN** a client sends a request to `/api/v1/Auth/<anything>?<query>` through the gateway
- **THEN** `ms-auth` receives exactly `/api/v1/Auth/<anything>?<query>`

### Requirement: Unknown paths
The gateway SHALL respond with 404 Not Found to any request whose path matches no service route, without contacting any service.

#### Scenario: No matching route
- **WHEN** a client sends a request to `/api/v1/Unknown` through the gateway
- **THEN** the gateway responds with 404 Not Found

### Requirement: Load across replicas
When a service runs several replicas, the gateway SHALL spread requests across all of them. It SHALL pick up replicas that are added or removed within 30 seconds, without a restart.

#### Scenario: Scaled service
- **WHEN** `ms-auth` runs 3 replicas and a client sends several requests through the gateway
- **THEN** the requests reach more than one replica

#### Scenario: Replica count changes
- **WHEN** `ms-auth` is scaled from 1 to 3 replicas, or from 3 back to 1, while the gateway runs
- **THEN** within 30 seconds the gateway sends requests to every running replica and to none that were removed

### Requirement: Service unavailable
When the service that owns a route cannot be reached, the gateway SHALL respond with 502 Bad Gateway or 503 Service Unavailable instead of waiting indefinitely. It SHALL keep running and resume forwarding once the service is back.

#### Scenario: Service stopped
- **WHEN** `ms-auth` is stopped and a client sends a request to an auth path through the gateway
- **THEN** the gateway responds with 502 or 503

#### Scenario: Service comes back
- **WHEN** `ms-auth` is started again after being stopped
- **THEN** within 30 seconds requests through the gateway reach it again, without restarting the gateway

### Requirement: Gateway health check
The gateway SHALL answer `GET /health` with 200 OK whenever the gateway itself is running, regardless of the state of the services behind it.

#### Scenario: Health with service down
- **WHEN** `ms-auth` is stopped and a client calls `GET /health` on the gateway
- **THEN** the gateway responds with 200 OK
