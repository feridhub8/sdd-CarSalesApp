# authentication Delta

## ADDED Requirements

### Requirement: Temporary account lock
After 5 wrong passwords in a row, an account MUST refuse sign-in for a period starting at 1 minute and growing with further failures up to 15 minutes, and MUST never lock permanently. A successful sign-in, or 12 hours without a failure, MUST reset the count. Wrong passwords below that count MUST NOT lock the account, however fast they arrive.

#### Scenario: Locked after five wrong passwords
- **WHEN** a client sends 5 wrong passwords for an account and then the correct one
- **THEN** the correct password is refused with 401 Unauthorized

#### Scenario: Lock ends
- **WHEN** the lock period passes and a client sends the correct password
- **THEN** sign-in succeeds

#### Scenario: Below the count
- **WHEN** a client sends 4 wrong passwords in quick succession and then the correct one
- **THEN** sign-in succeeds and the count starts again

### Requirement: A lock reveals nothing
While an account is locked, sign-in MUST respond exactly as for a wrong password, so a client cannot tell a locked account from a wrong password or an unknown account.

#### Scenario: Same answer
- **WHEN** a client signs in to a locked account, to an unlocked account with a wrong password, and to an unknown account
- **THEN** all three responses have the same status and body

### Requirement: A lock leaves open sessions alone
A lock MUST affect only new sign-ins: a session already open MUST keep working and refreshing.

#### Scenario: Refresh while locked
- **WHEN** someone else causes a lock on an account whose owner is signed in
- **THEN** the owner's refresh succeeds and they stay signed in
