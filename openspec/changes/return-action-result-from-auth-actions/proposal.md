# Proposal

## Why

The global rules now say that actions returning a body return `ActionResult<T>`, and actions without a body return `IActionResult`. `AuthController` still returns `IActionResult` everywhere. With `ActionResult<T>`:
- The response type is part of the signature, so the compiler catches a wrong return type.
- Swagger takes the success schema from the type itself.

## What Changes

- `Register` and `Me` return `ActionResult<GetUserOkResponseDto>`.
- `Login` and `Refresh` return `ActionResult<GetTokenOkResponseDto>`, and return the DTO directly instead of wrapping it in `Ok(...)`.
- `Logout` keeps `IActionResult`, because it answers 204 with no body.
- The `[ProducesResponseType]` attributes stay. They document the 400, 401, and 409 responses, as the global rules require.
- No HTTP behavior changes: the status codes, bodies, cookies, and headers are all the same.

## Capabilities

### New Capabilities
(none)

### Modified Capabilities
(none). The change declares `skip_specs: true`, because no externally visible behavior changes.

## Impact

- `services/ms-auth/src/WebApi/Data/Auth/Controllers/V1/AuthController.cs` only.
- The Swagger document keeps the same paths and schemas.
