# Design

## Context

- `ms-listing` has `Listing` with ownership checks in `ListingService`, soft delete through `BaseEntity`, and secrets loaded from Vault into configuration. `TokenKeycloakRepository` is the precedent for a repository over something other than the database.
- In Development and Production only the gateway publishes a host port. Local services run on the host and reach infrastructure tools through published ports.
- The gateway forwards paths unchanged, and YARP's config supports per-route `Methods`.
- Requirements are in `specs/listing-images`, `specs/listings`, and `specs/api-gateway`. Motivation is in proposal.md.

## Goals / Non-Goals

**Goals:**
- Stored files are always safe to serve publicly: known format, no metadata.
- Moving images to a separate service later stays possible: storage sits behind an interface, and URLs are built, not stored.

**Non-Goals:**
- Presigned uploads, a CDN, and image variants other than the two below.
- HEIC input.
- Cleaning up files of old deleted listings, and backups.
- Listing filters such as "only with photos".

## Decisions

### 1. Images stay in `ms-listing`, in a `ListingImage` feature

Ownership, soft delete state, and ordering all live in the listing database, so one service can check and change them in one transaction. A separate service would need service-to-service calls or events for each of these. `ms-media` becomes worthwhile when a second service needs files.

### 2. Upload through the service, not presigned URLs

The service validates, re-encodes, and records the image in one request. Presigned `PUT` cannot limit size, stores raw bytes with their GPS metadata, needs a confirm step and orphan cleanup, and requires browsers to reach storage on the host name the URL was signed for.

Trade-off: files pass through the gateway and the service, and processing uses CPU. Replicas scale with `--scale`.

### 3. MinIO through the `pgsty/minio` fork

The user chose MinIO to learn the tool. `minio/minio` and `quay.io/minio/minio` can no longer be pulled (checked with `docker manifest inspect`). The fork stays compatible and publishes versioned multi-arch images.

Pinned: `pgsty/minio:RELEASE.2026-08-04T00-00-00Z` and `pgsty/mc:RELEASE.2026-10-26T00-00-00Z`.

Tested on throwaway containers before writing this design:
- Anonymous `GET` of an object gives 200 with the stored `Content-Type` and `Cache-Control`.
- Anonymous list, `PUT`, and `DELETE` give 403, and a missing object gives 404.
- A bucket-scoped user can write only to its own bucket.
- The console answers on `9001`. Which admin features it keeps was not checked; the setup does not depend on them.

Alternatives: Garage and SeaweedFS are healthier long term. `AWSSDK.S3` and the repository interface keep a later switch small, apart from copying the files.

### 4. Infrastructure and secrets

`infrastructure/minio/` has `minio` and a one-shot `minio-setup` that is safe to run again, like `postgres-setup`.

| Environment | Bucket | Access key | Secret key in `minio/.env` | Vault path |
|---|---|---|---|---|
| Production | `listing-images` | `ms-listing` | `LISTING_STORAGE_SECRET_KEY` | `ms-listing` |
| Development | `listing-images-dev` | `ms-listing-dev` | `LISTING_STORAGE_SECRET_KEY_DEV` | `ms-listingDev` |
| Local | `listing-images-dev` | `ms-listing-dev` | `LISTING_STORAGE_SECRET_KEY_DEV` | `ms-listingLocal` |

- **Bucket name:** the `-dev` suffix replaces the usual `Dev`, because bucket names cannot contain capitals.
- **Key policy:** `GetObject`, `PutObject`, and `DeleteObject` on its own bucket. The service never lists.
- **Anonymous policy:** `GetObject` on objects only, written as JSON. `mc anonymous set download` is not used because it may also grant listing.
- **Vault:** `Storage:AccessKey` and `Storage:SecretKey`, added with `vault kv patch` so existing values stay. The endpoint and bucket hold no credentials, so they are in `appsettings*.json` as `Storage:Endpoint` and `Storage:Bucket`.
- **Ports:** `127.0.0.1:9000` and `127.0.0.1:9001`, needed by Local runs and reachable only from the machine itself.

### 5. Processing with SkiaSharp

MIT licensed, unlike ImageSharp 3's split license.

Steps:
1. Read the header with `SKCodec`. No codec, or a format other than JPEG, PNG, or WebP, gives 400.
2. Check the pixel count before allocating pixels, so a small file cannot exhaust memory.
3. Decode, apply `EncodedOrigin`, resize with Mitchell sampling, and encode WebP at quality 80.

Writing only pixels is what removes the metadata.

Tested with `SkiaSharp` and `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.154.0 as a non-root user on `mcr.microsoft.com/dotnet/aspnet:10.0`, the base image of our Dockerfile:
- A JPEG with EXIF orientation 6 and a GPS marker came out upright, with no marker and no EXIF.
- Garbage gave no codec, and GIF was detected and rejected.
- A 24 MP synthetic gradient JPEG took about 300 ms to decode and produce both versions. Real photos may take longer.

Only 90-degree orientation was tested on real output; the mirrored orientations are written from the same table and checked in task 4.1.

A 50 MP image takes about 200 MB when decoded, so a process-wide semaphore allows two images to be processed at a time per replica.

### 6. Data model

`listing_images`: `id` (v7), `listing_id`, `position`, `created_at`, and an index on `(listing_id, position)`.

- **No `BaseEntity`:** images are deleted for good, so soft delete columns would be dead. `README.md` states this exception.
- **Query filter:** `!image.Listing.IsDeleted`. Images of a deleted listing are hidden from direct reads, which gives the 404 in the spec and keeps EF from warning about a required relationship to a filtered entity. `IgnoreQueryFilters()` keeps them visible for `WithDeleted`.
- **File keys are derived from the id:** `{id}/large.webp` and `{id}/thumbnail.webp`. No URLs or bucket names are in the database, so changing either does not touch rows.
- **URLs are relative to the gateway:** `/{bucket}/{key}`. A web frontend on the same origin uses them as they are.
- **Position is not unique:** renumbering inside a transaction would collide. The row lock in decision 7 protects it instead.

### 7. Consistency without a shared transaction

File storage and the database cannot share a transaction, so each flow picks the safer failure.

**Upload:**
1. Check the listing, the owner, and the count (fast failure).
2. Process the file.
3. Write both files.
4. In a transaction, lock the listing row with `SELECT ... FOR UPDATE`, count again, and insert at `position = count`.

If step 3 or 4 fails, the written files are deleted with `CancellationToken.None`. Locking the row last means slow file work never holds a lock.

**Delete:** under the same lock, remove the row and renumber, commit, then delete the files. A failed file delete is logged as a warning and the client still gets 204. The file then stays readable by its old URL: rare, and visible in the log.

**Reorder:** under the same lock, compare the ids and assign positions.

### 8. API

Follows the global controller rule, not the `/Listing/{id}/Images` shape sketched during exploration, so controller names stay unique and the route uses `[controller]`.

| Action | Route | Who |
|---|---|---|
| Upload | `POST api/v1/ListingImage/Create/{listingId}` (multipart, `file`) | owner |
| Order | `PUT api/v1/ListingImage/Reorder/{listingId}` (`imageIds`) | owner |
| Delete | `DELETE api/v1/ListingImage/Delete/{id}` | owner or admin |

There are no `Restore` or `WithDeleted` actions, since deletes are final. Images are read through listing responses.

Limits: `file.Length` is checked by the validator, giving 400 naming `file` up to Kestrel's default 30 MB body limit. Beyond that the request is refused before the file is examined: model binding answers a multipart body with 400 on an empty key, and `GlobalExceptionHandler` answers other bodies with Kestrel's 413 instead of 500.

Naming: `IFileRepository` and `FileS3Repository` in `Data/Shared/Repositories/V1`, registered in `RepositoryExtensions` (not `IFileStorage`, to follow the repository naming rule). `StorageExtensions.InitStorage` registers the options and the S3 client. `IImageProcessingService` and `IListingImageService` are registered in `ServiceExtensions`. `StorageUnavailableException` maps to 503.

### 9. Listing responses

`GetPageAsync` uses a filtered include taking the first image by position, so a page of 100 listings does not load 2,000 rows. `GetByIdAsync` includes all images in order.

`GetListingOkResponseDto` and `GetListingWithDeletedOkResponseDto` gain `CoverImageUrl` and a nullable `Images` hidden from JSON when null. List handlers clear `Images`. Mappings build URLs through `IFileRepository`, resolved with `MapContext.Current.GetService`.

### 10. Gateway

- A `listing-image` route to the `ms-listing` cluster.
- A `listing-images` route with `Methods: [GET, HEAD]` to a new `minio` cluster.
- The base settings hold `/listing-images/{**rest}` and `http://minio:9000`. `appsettings.Development.json` and `appsettings.Local.json` override the path, and Local also overrides the address.

Files get `Cache-Control: public, max-age=86400`. A day is short enough for a deleted image to leave browsers soon, and `ETag` makes revalidation cheap.

## Risks / Trade-offs

- **Unmaintained upstream:** the fork depends on a small community. → Pinned versions, and the S3 API keeps a switch small.
- **Single node:** a lost disk loses every image, as with PostgreSQL today. → Backups are in the backlog for moving to a server.
- **Deleted listings keep public files:** anyone who has the old URL can still open them, and they use disk. → Accepted so restore works; cleanup is in the backlog.
- **Lost originals:** larger versions cannot be made later. → Accepted to guarantee no metadata is kept.
- **`AWSSDK.S3` with the fork is untested:** SDK v4's default checksums can trouble S3-compatible servers. → Task 2.1 tests it first; set `RequestChecksumCalculation` to `WHEN_REQUIRED` if needed.
- **`MapContext` service lookup is untested in this project.** → If it fails in task 5.1, build URLs in the handlers instead. Behavior does not change.
- **iPhone uploads:** browsers usually convert HEIC to JPEG, but this was not tested. → Otherwise the upload gets 400, and the frontend change handles it.

## Migration Plan

Additive. Each machine needs the new `.env`, a restart of the infrastructure, the Vault values, and `--migrate`. `infrastructure/README.md` gets these steps. Rollback: remove the routes and redeploy. The table and bucket can stay.
