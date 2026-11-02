# web-frontend Specification

## Purpose

Gives people a website to browse car listings and manage their own, and gives admins a place to manage the catalog and moderate listings, on top of the existing APIs.

## Requirements

### Requirement: Browse listings
Anyone SHALL see non-deleted listings, newest first, as a grid of cards with 20 per page. Each card MUST show the cover image, brand, model, year, price with currency, and city, and MUST link to the listing. The page number MUST be part of the address.

#### Scenario: First page
- **WHEN** a visitor opens the home page while 25 listings exist
- **THEN** 20 cards appear, newest first, with a way to reach the second page

#### Scenario: Page in the address
- **WHEN** a visitor moves to page 2 and reloads, or opens that address directly
- **THEN** page 2 appears with the remaining 5 listings

#### Scenario: Listing without images
- **WHEN** a listing has no images
- **THEN** its card shows a placeholder instead of a broken image

#### Scenario: No listings
- **WHEN** no listings exist
- **THEN** the page says so instead of showing an empty grid

### Requirement: Read a listing
Anyone SHALL open a listing by its address and see its images as a gallery and all of its fields with readable labels. A listing that is deleted or does not exist MUST show a not-found page.

#### Scenario: Open a listing
- **WHEN** a visitor opens a listing with three images
- **THEN** the images appear in order, the first shown largest, and selecting another shows it instead
- **AND** every field of the listing appears, with fixed values such as fuel type shown in Azerbaijani

#### Scenario: Direct address
- **WHEN** a visitor reloads a listing page or opens its address in a new tab
- **THEN** the same listing appears

#### Scenario: Missing listing
- **WHEN** a visitor opens the address of a deleted or unknown listing
- **THEN** a not-found page appears with a link to the home page

### Requirement: Accounts
A person SHALL register, sign in with a username or email and a password, and sign out. Registering MUST NOT sign the person in. Broken rules MUST appear beside their fields, and wrong credentials MUST NOT say which of the two was wrong.

#### Scenario: Register
- **WHEN** a person registers with valid values
- **THEN** the sign-in page appears with a message that the account was created

#### Scenario: Registration rejected
- **WHEN** a person registers with a weak password or a username or email already in use
- **THEN** a message appears beside the matching field and the values stay in the form, except the password

#### Scenario: Sign in
- **WHEN** a person signs in with valid credentials
- **THEN** the page shows them as signed in

#### Scenario: Wrong credentials
- **WHEN** a person signs in with a wrong password
- **THEN** one general message appears and they stay on the sign-in page

#### Scenario: Sign out
- **WHEN** a signed-in person signs out
- **THEN** the page shows them as signed out, and reloading keeps them signed out

### Requirement: Session
A signed-in person SHALL stay signed in across reloads and open tabs, and an expired access token MUST be renewed without interrupting them. The access token MUST NOT be kept in browser storage. Once a session has really ended, the site MUST show the person as signed out.

#### Scenario: Reload
- **WHEN** a signed-in person reloads the page
- **THEN** they are still signed in

#### Scenario: Expired access token
- **WHEN** the access token expires and the person performs an action
- **THEN** the action succeeds without asking them to sign in

#### Scenario: Several tabs at once
- **WHEN** two tabs of a signed-in person renew their session at the same moment
- **THEN** both stay signed in

#### Scenario: Protected page without a session
- **WHEN** a signed-out visitor opens a page that needs signing in
- **THEN** the sign-in page appears, and after signing in they return to that page

### Requirement: Own listings
A signed-in person SHALL list their own listings, create and edit one with a form, and delete one after confirming. Models offered MUST belong to the chosen brand. Rejected fields MUST appear beside their fields, and values typed so far MUST stay. Edit and delete MUST appear only on the person's own listings. The site MUST NOT create a listing without at least one image chosen in the form.

#### Scenario: Create a listing
- **WHEN** a person fills the form with valid values, including a brand, one of its models, a city, and at least one image
- **THEN** the listing is created with its images and its page appears

#### Scenario: No image chosen
- **WHEN** a person submits the form without choosing an image
- **THEN** a message asks for at least one image and no listing is created

#### Scenario: Models follow the brand
- **WHEN** a person changes the brand
- **THEN** the model list shows only that brand's models and the previous model is cleared

#### Scenario: Rejected values
- **WHEN** a person submits a year outside the allowed range
- **THEN** a message appears beside the year and the other values stay

#### Scenario: Electric car
- **WHEN** a person chooses the fuel type `Electric`
- **THEN** the engine volume field is not offered

#### Scenario: Delete
- **WHEN** a person deletes their listing and confirms
- **THEN** it no longer appears in their list or on the home page

#### Scenario: Someone else's listing
- **WHEN** a signed-in person opens another user's listing
- **THEN** no edit or delete controls appear

### Requirement: Listing images
The owner SHALL add several images at once, change their order, choose the cover, and delete an image after confirming, both while creating a listing and later. Each file MUST show its own result. Files that are not JPEG, PNG, or WebP or are over 10 MB MUST be refused before sending, and the site MUST show when the limit of 20 images is reached.

#### Scenario: Choose images in the form
- **WHEN** a person creating a listing chooses three images, moves one, and removes another
- **THEN** previews show the current order with the first as the cover, and saving stores them in that order

#### Scenario: Refused while choosing
- **WHEN** a person creating a listing chooses a PDF or an 11 MB image
- **THEN** a message names the file and it is not added to the previews

#### Scenario: An image fails while saving
- **WHEN** the listing is created but the server rejects one of its images
- **THEN** the image management page appears with the error and the file name, the other images are stored, and no second listing is created

#### Scenario: The listing fails while saving
- **WHEN** creating the listing fails because a service is unavailable
- **THEN** a message appears and the form keeps its values and chosen images

#### Scenario: Several files
- **WHEN** the owner selects three valid images
- **THEN** each shows its progress, and all three appear in the order selected

#### Scenario: One bad file among good ones
- **WHEN** the owner selects two valid images and a file the server rejects
- **THEN** the two appear, and the third shows an error with its file name

#### Scenario: Refused before sending
- **WHEN** the owner selects a PDF or an 11 MB image
- **THEN** a message names the file and it is not sent

#### Scenario: Order and cover
- **WHEN** the owner moves an image or chooses it as the cover
- **THEN** the new order is kept after a reload, and the home page card shows the new cover

#### Scenario: Limit
- **WHEN** a listing has 20 images
- **THEN** the page says the limit is reached and offers no more uploads

#### Scenario: Storage down
- **WHEN** the owner uploads while file storage is unavailable
- **THEN** a message says to try again later and the existing images stay

### Requirement: Admin area
A person with the admin role SHALL manage brands, models, and cities, see deleted listings and restore them, and delete any listing or image. Others MUST NOT see the area, and opening its address MUST show a not-allowed page. Conflicts reported by the server MUST appear as readable messages.

#### Scenario: Manage the catalog
- **WHEN** an admin adds, renames, deletes, or restores a brand, model, or city
- **THEN** the list updates, and a person creating a listing can use the entry if it is not deleted

#### Scenario: Catalog conflict
- **WHEN** an admin adds a name already in use, or deletes a brand that has models or a model or city that has listings
- **THEN** a message explains why and nothing changes

#### Scenario: Restore a listing
- **WHEN** an admin restores a deleted listing
- **THEN** it appears on the home page again with its images

#### Scenario: Restore conflict
- **WHEN** an admin restores a listing whose model or city is deleted
- **THEN** a message explains why and the listing stays deleted

#### Scenario: Remove someone's content
- **WHEN** an admin opens another user's listing
- **THEN** they can delete the listing or one of its images, but cannot edit the listing

#### Scenario: Not an admin
- **WHEN** a signed-in person without the role opens the admin address
- **THEN** a not-allowed page appears, and no admin link is shown to them

### Requirement: Language
All text on the site MUST be in Azerbaijani, including field names, fixed values, and error messages. Messages from the APIs, which are in English, MUST NOT be shown to people as they are.

#### Scenario: Server message
- **WHEN** an API rejects a field with an English message
- **THEN** the site shows an Azerbaijani message for that field

### Requirement: Screen sizes
Every page SHALL be usable from phone width to desktop, without sideways scrolling of the page.

#### Scenario: Phone
- **WHEN** a page is opened at 360 pixels wide
- **THEN** cards are in one column and every control can be reached

#### Scenario: Desktop
- **WHEN** the home page is opened at 1280 pixels wide
- **THEN** cards are in several columns

### Requirement: Failures
When an API cannot be reached or fails, the page MUST show a message and a way to try again, not an empty or broken page. A failed action MUST keep what the person typed.

#### Scenario: Service down
- **WHEN** a visitor opens the home page while `ms-listing` is stopped
- **THEN** a message and a retry control appear, and retrying works once the service is back

#### Scenario: Failed save
- **WHEN** saving a listing fails because a service is unavailable
- **THEN** a message appears and the form keeps its values

### Requirement: User content is only text
Text written by users MUST be shown as plain text, and the site MUST tell browsers not to run scripts from other sources or allow being framed by other sites.

#### Scenario: Script in a description
- **WHEN** a listing description contains `<script>alert(1)</script>`
- **THEN** the text appears as written and no script runs

#### Scenario: Response headers
- **WHEN** a client requests a page of the site in Development or Production
- **THEN** the response carries a content security policy limiting scripts to the site itself and forbidding framing

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

### Requirement: Rate limit messages
When an API answers 429, the site MUST show a message in Azerbaijani asking the person to try again shortly, distinct from the service-unavailable message, and MUST keep what they typed. A 429 MUST NOT sign the person out.

#### Scenario: Sign-in over the limit
- **WHEN** a person signs in and the gateway answers 429
- **THEN** the rate limit message appears, not the wrong-password message

#### Scenario: Saving a form over the limit
- **WHEN** saving a listing or registering gets 429
- **THEN** the rate limit message appears and the form keeps its values

#### Scenario: Page data over the limit
- **WHEN** loading a page's data gets 429
- **THEN** the rate limit message and a way to try again appear

#### Scenario: Seller contact and uploads over the limit
- **WHEN** the show-number control or an image upload gets 429
- **THEN** the rate limit message appears there, and the rest of the page stays

#### Scenario: Session renewal over the limit
- **WHEN** a signed-in person's session renewal gets 429
- **THEN** the login is not ended, and a later reload signs them back in

### Requirement: Hint for a locked account
The sign-in page's message for refused credentials SHALL also say to wait a little after several wrong tries, since a locked account answers like a wrong password.

#### Scenario: Refused credentials
- **WHEN** sign-in is refused
- **THEN** the single general message includes the hint, and does not say whether the account is locked

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
