# api-gateway Delta

## MODIFIED Requirements

### Requirement: Unknown paths
The gateway SHALL respond with 404 Not Found to any request under `/api/` whose path matches no service route, without contacting any service. Paths outside `/api/` that match no other route belong to the website.

#### Scenario: No matching route
- **WHEN** a client sends a request to `/api/v1/Unknown` through the gateway
- **THEN** the gateway responds with 404 Not Found

#### Scenario: Any method under the API
- **WHEN** a client sends `POST` to `/api/v1/Unknown` or `GET` to `/api/`
- **THEN** the gateway responds with 404 Not Found and the website is not contacted

## ADDED Requirements

### Requirement: Serve the website
The gateway SHALL forward `GET` and `HEAD` requests on paths that match no other route to the website, and SHALL respond to other methods on those paths with 405 Method Not Allowed. Page addresses MUST return the site's page, while a missing file such as an image or script MUST return 404 Not Found.

#### Scenario: Home page
- **WHEN** a client requests `/` through the gateway
- **THEN** the website's page is returned with 200 OK

#### Scenario: Page address
- **WHEN** a client requests `/listings/<id>` through the gateway
- **THEN** the website's page is returned with 200 OK

#### Scenario: Missing file
- **WHEN** a client requests a path ending in a file name that does not exist, such as `/listing-images/<id>/large.webp` through the Development gateway
- **THEN** the response is 404 Not Found

#### Scenario: Write method
- **WHEN** a client sends `POST` to `/listings`
- **THEN** the gateway responds with 405 Method Not Allowed

#### Scenario: Health stays with the gateway
- **WHEN** the website is stopped and a client calls `GET /health`
- **THEN** the gateway responds with 200 OK

#### Scenario: Website stopped
- **WHEN** the website is stopped and a client requests `/`
- **THEN** the gateway responds with 502, 503, or 504 within 5 seconds, and API requests keep working

#### Scenario: Website port
- **WHEN** the Development or Production stack is running
- **THEN** the website has no host port of its own and is reached only through the gateway
