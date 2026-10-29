# authentication Specification

## Purpose

Lets people create a CarSales account, sign in with email and password, keep their session alive, sign out, and read their own profile.

## Requirements

### Requirement: User registration
The system SHALL let a person create an account with an email, a password, a first name, and a last name. The email is the account's sign-in name and MUST be unique. Passwords MUST be at least 8 characters. Registration MUST NOT sign the person in.

#### Scenario: Successful registration
- **WHEN** a client registers with a valid, unused email, a password of at least 8 characters, a first name, and a last name
- **THEN** the system responds with 201 Created and the new user's id, email, first name, and last name
- **AND** the response contains no tokens

#### Scenario: Email already registered
- **WHEN** a client registers with an email that already belongs to an account
- **THEN** the system responds with 409 Conflict and no account is created

#### Scenario: Invalid input
- **WHEN** a client registers with a missing or malformed email, a password shorter than 8 characters, or a missing first or last name
- **THEN** the system responds with 400 Bad Request listing each invalid field and no account is created

### Requirement: User login
The system SHALL sign in a registered user with email and password and return an access token, a refresh token, and the lifetime of each in seconds.

#### Scenario: Valid credentials
- **WHEN** a client logs in with the email and password of an existing account
- **THEN** the system responds with 200 OK, an access token, a refresh token, and their lifetimes

#### Scenario: Wrong credentials
- **WHEN** a client logs in with an unknown email or a wrong password
- **THEN** the system responds with 401 Unauthorized without saying which of the two was wrong

#### Scenario: Missing fields
- **WHEN** a client logs in without an email or without a password
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
The system SHALL return the profile of the user identified by the access token in the request: id, email, first name, and last name. The endpoint MUST reject requests without a valid access token.

#### Scenario: Valid access token
- **WHEN** a client requests the current user with a valid, unexpired access token
- **THEN** the system responds with 200 OK and the user's id, email, first name, and last name

#### Scenario: Missing or invalid access token
- **WHEN** a client requests the current user without an access token, or with one that is malformed, expired, or issued for another environment
- **THEN** the system responds with 401 Unauthorized

#### Scenario: Access token of a signed-out session
- **WHEN** a client requests the current user with an unexpired access token whose session was ended by logout
- **THEN** the system responds with 401 Unauthorized
