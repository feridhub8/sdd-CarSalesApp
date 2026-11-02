# listings Delta

## MODIFIED Requirements

### Requirement: Public listing reads
The system SHALL let anyone, without signing in, list non-deleted listings and read one non-deleted listing by id. Each listing SHALL be returned with its fields, its model's id and name, its brand's id and name, its city's id and name, its owner's id, its `coverImageUrl`, and when it was created and last updated. Reading one listing SHALL also return its `images` in order.

#### Scenario: Read a listing
- **WHEN** a client without a token reads a non-deleted listing by id
- **THEN** the system responds with 200 OK and the listing with its brand, model, and city names and its owner id

#### Scenario: Deleted or unknown listing
- **WHEN** a client reads a listing by an id that is deleted or does not exist
- **THEN** the system responds with 404 Not Found

#### Scenario: Read a listing with images
- **WHEN** a client reads a non-deleted listing that has three images
- **THEN** the response has `images` with each image's `id`, `url`, `thumbnailUrl`, and `position`, in order
- **AND** `coverImageUrl` is the first image's `thumbnailUrl`

#### Scenario: Cover in lists
- **WHEN** a client reads any listing list, including `Mine` and the lists with deleted listings
- **THEN** each item has `coverImageUrl`, and no item has `images`

#### Scenario: Listing without images
- **WHEN** a client reads a listing that has no images
- **THEN** `coverImageUrl` is null and, when reading it by id, `images` is empty
