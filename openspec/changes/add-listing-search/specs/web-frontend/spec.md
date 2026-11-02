# web-frontend Delta

## ADDED Requirements

### Requirement: Filter and sort listings
The home page SHALL offer every filter and sort of the listing list, in Azerbaijani, with models following the chosen brand. Results MUST update as choices change, waiting for typing to finish in number fields. Changing a filter or the sort MUST return to the first page, and moving between pages MUST keep them.

#### Scenario: Choose a filter
- **WHEN** a visitor chooses a brand
- **THEN** only that brand's listings appear, with the number of results, on the first page

#### Scenario: Models follow the brand
- **WHEN** a visitor chooses a brand and then another one
- **THEN** the model list shows only the current brand's models, and the previous model is cleared

#### Scenario: Several values
- **WHEN** a visitor chooses Sedan and SUV
- **THEN** listings of either body type appear

#### Scenario: Typing a number
- **WHEN** a visitor types a price of several digits
- **THEN** one search runs after they stop typing, not one per digit

#### Scenario: Paging keeps filters
- **WHEN** a visitor with filters moves to page 2
- **THEN** the filters stay and the results continue

### Requirement: Filters in the address
The chosen filters, sort, and page MUST be in the address, so a search survives a reload and can be shared as a link. Invalid values in an address MUST NOT break the page.

#### Scenario: Reload
- **WHEN** a visitor reloads a filtered page
- **THEN** the same filters are selected and the same results appear

#### Scenario: Shared link
- **WHEN** another person opens that address
- **THEN** they see the same filters and results

#### Scenario: Bad address
- **WHEN** an address holds an unknown value such as `bodyType=Boat`
- **THEN** the page still works, ignoring that value

#### Scenario: Browser back
- **WHEN** a visitor opens a listing from filtered results and goes back
- **THEN** the same filters and results appear

### Requirement: Filter results and reset
When no listing matches, the page MUST say so and offer to clear the filters. A control MUST clear all filters at any time. The price filter MUST state that prices are compared at the Central Bank's rate.

#### Scenario: No match
- **WHEN** filters match nothing
- **THEN** a message appears with a control that clears the filters, and clearing shows all listings

#### Scenario: Prices stay as written
- **WHEN** a visitor filters by AZN and a matching listing is priced in USD
- **THEN** its card shows the price in USD

#### Scenario: Rejected values
- **WHEN** a visitor enters a from value above its to value
- **THEN** a message appears beside the fields in Azerbaijani, and the page does not break

### Requirement: Filters on small screens
On a phone the filters MUST be collapsed behind a control showing how many are active, so the results stay in view. On a computer they MUST be shown beside the results.

#### Scenario: Phone
- **WHEN** the home page opens at 360 pixels wide
- **THEN** the filters are collapsed, the page does not scroll sideways, and opening them shows every filter

#### Scenario: Desktop
- **WHEN** the home page opens at 1280 pixels wide
- **THEN** the filters are visible beside the results
