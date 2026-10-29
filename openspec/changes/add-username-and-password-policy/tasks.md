# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Keycloak realm settings

- [x] 1.1 Update `realms/carsales-realm.json` and `realms/carsalesDev-realm.json`: `registrationEmailAsUsername: false` and the new `passwordPolicy` (design §1); verify both files are valid JSON and differ only in realm name and secret placeholder
- [x] 1.2 Add `infrastructure/keycloak/setup/realms.sh` and the one-shot `keycloak-setup` service (Keycloak image, bootstrap admin from `.env`, waits for Keycloak, updates both realms, `restart: "no"`); verify on the running Keycloak that `up -d` makes `keycloak-setup` exit 0, both realms report `registrationEmailAsUsername=false` and the new policy, and a second `up -d` exits 0 again
- [x] 1.3 Verify a fresh import is correct too: in a throwaway Keycloak with the realm files mounted, both realms come up with the new settings
- [x] 1.4 Update `infrastructure/README.md` (what `keycloak-setup` does, that it owns these realm keys, the bootstrap admin dependency); verify `docker compose config --services` lists `keycloak-setup`

## 2. ms-auth

- [ ] 2.1 Add `Username` to `RegisterUserDto`, `GetUserOkResponseDto`, and the register mapping; add the username and password rules to `RegisterUserValidator` (design §2–3); verify the build passes and `TypeAdapterConfig.Compile()` succeeds at startup
- [ ] 2.2 Rename `Email` to `Login` in `LoginUserDto`, its validator, `ITokenService`, and `ITokenRepository`; verify the build passes
- [ ] 2.3 Translate the Keycloak 409 messages (username / email) and the `400 invalidPassword…` backstop in `UserKeycloakRepository` (design §3–4); verify the build passes
- [ ] 2.4 Verify the spec scenarios in Local: 201 with username, 409 for a taken username in another letter case, 409 for a taken email, 400 for each weak-password rule with one message per rule, 400 for a bad username (too short, `@`, space), login 200 with the username and with the email, 401 for wrong credentials, `Me` returns the username; the earlier refresh, logout, and `Me` 401 scenarios still pass
- [ ] 2.5 Update `services/ms-auth/README.md` (register body with `username` and the password rules, login body with `login`); verify the documented curl commands work as written

## 3. Integration check

- [ ] 3.1 Rebuild the Development and Production containers and run the scenarios against `6001` (`carsalesDev`) and `7001` (`carsales`); remove the test users afterwards
- [ ] 3.2 Run `graphify update .`; verify it finishes without errors

## Workflow follow-up

- Commit after group 1 (infrastructure) and after groups 2–3 (ms-auth), with the message shown for approval.
- Archive the change with `/opsx:archive` after review.
