# listing-catalog Specification

## Purpose

Keeps the shared lists of car brands, models, and cities that listings refer to, so every listing names them the same way. Anyone can read the catalog; only admins can change it.

## Requirements

### Requirement: Public catalog reads
The system SHALL let anyone, without signing in, list brands, models, and cities, and read one entry by id. Lists SHALL be sorted by name and are not paged. Deleted entries MUST NOT appear in these reads. A model SHALL be returned with its brand's id and name.

#### Scenario: List brands without a token
- **WHEN** a client without a token lists brands
- **THEN** the system responds with 200 OK and every non-deleted brand, sorted by name

#### Scenario: List models of one brand
- **WHEN** a client lists models with `brandId` set to an existing brand
- **THEN** the system responds with 200 OK and only that brand's non-deleted models, sorted by name, each with the brand's id and name

#### Scenario: Read one entry
- **WHEN** a client reads a brand, model, or city by the id of a non-deleted entry
- **THEN** the system responds with 200 OK and that entry

#### Scenario: Deleted or unknown entry
- **WHEN** a client reads a brand, model, or city by an id that is deleted or does not exist
- **THEN** the system responds with 404 Not Found

### Requirement: Admin-only catalog changes
Creating, updating, deleting, and restoring catalog entries, and reading deleted entries, SHALL require a signed-in user with the `admin` role. A request without a valid, active token MUST get 401 Unauthorized; a signed-in user without the role MUST get 403 Forbidden.

#### Scenario: No token
- **WHEN** a client without a token sends a create, update, delete, restore, or deleted-entries request
- **THEN** the system responds with 401 Unauthorized and nothing changes

#### Scenario: Signed in without the admin role
- **WHEN** a signed-in user without the `admin` role sends a create request
- **THEN** the system responds with 403 Forbidden and nothing changes

#### Scenario: Signed-out token
- **WHEN** an admin signs out and then sends a create request with the old access token
- **THEN** the system responds with 401 Unauthorized

#### Scenario: Admin
- **WHEN** a signed-in user with the `admin` role creates a brand with a valid name
- **THEN** the system responds with 201 Created and the new brand

### Requirement: Entry names
Brand, model, and city names SHALL be required, 1 to 100 characters after trimming surrounding spaces, and stored trimmed. Brand names and city names MUST be unique among non-deleted entries of their kind, ignoring letter case; model names MUST be unique among the non-deleted models of the same brand, ignoring letter case.

#### Scenario: Missing or too long name
- **WHEN** an admin creates or updates an entry with an empty name, a name of only spaces, or a name longer than 100 characters
- **THEN** the system responds with 400 Bad Request naming the `name` field and nothing changes

#### Scenario: Duplicate name in another letter case
- **WHEN** an admin creates a brand named `bmw` while a non-deleted brand `BMW` exists
- **THEN** the system responds with 409 Conflict and no brand is created

#### Scenario: Same model name under another brand
- **WHEN** an admin creates a model `Golf` under one brand while another brand already has a non-deleted model `Golf`
- **THEN** the system responds with 201 Created

#### Scenario: Name of a deleted entry
- **WHEN** an admin creates a city with the name of a deleted city
- **THEN** the system responds with 201 Created

#### Scenario: Name is trimmed
- **WHEN** an admin creates a city named `  Baku  `
- **THEN** the city is stored and returned as `Baku`

### Requirement: Create and update entries
Admins SHALL create entries and update an entry's name; a model's brand can also be changed. Create SHALL respond with 201 Created and the entry, update with 200 OK and the entry. A model MUST belong to an existing, non-deleted brand.

#### Scenario: Create a model
- **WHEN** an admin creates a model with a valid name and the id of a non-deleted brand
- **THEN** the system responds with 201 Created and the model with its brand's id and name

#### Scenario: Model under a missing or deleted brand
- **WHEN** an admin creates or updates a model with a brand id that does not exist or belongs to a deleted brand
- **THEN** the system responds with 400 Bad Request naming the `brandId` field and nothing changes

#### Scenario: Update a name
- **WHEN** an admin updates a non-deleted brand with a new valid, unused name
- **THEN** the system responds with 200 OK and the brand with the new name

#### Scenario: Update a deleted or unknown entry
- **WHEN** an admin updates an entry that is deleted or does not exist
- **THEN** the system responds with 404 Not Found

### Requirement: Soft delete
Deleting an entry SHALL mark it deleted and record when, without removing it, and respond with 204 No Content. A brand that has non-deleted models MUST NOT be deleted.

#### Scenario: Delete a city
- **WHEN** an admin deletes a non-deleted city
- **THEN** the system responds with 204 No Content
- **AND** the city no longer appears in public reads

#### Scenario: Brand with active models
- **WHEN** an admin deletes a brand that still has non-deleted models
- **THEN** the system responds with 409 Conflict and the brand stays

#### Scenario: Delete an already deleted or unknown entry
- **WHEN** an admin deletes an entry that is already deleted or does not exist
- **THEN** the system responds with 404 Not Found

### Requirement: Restore
Restoring a deleted entry SHALL make it non-deleted again and respond with 204 No Content. A restore MUST fail with 409 Conflict when the entry is not deleted, when a non-deleted entry with the same name now exists in its scope, or, for a model, when its brand is deleted.

#### Scenario: Restore a brand
- **WHEN** an admin restores a deleted brand whose name is not used by a non-deleted brand
- **THEN** the system responds with 204 No Content
- **AND** the brand appears in public reads again

#### Scenario: Name taken meanwhile
- **WHEN** an admin restores a deleted city while a non-deleted city with the same name exists
- **THEN** the system responds with 409 Conflict and the city stays deleted

#### Scenario: Model of a deleted brand
- **WHEN** an admin restores a deleted model whose brand is deleted
- **THEN** the system responds with 409 Conflict

#### Scenario: Entry is not deleted
- **WHEN** an admin restores an entry that is not deleted
- **THEN** the system responds with 409 Conflict

#### Scenario: Unknown entry
- **WHEN** an admin restores an id that does not exist
- **THEN** the system responds with 404 Not Found

### Requirement: Admin reads including deleted entries
Admins SHALL be able to list each kind of entry and read one entry by id including deleted entries. These responses SHALL show whether each entry is deleted and when it was deleted.

#### Scenario: List with deleted entries
- **WHEN** an admin lists brands including deleted ones
- **THEN** the system responds with 200 OK and every brand, sorted by name, each showing whether it is deleted and when

#### Scenario: Read a deleted entry
- **WHEN** an admin reads a deleted model by id including deleted ones
- **THEN** the system responds with 200 OK and the model marked as deleted
