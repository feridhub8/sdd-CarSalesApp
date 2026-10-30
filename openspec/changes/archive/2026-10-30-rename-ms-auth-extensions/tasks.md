# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. Rename and split

- [x] 1.1 Rename `SecretExtensions`/`InitSecretsAsync` to `VaultExtensions`/`InitVaultAsync` and `ExceptionHandlingMiddleware` to `GlobalExceptionHandler` (files with `git mv`); add `RepositoryExtensions.InitRepositories` (repositories, singletons) and `ServiceExtensions.InitServices` (services, scoped), trim `KeycloakExtensions` to options and the named `HttpClient`, and update `Program.cs`; verify `dotnet build CarSales.slnx` passes with no warnings and no old name remains in `services/ms-auth`
- [x] 1.2 Verify behavior is unchanged: run the auth scenarios against `ms-auth` in Local (register, login with username and email, web and mobile refresh and logout, rotation, `Me`, wrong credentials, other-realm token) and through the gateway; all pass; remove the test users afterwards; run `graphify update .`

## Workflow follow-up

- Commit after group 1 with the message shown for approval.
- Archive with `/opsx:archive` after review.
