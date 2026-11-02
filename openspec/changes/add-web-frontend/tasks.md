# Tasks

Apply one group at a time: stop after each group, report what to open and what should appear, and wait for the user to check it and approve the commit.

## 1. Site skeleton and gateway

- [x] 1.1 Add the "Frontend" section to `~/Desktop/sdd-global-rules/microservices.md` and the frontend line to `openspec/config.yaml` (design §2); verify with `git diff` that only those parts changed
- [x] 1.2 Create `web/` with the pinned packages, routing, layout (header, footer, page width), design tokens, `src/i18n/az.ts`, and a not-found page; verify `npm run build` passes and `npm run dev` serves the layout on `5081`, adjusting versions only if incompatible and recording any change in design §1
- [x] 1.3 Add `web/Dockerfile`, `nginx.conf`, compose files, both root compose includes, `.dockerignore`, and `.vscode` entries (design §2, §4); add the `web` route and cluster and the `/api/` 404 to the gateway (design §3); verify every scenario of the `api-gateway` delta and the existing gateway scenarios in Development, and the response headers
- [x] 1.4 Add `web/README.md`, and update the root `README.md` and `api-gateway/README.md`; verify the documented commands work as written

## 2. Accounts and session

- [ ] 2.1 Add the API client with the session handling from design §5, and the register, sign-in, and sign-out pages; verify with Playwright the `Accounts` scenarios at both widths and read the screenshots
- [ ] 2.2 Verify the `Session` scenarios with Playwright: reload, an expired access token, two tabs renewing at once (the login must survive), a protected page, and that no token is in browser storage

## 3. Browsing

- [ ] 3.1 Add the listing grid and listing page with its gallery; verify with Playwright the `Browse listings`, `Read a listing`, `Screen sizes`, `Failures`, and script-in-description scenarios against test data, and read the screenshots at both widths

## 4. Own listings and images

- [ ] 4.1 Add `/my-listings` and the listing form (brand and model lists, electric cars, field errors, delete with confirmation); verify with Playwright the `Own listings` scenarios and the failed-save scenario
- [ ] 4.2 Add the image page (several files with per-file results, checks before sending, move and cover controls, delete with confirmation, limit); verify with Playwright the `Listing images` scenarios, including the storage-down one, with real JPEG, PNG, and WebP sample files

## 5. Admin area

- [ ] 5.1 Add `/admin/catalog` and `/admin/listings`, and admin delete controls on listing pages; verify with Playwright the `Admin area` scenarios, including conflicts, with a temporary admin and a plain user

## 6. Whole site in Docker

- [ ] 6.1 Run the full Playwright walk in Development (`6080`) and then Production (`7080`): register, sign in, create a listing, upload, reorder, delete, admin catalog and restore, reload at each step, and the `Language` scenario by searching pages for English; remove test data, users, and containers afterwards
- [ ] 6.2 Run `graphify update .`; verify `git status` shows no `node_modules`, `dist`, secrets, or `.env` files, and the root `README.md` matches the running stack

## Workflow follow-up

- Commit after each group, with the message shown for approval. Group 1 also commits in `sdd-global-rules`.
- Archive with `/opsx:archive` (specs are synced automatically).
- Next changes to explore: seller phone, then search and filters.
