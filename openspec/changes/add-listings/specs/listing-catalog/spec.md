# Spec Delta

## MODIFIED Requirements

### Requirement: Soft delete
Deleting an entry SHALL mark it deleted and record when, without removing it, and respond with 204 No Content. A brand that has non-deleted models MUST NOT be deleted. A model or city that non-deleted listings use MUST NOT be deleted.

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

#### Scenario: Model or city with active listings
- **WHEN** an admin deletes a model or a city that a non-deleted listing uses
- **THEN** the system responds with 409 Conflict and the entry stays
