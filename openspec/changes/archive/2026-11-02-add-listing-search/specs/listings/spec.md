# listings Delta

## MODIFIED Requirements

### Requirement: Paged lists
Listing lists SHALL be paged with `page` (1-based, default 1, at most 500) and `pageSize` (default 20, 1 to 100), sorted newest first unless the public list is given another sort, and SHALL return `items`, `page`, `pageSize`, and `totalCount`. Values outside these ranges MUST give 400 Bad Request naming the parameter.

#### Scenario: First page
- **WHEN** a client lists listings without paging parameters while 25 non-deleted listings exist
- **THEN** the system responds with 200 OK, 20 items newest first, `page` 1, `pageSize` 20, and `totalCount` 25

#### Scenario: Second page
- **WHEN** a client lists with `page=2&pageSize=20` while 25 non-deleted listings exist
- **THEN** the response has the 5 oldest listings and `totalCount` 25

#### Scenario: Out of range
- **WHEN** a client lists with `pageSize=101`, `pageSize=0`, `page=0`, or `page=501`
- **THEN** the system responds with 400 Bad Request naming the parameter
