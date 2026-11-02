# web-frontend Delta

## ADDED Requirements

### Requirement: Seller contact on a listing
A listing page SHALL offer a control that shows the seller's first name and number when pressed, without signing in, and MUST NOT request them before it is pressed. The number MUST be shown as `+994 50 123 45 67` and MUST start a call when used on a phone.

#### Scenario: Show the number
- **WHEN** a visitor presses the control on a listing whose seller has a number
- **THEN** the seller's first name and number appear

#### Scenario: Nothing requested before pressing
- **WHEN** a visitor opens a listing page and does not press the control
- **THEN** no request for the seller's contact is sent

#### Scenario: Call link
- **WHEN** the number is shown
- **THEN** it is a link that starts a call to `+994501234567`

#### Scenario: Seller without a number
- **WHEN** a visitor presses the control and the seller has no number, or the account no longer exists
- **THEN** a message says that no contact details are available

#### Scenario: Service down
- **WHEN** a visitor presses the control while `ms-auth` is unavailable
- **THEN** a message and a way to try again appear, and the rest of the listing stays

#### Scenario: Listing with the auth service stopped
- **WHEN** a visitor opens a listing while `ms-auth` is stopped
- **THEN** the listing and its images appear as usual

### Requirement: Contact number when publishing
The new-listing form SHALL ask for a contact number, filled in from the account when it has one, and MUST NOT create a listing without a valid number. Saving MUST store the number on the account before creating the listing. The edit form MUST NOT ask for it.

#### Scenario: Account without a number
- **WHEN** a person without a number opens the new-listing form
- **THEN** the number field is empty, and saving without it shows a message beside the field and creates nothing

#### Scenario: Account with a number
- **WHEN** a person with a number opens the new-listing form
- **THEN** the field shows it, and saving without changes creates the listing

#### Scenario: Number entered in the form
- **WHEN** a person enters a valid number and saves the form
- **THEN** the number is on their account, and it appears when a visitor presses the control on their listings

#### Scenario: Rejected number
- **WHEN** a person enters an invalid number and saves
- **THEN** a message appears beside the field, the form keeps its values and images, and no listing is created

### Requirement: Profile page
A signed-in person SHALL open a profile page that shows their username, email, first name, and last name, and SHALL change their number there. The new number MUST apply at once to all of their listings. A signed-out visitor MUST be sent to the sign-in page.

#### Scenario: Change the number
- **WHEN** a person saves a new valid number on the profile page
- **THEN** a success message appears, and the listing page control shows the new number

#### Scenario: Rejected number
- **WHEN** a person saves an invalid number
- **THEN** a message appears beside the field and the stored number does not change

#### Scenario: Reaching the page
- **WHEN** a signed-in person selects their name in the header
- **THEN** the profile page opens

#### Scenario: Signed out
- **WHEN** a signed-out visitor opens the profile address
- **THEN** the sign-in page appears, and after signing in they return to the profile page
