# Duluka Account — v0 auth/security slice

Central account system for the Duluka ecosystem (NVIDIA ShadowPlay, Duluka Web,
future products). **This directory is a separate product — no ShadowPlay code
depends on it and none was modified.**

Architecture discovery: see the C/4 + C/5 + C/6 v0.1 design (data model, server
modules, security model). Implemented here: the **C/6 auth/security slice**.

## Implemented (v0)

- **GitHub OAuth** — Authorization Code + PKCE (S256) + one-time state (10 min TTL),
  server-side code exchange, redirect-uri = the single configured value,
  identity = GitHub `id` (numeric, stable; login/email are display-only)
- **Account provisioning** — unknown identity creates a NEW account; known
  `(ProviderKey, ProviderUserId)` logs into ITS account; the unique-anchor race
  converges on one account (constraint + fallback re-read)
- **Sessions** — opaque 256-bit tokens (`duluka_st_…`), SHA-256 hashed at rest
  (raw token never stored), device-bound, exactly ONE active session per
  device (a second login supersedes the first, contract §5.2), sliding 7d /
  absolute 30d (both configurable), revoke per-session / per-device /
  logout-all
- **Devices** — client-generated 256-bit device key (hashed at rest), revoked
  keys can never silently re-register, cross-account key reuse rejected
- **Provider unlink** — last-provider guard (409), credential row destroyed,
  sessions issued via that link revoked; a re-login with a previously
  unlinked identity returns to the SAME account (a returning identity
  re-enters its own home on a fresh link period; the anchor is a partial
  unique index over ACTIVE links, schema v2 migration rebuilds v1
  databases). A never-seen identity with a device key bound to another
  account is still refused pre-persist (no orphan account/link).
- **NVIDIA provider = reserved** — `ProviderKeys.Nvidia` exists but has NO auth
  flow. Hardware-derived identity (GPU UUID/serial/driver/fingerprint) is
  explicitly banned as an auth identity (spoofable = bypass).
- **Native accounts + profile** — username/password (PBKDF2, canonical anchor),
  display name + profile image (avatar data URL) editable via
  `PUT /v1/account/profile`; the profile is PRESENTATION-ONLY — AccountId,
  username, devices and sessions are structurally untouched by a profile edit.
  Profile image: `data:image/png|jpeg|webp;base64` only, 256 KB decoded cap,
  validated before base64 decode; schema v4 adds the column additively
- **Rate limiting** — per-IP fixed window: 10/min on auth starts, 240/min on API
- **Error envelope (contract §7.1)** — every response carries
  `ok`, `reqId` (client-issued `X-ReqId`, echoed verbatim), `errorCode`,
  `httpStatus`, `retryable`; payloads sit under `resource`. Errors use the
  §7.2 registry codes (`auth.session_expired`, `auth.session_revoked`,
  `perm.device_removed`, `nf.link`, `conflict.link_conflict`, `server.internal`)
  plus documented extensions for 400-family input errors (`invalid_*`,
  `provider_*`, 501 `provider_reserved`, 429 `server.rate_limited`)
- **Secret redaction** — single implementation (`Security/Secrets.cs`); raw
  tokens/codes/state never logged. The OAuth `state` is returned RAW in the
  start response (it is the client's own correlation value; redaction is a
  LOG rule, §9.2)

## NOT implemented in v0 (deliberate)

- Sync endpoints (`/v1/sync/{product}`) — schema table exists, endpoints next phase
- Provider token persistence — tokens are used transiently and discarded
  (`CredentialReference` table exists, v0 writes no rows)
- Migration framework — v0 applies idempotent DDL at startup + `SchemaHistory`
- E2E encryption for sync blobs — open decision
- Web client (cookie transport) — v0 returns the session token in the JSON body
  (ShadowPlay-first)

## Configuration

| Key | Source | Notes |
|---|---|---|
| `GitHub:ClientId` | config or env `DULUKA_GitHub__ClientId` | non-secret |
| `GitHub:ClientSecret` | **env `DULUKA_GitHub__ClientSecret` only** | never in config files |
| `GitHub:RedirectUri` | config | must exactly match the GitHub app setting |
| `Database:Path` | config | default `AppData/DulukaAccount.db` (gitignored) |

## Run

```text
dotnet run --project Duluka/Duluka.Server -c Release
dotnet run --project Duluka/Duluka.Server.Tests -c Release   # 28 tests (service-level + live HTTP + CWD independence)
dotnet run --project Duluka/Duluka.Http.Integration.Tests -c Release  # 102 black-box HTTP tests (real server + real SQLite)
```

## Black-box HTTP regression (C/2)

`Duluka.Http.Integration.Tests` launches the real server binary per test group (fresh
SQLite per group, loopback bind, per-group auth-start rate-limit budget) and pins the
live HTTP contract: health/readiness, auth flows, account/provider/device/session
lifecycles, revoke cascades, idempotency, store-level CAS races, rate limiting, restart
persistence, the 401/404/409 matrix and the error envelope. GitHub identity exchange is
never faked; the one egress-dependent group is honestly SKIP-gated.

Contract violations found by the suite are tracked in the mismatch ledger
(M-1..M-11 — M-1/M-3/M-4/M-5/M-8 fixed and now contract-PASS tests;
M-2/M-6/M-7/M-9/M-10/M-11 still open) in
[Duluka.Http.Integration.Tests/TEST-MATRIX.md](Duluka.Http.Integration.Tests/TEST-MATRIX.md) —
each is a fix required in `Duluka.Server`, not in the tests. A test passes only while the
tracked violation still reproduces exactly, so the ledger can never go stale silently.

Endpoints: `POST /v1/auth/github/start` → GitHub authorize URL →
`POST /v1/auth/github/callback {code, state, deviceKey}` → `{sessionToken,…}`;
then `GET /v1/account/me`, `PUT /v1/account/profile`,
`GET|POST|DELETE /v1/account/providers[…]`,
`GET /v1/account/devices`, `POST /v1/account/devices/{id}/revoke`,
`POST /v1/auth/session/{refresh,revoke}`, `POST /v1/auth/sessions/revoke-all`.

## Ports

- **Duluka HTTP API binds `http://127.0.0.1:5115`** (appsettings `Urls`).
- Port **5000 is RESERVED by the ShadowPlay TCP Hub** on the same machine —
  never bind Duluka there (a hub/Duluka collision silently kills the
  overlay's client connections).
- Clients override the base with the `DULUKA_API_BASE` environment variable.
