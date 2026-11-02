# Proposal

## Why

Accounts, listings, and images work, but only through API calls. People need a website to browse cars and manage their own listings, and admins need one to fill the catalog: without models and cities nobody can publish a listing.

## What Changes

- New `web/` application, in Azerbaijani, usable on a phone and on a computer, served through `api-gateway`.
- **Anyone:** browse listings as a card grid with paging, open a listing with its image gallery, register, and sign in.
- **Signed-in users:** stay signed in across reloads and tabs, sign out, list their own listings, create, edit, and delete them, and upload, order, and delete their images.
- **Admins:** manage brands, models, and cities, see deleted listings and restore them, and delete any listing or image.
- **Gateway:** paths outside the API and the image files now go to the website. **BREAKING:** an unknown path outside `/api/` no longer gives 404 from the gateway; unknown `/api/` paths still do.
- New global rule section for a frontend in a microservices repository, and service number `81` for `web`.

## Capabilities

### New Capabilities

- `web-frontend`: what people can see and do on the website, and how it behaves on errors, sessions, language, and screen size.

### Modified Capabilities

- `api-gateway`: unknown paths, and a new route that serves the website.

## Impact

- **New:** `web/` with its Dockerfile, nginx config, and compose files. It has no secrets and no `.env`.
- **`api-gateway`:** routes, a `web` cluster, and a 404 for unknown `/api/` paths.
- **Root files:** both root compose files, `README.md`, `.vscode/`, `.dockerignore`, and `openspec/config.yaml`.
- **Global rules:** `~/Desktop/sdd-global-rules/microservices.md`.
- **Backend:** no change to `ms-auth` or `ms-listing`.
- **Out of scope, kept for later:** search and filters (no API yet), the seller's name and phone (no API yet), password change and profile editing (no API yet), other languages, and search engine optimization.
