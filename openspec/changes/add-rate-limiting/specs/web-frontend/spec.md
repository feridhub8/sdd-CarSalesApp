# web-frontend Delta

## ADDED Requirements

### Requirement: Rate limit messages
When an API answers 429, the site MUST show a message in Azerbaijani asking the person to try again shortly, distinct from the service-unavailable message, and MUST keep what they typed. A 429 MUST NOT sign the person out.

#### Scenario: Sign-in over the limit
- **WHEN** a person signs in and the gateway answers 429
- **THEN** the rate limit message appears, not the wrong-password message

#### Scenario: Saving a form over the limit
- **WHEN** saving a listing or registering gets 429
- **THEN** the rate limit message appears and the form keeps its values

#### Scenario: Page data over the limit
- **WHEN** loading a page's data gets 429
- **THEN** the rate limit message and a way to try again appear

#### Scenario: Seller contact and uploads over the limit
- **WHEN** the show-number control or an image upload gets 429
- **THEN** the rate limit message appears there, and the rest of the page stays

#### Scenario: Session renewal over the limit
- **WHEN** a signed-in person's session renewal gets 429
- **THEN** the login is not ended, and a later reload signs them back in

### Requirement: Hint for a locked account
The sign-in page's message for refused credentials SHALL also say to wait a little after several wrong tries, since a locked account answers like a wrong password.

#### Scenario: Refused credentials
- **WHEN** sign-in is refused
- **THEN** the single general message includes the hint, and does not say whether the account is locked
