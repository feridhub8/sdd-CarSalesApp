# Proposal

## Why

A car listing without photos is hard to sell, and the planned web frontend needs images for its list and detail pages. Listings have no images yet, so this comes before the frontend.

## What Changes

- Owners add images to their listings, one file per request (JPEG, PNG, or WebP; at most 10 MB, 50 megapixels, and 20 images per listing).
- Every upload is checked by content and re-encoded into two WebP versions (1600 px and 400 px on the long side). The original and its metadata, including GPS location, are not kept.
- Images have an order, which the owner can change; the first image is the listing's cover.
- The owner or an admin deletes an image for good. This is an exception to the project's soft delete rule: images are public, so a deleted image must really go. Soft deleting a listing leaves its images in place so that a restore brings them back.
- Listing responses gain `coverImageUrl`; a single listing also gains `images`.
- New infrastructure tool: MinIO, using the maintained `pgsty/minio` fork, because official MinIO images can no longer be pulled. One bucket per environment, each with its own key.
- The gateway gets a route for the image upload API and a read-only route for image files.

## Capabilities

### New Capabilities

- `listing-images`: uploading, processing, ordering, deleting, and publicly reading listing images.

### Modified Capabilities

- `listings`: responses carry the cover image, and a single listing carries all its images.
- `api-gateway`: forwards the image API to `ms-listing` and serves image files read-only.

## Impact

- **Infrastructure:** new `infrastructure/minio/` with `minio` and a one-shot `minio-setup`; secrets in `infrastructure/minio/.env`.
- **`ms-listing`:** a new `ListingImage` feature, a `listing_images` table and migration, file storage access, image processing, and changes to listing responses. New packages: `AWSSDK.S3`, `SkiaSharp`, and `SkiaSharp.NativeAssets.Linux.NoDependencies`.
- **Vault:** `Storage:AccessKey` and `Storage:SecretKey` on the three `ms-listing` paths.
- **`api-gateway`:** two routes and one cluster.
- **Docs:** `services/ms-listing/README.md`, `api-gateway/README.md`, `infrastructure/README.md`, and `openspec/config.yaml`.
- **API:** additive; existing listing fields do not change.
- **Out of scope, kept for later:** cleaning up files of old deleted listings, MinIO backups, and a separate media service.
