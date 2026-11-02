# listing-search Specification

## Purpose

Lets anyone narrow and order the public listing list by what matters when buying a car, comparing prices fairly across currencies.

## Requirements

### Requirement: Filter by catalog
The public listing list SHALL take optional `brandId`, `modelId`, and `cityId`, and return only matching non-deleted listings. Filters combine with AND. An id that matches nothing MUST give an empty page, not an error, and `totalCount` MUST count only matches.

#### Scenario: One brand
- **WHEN** a client lists with `brandId` of a brand that has 3 listings among others
- **THEN** the response has those 3 listings and `totalCount` 3

#### Scenario: Combined filters
- **WHEN** a client lists with a `modelId` and a `cityId`
- **THEN** only listings of that model in that city are returned

#### Scenario: Unknown id
- **WHEN** a client lists with a `brandId` that does not exist
- **THEN** the system responds with 200 OK, no items, and `totalCount` 0

#### Scenario: No filters
- **WHEN** a client lists without any filter
- **THEN** the response is the same as before filters existed

#### Scenario: Paging within results
- **WHEN** a filter matches 25 listings and a client asks for `page=2`
- **THEN** the response has the remaining 5 and `totalCount` 25

### Requirement: Filter by ranges
The list SHALL take optional `yearFrom`, `yearTo`, `mileageFrom`, `mileageTo`, `engineFrom`, and `engineTo`, both ends included. A from value above its to value, a negative value, or a value that is not a number MUST give 400 Bad Request naming the parameter. Mileage MUST compare in kilometres, converting listings written in miles at 1.609.

#### Scenario: Year range
- **WHEN** a client lists with `yearFrom=2015&yearTo=2018`
- **THEN** listings from 2015 to 2018, inclusive, are returned

#### Scenario: Mileage across units
- **WHEN** a client lists with `mileageTo=100000` while one listing has 90,000 km, one has 50,000 mi, and one has 70,000 mi
- **THEN** the 90,000 km and 50,000 mi listings are returned, and each still shows the mileage and unit its seller wrote

#### Scenario: Electric cars and engine volume
- **WHEN** a client lists with `engineFrom=1.0`
- **THEN** electric listings, which have no engine volume, are not returned

#### Scenario: Reversed or invalid range
- **WHEN** a client lists with `yearFrom=2020&yearTo=2010`, `mileageFrom=-1`, or `yearFrom=abc`
- **THEN** the system responds with 400 Bad Request naming the parameter

### Requirement: Filter by fixed values
The list SHALL take `bodyType`, `fuelType`, `transmission`, `drivetrain`, `color`, and `market`, each repeatable to match any of its values. Values MUST be names from the listing's fixed lists; numbers and unknown names MUST give 400 Bad Request naming the parameter.

#### Scenario: Several values
- **WHEN** a client lists with `bodyType=Sedan&bodyType=Suv&fuelType=Diesel`
- **THEN** diesel listings that are sedans or SUVs are returned

#### Scenario: Unknown or numeric value
- **WHEN** a client lists with `bodyType=Boat` or `fuelType=1`
- **THEN** the system responds with 400 Bad Request naming the parameter

### Requirement: Filter by flags
The list SHALL take optional `isCredit`, `isBarter`, `notPainted`, `notDamaged`, and `hasImages`. Each narrows the list only when `true`; a missing or `false` flag MUST NOT filter. A value that is not a boolean MUST give 400 Bad Request naming the parameter.

#### Scenario: Flags narrow
- **WHEN** a client lists with `isCredit=true&notDamaged=true`
- **THEN** only listings offered on credit and not damaged are returned

#### Scenario: False does not filter
- **WHEN** a client lists with `notPainted=false`
- **THEN** both painted and unpainted listings are returned

#### Scenario: Has images
- **WHEN** a client lists with `hasImages=true`
- **THEN** only listings with at least one image are returned

### Requirement: Filter by price across currencies
The list SHALL take `priceFrom`, `priceTo`, and `priceCurrency` (`AZN` by default, `USD`, or `EUR`). Prices of all listings MUST be compared by converting them at the current rates. The price returned MUST stay as written by the seller. An unknown currency, a negative value, or a reversed range MUST give 400 Bad Request naming the parameter.

#### Scenario: Budget in AZN
- **WHEN** rates are 1 USD = 1.7 AZN, a client lists with `priceTo=30000`, and listings cost 25,000 AZN, 15,000 USD, and 40,000 AZN
- **THEN** the first two are returned, the second still showing 15,000 USD

#### Scenario: Budget in another currency
- **WHEN** rates are 1 USD = 1.7 AZN and a client lists with `priceTo=15000&priceCurrency=USD`
- **THEN** a listing of 25,500 AZN or less is returned, and one of 26,000 AZN is not

#### Scenario: Invalid price filter
- **WHEN** a client lists with `priceCurrency=GBP`, `priceFrom=-1`, or `priceFrom=5&priceTo=1`
- **THEN** the system responds with 400 Bad Request naming the parameter

### Requirement: Sort
The list SHALL take `sort`: `Newest` (default), `PriceAsc`, `PriceDesc`, `YearDesc`, or `MileageAsc`. Price MUST sort by the converted value, and mileage by kilometres. Equal values MUST keep a stable order across pages. An unknown value MUST give 400 Bad Request naming `sort`.

#### Scenario: Cheapest across currencies
- **WHEN** rates are 1 USD = 1.7 AZN, a client lists with `sort=PriceAsc`, and listings cost 20,000 AZN and 10,000 USD
- **THEN** the 10,000 USD listing, worth 17,000 AZN, comes first

#### Scenario: Stable pages
- **WHEN** many listings have the same year and a client pages through `sort=YearDesc`
- **THEN** no listing appears twice or is missed

#### Scenario: Unknown sort
- **WHEN** a client lists with `sort=Random`
- **THEN** the system responds with 400 Bad Request naming `sort`

### Requirement: Exchange rates
The system SHALL fetch USD and EUR rates against AZN from the Central Bank of Azerbaijan when it starts and every 6 hours, and keep them in its database. If the bank cannot be reached or answers with something unusable, the system MUST keep working with the last rates it has, or with rates from its settings if it has none, and MUST log a warning. New rates MUST apply without a restart.

#### Scenario: Rates arrive
- **WHEN** the service starts while the bank is reachable
- **THEN** the stored USD and EUR rates match the bank's file for the latest working day

#### Scenario: Bank unreachable
- **WHEN** the bank cannot be reached
- **THEN** searches keep using the stored rates, and a warning is logged

#### Scenario: Unusable answer
- **WHEN** the bank answers with invalid data or a rate of zero
- **THEN** the stored rates do not change, and a warning is logged

#### Scenario: Never fetched
- **WHEN** no rates are stored and the bank cannot be reached
- **THEN** searches work with the rates from settings

#### Scenario: Several replicas
- **WHEN** two replicas fetch rates at the same time
- **THEN** both succeed and one row per currency is stored
