# Design

## Context

- No rate limiting exists anywhere, and Keycloak's `bruteForceProtected` is off.
- All client traffic passes through the gateway, a YARP proxy with one route per API area and a catch-all for the website.
- The gateway's Kestrel sees the connection's IP. No forwarded-header handling is configured, so `RemoteIpAddress` cannot be set by a client.
- `ms-auth` turns Keycloak's `invalid_grant` into a general 401 on sign-in.
- The site's error handling has one "service unavailable" message for everything but 400, 401, 404, and 409.
- Requirements are in the three delta specs. Motivation is in proposal.md.

## Goals / Non-Goals

**Goals:**
- Stop password guessing against one account from many IPs, and many accounts from one IP.
- Slow down bulk registration and phone number collection.
- Ordinary people, including several behind one mobile operator's IP, should not notice.

**Non-Goals:**
- Stopping a determined attacker with many IPs: limits raise the cost, they do not prevent it.
- Shared counters across gateway replicas, trusted proxy headers, and CAPTCHA.

## Decisions

### 1. Two layers

| Attack | IP limit | Account lock |
|---|---|---|
| One IP, many accounts | caught | not caught |
| Many IPs, one account | not caught | caught |
| Collecting numbers | slowed | not relevant |
| Bulk registration | slowed | not relevant |

Chosen by the user over either layer alone.

### 2. Limits live in the gateway, using ASP.NET Core's rate limiter

The gateway sees every request, so the services need no change, and rejected requests cost them nothing. The built-in limiter adds no package or infrastructure. YARP attaches a named policy to a route through `RateLimiterPolicy` in its config, and routes without one stay unlimited, which is how the website and files stay free.

Alternative: in each service. Rejected: repeated code, and work is already done by the time the request is refused.

### 3. Policies and routes

| Policy | Default | Routes |
|---|---|---|
| `login` | 10 per 60 s | new route `/api/v{version}/Auth/Login` |
| `register` | 5 per 600 s | new route `/api/v{version}/Auth/Register` |
| `seller` | 30 per 60 s | new route `/api/v{version}/Auth/Sellers/{**rest}` |
| `api` | 300 per 60 s | existing `auth`, `brand`, `model`, `city`, `listing`, `listing-image` |
| none | | `web`, `listing-images`, `/health`, the `/api/` 404 |

The specific routes win over `Auth/{**rest}` because routing prefers the more specific template. All forward to the `ms-auth` cluster unchanged.

Fixed window with `QueueLimit` 0: simple, and `Retry-After` comes straight from the limiter's metadata. A burst of up to twice the limit across a window boundary is acceptable here. Each policy partitions by `RemoteIpAddress`, with IPv4-mapped addresses normalized and a missing address sharing one partition.

`Refresh` stays under `api`: a tight limit there would throw active people out, and rotation already limits what a stolen token can do.

Numbers are in `appsettings.json` under `RateLimiting`, the same in every environment. Rejections respond with ProblemDetails, in the style of the services.

Code: `RateLimitExtensions.InitRateLimiting` in `Extensions/V1` and `RateLimitOptions` in a `RateLimiting/V1` folder, next to `ServiceDiscovery/V1`. `Program.cs` calls `app.UseRateLimiter()` before `MapReverseProxy()`.

### 4. Why these numbers are gentle

Mobile operators often put many customers behind one IP, so a tight per-IP limit would block innocent people. Account locks cover the guessing that a gentle IP limit lets through. The website counts: a page view uses 1 to 3 API requests, and uploading 20 images uses 20 to 40, well inside 300.

### 5. Only the connection IP counts

Trusting `X-Forwarded-For` would let any client choose its own bucket and defeat the limit. So nothing calls `UseForwardedHeaders`.

Consequences, tested with a throwaway container:

- **Docker Desktop:** every client appears as `192.168.65.1`, so on this Mac the Development and Production stacks treat all clients as one. Separate clients can only be tested in Local, where the gateway runs directly on the host and sees real addresses.
- **A real Linux server:** the real address is expected to arrive, but this is not tested. It belongs on the checklist for moving to a server.
- **A load balancer in front of the gateway:** would make every client one IP. Trusted proxy settings must come with it.

### 6. Keycloak settings

Set in `realms.sh` for both realms:

| Setting | Value |
|---|---|
| `bruteForceProtected` | `true` |
| `permanentLockout` | `false` |
| `failureFactor` | `5` |
| `waitIncrementSeconds` | `60` |
| `maxFailureWaitSeconds` | `900` |
| `maxDeltaTimeSeconds` | `43200` |
| `quickLoginCheckMilliSeconds` | `0` |
| `minimumQuickLoginWaitSeconds` | `0` |

Tested on a throwaway Keycloak 26.4.7:

- **The quick login check must be off.** With its default, two wrong passwords within a second lock the account for 60 seconds, so anyone could lock any account with two requests. The IP limit handles fast guessing instead.
- With it off, 4 fast wrong passwords do not lock, and a correct one then resets the count.
- A locked account answers `401 invalid_grant, Invalid user credentials`, identical to a wrong password or an unknown user. `ms-auth` needs no change.
- A refresh token of an open session works during a lock.
- The lock ends on its own after the wait.

### 7. Website

`ApiError` already carries the status. A helper maps a failure to a message: 429 gives the new "too many requests" text, and everything else keeps today's text. It replaces the direct use of the unavailable message in sign-in, registration, the listing form, the profile page, uploads, the seller block, listing and image actions, and `ErrorRetry`.

`requestRefresh` already signs out only on 401, so a 429 cannot end a login; a test proves this.

### 8. Tests and the limits

The existing website suites sign in many times a minute from one IP, which the real limit would stop, correctly. So:

- **Local:** the test stack raises limits with environment variables on the gateway.
- **Docker:** suites run with a compose override file kept in the scratchpad, which raises the limits. It is not committed.
- **The real numbers:** a separate test runs against the default settings in Local, Development, and Production.

## Risks / Trade-offs

- **Locking someone else out:** anyone can keep an account locked by sending wrong passwords. → The lock is short and open sessions keep working. The IP limit slows it from one IP.
- **Correct password refused during a lock:** the person sees "wrong credentials". → The message includes a hint to wait. Accepted so that a lock reveals nothing.
- **Shared IPs:** a busy operator IP can reach the sign-in limit. → Gentle numbers, changeable in settings.
- **Counters reset on restart, and are per replica:** the real limit multiplies by the number of gateways. → One gateway today; Redis when that changes.
- **Docker Desktop hides client IPs:** all local clients share a bucket, so heavy manual testing on this Mac can hit 429. → Wait a minute, or raise the limit locally.
- **Direct calls to services:** skip the limit. → Docker publishes no service port. In Local they are reachable by design.
- **Keycloak's own login screens:** skip the gateway's limit, on port `8080`. → Account locks still apply. The port must be closed on a real server, along with `5432`.

## Migration Plan

Additive. `keycloak-setup` applies the realm settings on the next start. Rollback: remove the route policies and `UseRateLimiter`, and set `bruteForceProtected` to false.

## Open Questions

- Whether a real Linux server shows real client IPs behind Docker's published port. This cannot be tested on this Mac and does not change the design: it is checked when moving to a server.
