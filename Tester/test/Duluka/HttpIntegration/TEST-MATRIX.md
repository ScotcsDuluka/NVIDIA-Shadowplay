# Duluka HTTP Integration — Test Matrix & Regression Report (C/2)

**Suite:** `Duluka/Duluka.Http.Integration.Tests` · Run:
`dotnet run --project Duluka/Duluka.Http.Integration.Tests -c Release`

**This revision verifies the C/5 reconcile commit (`bf54d08`, the `ee07eb7` lineage)
against the baseline HTTP suite from `038589c`.** The server was reworked to the frozen
C/1 contract: §7.1 envelope (`{ok, reqId, errorCode, httpStatus, retryable, conflict,
message}`, success payload under `resource`), §7.2 registry codes, raw OAuth state,
per-device session supersession, schema v2 (partial unique index + v1→v2 migration),
429 envelopes, catch-all 500 envelopes on the auth callbacks.

**Under test:** the REAL `Duluka.Server` binary launched as a real process per test group
(loopback-only bind) against its REAL SQLite store. Nothing about production behavior is
mocked: GitHub identity exchange is never faked (pre-exchange groups run against an
*unconfigured* server; the one network-dependent group is egress-gated with honest SKIP);
authenticated surfaces are exercised via rows seeded into the live database through the
production data layer. Since the reconcile, `CreateSession` supersedes the previous
session per device (§5.2), so fixtures seed ONE live session per device.

**Honest before-state:** the 038589c suite (after a compile-only fix for the new
`RefreshSession` parameter — no assertion changes) scored **33 passed / 62 failed** against
the reconciled server. Every failure was triaged into one of: obsolete pin (production
fixed or changed the wire shape), fixture conflict with the new supersede rule, or a still-
violated contract. No test was altered merely to make it green — the ledger below is the
documentation of that triage.

**Determinism law:** one fresh server process + fresh SQLite file per group; explicit
per-group auth-start rate-limit budget (G0=3, G1=7, G2=7, G3=5, G7=9, G9=11-by-design,
G10=2, G11=8 — all ≤ 10); every expiry assertion is tick-exact arithmetic (no sleeps).
Verified: three consecutive full-suite runs produce identical results including the
mismatch ledger.

**Outcome vocabulary (C/4 honesty):** PASS = behaves as asserted · FAIL = drift (suite
exits non-zero) · MISMATCH CONFIRMED = tracked contract violation, passes only while it
still reproduces exactly · SKIP = environment not capable.

---

## 1. M-1..M-9 verification verdict (C/5 reconcile vs the previous C/2 report)

| ID | Violation (previous report) | Verdict | Evidence in this suite |
|---|---|---|---|
| M-1 | Malformed/empty JSON → naked 500, empty body | **FIXED** (dedicated commit after the verification pass) | S-6/S-7/S-8/S-9/S-10 + ENV-1: every body-parsing endpoint (`start`, `callback`, link-flow `start`, `complete`) now answers 400 `bad_request` in the canonical §7.1 envelope (ok=false, reqId echo, httpStatus=400, retryable=false, conflict, message) for malformed/empty/truncated bodies; valid bodies unchanged |
| M-2 | Suspended account → 401 (contract §6.3: 403 `account_suspended`) | **STILL VIOLATED** | ME-9: suspended → 401 `auth.session_expired` (code updated, status still wrong) |
| M-3 | No one-active-session-per-device | **FIXED** | ME-8: `CreateSession` now revokes-before-insert; superseded token → 401 `auth.session_revoked`, new token → 200 |
| M-4 | Envelope `{error:{code,message}}`, no §7.1 fields, no registry codes | **FIXED** | ENV-2 + every `ExpectErr`: full §7.1 envelope, X-ReqId echoed verbatim, registry codes on the wire; `invalid_*`/`provider_reserved` remain as documented §7.2 extensions |
| M-5 | Re-unlink → 400 (C/2: 409 resource-shape) | **FIXED** | UNL-3/UNL-6: 409 `conflict.link_conflict` (the §7.2 registry family code — accepted alias for C/2's `provider_not_linked` / `last_provider_cannot_unlink` literals) |
| M-6 | Device-key UNIQUE race escapes `LoginOrLink` | **STILL VIOLATED** (severity reduced) | RACE-DEV evidence: 7/8 threads still hit the unguarded `CreateDevice` UNIQUE violation; the callback's new catch-all now answers a well-formed 500 envelope instead of a naked one — the functional loss (a concurrent same-device login fails instead of converging) remains |
| M-7 | No endpoint carries the `api` rate-limit policy | **STILL VIOLATED** | RL-3: 260 consecutive `/me` hits, zero 429 |
| M-8 | 401 root causes conflated into `session_invalid` | **FIXED** | ME-7: revoked → `auth.session_revoked`, expired/unknown/missing → `auth.session_expired` (frozen §7.2 registry has no `unknown_session` code; the conflation of unknown with expired is the documented registry decision) |
| M-9 | Second revoke → 401 (C/2 SES-2b: idempotent 200) | **STILL VIOLATED** | SES-5: second revoke → 401 `auth.session_revoked`; no-resurrect security property holds |

**Score: 5 of 9 fixed (M-1, M-3, M-4, M-5, M-8); 4 still violated (M-2, M-6, M-7, M-9).**

## 2. Newly discovered mismatches (this pass)

| ID | Violation | Evidence |
|---|---|---|
| M-10 | 409s carry `conflict=null`; §6.2 requires the CURRENT server resource + version attached so the client can re-apply or drop deterministically | ENV-3 (`last-provider` 409) |
| M-11 | `/v1/account/providers/{provider}/complete` hardcodes `auth.session_expired` for a REVOKED binding session, while every other endpoint distinguishes revocation via `DeadSessionCode` | PRV-9 |

**Carried findings:** F-3 — `revoke-all` counts already-expired-but-unrevoked sessions
(no expiry filter; SES-6). **Resolved findings:** F-1 — state now returned RAW (S-4 pin
inverted); F-2 — missing-token 401 codes are now uniform (PRV-0/COMP-1); F-4 — callback
failures now converge to a `server.internal` envelope (§5.4-2).

## 3. Additional checks required by this pass

- **428 missing If-Match / 409 stale revision / conflict resource on sync** — sync
  endpoints remain ABSENT (only the `SyncProfile` DDL exists). COMP-2 pins the tripwire:
  `/v1/sync*` → 404; the SYN matrix (428 `missing_if_match`, 409 `stale_version`,
  conflict attachment) is executable in `Duluka.Account.Tests` and MUST be enabled here
  the moment the endpoints ship. Blocked on owner decisions O-1/O-2, unchanged.
- **Provider-link race** — RACE-5 (store): `CreateLink` loser converges on the winner's
  row and the endpoint's ownership check turns it into 409 — no silent cross-account
  merge. RACE-6: v1→v2 partial unique index removes the permanent-500 on re-login with a
  previously unlinked identity; documented v2 behavior: the unlinked identity reads as
  unknown and provisions a NEW account.
- **Device revoke cascade** — DEV-2/3/4/5/8: exact cascade count, no collateral across
  devices, idempotent re-revoke, own-device self-cascade.
- **Cross-tenant hiding** — DEV-7/UNL-5: cross-tenant device/link access → 404 with the
  registry codes, no leak, no collateral (§6.3 cross-tenant rule).
- **Restart persistence** — RE-1..14 across two process generations on one SQLite file:
  sessions, revocations (session + device) and the tick-exact sliding cap persist;
  in-memory OAuth flow state does not (documented v0 design → `invalid_state`);
  `SchemaHistory` = v2 (v1→v2 migration verified on a fresh store).
- **Auth start throttling** — RL-1: 10th request passes, 11th → 429 `server.rate_limited`
  envelope, retryable=true, X-ReqId echoed; per-policy isolation (healthz unaffected).
- **GitHub callback/state handling** — C-2..C-5, E-1/E-2: validation order, unknown state,
  single-use state even when the exchange fails, raw-code never echoed in body or log
  (egress-gated RUN / honest SKIP).

## 4. Counts

| Suite | Baseline (038589c era) | Current HEAD (post C/5) |
|---|---|---|
| `Duluka/Duluka.Server.Tests` | 16 / 0 | **24 / 0** (C/5 added 8) |
| `Duluka.Account.Tests` (C/2 executable spec) | 19 / 0 (+1 skipped group) | 19 / 0 (+1 skipped group) |
| `API.Hub.Boundary.Tests` | 10 / 0 | 9 / 0 (+1 env-dependent skip: live-hub ping, hub not running) |
| **`Duluka/Duluka.Http.Integration.Tests`** | **95 / 0** (038589c) | **102 / 0** (verification pass 99, M-1 fix pass +3: S-8 truncated, S-9 callback, S-10 link-start malformed) |
| **Total** | 140 | **151** |

**HTTP-suite before/after for this task: 95 tests → 99 tests (verification pass) → 102
tests (M-1 fix pass); against the CURRENT server the unmodified baseline scored 33/95 —
the updated suite scores 102/102 with 4 still-violated + 2 newly-discovered tracked
mismatches (M-2, M-6, M-7, M-9, M-10, M-11).**

**Determinism evidence:** three consecutive full runs → identical `RESULT: 102 passed,
0 failed` and identical mismatch ledgers; no timing-based assertions; the only
environment-dependent group (G3) is honestly SKIP-gated on github.com egress.

**M-1 fix note (dedicated commit):** the only production change since the verification
pass is `Wire.TryParseBodyAsync` — a `JsonException`-scoped body-parse guard at the four
request-parsing sites, answering 400 `bad_request` in the canonical envelope. Valid-body
behavior, all other endpoints, and the remaining mismatches (M-2, M-6, M-7, M-9, M-10,
M-11) are untouched — fixes for those still belong to C/5.

## 5. G15 — GitHub bootstrap → first-time setup (account-setup regression)

Added with the "Set button does nothing" fix (client + suite). One fresh server
per group; NO auth-start usage (bootstrap state is seeded through the production
data layer, same as G4/G14); `budget: 0`.

| ID | Scenario | Expected |
|---|---|---|
| SETUP-1 | bootstrap account (link, NO native credential) → `/me` | `username` null (setup REQUIRED), displayName independent |
| SETUP-2 | invalid setup input (short password / bad username) | 400 `invalid_password` / `invalid_username`; nothing persisted |
| SETUP-3 | first-time `POST /v1/account/password` `{username,newPassword}` | 200 `{changed:true, username}`; `/me` reports the username |
| SETUP-4 | native login with chosen credentials (case variant) | 200 on the SAME accountId; wrong password → generic 401 `invalid_credentials` |
| SETUP-5 | username immutability: repeat first-time body / duplicate username | 400 `invalid_credentials` (first-time branch closed) / 409 `conflict.username_taken`; `/me` username unchanged |
| SETUP-6 | display-name separation | displayName still the bootstrap value, ≠ username |
| SETUP-7 | server (client) restart → `/me` + native login | username persists; login works; setup never asked again |
| SETUP-8 | log sweep | setup password never in any response body or server log |
