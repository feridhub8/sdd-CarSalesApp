# Proposal

## Why

Nothing limits how fast clients call the system. Passwords can be guessed without limit, since Keycloak's brute force detection is off, fake accounts can be created in bulk, and the public seller endpoint lets a bot collect phone numbers. These must be in place before the system faces the internet.

## What Changes

- **Gateway:** limits per client IP, returning `429 Too Many Requests` with `Retry-After`: sign-in 10 per minute, registration 5 per 10 minutes, seller contact 30 per minute, all other API requests 300 per minute. The website and image files have no limit. The numbers are in settings.
- **Keycloak:** both realms lock an account for a short time after 5 wrong passwords in a row, with the wait growing up to 15 minutes. Never permanently. A locked account answers exactly like a wrong password, and sessions already open keep working.
- **Website:** a `429` shows its own Azerbaijani message, and the wrong-password message adds a hint to wait after several tries.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `api-gateway`: per-IP request limits.
- `authentication`: temporary account lock after repeated wrong passwords.
- `web-frontend`: messages for rate limits and for a possibly locked account.

## Impact

- **`api-gateway`:** rate limiter setup, settings, new routes for sign-in, registration, and seller contact; `README.md`.
- **`infrastructure/keycloak/setup/realms.sh`:** brute force settings; `infrastructure/README.md`.
- **`web/`:** error handling for `429`, and the sign-in message.
- **No change:** `ms-auth`, `ms-listing`, and the API's successful responses.
- **Out of scope, kept for later:** shared counters across gateway replicas (Redis), trusted proxy headers for a load balancer in front of the gateway, and CAPTCHA.
