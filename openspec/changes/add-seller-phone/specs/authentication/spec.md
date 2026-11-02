# authentication Delta

## MODIFIED Requirements

### Requirement: Current user profile
The system SHALL return the profile of the user identified by the access token in the request: id, username, email, first name, last name, and phone, which is null when the account has none. The endpoint MUST reject requests without a valid, active access token.

#### Scenario: Valid access token
- **WHEN** a client requests the current user with a valid, unexpired access token
- **THEN** the system responds with 200 OK and the user's id, username, email, first name, last name, and phone

#### Scenario: Missing or invalid access token
- **WHEN** a client requests the current user without an access token, or with one that is malformed, expired, or issued for another environment
- **THEN** the system responds with 401 Unauthorized

#### Scenario: Access token of a signed-out session
- **WHEN** a client requests the current user with an unexpired access token whose session was ended by logout
- **THEN** the system responds with 401 Unauthorized

## ADDED Requirements

### Requirement: Change own phone
A signed-in user SHALL set or change their own phone number and receive 200 OK with their profile. Spaces, dashes, and brackets MUST be ignored, and a number starting with `0` or `994` MUST be stored as `+994` followed by 9 digits. Anything else MUST give 400 Bad Request naming `phone`. A request without a valid, active access token MUST get 401 Unauthorized.

#### Scenario: Set a number
- **WHEN** a signed-in user sends `050 123-45-67`
- **THEN** the system responds with 200 OK and the profile with phone `+994501234567`

#### Scenario: Accepted forms
- **WHEN** a signed-in user sends `+994501234567`, `994501234567`, or `(050) 123 45 67`
- **THEN** the phone is stored as `+994501234567`

#### Scenario: Rejected numbers
- **WHEN** a signed-in user sends an empty value, letters, too few or too many digits, or a number of another country
- **THEN** the system responds with 400 Bad Request naming `phone` and the stored number does not change

#### Scenario: Rest of the account stays
- **WHEN** a user changes their phone
- **THEN** their username, email, first name, last name, roles, and password stay the same, and they can still sign in

#### Scenario: No token
- **WHEN** a client without a token changes a phone
- **THEN** the system responds with 401 Unauthorized

### Requirement: Seller contact
Anyone SHALL read a user's first name and phone by user id, without signing in, and the response MUST contain nothing else about the account. The phone MUST be null when the account has none. An unknown or disabled user MUST give 404 Not Found.

#### Scenario: Seller with a number
- **WHEN** a client without a token reads the contact of a user who has a phone
- **THEN** the system responds with 200 OK and only the first name and phone

#### Scenario: Seller without a number
- **WHEN** a client reads the contact of a user who has no phone
- **THEN** the system responds with 200 OK, the first name, and a null phone

#### Scenario: Unknown or disabled user
- **WHEN** a client reads the contact of a user id that does not exist or is disabled
- **THEN** the system responds with 404 Not Found

### Requirement: Phone stays out of tokens
The phone MUST NOT appear in access or refresh tokens. Accounts without a phone MUST be able to register, sign in, and refresh as before.

#### Scenario: Token contents
- **WHEN** a user with a phone signs in
- **THEN** the access token does not contain the number

#### Scenario: Existing account
- **WHEN** an account created before phones existed signs in
- **THEN** sign-in succeeds, and its profile shows a null phone
