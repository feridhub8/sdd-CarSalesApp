# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Keycloak rotation

- [x] 1.1 Add `revokeRefreshToken: true` and `refreshTokenMaxReuse: 0` to both realm files and to `keycloak/setup/realms.sh`; mention rotation in `infrastructure/README.md`; verify after `up -d` that `keycloak-setup` exits 0 and both running realms report `revokeRefreshToken=true` and `refreshTokenMaxReuse=0`

## 2. ms-auth hybrid transport

- [ ] 2.1 Add the `ClientType` enum and the `RefreshTokenCookie` constants class (name, `HttpOnly`, `Secure`, `SameSite=Strict`, `Path=/api/v1/Auth`, `Max-Age` from the token) in `Data/Auth`; verify the build passes
- [ ] 2.2 Make `GetTokenOkResponseDto.RefreshToken` nullable and omitted when null; make the refresh and logout bodies optional; verify the build passes
- [ ] 2.3 Implement the transport in `AuthController` (header parameter, cookie write and delete, cookie-only for web, body-only for mobile, 401 for web without a cookie, with a comment naming the isolation rule); verify the build passes and Swagger shows `X-Client-Type` on Login, Refresh, and Logout
- [ ] 2.4 Verify every row of the design §1 endpoint table in Local, web with a cookie jar and mobile with the header: Login, Refresh, and Logout each in both modes, including no body for web Refresh and Logout, missing token (web 401, mobile 400), invalid token (401, web cookie deleted), a mobile request with only a cookie gives 400, a web request with only a body token gives 401, a reused refresh token gives 401 and its newer token and access token stop working, and the cookie carries `HttpOnly`, `Secure`, `SameSite=Strict`, `Path=/api/v1/Auth`; `Register`, `Me`, and the earlier scenarios still pass
- [ ] 2.5 Update `services/ms-auth/README.md` (both modes per endpoint, the header, cookie attributes, the Safari-on-localhost note, the single-refresh rule for the web frontend, curl examples for both modes); verify the documented commands work as written

## 3. Integration check

- [ ] 3.1 Rebuild the Development and Production containers and run the scenarios for both modes against `6001` (`carsalesDev`) and `7001` (`carsales`); remove the test users afterwards
- [ ] 3.2 Run `graphify update .`; verify it finishes without errors

## Workflow follow-up

- Commit after group 1 (infrastructure) and after groups 2–3 (ms-auth), with the message shown for approval.
- Archive the change with `/opsx:archive` after review.
