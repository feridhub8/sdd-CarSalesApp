# api-gateway Delta

## ADDED Requirements

### Requirement: Forward listing image requests unchanged
The gateway SHALL forward every request whose path is `/api/v{version}/ListingImage` or starts with `/api/v{version}/ListingImage/` to `ms-listing`, without changing the method, path, query string, body, or headers, and return the service's response unchanged.

#### Scenario: Upload through the gateway
- **WHEN** the owner uploads an image through the gateway with a bearer token
- **THEN** `ms-listing` receives the file and the `Authorization` header and responds with 201 Created

### Requirement: Read-only image files
The gateway SHALL forward `GET` and `HEAD` requests under the environment's image path to file storage, without a token: `/listing-images/` in Production, and `/listing-images-dev/` in Development and Local. Any other method on these paths MUST get 405 Method Not Allowed without contacting storage. File storage MUST NOT be reachable from outside the machine except through the gateway.

#### Scenario: Read a file through the gateway
- **WHEN** a client without a token sends `GET` to an image's `url` through the gateway
- **THEN** the client receives the file with the type `image/webp`

#### Scenario: Write method
- **WHEN** a client sends `PUT` or `DELETE` to a path under the image path through the gateway
- **THEN** the gateway responds with 405 Method Not Allowed

#### Scenario: Another environment's path
- **WHEN** a client requests `/listing-images/<file>` through the Development gateway
- **THEN** the gateway responds with 404 Not Found

#### Scenario: Storage stopped
- **WHEN** file storage is stopped and a client requests an image through the gateway
- **THEN** the gateway responds with 502, 503, or 504 within 5 seconds
