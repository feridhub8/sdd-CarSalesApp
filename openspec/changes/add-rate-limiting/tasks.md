# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Account lock

- [x] 1.1 Add the brute force settings to `realms.sh` (design §6) and update `infrastructure/README.md`; verify `keycloak-setup` succeeds twice, both realms show the settings, and existing accounts are unchanged
- [x] 1.2 Verify every scenario of `specs/authentication` through `ms-auth` in Local with test users: lock after 5, correct password refused, lock ends, 4 fast failures do not lock, the three answers are identical byte for byte, and refresh works during a lock; rerun the auth suite; remove test users

## 2. Gateway limits

- [x] 2.1 Add `RateLimitOptions`, `RateLimitExtensions.InitRateLimiting`, `UseRateLimiter`, the settings, the three new routes, and the policies on existing routes (design §3); verify the build has no warnings and existing routes still forward
- [x] 2.2 Verify every scenario of the `api-gateway` delta in Local with the default numbers, including two client IPs (loopback and the LAN address) staying separate, forged `X-Forwarded-For`, `Retry-After` matching the real wait, changed settings, and 500 image and page requests with no 429; rerun the existing gateway scenarios
- [x] 2.3 Update `api-gateway/README.md` (limits, settings, the connection IP rule, Docker Desktop's single IP) and add "moving to a server" notes to `infrastructure/README.md` (real IPs, trusted proxy, closing ports `8080` and `5432`); verify the documented commands work as written

## 3. Website

- [ ] 3.1 Add the rate limit message, its use in every place from design §7, and the sign-in hint; verify with Playwright the `Rate limit messages` and `Hint for a locked account` scenarios at both widths, including that a 429 on renewal does not end the login; update the existing suites and run them with raised limits

## 4. Whole system in Docker

- [ ] 4.1 Run all suites in Development and then Production with raised limits from a scratchpad override, then the real default limits against each stack; verify the override is not committed; remove test data and users, leaving the user's own data, and bring Development back up with its default limits; run `graphify update .`; verify `git status` shows no secrets

## Workflow follow-up

- Commit after each group, with the message shown for approval.
- Archive with `/opsx:archive` (specs are synced automatically).
- Next change to explore: search and filters.
