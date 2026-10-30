# listings Specification

## Purpose

Lets signed-in users publish their cars for sale, anyone browse the listings, owners manage their own listings, and admins remove and restore any listing.

## Requirements

### Requirement: Public listing reads
The system SHALL let anyone, without signing in, list non-deleted listings and read one non-deleted listing by id. Each listing SHALL be returned with its fields, its model's id and name, its brand's id and name, its city's id and name, its owner's id, and when it was created and last updated.

#### Scenario: Read a listing
- **WHEN** a client without a token reads a non-deleted listing by id
- **THEN** the system responds with 200 OK and the listing with its brand, model, and city names and its owner id

#### Scenario: Deleted or unknown listing
- **WHEN** a client reads a listing by an id that is deleted or does not exist
- **THEN** the system responds with 404 Not Found

### Requirement: Paged lists
Listing lists SHALL be paged with `page` (1-based, default 1, at most 500) and `pageSize` (default 20, 1 to 100), sorted newest first, and SHALL return `items`, `page`, `pageSize`, and `totalCount`. Values outside these ranges MUST give 400 Bad Request naming the parameter.

#### Scenario: First page
- **WHEN** a client lists listings without paging parameters while 25 non-deleted listings exist
- **THEN** the system responds with 200 OK, 20 items newest first, `page` 1, `pageSize` 20, and `totalCount` 25

#### Scenario: Second page
- **WHEN** a client lists with `page=2&pageSize=20` while 25 non-deleted listings exist
- **THEN** the response has the 5 oldest listings and `totalCount` 25

#### Scenario: Out of range
- **WHEN** a client lists with `pageSize=101`, `pageSize=0`, `page=0`, or `page=501`
- **THEN** the system responds with 400 Bad Request naming the parameter

### Requirement: Create a listing
A signed-in user SHALL create a listing and become its owner; the owner MUST be the user of the access token, whatever the body contains. Creating SHALL respond with 201 Created and the listing. A request without a valid, active token MUST get 401 Unauthorized.

#### Scenario: Create
- **WHEN** a signed-in user creates a listing with valid fields
- **THEN** the system responds with 201 Created and the listing, whose owner id is the user's id

#### Scenario: No token
- **WHEN** a client without a token creates a listing
- **THEN** the system responds with 401 Unauthorized and no listing is created

### Requirement: Listing references
A listing MUST reference a non-deleted model and a non-deleted city. A model or city that does not exist or is deleted MUST give 400 Bad Request naming `modelId` or `cityId`.

#### Scenario: Missing or deleted model or city
- **WHEN** a user creates or updates a listing with a model or city that does not exist or is deleted
- **THEN** the system responds with 400 Bad Request naming `modelId` or `cityId`

### Requirement: Listing field values
Year MUST be from 1900 to next year; mileage from 0 to 2,000,000 with unit `Km` or `Mi`; horsepower from 1 to 2,000; seats from 1 to 60; price from 1 to 100,000,000 with currency `AZN`, `USD`, or `EUR`; description at most 3,000 characters. Body type, fuel type, transmission, drive type, color, and market MUST come from their fixed lists. Each broken rule MUST give 400 Bad Request naming its field.

#### Scenario: Out-of-range values
- **WHEN** a user creates a listing with year 1899, mileage -1, horsepower 0, seats 61, or price 0
- **THEN** the system responds with 400 Bad Request naming each such field

#### Scenario: Unknown enum value
- **WHEN** a user creates a listing with `fuelType` set to a value outside its list
- **THEN** the system responds with 400 Bad Request

### Requirement: Engine volume and VIN
Engine volume, in litres with one decimal, MUST be empty for electric cars and from 0.1 to 10.0 for all other fuel types. A VIN is optional; when given it MUST be 17 letters and digits without `I`, `O`, or `Q`. A broken rule MUST give 400 Bad Request naming `engineVolume` or `vin`.

#### Scenario: Engine volume of an electric car
- **WHEN** a user creates a listing with fuel type `Electric` and an engine volume
- **THEN** the system responds with 400 Bad Request naming `engineVolume`

#### Scenario: Missing engine volume
- **WHEN** a user creates a listing with fuel type `Petrol` and no engine volume
- **THEN** the system responds with 400 Bad Request naming `engineVolume`

#### Scenario: Invalid VIN
- **WHEN** a user creates a listing with a VIN of 16 characters or one containing `O`
- **THEN** the system responds with 400 Bad Request naming `vin`

### Requirement: Owner updates and deletes
The owner SHALL update all fields of their own non-deleted listing (200 OK with the listing) and delete it (204 No Content); deleting marks it deleted and records when, without removing it. Any other signed-in user, including an admin for updates, MUST get 403 Forbidden.

#### Scenario: Owner updates
- **WHEN** the owner updates their listing with valid fields
- **THEN** the system responds with 200 OK and the updated listing

#### Scenario: Another user updates
- **WHEN** a signed-in user who is not the owner updates the listing
- **THEN** the system responds with 403 Forbidden and the listing does not change

#### Scenario: Admin updates someone else's listing
- **WHEN** an admin who is not the owner updates a listing
- **THEN** the system responds with 403 Forbidden

#### Scenario: Owner deletes
- **WHEN** the owner deletes their listing
- **THEN** the system responds with 204 No Content
- **AND** the listing no longer appears in public reads

#### Scenario: Another user deletes
- **WHEN** a signed-in user who is neither the owner nor an admin deletes the listing
- **THEN** the system responds with 403 Forbidden and the listing stays

#### Scenario: Deleted or unknown listing
- **WHEN** the owner updates or deletes a listing that is already deleted or does not exist
- **THEN** the system responds with 404 Not Found

### Requirement: Own listings
A signed-in user SHALL list their own non-deleted listings, paged and newest first, at `Mine`. A request without a valid token MUST get 401 Unauthorized.

#### Scenario: Mine
- **WHEN** a user with two listings lists `Mine` while other users also have listings
- **THEN** the system responds with 200 OK and only that user's two listings

### Requirement: Admin moderation
An admin SHALL delete any listing, restore any deleted listing, and list and read listings including deleted ones, with each showing whether it is deleted and when. Restore SHALL respond with 204 No Content and MUST fail with 409 Conflict when the listing is not deleted, or when its model or city is deleted. These actions MUST give 403 Forbidden to signed-in users without the `admin` role.

#### Scenario: Admin deletes any listing
- **WHEN** an admin deletes another user's listing
- **THEN** the system responds with 204 No Content

#### Scenario: Admin restores
- **WHEN** an admin restores a deleted listing whose model and city are not deleted
- **THEN** the system responds with 204 No Content and the listing appears in public reads again

#### Scenario: Owner cannot restore
- **WHEN** the owner, without the admin role, restores their deleted listing
- **THEN** the system responds with 403 Forbidden

#### Scenario: Restore conflicts
- **WHEN** an admin restores a listing that is not deleted, or one whose model or city is deleted
- **THEN** the system responds with 409 Conflict

#### Scenario: Read with deleted
- **WHEN** an admin lists listings including deleted ones
- **THEN** the system responds with 200 OK, paged, each listing showing whether it is deleted and when
