# Design

## Context

`AuthController` returned `IActionResult` from every action and wrapped bodies in `Ok(...)`. The global rules now require `ActionResult<T>` for actions with a body. See proposal.md.

## Goals / Non-Goals

**Goals:**
- Follow the global rule in ms-auth with no change in HTTP behavior.

**Non-Goals:**
- Any change to routes, status codes, bodies, cookies, headers, or the Swagger document.

## Decisions

- **Return types per action:**
  - `Register` and `Me` return `ActionResult<GetUserOkResponseDto>`.
  - `Login` and `Refresh` return `ActionResult<GetTokenOkResponseDto>`.
  - `Logout` stays `IActionResult`, since 204 has no body.
- **Success values are returned directly**, with no `Ok(...)` wrapper. `ActionResult<T>` turns a `T` into a 200 response. `Register` keeps `StatusCode(201, user)`, because an `ObjectResult` converts to `ActionResult<T>` implicitly.
- **`[ProducesResponseType]` attributes stay.** The global rules require them, and they document the 400, 401, and 409 responses that the return type cannot express.

## Risks / Trade-offs

- [The refactor could silently change the API surface] → The Swagger document is compared before and after the change, and the full scenario script is re-run in both client modes.
