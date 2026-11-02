# web

The CarSales website: React, Vite, and TypeScript. It holds no secrets and calls the APIs through [`api-gateway`](../api-gateway/README.md), on one origin, so the refresh token cookie works without CORS.

## Requirements

- Node.js 22
- The packages: `npm ci` in this folder
- For anything beyond the page frame, the gateway and the services running

## Run

From `web/`:

```bash
npm run dev
```

Open `http://localhost:5081`. Changes appear without a reload. The dev server passes `/api` and `/listing-images-dev` to the gateway on `http://localhost:5080`. In VS Code, **All services (Local)** starts it together with the gateway and every service.

## Build

From `web/`:

```bash
npm run build
```

This checks the types and writes the static files to `dist/`.

## Docker

Requires the `carsales_network` network. The root compose files start the website with everything else (see the root [`README.md`](../README.md)). On its own, from `web/`:

```bash
docker compose -f docker-compose.dev.yml up -d --build
docker compose -f docker-compose.dev.yml down
```

The image serves the built files with nginx on the internal port `8080`, as a non-root user. It publishes no host port: open it through the gateway on `http://localhost:6080` (Development) or `http://localhost:7080` (Production).

[`nginx.conf`](nginx.conf) decides three things:

- A page address returns `index.html`, and the app resolves it in the browser.
- A path ending in a file name returns that file, or `404`.
- Every response carries the security headers. The dev server does not send them, so check them in Docker.

## Text

Every text on the site is in [`src/i18n/az.ts`](src/i18n/az.ts), in Azerbaijani. Messages from the APIs, which are in English, are not shown to people.

## Colors

The colors are named in [`src/index.css`](src/index.css). Change them there, not in the pages.
