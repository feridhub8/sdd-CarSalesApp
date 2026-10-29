# Tasks

## 1. AuthController

- [x] 1.1 Change `Register` and `Me` to `ActionResult<GetUserOkResponseDto>`, and `Login` and `Refresh` to `ActionResult<GetTokenOkResponseDto>` (returning the DTO directly); keep `Logout` as `IActionResult`; verify `dotnet build` passes with 0 warnings
- [x] 1.2 Compare the Swagger document before and after; verify the paths, parameters, request bodies, and response schemas are unchanged
- [x] 1.3 Run the full endpoint scenario script in Local (both web and mobile modes); verify every scenario still passes

## 2. Wrap-up

- [x] 2.1 Run `graphify update .`; verify it finishes without errors

## Workflow follow-up

- Commit once after the tasks, with the message shown for approval.
- Archive the change with `/opsx:archive` after review.
