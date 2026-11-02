# api-gateway Delta

## ADDED Requirements

### Requirement: Request limits per client
The gateway SHALL limit API requests per client IP, by default: sign-in 10 per minute, registration 5 per 10 minutes, seller contact 30 per minute, and every other API path 300 per minute. A request over a limit MUST get 429 Too Many Requests with `Retry-After` and MUST NOT reach any service. The limits MUST come from settings.

#### Scenario: Over the sign-in limit
- **WHEN** one client sends 11 sign-in requests within a minute
- **THEN** the first 10 reach `ms-auth`, and the 11th gets 429 with a `Retry-After` header

#### Scenario: Limit applies to correct passwords too
- **WHEN** a client over the sign-in limit sends a correct password
- **THEN** the gateway responds with 429

#### Scenario: Window passes
- **WHEN** a client got 429 and waits for the time in `Retry-After`
- **THEN** its next request is forwarded

#### Scenario: Limits are separate
- **WHEN** a client is over the sign-in limit
- **THEN** its listing requests still work, up to their own limit

#### Scenario: Clients are separate
- **WHEN** one client IP is over a limit and another client IP sends a request
- **THEN** the other client's request is forwarded

#### Scenario: Registration and seller contact
- **WHEN** a client exceeds 5 registrations in 10 minutes, or 30 seller contact requests in a minute
- **THEN** the gateway responds with 429

#### Scenario: Changed in settings
- **WHEN** a limit is changed in the gateway's settings and the gateway restarts
- **THEN** the new number applies

### Requirement: Client IP cannot be faked
The gateway MUST count requests by the IP of the connection it receives, and MUST ignore client-supplied headers such as `X-Forwarded-For` when counting.

#### Scenario: Forged header
- **WHEN** a client over a limit sends a different `X-Forwarded-For` on each request
- **THEN** the gateway still responds with 429

### Requirement: Website and files are not limited
Requests for the website and for image files MUST NOT count toward any limit and MUST NOT get 429. The gateway health check MUST NOT be limited either.

#### Scenario: Pages and images while over a limit
- **WHEN** a client over the API limits requests a page, a script, an image file, or `/health`
- **THEN** the gateway answers as usual

#### Scenario: Many files at once
- **WHEN** a client requests 500 image files within a minute
- **THEN** none gets 429
