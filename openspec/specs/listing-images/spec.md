# listing-images Specification

## Purpose

Lets listing owners add, order, and delete the photos of their cars, and lets anyone view those photos, without exposing the original files or their metadata.

## Requirements

### Requirement: Upload an image
The owner SHALL add one image per request to their own non-deleted listing and SHALL receive 201 Created with the image's `id`, `url`, `thumbnailUrl`, and `position`. A new image MUST take the last position.

#### Scenario: Owner uploads
- **WHEN** the owner uploads a valid image to a listing that has two images
- **THEN** the system responds with 201 Created and the image at position 2
- **AND** both of its URLs can be read without a token

#### Scenario: No token
- **WHEN** a client without a token uploads an image
- **THEN** the system responds with 401 Unauthorized and nothing is stored

#### Scenario: Another user or an admin uploads
- **WHEN** a signed-in user who is not the owner, including an admin, uploads an image to the listing
- **THEN** the system responds with 403 Forbidden and nothing is stored

#### Scenario: Deleted or unknown listing
- **WHEN** a signed-in user uploads an image to a listing that is deleted or does not exist
- **THEN** the system responds with 404 Not Found and nothing is stored

### Requirement: Accepted files
An upload MUST be a non-empty JPEG, PNG, or WebP file of at most 10 MB and 50 megapixels. The format MUST be decided from the file's content, not its name or declared type. A broken rule MUST give 400 Bad Request naming `file`, and nothing is stored.

#### Scenario: Not an image
- **WHEN** the owner uploads a text file named `photo.jpg` with the type `image/jpeg`
- **THEN** the system responds with 400 Bad Request naming `file`

#### Scenario: Other image format
- **WHEN** the owner uploads a GIF file
- **THEN** the system responds with 400 Bad Request naming `file`

#### Scenario: File too large
- **WHEN** the owner uploads an 11 MB file
- **THEN** the system responds with 400 Bad Request naming `file`

#### Scenario: Too many pixels
- **WHEN** the owner uploads a file under 10 MB that is 60 megapixels
- **THEN** the system responds with 400 Bad Request naming `file`

#### Scenario: Missing or empty file
- **WHEN** the owner sends an upload with no file or an empty file
- **THEN** the system responds with 400 Bad Request naming `file`

#### Scenario: Request body over 30 MB
- **WHEN** a client sends a request body over 30 MB
- **THEN** the system refuses it with 400 Bad Request or 413 Content Too Large before examining the file, and nothing is stored

### Requirement: Image limit per listing
A listing MUST have at most 20 images, also when uploads arrive at the same time. An upload over the limit MUST give 409 Conflict, and nothing is stored. A listing MAY have no images.

#### Scenario: Limit reached
- **WHEN** the owner uploads an image to a listing that has 20 images
- **THEN** the system responds with 409 Conflict and the listing keeps 20 images

#### Scenario: Parallel uploads at the limit
- **WHEN** the owner sends two uploads at the same time to a listing that has 19 images
- **THEN** one responds with 201 Created and the other with 409 Conflict
- **AND** the listing has 20 images with distinct positions

### Requirement: Processed versions
Each upload SHALL be stored only as two WebP versions: `url` at most 1600 pixels and `thumbnailUrl` at most 400 pixels on the long side, keeping proportions and never enlarging. The stored files MUST NOT contain the original's metadata, and MUST appear the way the original's orientation metadata showed it.

#### Scenario: Large photo
- **WHEN** the owner uploads a 6000x4000 JPEG
- **THEN** `url` serves a 1600x1067 WebP file and `thumbnailUrl` a 400x267 WebP file

#### Scenario: Small photo
- **WHEN** the owner uploads a 300x200 PNG
- **THEN** both URLs serve a 300x200 WebP file

#### Scenario: Location removed
- **WHEN** the owner uploads a JPEG whose metadata holds a GPS location
- **THEN** neither stored file contains that location

#### Scenario: Rotated photo
- **WHEN** the owner uploads a 40x20 JPEG whose metadata says to rotate it 90 degrees
- **THEN** both URLs serve an upright 20x40 file

### Requirement: Image order
The owner SHALL set the order of their non-deleted listing's images by sending every image id once, and SHALL receive 200 OK with the images in the new order. A list that omits an image, repeats one, or holds an id from outside the listing MUST give 400 Bad Request naming `imageIds`. Others MUST get 403 Forbidden, and a missing or deleted listing 404 Not Found.

#### Scenario: Owner reorders
- **WHEN** the owner sends the listing's three image ids in a new order
- **THEN** the system responds with 200 OK and positions 0, 1, and 2 follow that order
- **AND** the first image in the new order becomes the listing's cover

#### Scenario: Wrong list of ids
- **WHEN** the owner sends a list that omits an image, repeats one, or contains another listing's image
- **THEN** the system responds with 400 Bad Request naming `imageIds` and the order does not change

#### Scenario: Another user or an admin reorders
- **WHEN** a signed-in user who is not the owner, including an admin, reorders the images
- **THEN** the system responds with 403 Forbidden and the order does not change

#### Scenario: Deleted or unknown listing
- **WHEN** the owner reorders the images of a listing that is deleted or does not exist
- **THEN** the system responds with 404 Not Found

### Requirement: Delete an image
The owner or an admin SHALL delete an image of a non-deleted listing with 204 No Content. Deleting MUST remove the image for good: it cannot be restored, its URLs stop working, and the remaining images keep their order with positions from 0 and no gaps.

#### Scenario: Owner deletes the cover
- **WHEN** the owner deletes the first of three images
- **THEN** the system responds with 204 No Content and both of its URLs give 404 Not Found
- **AND** the other two images have positions 0 and 1, and the first of them is the cover

#### Scenario: Admin deletes
- **WHEN** an admin who is not the owner deletes an image
- **THEN** the system responds with 204 No Content

#### Scenario: Another user deletes
- **WHEN** a signed-in user who is neither the owner nor an admin deletes an image
- **THEN** the system responds with 403 Forbidden and the image stays

#### Scenario: Unknown image or deleted listing
- **WHEN** the owner or an admin deletes an image that does not exist or belongs to a deleted listing
- **THEN** the system responds with 404 Not Found

### Requirement: Images of a deleted listing
Soft deleting a listing MUST leave its images in place, and restoring the listing MUST bring them back with the same order. While the listing is deleted, public listing reads MUST NOT show them.

#### Scenario: Delete and restore
- **WHEN** a listing with images is deleted and an admin then restores it
- **THEN** the listing is returned with the same images in the same order

### Requirement: Public image files
Anyone SHALL read an image file by its URL without a token. Clients MUST NOT be able to list the stored files, or write or delete them through the file URLs.

#### Scenario: Read without a token
- **WHEN** a client without a token requests an image's `url`
- **THEN** the system responds with 200 OK and the type `image/webp`

#### Scenario: Listing the files
- **WHEN** a client without a token requests the folder that holds the files
- **THEN** the system does not return a list of files

#### Scenario: Writing through a file URL
- **WHEN** a client sends `PUT` or `DELETE` to a file URL
- **THEN** the file does not change

### Requirement: Storage unavailable
When file storage cannot be reached, an upload MUST give 503 Service Unavailable and MUST record no image. Listing reads MUST keep working.

#### Scenario: Storage down during upload
- **WHEN** the owner uploads a valid image while file storage is stopped
- **THEN** the system responds with 503 Service Unavailable and the listing's images do not change

#### Scenario: Storage down during reads
- **WHEN** a client reads a listing while file storage is stopped
- **THEN** the system responds with 200 OK and the listing with its image URLs
