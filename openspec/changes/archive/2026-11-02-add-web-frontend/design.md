# Design

## Context

- The refresh token cookie is `HttpOnly`, `Secure`, `SameSite=Strict`, with `Path=/api/v1/Auth`, and no service has CORS. The site must therefore share one origin with the APIs.
- Image URLs are already relative to the gateway.
- Refresh tokens work once: reusing one ends the login. This matters when several tabs are open.
- API messages are in English. Validation errors are keyed by camelCase field name.
- The gateway forwards paths unchanged and has no `/api/` catch-all today.
- The user writes no frontend code and checks only how the site looks and works, so choices that stay inside `web/` are made here.
- Requirements are in `specs/web-frontend` and `specs/api-gateway`. Motivation is in proposal.md.

## Goals / Non-Goals

**Goals:**
- A site that works through the gateway in all three environments, with no backend change.
- Later additions (filters, seller phone, other languages) fit without restructuring.

**Non-Goals:**
- Server-side rendering and search engine optimization.
- Drag and drop for ordering: move buttons work on phones too.
- Committed automated tests: like the backend, verification scripts stay out of the repository.
- Editing a profile, changing a password, and anything else without an API.

## Decisions

### 1. React, Vite, and TypeScript, served as static files

Chosen by the user. Next.js would add a Node server and more to operate, and search engines are out of scope today.

Pinned, adjusted at task 1.1 only if they prove incompatible:

| Package | Version |
|---|---|
| `react`, `react-dom` | 19.3.1 |
| `vite` | 8.3.5 |
| `@vitejs/plugin-react` | 6.2.0 |
| `typescript` | 7.0.2 |
| `react-router` | 8.5.1 |
| `@tanstack/react-query` | 5.105.1 |
| `tailwindcss`, `@tailwindcss/vite` | 4.3.3 |

Images: `node:22-alpine` to build, matching local Node 22, and `nginxinc/nginx-unprivileged:1.31.7-alpine` to serve on the internal port `8080` without root.

### 2. Repository layout and ports

`web/` at the repository root, with `src/`, `Dockerfile`, `nginx.conf`, `docker-compose.yml`, `docker-compose.dev.yml`, and `README.md`. The build context is the repository root, as for services.

Service number `81`:
- **Local:** `npm run dev` on `http://localhost:5081`. The dev server proxies `/api` and `/listing-images-dev` to the gateway on `5080`, so the browser sees one origin and changes appear without a reload. This is the address to open in Local.
- **Development and Production:** no host port, reached through `6080` and `7080`.

A new "Frontend" section in the global rules records this layout, so a later project does not decide it again.

### 3. Gateway routing

- A `web` route with `Path: /{**rest}` and `Methods: [GET, HEAD]` to a `web` cluster (`http://web:8080`; Local `http://localhost:5081`). Routing prefers more specific templates, so API, image, and `/health` routes win.
- `/api/{**rest}` mapped in `Program.cs` to a plain 404, so an unknown API path never reaches the site.
- `PUT /listing-images/x` stays 405, since both matching routes allow only `GET` and `HEAD`.

Alternative: a separate host name for the site. Rejected, since it would need CORS and break the cookie.

### 4. nginx serves pages and refuses missing files

- `try_files $uri /index.html` for page addresses.
- A path ending in a file extension uses `try_files $uri =404`, so `/listing-images/<id>/large.webp` in Development stays 404 instead of returning a page, as the existing gateway scenario requires.
- Hashed assets get a one-year `immutable` cache; `index.html` gets `no-cache`, so a new release is picked up at once.
- Headers on every response: `Content-Security-Policy` (`default-src 'self'; img-src 'self' blob: data:; style-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'`), `X-Content-Type-Options: nosniff`, and `Referrer-Policy: same-origin`.

The dev server in Local does not send these headers. They are checked in Development and Production.

### 5. Session handling

- The access token lives in a module variable, never in `localStorage`, so a script cannot read a saved copy and the cookie stays out of reach.
- On load the site calls `POST /Auth/Refresh`; 200 means signed in, 401 means signed out.
- A `401` from any API triggers one refresh and one retry of that request.
- Parallel requests in a tab share one refresh promise.
- **Across tabs:** each refresh runs inside `navigator.locks.request`. Without this, two tabs would send the same token and the second would end the login, since refresh tokens work once.

Tested in Chromium on `http://localhost`: the `Secure` cookie is stored and sent, it is hidden from scripts, and two tabs refreshing under the lock send the old then the new cookie.

Web Locks need a secure context, which `localhost` and any HTTPS server provide, but plain HTTP on another host does not. The site treats a missing lock API as a single tab.

Who the user is comes from `GET /Auth/Me`. The admin role is read from the access token's `realm_access.roles`, only to decide what to show; the APIs still enforce every rule.

### 6. Data and forms

TanStack Query holds server data, with retry controls and refresh after changes. Forms use plain React state: there are few of them and they are simple.

Rules are repeated in the browser only where this avoids a useless request or helps the person: file type and size, required fields, and the password policy. The server stays the authority.

### 7. Azerbaijani text

All text lives in `src/i18n/az.ts`, including labels for every enum value. API English is never shown:
- A `400` maps each field to an Azerbaijani message for that field.
- `401`, `403`, `404`, `409`, and `503` map to messages chosen by the page, for example "Bu ad artıq istifadə olunur" for a catalog `409`.

Alternative: translating the APIs' messages. Rejected, since it would need a backend change and a language header.

### 8. Pages

| Address | Page | Who |
|---|---|---|
| `/` (`?page=`) | Listing grid | anyone |
| `/listings/:id` | Listing and gallery | anyone |
| `/register`, `/login` | Accounts | anyone |
| `/my-listings` | Own listings | signed in |
| `/listings/new`, `/listings/:id/edit` | Listing form | signed in; edit by owner |
| `/listings/:id/images` | Image management | owner |
| `/admin/catalog` | Brands, models, cities | admin |
| `/admin/listings` | All listings with deleted ones, restore | admin |

Admins delete another user's listing or image from its normal page.

Images are chosen in the new-listing form, as the user asked, with at least one required there. The API still creates a listing first and takes images one at a time, so saving runs in that order: create, then upload each file. A listing exists once the first step succeeds, so if an upload then fails the site must not stay on the form, where saving again would create a second listing. It moves to the image page and shows which files failed. The one-image rule lives only in the form; the API keeps allowing listings without images.

### 9. Look

Light and clean: white background, neutral grays, one accent color, and system fonts, with images in front. Colors are named tokens, so a dark mode can come later. The user reviews the first version and asks for changes.

### 10. Verification

After each group, Playwright scripts in the scratchpad walk the pages in Chromium and take screenshots at 360 and 1280 pixels, which are read as images. The user then opens the same pages in Chrome. Tested so far: Playwright installs and runs here.

## Risks / Trade-offs

- **Search engines see an empty page:** listings will not rank well. → Accepted by choosing a static site; moving to server rendering later reuses most components.
- **Reload during a refresh:** if the browser drops the response, it still holds the old cookie, and the next use ends the login. → Rare; the person signs in again.
- **Safari on `http://localhost`:** may refuse the `Secure` cookie, so sessions may not survive a reload there. Not tested. → The user works in Chrome, and a real server uses HTTPS.
- **Closing the tab while saving:** a listing may stay with some or none of its images, since the API has no single call for both. → The owner sees it in their list and can add images on its image page.
- **No filters and no seller contact:** the site can only browse and cannot yet connect buyers and sellers. → Both are planned as later changes.
- **TypeScript 7 and Vite 8 are new major versions:** tools may not all support them. → Task 1.1 checks this first and may pin older versions.
- **Keeping JavaScript dependencies current:** this is the repository's first npm project. → Versions are pinned and `package-lock.json` is committed.

## Migration Plan

Additive for the APIs. Each machine runs `npm ci` in `web/` for Local; Docker builds on its own. Rollback: remove the `web` route, the `/api/` map, and the compose include.
