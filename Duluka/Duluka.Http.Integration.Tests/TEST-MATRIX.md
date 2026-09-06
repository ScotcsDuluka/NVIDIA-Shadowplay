# Duluka HTTP Integration — Test Matrix & Regression Report (C/2)

**Suite:** `Duluka/Duluka.Http.Integration.Tests` · Run:
`dotnet run --project Duluka/Duluka.Http.Integration.Tests -c Release`

**What is under test:** the REAL `Duluka.Server` binary launched as a real process per
test group (loopback-only bind), against its REAL SQLite store. Nothing about production
behavior is mocked: GitHub identity exchange is never faked (groups that need
deterministic pre-exchange behavior run against an *unconfigured* server; the one
network-dependent group is egress-gated with honest SKIP), and authenticated surfaces are
exercised via rows seeded into the live database through the production data layer
(`Database`/`Secrets.Sha256Hex`), exactly like a client that provisioned offline.

**Determinism law:** one fresh server process + fresh SQLite file per group; the
in-memory per-IP rate limiter (10/min on auth-start endpoints) is respected by an
explicit per-group budget (G0=3, G1=7, G2=7, G3=5, G7=9, G9=11-by-design, G10=2, G11=8 —
all ≤ 10). Every session-expiry assertion is tick-exact arithmetic on stored timestamps
(no sleeps). Verified: byte-identical output across consecutive runs.

**Outcome vocabulary (C/4 honesty):**
- `PASS` — production behaves exactly as asserted
- `FAIL` — production drifted from a pinned behavior (alarm; suite exits non-zero)
- `MISMATCH CONFIRMED` — the behavior violates the v0.1 contract / C/2 spec in a way the
  mismatch ledger tracks. The test passes ONLY while the violation still reproduces
  exactly; fixing or changing it fails the test and forces ledger reclassification.
- `SKIP` — environment not capable (e.g. no github.com egress); never a fake PASS

---

## 1. Matrix (95 tests, 12 groups)

| Group | Area | Tests | Requirement coverage |
|---|---|---|---|
| G0 | health / readiness / transport | 9 | `/healthz` live, `/healthz/ready` schema=1, unknown-route 404, method 405, reserved providers (nvidia/unknown → 501), case-insensitive provider route, loopback-only bind (R1), log-secret sweep |
| G1 | auth start validation | 8 | deviceName/deviceKey limits + 42/43-char boundary, authorize-URL shape (PKCE S256, state, scope), no-upper-bound pin, malformed/empty JSON body, redaction sweep |
| G2 | auth callback + exactly-once | 6 | missing code, short deviceKey, unknown state, **state single-use even when the exchange fails** (C/3), redaction sweep (unconfigured server → deterministic, no network) |
| G3 | real provider exchange | 2 | bogus code → 400 exchange-rejected envelope (never 5xx), state still single-use, raw code never echoed in body or log. **Egress-gated: RUN when github.com reachable, SKIP when not** |
| G4 | account / 401 matrix | 11 | `/me` full shape; 401 for no-header / empty-Bearer / non-Bearer scheme / unknown token / **expired** / **revoked**; suspended-account behavior; multi-session-per-device probe; body + log secret sweeps |
| G5 | device + revoke cascade | 9 | device list (own only, revoked visible), revoke → exact cascade count (2), both victims dead, no collateral, revoke-twice idempotency (0 re-counted), unknown → 404, **cross-tenant → 404 with no leak**, own-device revoke kills own session |
| G6 | session lifecycle | 9 | refresh (sliding never shortens, tick-exact), **absolute cap createdAt+30d (tick-exact)**, expired/unknown/no-token 401 codes, revoke → sibling survives, revoke-twice, revoke-all exact count, **revoke-all idempotency (dead sessions never re-counted)** |
| G7 | provider links + unlink | 13 | link list (own Active only), link-flow start 200 + state-in-URL, complete pre-network failure (`github_not_configured`), **complete state single-use**, complete after binding-session death → 401, unlink → via-link session cascade (exactly 1), **last-provider 409**, unknown 404, **cross-tenant 404**, re-unlink behavior |
| G8 | CAS races (real store) | 4 | **8 parallel first-logins same identity → exactly ONE account, orphan cleaned** (unique-anchor CAS); **8 parallel same NEW device key → one device row, only UNIQUE(19) violations may escape** (M-6 evidence: 7/8 manifest); parallel revoke+refresh → no resurrect; sequential duplicate identity → same account |
| G9 | rate limiting | 4 | **auth-start fixed window: 10 pass, 11th → 429** (empty body pin); per-policy isolation (healthz unaffected); untagged endpoints have NO api limiter (260 hits, zero 429 — M-7) |
| G10 | restart behavior | 14 | same SQLite file across two process generations: **sessions survive**, **revocations survive** (session + device), **sliding-cap expiry persists tick-exact**, OAuth flow state does NOT survive (documented v0 design → `invalid_state`), ready/schema=1 both generations, SchemaHistory v1, dual-generation secret sweep |
| G11 | envelope + contract sweep | 6 | 9-case error-envelope corpus (400/401/404/409/500/501, single schema, stable codes, no internals), envelope-shape mismatch (M-4), link-start auth gates, **401 matrix over all 9 authenticated surfaces**, **sync tripwire** (`/v1/sync*` absent → 404; tripwire forces the SYN/If-Match/428 matrix to be enabled the moment sync ships) |

HTTP-status coverage over the live server: **200, 400, 401, 404, 405, 409, 429, 500, 501**.
**403**: not reachable over the v0 HTTP surface (the only production 403, `device_revoked`
at callback, sits behind a successful provider exchange) — the contract-required 403 for
suspended accounts is a confirmed mismatch (M-2). **428 + sync (If-Match/409
stale_version)**: blocked on the unimplemented sync slice (owner decisions O-1/O-2); the
in-memory executable spec remains `Duluka.Account.Tests` (SYN-1..3, RACE-2), and COMP-2
is the tripwire that forces enabling the HTTP matrix when the endpoints appear.

---

## 2. Failures found (mismatch ledger — every one is a fix required OUTSIDE this suite)

| ID | Violation | Evidence (live) | Cross-ref |
|---|---|---|---|
| M-1 | Malformed/empty JSON body on any body-parsing endpoint → **naked HTTP 500 with empty body** (unhandled `JsonException`) | S-6, S-7, ENV-1 corpus | *not in R2 §14* — found by this suite |
| M-2 | Suspended account → 401 `session_invalid`; contract §6.3 requires **403 `account_suspended`** (identity established, action not permitted) | ME-9 | R2 §14 G-5 |
| M-3 | **No one-active-session-per-device enforcement**: two concurrent live sessions on one device both validate; contract §5.2 requires second login to revoke the previous and answer 409 `session_conflict` | ME-8 | *not in R2 §14* — found by this suite |
| M-4 | Error envelope is `{error:{code,message}}` only — no `ok/reqId/errorCode/httpStatus/retryable` (§7.1), codes outside the §7.2 registry | ENV-2 | R2 §14 G-6 (partial) |
| M-5 | Re-unlink of an already-unlinked provider → 400 `link_already_unlinked`; C/2 pins resource-shape **409 `provider_not_linked`** | UNL-6 | adjacent to R2 §14 G-2 |
| M-6 | `LoginOrLink` has **no `SqliteException(19)` handler on `CreateDevice`**: concurrent first-login with the same NEW device key lets the UNIQUE violation escape (7/8 threads in the race probe) → would surface as HTTP 500 at the callback endpoint. The `UpsertLink` path already converges correctly — mirror it | RACE-DEV evidence | adjacent to R2 §14 G-4 (`device_key_in_use` variant) |
| M-7 | **No endpoint carries the `api` rate-limit policy** (C/6 claims 240/min on the authenticated API): 260 consecutive `/me` hits, zero 429 | RL-3 | R2 §14 G-6 (partial) |
| M-8 | 401 root-cause codes conflated: revoked AND expired (and unknown) sessions all answer `session_invalid`; §7.2 registry (`auth.session_revoked` / `auth.session_expired`) and the C/2 root-cause precedence are not implemented | ME-7 | R2 §14 G-1 |
| M-9 | Second revoke of an already-revoked session → 401; C/2 SES-2b requires an **idempotent 200** (goal state already reached). The security property (no resurrect) holds | SES-5 | R2 §14 G-3 |

**Additional findings (pinned, not contract violations):**
- **F-1** — `POST /v1/auth/*/start` returns the OAuth `state` field REDACTED; the usable
  state only travels inside `authorizationUrl` (S-4 pin). Anti-replay-safe but a
  client-side footgun; if this pin ever fails, the field became usable.
- **F-2** — 401 code inconsistency for a missing token: `session_missing` on
  refresh/revoke/link-start, `session_invalid` on me/providers/devices/unlink/
  device-revoke/revoke-all (COMP-1 pins all nine).
- **F-3** — `revoke-all` counts already-expired-but-unrevoked sessions in
  `revokedSessions` (no expiry filter in `RevokeAllForAccount`); harmless but the
  reported number overcounts (SES-6).
- **F-4** — `ExchangeForIdentityAsync` does not catch transport failures
  (`HttpRequestException`): an unreachable github.com during a callback would produce
  another naked 500 instead of a retryable server envelope (code-path evidence; not
  deterministically reachable with egress present).

---

## 3. Fixes required elsewhere (owner work list, ordered)

1. `Program.cs` — wrap body parsing; map malformed/empty JSON to 400 envelope (M-1).
2. `Data/Database.cs` — surface the validation failure CAUSE; `Program.cs` — map
   suspended → 403, revoked/expired/unknown → distinct 401 codes per §7.2/C-2 precedence
   (M-2, M-8); R2 G-1/G-5.
3. `Program.cs` (`/v1/auth/session/revoke`) — idempotent 200 on already-revoked (M-9); R2 G-3.
4. `Data/Database.cs` + session creation path — enforce one active session per device with
   409 `session_conflict` (M-3).
5. `Auth/GitHubOAuth.cs` (`AccountProvisioningService.LoginOrLink`) — catch
   `SqliteException(19)` on `CreateDevice` and converge (mirror `UpsertLink`), map
   `device_key_in_use` to a 4xx (M-6); R2 G-4.
6. `Program.cs` — attach the `api` rate-limit policy to all authenticated endpoints (M-7).
7. Envelope: adopt §7.1 shape + registry codes, or freeze the current shape as the
   documented v0 exception (M-4, M-5, F-2 — owner call); R2 G-2/G-6.
8. Sync slice (O-1/O-2) — when it lands, enable the SYN/428 matrix here (COMP-2 trips); R2 G-7.
9. `Duluka.Account.Tests/DulukaHttpContractProbe.vb` — retarget at the real §13 routes
   when `DULUKA_BASE_URL` testing starts; R2 G-8.

**Production code was not modified by this suite.** No test was adjusted to fit a bug;
every bug is pinned as a tracked mismatch.

---

## 4. Counts

| Suite | Before (baseline) | After |
|---|---|---|
| `Duluka/Duluka.Server.Tests` (C/6, in-process) | 16 passed / 0 failed | 16 / 0 (unchanged) |
| `Duluka.Account.Tests` (C/2 executable spec) | 19 passed / 0 failed (+1 skipped group) | 19 / 0 (unchanged) |
| `API.Hub.Boundary.Tests` (R1) | 10 passed / 0 failed | 10 / 0 (unchanged) |
| **`Duluka/Duluka.Http.Integration.Tests` (this suite)** | — | **95 passed / 0 failed / 0 skipped** |
| **Total** | **45** | **140** |

Determinism proof: consecutive full-suite runs produce **byte-identical output**
(GUID-masked), including the mismatch section. No timing-based assertion exists; the only
environment-dependent group (G3) is honestly SKIP-gated on github.com egress.

Exit code: `0` while every pin holds (mismatches included — they are tracked ledger
entries, printed prominently); non-zero the moment production drifts in either direction.
