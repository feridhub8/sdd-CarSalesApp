# Tasks

Apply one group at a time: stop after each group, report how to test it, wait for the user to test and approve the commit.

## 1. MinIO

- [x] 1.1 Add `infrastructure/minio/` (`docker-compose.yml` with `minio`, healthcheck, `minio_data`, ports `127.0.0.1:9000` and `127.0.0.1:9001`, and a one-shot `minio-setup`; `.env.example`; `setup/` with the script and policies from design §4) and include it in `infrastructure/docker-compose.yml`; generate the values in `infrastructure/minio/.env` without printing them; verify `up` creates both buckets, `minio-setup` succeeds when run twice, and `git status` does not show `.env`
- [x] 1.2 Verify on the real setup: anonymous `GET` of a test object works, anonymous list, `PUT`, and `DELETE` are refused, the dev key cannot touch `listing-images`, and port `9000` is bound to `127.0.0.1` only; remove the test object
- [x] 1.3 Update `infrastructure/README.md` (tool table, one-shot services, `.env` values, the new Vault lines, the steps for an existing setup) and `openspec/config.yaml` (MinIO and why the fork); verify the documented commands work as written

## 2. Storage access

- [x] 2.1 Add `AWSSDK.S3`, `StorageOptions`, `IFileRepository`/`FileS3Repository` (upload with `Content-Type` and `Cache-Control`, delete, public path), `StorageExtensions.InitStorage`, `StorageUnavailableException` (503 in `GlobalExceptionHandler`), the 413 handling from design §8, and `Storage` in `appsettings*.json`; verify the build has no warnings and a temporary call writes, reads, and deletes a file in `listing-images-dev`, then remove the call
- [x] 2.2 Add `Storage:AccessKey` and `Storage:SecretKey` to the three Vault paths with `vault kv patch` after the user signs in, then revoke the token; verify existing values remain and the service starts in Local

## 3. Data

- [x] 3.1 Add the `ListingImage` entity, `ListingImageConfigurations` (index, FK with restrict, query filter), the `Images` navigation on `Listing`, the DbSet, and the `AddListingImages` migration; verify `--migrate` creates the table with its index, existing listing reads still pass, and startup logs no query filter warning

## 4. Image API

- [x] 4.1 Add `SkiaSharp` with its Linux assets and `IImageProcessingService`/`ImageProcessingService` (design §5, with the semaphore); verify with sample files for each orientation, format, and size that output is upright, WebP, correctly sized, and free of the GPS marker
- [x] 4.2 Add the `ListingImage` feature (DTOs, commands, validators, mapping, `IListingImageRepository`/`ListingImageEfCoreRepository` with the row lock, `IListingImageService`/`ListingImageService` with the flows from design §7, handlers, `ListingImageController`) and register them in `RepositoryExtensions` and `ServiceExtensions`; verify the build and that Swagger shows the three actions with file upload
- [x] 4.3 Verify in Local every scenario of `specs/listing-images` except those needing the gateway: upload and its rules, the limit with parallel uploads, processed versions, order, delete with renumbering, a deleted listing's images, and 503 with MinIO stopped, including that a failed upload leaves no files; remove test data and users afterwards
- [x] 4.4 Update `services/ms-listing/README.md` (image endpoints, limits, what is kept and what is not, relative URLs, the delete exception, `Storage` settings, a curl upload example); verify the documented commands work as written

## 5. Listing responses

- [ ] 5.1 Add the filtered and full includes, `coverImageUrl`, and `images` to both listing response DTOs and mappings (design §9); verify the scenarios in the `listings` delta, that all listing scenarios still pass, and through the SQL log that a list page does not load every image
- [ ] 5.2 Add the response fields to the listing section of `services/ms-listing/README.md`; verify they match a real response

## 6. Gateway and Docker

- [ ] 6.1 Add the two routes and the `minio` cluster to the gateway settings and `api-gateway/README.md` (design §10); verify in Local through `5080`: upload, reading a file, `PUT` and `DELETE` give 404, and the other environment's path gives 404
- [ ] 6.2 Verify in Development (`6080`) and then Production (`7080`): upload with an image in its own bucket, reading files, order, delete, an 11 MB file (400), a body over 30 MB (400 or 413), MinIO stopped (502, 503, or 504 within 5 seconds on files, 503 on upload, listing reads still 200), and `--scale ms-listing=2` with parallel uploads at the limit; remove test data and containers afterwards
- [ ] 6.3 Run `graphify update .`; verify `git status` shows no secrets or `.env` files and the root `README.md` still matches the running stack

## Workflow follow-up

- Commit after groups 1, 2, 3, 4, 5, and 6, each with the message shown for approval.
- Archive with `/opsx:archive` (specs are synced automatically).
- Add the backlog items to the next exploration: cleanup of old deleted listings' files, MinIO backups.
