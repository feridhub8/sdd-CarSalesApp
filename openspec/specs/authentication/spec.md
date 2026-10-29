# authentication Specification

## Purpose

Lets people create a CarSales account, sign in with email and password, keep their session alive, sign out, and read their own profile.

## Requirements

### Requirement: User registration
The system SHALL let a person create an account with a username, an email, a password, a first name, and a last name. Username and email MUST each be unique; the username is case-insensitive, 3–30 characters of letters, digits, `.`, `_` or `-`. Passwords MUST be at least 8 characters with an uppercase letter, a lowercase letter, a digit, and a symbol. Registration MUST NOT sign the person in.

#### Scenario: Successful registration
- **WHEN** a client registers with a valid, unused username and email, a password that meets the policy, a first name, and a last name
- **THEN** the system responds with 201 Created and the new user's id, username, email, first name, and last name
- **AND** the response contains no tokens

#### Scenario: Email already registered
- **WHEN** a client registers with an email that already belongs to an account
- **THEN** the system responds with 409 Conflict and no account is created

#### Scenario: Username already taken
- **WHEN** a client registers with a username that already belongs to an account, in any letter case
- **THEN** the system responds with 409 Conflict and no account is created

#### Scenario: Invalid input
- **WHEN** a client registers with a missing or malformed username or email, a missing first or last name, or a password that does not meet the policy
- **THEN** the system responds with 400 Bad Request listing each invalid field and no account is created

#### Scenario: Weak password
- **WHEN** a client registers with a password that is shorter than 8 characters or lacks an uppercase letter, a lowercase letter, a digit, or a symbol
- **THEN** the system responds with 400 Bad Request naming each rule the password breaks

### Requirement: User login
The system SHALL sign in a registered user with a login and a password, where the login is the user's username or email, and return an access token, a refresh token, and the lifetime of each in seconds.

#### Scenario: Valid credentials
- **WHEN** a client logs in with the username and password of an existing account
- **THEN** the system responds with 200 OK, an access token, a refresh token, and their lifetimes

#### Scenario: Login with email
- **WHEN** a client logs in with the email and password of an existing account
- **THEN** the system responds with 200 OK, an access token, a refresh token, and their lifetimes

#### Scenario: Wrong credentials
- **WHEN** a client logs in with an unknown login or a wrong password
- **THEN** the system responds with 401 Unauthorized without saying which of the two was wrong

#### Scenario: Missing fields
- **WHEN** a client logs in without a login or without a password
- **THEN** the system responds with 400 Bad Request

### Requirement: Token refresh
The system SHALL exchange a valid refresh token for a new access token and a new refresh token.

#### Scenario: Valid refresh token
- **WHEN** a client sends a valid, unexpired refresh token
- **THEN** the system responds with 200 OK and a new access token, a new refresh token, and their lifetimes

#### Scenario: Invalid or expired refresh token
- **WHEN** a client sends a refresh token that is malformed, expired, or belongs to a signed-out session
- **THEN** the system responds with 401 Unauthorized

### Requirement: User logout
The system SHALL end the session that a refresh token belongs to, so that the refresh token can no longer be used.

#### Scenario: Successful logout
- **WHEN** a client logs out with a valid refresh token
- **THEN** the system responds with 204 No Content
- **AND** a later refresh with the same refresh token responds with 401 Unauthorized

#### Scenario: Invalid refresh token
- **WHEN** a client logs out with a refresh token that is malformed, expired, or already signed out
- **THEN** the system responds with 401 Unauthorized

### Requirement: Current user profile
The system SHALL return the profile of the user identified by the access token in the request: id, username, email, first name, and last name. The endpoint MUST reject requests without a valid access token.

#### Scenario: Valid access token
- **WHEN** a client requests the current user with a valid, unexpired access token
- **THEN** the system responds with 200 OK and the user's id, username, email, first name, and last name

#### Scenario: Missing or invalid access token
- **WHEN** a client requests the current user without an access token, or with one that is malformed, expired, or issued for another environment
- **THEN** the system responds with 401 Unauthorized

#### Scenario: Access token of a signed-out session
- **WHEN** a client requests the current user with an unexpired access token whose session was ended by logout
- **THEN** the system responds with 401 Unauthorized
