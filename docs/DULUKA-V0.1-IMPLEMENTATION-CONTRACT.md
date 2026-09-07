# DULUKA ACCOUNT — v0.1 IMPLEMENTATION CONTRACT

Status: **FROZEN — R3 (2026-09-06, final audit amendment). R2 was reconciled against the
pre-integration server; the owner then re-decided three R2 rulings via commit `bf54d08`
("C/6: reconcile Duluka auth slice with frozen C/1 contract", green at 24/24 incl.
HTTP-INT-1..4): (1) the §7.1 wire envelope is the `ok/reqId/errorCode` shape, (2) the
§7.2 registry is the dotted vocabulary, (3) §5.2 is one-active-session-per-device, and
(4) O-6 is answered YES — `reqId` is on the wire. R3 amends those sections to match.
R3 is a docs-only amendment applied by the C/1 final audit (reconciled again to the
C/2 verification pass `e3b6432`: suite reclassified to 99 tests, 99/99 PASS).
Conformance gaps are enumerated in §14 (live status of the C/2 ledger M-1..M-11).**
Scope owner: ScotcsDuluka · R1 synthesized from C/4 + C/5 + C/6 discoveries; R2 reconciled
read-only against the actual repository state (working tree of `Engine-Rebuild-Stabilization`):
`Duluka/Duluka.Server` (auth/account implementation), `Duluka/Duluka.Server.Tests` (16/16 PASS),
`Duluka.Account.Tests` (19/19 PASS, HTTP probe group skipped). Every rule cites its origin.
Where no evidence decides, the item is marked **OPEN (owner)** — it is NOT invented here.

---

## 0. SCOPE / NON-GOALS

In scope: identity model, identifier canonical forms, state machines, concurrency control,
HTTP semantics, error envelope, module boundaries, at-rest rules.

Out of scope (hard):
- No UI, no changes to ShadowPlay production code.
- **No NVIDIA implementation decisions.** The NVIDIA capture/encoder modules are bound
  only by the module boundary rules in §3; every internal of `DdagrabBackend`,
  `NvencEncoderBackend`, `LiveMuxSession`, `CaptureSession` stays sovereign.

---

## 1. DISCOVERY GROUNDING (evidence base)

| Origin | Discovery | Contract rule derived |
|---|---|---|
| C/4 (`1c3b58f`, F-01/F-02) | Honest outcomes: RUN/SKIP/FAIL are distinct; a claim ("saved/valid") is backed by real evidence (probe), never existence-only | §6.4 truthfulness rules, §7 success-evidence rule |
| C/5 (`de2052e`, RECORD_START H1) | ONE validator seam per input class, BEFORE anything downstream exists; fixed value vocabulary; unknown-value fallback documented (`OverlayConfig.GetEngineMode`) | §3.7 identifier validation, §5 state vocabulary, §7.2 code registry |
| C/6 (F-05/F-07/C-1/C-2, `5f4bf61`/`23c74f7`/`3cee799`) | State commit atomic with effect; failure states CONVERGE (never wedge); timeout = intermediate state, not a lying terminal; ownership for every spawned thing; soak provenance | §5 state machines, §6.5 concurrency, §8 provenance |
| C/2 (`5c9c174`, `dd0692d`, LoopbackGate, API.Hub.Boundary.Tests, `Duluka.Account.Tests`) | Loopback-only transport; `reqId` correlation on the HUB channel; reflected-output encoding; secret redaction in logs; hub is an UNAUTHENTICATED relay (S1); executable account/sync matrix (statuses + stable codes) | §3.7 transport boundary, §7 envelope, §9 at-rest/redaction, §6 sync CAS |
| C/3 (`0eb33d9`, SessionEndBroadcastPolicy) | Exactly-once terminal events; expiry and async-failure are first-class terminal triggers; no double broadcast | §5.4 session terminal semantics, §6.6 retry/idempotency |
| F-S2 (`42c8d2e`, GHS suite) | OAuth token at-rest = DPAPI CurrentUser (client side), plaintext never on disk, legacy migration scrubs (incl. `.bak`), persistence justified by startup consumer | §4.4 client-side token at-rest, §9 |
| C/6 slice (commit `e8d7d09`, `Duluka/Duluka.Server`) | Implemented auth/account surface: GitHub OAuth PKCE, opaque prefixed identifiers, hash-at-rest session/device secrets, revoke cascades, unlink guards, rate limits | §3 identifiers, §4 at-rest, §5.1/5.2/5.3, §7.2 codes |
| C/2 executable spec (`dd0692d`, `Duluka.Account.Tests/Contract/`) | Status-code semantics pinned as working code + TEST-MATRIX (R1–R7 gates) | §6 sync CAS, §6.3 401/403/404 split, §7.2 registry (canonical names) |
| R2 reconciliation (this pass, 2026-09-06) | Read-only audit: `Secrets.cs`, `Program.cs`, `Database.cs` DDL, `SessionService.cs`, `GitHubOAuth.cs`, `DulukaContracts.vb`, `InMemoryDulukaAccountService.vb`, `TEST-MATRIX.md`, both suites green | All sections; conflict resolutions in §11; gaps in §14 |

Evidence note (R2): the referenced "C/4 = `d9bdec3`" and "C/5 = `2cca39e`" commits are NOT
reachable in this clone; no ULID generation and no `dacc_` prefix exist anywhere in the
repository (verified by grep). The current implementation mints opaque prefixed tokens —
§3 is frozen from that evidence. No identifier in the system is a UUIDv4/ULID.

---

## 2. MODULE BOUNDARIES

```text
┌─────────────────────────────┐   ┌──────────────────────────┐
│ Duluka Account module        │   │ ShadowPlay production     │
│ (identity/sync — THIS doc)   │   │ (capture/encode/mux)      │
│                              │   │                          │
│ owns: AccountId, LinkId,     │   │ owns: SessionId (capture  │
│       DeviceId, Account-     │   │ recording session),       │
│       SessionId, session     │   │ pipeline processes,       │
│       token, deviceKey,      │   │ config.json ownership     │
│       states, sync version,  │   │                          │
│       envelope               │   │                          │
└──────────────┬───────────────┘   └────────────┬─────────────┘
               │  ONE interface (§3.7)          │
               └────────────┬───────────────────┘
                            ▼
              Duluka.Server (ASP.NET Core minimal APIs + SQLite,
              implemented — see §13/§14 for the surface and gaps)
```

### 2.1 Rules
1. The Account module **never imports** capture/encoder/mux internals. It consumes
   only the §2 interface (and vice versa).
2. The capture pipeline **never calls the account service directly.** Session telemetry
   reaches the account module only through the §2 event pass-through — same shape the
   C/3 session-end broadcast already uses (`SessionEndBroadcastPolicy.Decide`).
3. `config.json` remains the ONLY user-facing store (CONFIG_OWNERSHIP_MATRIX §1 law).
   The Account module gets its OWN store (server: SQLite `DulukaAccount.db`; client:
   the §4.5 account store) and never writes `config.json`.
4. NVIDIA implementation (capture/encoder internals) is out of this contract's authority.
   The contract may state WHAT the module boundary passes (session identity, terminal
   action) and never HOW capture/encode works.
5. (R2) The capture pipeline's recording-session identifier and the account session
   identifier MUST NOT share a namespace. They do not: the account session is identified
   by `duluka_sess_*` / authenticated by `duluka_st_*`; the recording session stays
   inside the ShadowPlay module (C/2 namespace lesson).

---

## 3. IDENTIFIERS — FINAL MATRIX (R2)

Authoritative source: `Duluka/Duluka.Server/Security/Secrets.cs` (`Secrets.NewToken`),
`Data/Database.cs` (DDL, PK columns), `Program.cs` (validation length checks), and the
green suites. Every identifier is **opaque** — never derived from user-identifying data
(S8 PII rule: no hardware serials, no usernames in the ID itself).

| # | Identifier | Canonical form (FROZEN) | Minted by | At rest | Evidence |
|---|---|---|---|---|---|
| I-1 | **AccountId** | `duluka_acc_` + 43 chars base64url (256-bit random) | Server, at first login/account provisioning | Plaintext column, PK of `DulukaAccount` | `Database.CreateAccount` → `Secrets.NewToken("duluka_acc_")` |
| I-2 | **LinkId** | `duluka_link_` + 43 chars base64url | Server, at link creation | Plaintext column, PK of `AccountProviderLink` | `Database.UpsertLink` → `Secrets.NewToken("duluka_link_")` |
| I-3 | **DeviceId** | `duluka_dev_` + 43 chars base64url | **Server** (at device registration, inside the login/link flow) | Plaintext column, PK of `AccountDevice` | `Database.CreateDevice` → `Secrets.NewToken("duluka_dev_")` |
| I-4 | **DeviceKey** (device secret — client-held) | ≥43 chars base64url (≥256-bit random), client-generated | Client, at install/registration | **SHA-256 hex digest only** (`DeviceKeyHash`, UNIQUE); raw value never stored | `Program.cs` callback validation (min length 43); `Secrets.Sha256Hex` |
| I-5 | **Account-SessionId** | `duluka_sess_` + 43 chars base64url | Server, at session creation | Plaintext column, PK of `AccountSession` | `Database.CreateSession` → `Secrets.NewToken("duluka_sess_")` |
| I-6 | **SessionToken** (bearer secret) | `duluka_st_` + 43 chars base64url | Server, at session creation; shown to the client EXACTLY ONCE in the callback/JSON response | **SHA-256 hex digest only** (`SessionTokenHash`, UNIQUE); raw value never stored, never logged (`Secrets.Redact`) | `SessionService.TokenPrefix`; SECRETS-3 PASS |
| I-7 | **ProviderKey** | `github` \| `nvidia` (case-insensitive set; OrdinalIgnoreCase) | Fixed vocabulary | Plaintext | `Domain.ProviderKeys` |
| I-8 | **ProviderUserId** | Provider-scoped stable identity string. GitHub = numeric user `id` as a decimal string. Email/login are DISPLAY-ONLY, never identity | Provider, resolved during OAuth exchange | Plaintext (not a secret) | `GitHubOAuth.FetchIdentityAsync`; Entities.cs header |
| I-9 | **OAuth flow state** | `duluka_state_` + 43 chars base64url, single-use, 10-min TTL | Server, per flow start | In-memory only (`OAuthFlowStore`) | `Program.cs` /v1/auth/*/start |
| I-10 | **requestId (Duluka)** | Client-issued `X-ReqId` header, **echoed verbatim** in every response envelope (`reqId` field; `null` when the client sent none). Server never mints or validates it. **R3 (owner decision, `bf54d08`)** — supersedes the R2 "hub-only" ruling | — | Not stored | `Program.cs Wire.ReqId`; HTTP-INT-1/2/4 PASS |
| I-11 | Recording-session id (ShadowPlay-owned) | Out of Duluka's namespace; passed opaquely if ever referenced | Capture pipeline | — | §2.1(5) |

Superseded (R2): R1's "UUIDv4-shaped" AccountId/DeviceId rules matched no artifact and
are retired. The R2 audit premise candidates "ULID 26-char" and "`dacc_` prefix" were
tested against the repository and DO NOT EXIST — both disproven by grep and by reading
every minting site. Nothing in the system is a UUIDv4 or a ULID.

### 3.7 Validation (C/5 seam rule)
- EVERY identifier crossing the network seam passes ONE canonical validator (format +
  charset) at the client boundary, BEFORE any request object exists.
- Validator vocabulary is fixed here:
  - Server-issued identifiers: `^duluka_(acc|link|dev|sess|st|state)_[A-Za-z0-9_-]{43}$`
  - DeviceKey: ≥43 base64url chars (≥256 bits) — same rule the server enforces
    (`invalid_device_key`, Program.cs).
  - ProviderUserId: provider-scoped (GitHub: decimal digits).
  - Anything else is rejected client-side (`error.client.bad_id` class) and never reaches
    the wire; server-side re-checks return `400 invalid_*` (§7.2).

---

## 4. AT-REST RULES

Split by side (R2 — both halves are implemented and tested):

**4A. Server store (SQLite `DulukaAccount.db` — implemented):**
1. Session tokens and device keys are stored ONLY as SHA-256 hex digests
   (`SessionTokenHash` UNIQUE, `DeviceKeyHash` UNIQUE). Raw secrets never reach the
   data layer (SECRETS-3, REVOKE-2 PASS).
2. OAuth provider tokens are TRANSIENT: used inside the callback exchange, then
   discarded. v0 writes no `CredentialReference` rows (blast-radius minimization).
3. Non-secrets (AccountId, LinkId, DeviceId, ProviderUserId, display fields,
   timestamps, statuses) persist as plaintext columns.
4. Writes are parameterized SQL only; WAL journal; `foreign_keys=ON` (Database.cs).
5. Unlinking a link destroys its `CredentialReference` rows and revokes every session
   issued via that link (UNLINK-2 PASS).

**4B. Client account store (ShadowPlay side — F-S2 contract, unchanged):**
1. Account store = its own local file, owned exclusively by the Account module
   (CONFIG_OWNERSHIP_MATRIX law). NOT `config.json`.
2. At-rest secrets on the CLIENT (session token, deviceKey, any OAuth token the client
   persists): DPAPI `ProtectedData(CurrentUser)` — the F-S2 GHS contract, verbatim:
   plaintext never on disk, migration scrubs legacy plaintext including `.bak`,
   plaintext never in the error envelope or logs.
3. Non-secrets (AccountId, DeviceId, display data) persist as plain JSON.
4. Writes are atomic (temp → `.bak` = previous → rename); secrets are encrypted BEFORE
   the file is written, so any `.bak`/`.tmp` snapshot is already safe (GHS-2 invariant).
5. v0 transport note (implemented): the session token is returned in the JSON response
   body (ShadowPlay-first client); cookie transport is deferred (owner decision).

---

## 5. STATE MACHINES — FINAL (R2)

Vocabulary is FIXED here (C/5 lesson: one selector, one vocabulary, unknown values have
a documented fallback). Unknown state received from the server is treated as the safest
fallback of its family and reported once — never silently mapped.

### 5.1 Account states (implemented vocabulary)
```text
Active ⇄ Suspended ; Active|Suspended → Closed (terminal)
```
- An account is created **Active with its first link atomically** at first provider
  login — there is no observable "Created/Linked" pre-active phase (R1's five-state
  diagram is superseded; `Database.UpsertLink` + `AccountStatus` are the evidence).
- `Suspended`: server-set, reversible. No v0 endpoint sets it (operator-level DB action
  until the admin boundary is decided, O-7). While suspended, session validation fails
  full-chain (ValidateSession requires `Status='Active'`). **Frozen client rule: the
  server MUST report a suspended account as `403 account_suspended`** (distinct reality
  = distinct outcome, C/4) — current server folding into 401 is open item M-2 (§14).
- `Closed`: terminal. Client behavior on encountering it: wipe local account store
  secrets, keep DeviceId/DeviceKey, surface one terminal notice (exactly once — C/3
  rule). No v0 endpoint sets it.

### 5.2 Account-session states (derived, implemented)
Sessions have no status column; state is DERIVED from `RevokedAt`/`ExpiresAt`
(`Database.ValidateSession`):
```text
Active → (ExpiresAt passed)        → Expired   [terminal]
Active → (RevokedAt set)           → Revoked   [terminal, reason recorded]
```
- **Exactly ONE active session per DeviceId (R3, owner decision `bf54d08` — supersedes
  the R2 "multiple allowed" ruling and implements R1 §5.2's invariant).** A second login
  on the same device REVOKES the previous session first (revoke-before-insert,
  `RevokedReason='superseded'` — `Database.CreateSession`, SESSION-3 PASS). The second
  login itself SUCCEEDS (it completes a real OAuth exchange); there is NO
  `409 session_conflict` — R1's 409-on-login variant is retired. The superseded
  session's client learns of the supersession on its next request via
  `401 auth.session_revoked`. The transient zero-active window during supersession is
  fail-closed and invisible on the single connection.
- Terminal semantics fire EXACTLY ONCE per (SessionId, terminal kind) on the client
  (C/3 exactly-once); the client clears in-memory tokens, keeps DeviceId/DeviceKey.
- Sliding refresh: 7 days sliding / 30 days absolute cap from creation, both
  configurable (`Session:SlidingDays`, `Session:AbsoluteDays`); refresh never shortens
  the window (SESSION-1 PASS). Values are implemented defaults — owner may change the
  config, not the mechanism.

### 5.3 Device states (implemented vocabulary)
A device is a row with `RevokedAt` + `LastSeenAt`; there is no server-side
Registered/Active/Stale enum (R1's four-state diagram is superseded):
```text
Registered (row exists) → Revoked (RevokedAt set) [terminal for the KEY]
```
- Liveness: `LastSeenAt` is touched on successful authentication (`TouchDevice`).
  "Stale" is a CLIENT-side derivation from `LastSeenAt` + the liveness window — the
  window value is **OPEN (O-3a)**; staleness is informational, never an error, and
  never invents a failure (C/4 honesty: offline ≠ failed).
- Revocation cascades: `RevokeDevice` revokes the device row AND every live session of
  that device in one action (DEV-1, REVOKE-2 PASS). A revoked device KEY can never
  silently re-register (`device_revoked` at login); cross-account key reuse is rejected
  (`device_key_in_use`). These guards are keyed on the KEY HASH, not the DeviceId —
  that is why the key (I-4), not the id, is the security anchor.

### 5.4 Transition rules (C-1/C-2 derived — unchanged, still valid)
1. A state transition and its causing effect commit atomically server-side — no
   intermediate state observable that lies about what happened.
2. Every failure path CONVERGES to a terminal or retryable state within a bounded
   time — no state may wedge forever; timeouts are intermediate states, never lying
   terminals.
3. Terminal semantics fire EXACTLY ONCE per (SessionId, terminal kind); retries at the
   transport level must be idempotent (§6.6).

---

## 6. SYNC + CONCURRENCY — FINAL (R2)

**Implementation status:** the sync endpoints are NOT implemented in v0 (`SyncProfile`
table + indexes exist; README declares sync "next phase"). The semantic contract below
is FROZEN from the C/2 executable spec (`DulukaContracts.vb` + `InMemoryDulukaAccountService.vb`
+ TEST-MATRIX SYN/RACE rows) — it is the acceptance spec the sync slice must pass.

### 6.1 Sync docs and versions
- A sync doc is scoped per (AccountId, ProductKey): `SyncProfile` UNIQUE(AccountId,
  ProductKey) — R1's single "SyncDoc 1..1 per Account" generalizes to 1..N per product
  (implemented DDL is the evidence).
- Each doc carries a server-owned monotonically increasing integer `syncVersion`.
  **Initial value = 1** (C/2 reference spec; R1's "0 = never synced" is superseded).
- The client never computes it; it echoes the last value it received.

### 6.2 If-Match CAS (lost-update protection is NOT optional — TEST-MATRIX R6)
- Every mutating sync request carries `If-Match: <syncVersion>`.
- Missing If-Match on a write → **`428 conflict.missing_if_match`** (C/2 SYN-3, executable and
  tested in the reference). R1 was silent on this case; 428 is the frozen rule.
- Mismatched If-Match → **`409 conflict.stale_version`**, and the response body MUST reveal the
  server's current version (C/2 SYN-2: "current version revealed"); the loser must not
  overwrite (RACE-2: exactly one winner, rest 409, version advances by exactly 1).
  Exact JSON field name for the revealed version is an implementation freedom of the
  sync slice; the SEMANTIC is frozen.
- Match → apply, return the new version, read-back matches (SYN-1).
- Conflict resolution policy per resource kind is **OPEN (owner, O-2)** — v0.1 ships
  LWW as the documented default; per-field merge must not be silently introduced.

### 6.3 HTTP status semantics (fixed — never conflated; R2 consolidated)
| Status | Meaning | Codes (canonical, §7.2) | Evidence |
|---|---|---|---|
| 400 | Malformed input / failed provider callback | `invalid_device_name`, `invalid_device_key`, `invalid_callback`, `invalid_state`, `invalid_provider`, `github_not_configured`, `provider_token_exchange_failed`, `provider_callback_rejected`, `provider_identity_fetch_failed` | Program.cs paths (documented registry extensions) |
| 401 | Identity not established / not usable | `auth.session_expired` (no/empty/non-Bearer header, unknown token, expired) · `auth.session_revoked` (revoked or superseded-by-second-login) — the wire distinguishes revoked-vs-expired only (R3: `DeadSessionCode`; HTTP-INT-2 PASS; finer unknown-vs-expired granularity retired) | Program.cs + SessionService.cs |
| 403 | Identity established, action not permitted | `perm.device_removed` (revoked KEY presented at login — implemented, callback catch) · `perm.account_suspended` (REQUIRED, **open M-2** — suspended currently folds to 401 `auth.session_expired`) · `cross_account_forbidden` (forward) | Program.cs; TEST-MATRIX ME-9 |
| 404 | Identity established, resource does not exist | `nf.link`, `nf.device` (foreign or unknown — cross-tenant never leaks existence) | Program.cs DELETE provider / device revoke |
| 409 | State conflict (deterministic, never 403) | `conflict.link_conflict` — covers: identity already linked to another account, last-provider unlink guard, re-unlink of an unlinked link, device key bound to another account. (`stale_version` and C/2's `provider_not_linked` name are retired in favor of the single implemented code) | Program.cs, three sites + LoginOrLink |
| 428 | Missing precondition on sync writes | `missing_if_match` | C/2 SYN-3 (sync slice future) |
| 429 | Rate limited (per-IP fixed windows: 10/min auth-start, 240/min api policy) | `server.rate_limited`, `retryable=true`, full envelope (**R3: implemented**; note **open M-7**: no authenticated endpoint carries the `api` policy yet) | Program.cs `OnRejected`; RL-1/RL-3 |
| 501 | Provider reserved (known key, no flow — `nvidia` and any unimplemented key) | `provider_reserved` | Program.cs, three endpoints; HTTP-INT-3 PASS |
| 5xx | Unhandled server fault | `server.internal` (retryable) — full envelope via callback catch-all (**R3: no more naked 500s on the callback paths**; body-parse paths remain open **M-1**) | Program.cs catch-all; S-6/S-7 |

- **401 vs 403 vs 404 rule (kept from R1, now evidence-aligned):** 401 = re-auth may
  fix it; 403 = authenticated but forbidden; 404 = authenticated, resource absent.
  Cross-tenant resources are 404 (never leak existence): implemented — device revoke
  and link lookup of another account's row return 404, not 403.
- **Device revocation (R3):** one root cause, two surfaces: USE of a session whose
  device was revoked → `401 auth.session_revoked`; LOGIN presenting a revoked KEY →
  `403 perm.device_removed`. Unknown/foreign DeviceId → `404 nf.device`. R1's
  `device_removed`-in-404 contradiction stays resolved; the flat R2 names are retired.
- Uniqueness races on the provider anchor are state conflicts → 409, never 403
  (kept). BUT the duplicate-account race CONVERGES rather than conflicts: the same
  (ProviderKey, ProviderUserId) ALWAYS logs into its existing account — first-login
  races produce ONE account (LOGIN-1/LOGIN-2, RACE-1 PASS). 409 is reserved for
  linking an identity bound to a DIFFERENT account (no silent merge).

### 6.4 Truthfulness (C/4 — unchanged)
- Any success response that claims a persisted/derived effect carries verifiable
  evidence (ids + timestamps + `existingAccount` flag on login; `revokedSessions`
  counts on revokes; sync version on sync writes).
- "Environment not capable" is reported as `error.client.env` locally, never sent to
  the server as a server failure.

### 6.5 Concurrency on the client (C-1 rule — unchanged)
- Client-side state transitions of the local store commit atomically with the effect
  that caused them (single lock), and a failure may never downgrade an
  already-committed state.
- Server-side serialization is at the STORE: unique index on (ProviderKey,
  ProviderUserId) + optimistic concurrency on sync version (TEST-MATRIX production
  note; the in-memory lock is the reference model only). Implemented: UNIQUE anchors
  exist in DDL; SQLite serializes writers.

### 6.6 Retries / idempotency (C-3 rule, R2-sharpened)
- **Revoke idempotency:** device-level revoke is idempotent (second call → 200 with
  `sessionsRevoked=0`). Session-level revoke of an already-revoked session currently
  answers `401 auth.session_revoked`; C/2 SES-2b pins an idempotent `200` — **open M-9
  (owner call: keep terminal-401 or align to 200)**. Use-after-revoke is always `401
  auth.session_revoked` forever (AUTH-3: revocation outlives TTL).
- Use-after-revoke remains `401 session_revoked` forever (AUTH-3: revocation outlives
  TTL).
- Future write retries (sync phase) carry the SAME (SessionId, If-Match precondition);
  terminal events are deduplicated by `(SessionId, terminalKind, syncVersion)`.
- Backoff: bounded, starts 1 s, doubles, caps 30 s (TcpClientHelper proven cadence) —
  never infinite silent retry without surfacing state.

---

## 7. ERROR ENVELOPE (single schema — R2 frozen to the implemented wire)

### 7.1 Shape (R3 — the owner adopted the R1 `ok/reqId` envelope via `bf54d08`; HTTP-INT-1/2/4 PASS)

Every response (success AND error) carries ONE schema:

```json
// success
{ "ok": true,  "reqId": "<X-ReqId echoed verbatim, or null>", "resource": { …payload… } }

// error
{ "ok": false, "reqId": "<echo or null>", "errorCode": "<registry code, §7.2>",
  "httpStatus": 429, "retryable": false, "conflict": null,
  "message": "<human-readable, safe-for-display>" }
```

- `reqId` comes from the client's `X-ReqId` header, echoed verbatim, never minted or
  validated server-side (I-10, R3).
- `retryable` is `true` for 429 and 5xx, `false` otherwise (implemented in `Wire.Err`).
- `conflict` is `null` on every v0 auth surface today — that is the current
  implementation, and the C/2 ledger tracks wiring it up as **open M-10** (409 bodies
  should carry the current server resource so the client can re-apply or drop; exact
  payload shape is the owning fix's to define). The sync slice's `409
  conflict.stale_version` body MUST carry the current version (§6.2).
- `message` is display text: HTML-encode on render (C/2 reflected-XSS fix); it NEVER
  contains tokens, ProviderKeys, or raw OAuth payloads (redaction enforced by
  `Secrets.Redact` in all log paths; 10 log sweeps green in the HTTP suite).
- R2's `{"error":{"code","message"}}` shape is RETIRED (it was the pre-integration
  shape; R2 §7.1 had frozen it — the owner's `bf54d08` reversal is the authoritative
  evidence, per the R3 header note).

### 7.2 Code registry (R3 — the DOTTED vocabulary is canonical; implemented set frozen)

```text
client.* (local only, never sent):  bad_id, env, protocol
400 family:    invalid_device_name, invalid_device_key, invalid_callback,
               invalid_state, invalid_provider,
               github_not_configured, provider_token_exchange_failed,
               provider_callback_rejected, provider_identity_fetch_failed
401 family:    auth.session_expired, auth.session_revoked
403 family:    perm.device_removed (implemented, login path)
               perm.account_suspended (REQUIRED — open M-2)
               cross_account_forbidden (forward)
404 family:    nf.link, nf.device
409 family:    conflict.link_conflict
               conflict.stale_version (sync slice future)
428 family:    conflict.missing_if_match (sync slice future)
429 family:    server.rate_limited
501 family:    provider_reserved
5xx family:    server.internal (retryable)
```

- Implemented today: every code above except `perm.account_suspended` (M-2),
  `cross_account_forbidden`, and the two sync-slice codes.
- R2's flat registry (`session_expired`, `stale_version`, …) and R2's claim that the
  dotted registry "matched no artifact" are RETIRED — `bf54d08` implemented the dotted
  registry and pinned it with green tests (HTTP-INT-2). The C/2 in-memory executable
  spec (`DulukaContracts.vb`) still speaks flat names: its STATUS assertions remain
  binding; its code-name constants are superseded by this table (owner may update the
  VB constants at the next sync-slice touch).
- Unknown `errorCode` received → client renders generic per-HTTP-class message and
  logs the raw code once (C/5 unknown-value fallback rule).

### 7.3 Exactly-once (C/3)
- One request → one terminal envelope. Replayed OAuth state (`invalid_state`), single-use
  flows, and idempotent revokes (§6.6) are the v0 idempotency surface; the server-side
  idempotency window for retried WRITES is a sync-phase requirement (§6.6).

---

## 8. PROVENANCE (C/6 soak/BL-1 rule — unchanged)

- Every account-module client build embeds the same provenance the BL-1 work gave the
  product: product version + git sha, printed in the module's diagnostic banner.
- Sync payloads never include machine-identifying data beyond DeviceId/DeviceKey (§3).

---

## 9. SECRETS / REDACTION (unchanged rules, R2 evidence updated)

1. Server: session token + device key exist ONLY as SHA-256 digests at rest; provider
   tokens transient; flows/state in-memory with 10-min TTL (§4A).
2. Client: persisted secrets DPAPI CurrentUser (F-S2), §4B.
3. Logs redact by single implementation (`Secrets.Redact`): keep ≤6-char prefix + length,
   destroy the body; never log raw tokens/codes/verifiers (SECRETS-2 PASS; GitHubOAuth
   logs only redacted values).
4. The account transport never rides the unauthenticated hub relay (S1): the hub stays
   a loopback command relay; account traffic uses its own authenticated channel.
   Implemented by construction — Duluka.Server has no hub dependency.

---

## 10. OPEN DECISIONS (owner — do NOT implement from this doc)

| # | Decision | Status at R2 |
|---|---|---|
| O-1 | What syncs (payload contents per product) | **Still OPEN** — gates the sync slice |
| O-2 | Conflict policy per resource (LWW default vs field-merge) | **Still OPEN** — LWW is the documented default |
| O-3 | Device liveness window value | Split: **O-3a device window still OPEN**; session TTL values RESOLVED by implementation defaults (7d sliding / 30d absolute, configurable) |
| O-4 | Server stack/hosting | **RESOLVED by implementation**: ASP.NET Core minimal APIs + Kestrel + SQLite (`Duluka.Server.csproj`, `Program.cs`) |
| O-5 | Provider list for v0.1 | **RESOLVED by implementation**: `github` implemented; `nvidia` reserved with the hardware-identity ban (ProviderKeys, NVIDIA-1 PASS) |
| O-6 | Duluka request-correlation id (hub `reqId` analog) | **RESOLVED (R3, `bf54d08`): YES** — client-issued `X-ReqId`, echoed verbatim in every envelope (I-10) |
| O-7 | Admin boundary (who may set Suspended/Closed; operator vs endpoint) | **NEW (R2)** — v0 has NO admin surface; suspension/close are DB-operator actions today |

---

## 11. CONFLICTS RESOLVED (the "ชน" log — R2 rows appended)

| Conflict | Sides | Resolution |
|---|---|---|
| C/2 hub relay vs account authn | Hub = unauthenticated broadcast (S1) vs Account needs 401/403 | Account traffic gets its OWN authenticated channel; hub stays loopback command relay. Upheld by construction in the implementation. |
| C/3 exactly-once vs sync retries | Terminal events must fire once vs transport retries | Idempotency key `(SessionId, terminalKind, syncVersion)` + revoke idempotency (§6.6) |
| C/4 SKIP honesty vs HTTP errors | "environment not capable" must not masquerade as a failure | Local `error.client.env` never sent to server; 401/403/404 reserved for server-side realities (§6.3) |
| C/5 validator seam vs identifier zoo | Each identifier class needs validation but only ONE seam per class | §3.7: one canonical validator at the client boundary, fixed charset, single rejection class |
| C/6 state convergence vs async sync | Client state must converge, but server round-trips are async | §5.4 bounded convergence + §6.6 bounded backoff |
| CONFIG_OWNERSHIP_MATRIX vs cloud sync | Sync would shadow user settings | §2.1(3): account store separate; synced-settings ownership = O-1/O-2 |
| F-S2 DPAPI persistence vs token rotation | Token persisted but must rotate | Rotation = server-issued re-issue over the same LinkId; at-rest contract unchanged |
| **R2: UUIDv4 (R1 doc) vs opaque tokens (code) vs "ULID/dacc_" (task premise)** | Three claimed canonical forms | **Code wins**: opaque 256-bit base64url tokens with fixed `duluka_*` prefixes. ULID/dacc_ proven nonexistent (grep + every minting site read). UUIDv4 rules retired. |
| **R2: DeviceId minted client-side (R1) vs server-side (code)** | R1 §3.3 vs `Database.CreateDevice` | **Code wins**: DeviceId is server-minted; the client generates and holds the DEVICE KEY (I-4), which is the revocation/security anchor (hashed at rest). |
| **R2: one-session-per-device + `409 session_conflict` (R1 §5.2) vs both implementations** | R1 vs C/2 reference vs server REVOKE-1 | **Implementations win**: multiple live sessions per device are allowed; `session_conflict` never existed. Rule retired. |
| **R2: `device_removed` in 403 AND 404 families (R1 internal contradiction)** | R1 §5.3/§6.3/§7.2 | **Resolved per C/2 + code**: no `device_removed` code; 401 device_revoked (use), 403 device_revoked (login w/ revoked key), 404 device_not_found (unknown/foreign). |
| **R2: dotted error registry (R1 §7.2) vs flat codes (C/2 spec + server)** | Three vocabularies | **C/2 executable vocabulary is canonical** (it is asserted by tests); server aliases renamed at conformance time (G-2). R1 dotted registry retired (never emitted by anything). |
| **R2: envelope `ok/reqId/errorCode` (R1) vs `{"errorCode"}` (C/2 sketch) vs `{"error":{code,message}}` (server)** | Three shapes | **Server shape wins** (the only implemented HTTP surface); C/2's regex/body gates (R7) still apply. `reqId` dropped (hub-only, I-10/O-6). |
| **R2: sync version initial 0 (R1) vs 1 (C/2 reference)** | R1 vs C/2 | **C/2 wins** (executable spec): initial version = 1. |
| **R2: missing If-Match (R1 silent) vs 428 (C/2)** | — | **C/2 wins**: 428 `missing_if_match` (SYN-3). |
| **R2: 201-on-create (C/2 abstract seam) vs 200 login callback (server)** | C/2 interface vs server | Both stand, different surfaces: the abstract `CreateOrLinkAccount` seam may return 201; the implemented OAuth login flow returns `200` + `existingAccount` (creation is a callback side effect). The HTTP probe asserts statuses only. |
| **R2: 409 `invalid_identity` on blank input (C/2 reference quirk) vs 400 `invalid_*` (server)** | C/2 reference vs server | **Server wins for HTTP** (400 = malformed input); the reference's 409 is a seam artifact, never in the P0 matrix, not promoted to the contract. |
| **R3: R2 envelope/registry/one-session rulings vs `bf54d08` implementation** | R2 doc (frozen after `bf54d08` was written) vs `bf54d08` code + green HTTP-INT tests | **`bf54d08` wins** — the owner implemented and test-pinned the R1-style `ok/reqId` envelope, the dotted registry, one-active-session-per-device (supersede semantics), and reqId on the wire. §5.2/§6.3/§7.1/§7.2/I-10/O-6 amended; R2's "retired" claims on those points are void. |
| **R3: one-session enforcement vs C/2 in-memory reference (multi-session fixtures)** | `bf54d08` vs `DulukaContracts.vb` DEV-1/REVOKE-1 shapes | **Server wins** (owner decision); the C/2 in-memory spec's STATUS semantics stay binding, its multi-session fixtures and flat code-name constants are legacy until the owner touches the VB spec. |
| **R3: `C:\My Project\Duluka.Server` repo + commits `ee07eb7`/`b7c1801` + "C/6 M1..M6" (task premise)** | Task premise vs repository | **Not found anywhere** — the synchronized server lives in THIS repo under `Duluka/`; `ee07eb7`/`b7c1801` do not exist in any ref; no M1..M6 artifact exists (the C/6 matrix is SECRETS/FLOW/LOGIN/SESSION/REVOKE/UNLINK/SCHEMA/NVIDIA + HTTP-INT-1..4, 24 tests). Audited against actual HEAD state. |

---

## 12. VERDICT (R3)

**FROZEN (amended).** Identifiers (§3), state machines (§5), sync CAS (§6), HTTP/error
semantics (§6.3/§7), boundaries (§2), and at-rest rules (§4) are specified, mutually
consistent, and grounded in implemented, green-suite code (C/6: 24/24 at HEAD). R3
resolved the R2↔`bf54d08` contradiction in the owner's direction (ok/reqId envelope,
dotted registry, one-active-session-per-device, reqId on the wire) — the three-way
R1/R2/code conflict is closed. No UUID/ULID ambiguity exists (none ever did).

**Release-readiness is governed by §14 (live gap status), NOT by this freeze:** the
remaining open items are the production mismatches M-1, M-2, M-6, M-7, M-9 plus the
newly tracked M-10/M-11 (§14), and the sync slice (O-1/O-2, COMP-2 tripwire). The C/2
HTTP suite is green at 99/99 after its `e3b6432` reclassification.

---

## 13. IMPLEMENTED HTTP SURFACE (R2 inventory — `Program.cs`)

| Route | Auth | Semantics (statuses per §6.3) |
|---|---|---|
| `GET /healthz`, `GET /healthz/ready` | none | liveness / readiness (schema version) |
| `POST /v1/auth/{provider}/start` | rate-limited per-IP | 501 reserved; 400 invalid device fields; 200 `{authorizationUrl, state(RAW), expiresInMinutes:10}` (HTTP-INT-1) |
| `POST /v1/auth/{provider}/callback` | rate-limited | 400 invalid callback/state/device key/provider errors; 403 device_revoked; 200 `{sessionToken, accountId, deviceId, existingAccount, sessionExpiresAt}` |
| `POST /v1/account/providers` | Bearer | start LINK flow onto THIS account; 401 `auth.session_expired`; 501 reserved |
| `POST /v1/account/providers/{provider}/complete` | flow-bound | 400 `invalid_state`; 401 `auth.session_expired`; 409 `conflict.link_conflict` (identity bound elsewhere, incl. race-loser guard); 200 `{linked, linkId, already}` |
| `POST /v1/auth/session/refresh` | Bearer | 200 `{sessionExpiresAt}` / 401 |
| `POST /v1/auth/session/revoke` | Bearer | 200 `{revoked:true}` / 401 `auth.session_revoked` (second revoke = open M-9) |
| `POST /v1/auth/sessions/revoke-all` | Bearer | 200 `{revokedSessions:n}` / 401 |
| `GET /v1/account/me` | Bearer | 200 `{accountId, displayName, createdAt, currentDevice}` |
| `GET /v1/account/providers` | Bearer | 200 `{providers:[{linkId, providerKey, providerEmail, status, linkedAt}]}` |
| `DELETE /v1/account/providers/{linkId}` | Bearer | 200 `{unlinked:true, revokedSessions:n}` / 409 `conflict.link_conflict` (last-provider, re-unlink) / 404 `nf.link` (unknown or cross-tenant) |
| `GET /v1/account/devices` | Bearer | 200 `{devices:[{deviceId, deviceName, createdAt, lastSeenAt, revokedAt}]}` |
| `POST /v1/account/devices/{deviceId}/revoke` | Bearer | 200 `{revoked:true, sessionsRevoked:n}` (idempotent; second call `sessionsRevoked=0`) / 404 `nf.device` (incl. cross-tenant) |
| `DELETE /v1/account` | Bearer (+ `currentPassword` iff the account has a native credential) | IRREVERSIBLE self-deletion: one transaction removes the account and every dependent row (sessions, devices, links + credential references, native credential, sync profiles). 200 `{deleted:true, deletedSessions:n}`; 400 `invalid_credentials` (missing/wrong password); 400 `bad_request` (malformed body); 401. Body optional — provider-only accounts delete on session possession. Frees every UNIQUE anchor: username re-registrable, device key re-enrollable, same provider identity bootstraps a FRESH account |
| (future) sync routes | Bearer + If-Match | per §6 — not implemented in v0 |

---

## 14. CONFORMANCE GAPS — LIVE STATUS (final audit, HEAD `e3b6432`, 2026-09-06)

Suite truth at HEAD: C/6 `Duluka.Server.Tests` **24/24 PASS** · C/2 HTTP suite
**99/99 PASS** (reclassified to the frozen contract in `e3b6432` — 95 → 99 tests;
the unmodified pre-reclassification suite had scored 33/95 against the reconciled
server, which was the designed ledger-drift signal) · C/2 in-memory spec **19/0 PASS**.

| ID | Item | Status at HEAD | Owner file |
|---|---|---|---|
| M-1 | Malformed/empty JSON body → naked 500, empty body | **OPEN (CONFIRMED by S-6/S-7)** → map to 400 envelope | `Duluka/Duluka.Server/Program.cs` (body parsing) |
| M-2 | Suspended account → 401; contract requires 403 `perm.account_suspended` | **OPEN** (now 401 `auth.session_expired` — code improved, status still wrong) | `Data/Database.cs` + `Program.cs` |
| M-3 | One-active-session-per-device | **FIXED** (`bf54d08`: revoke-before-insert; SESSION-3 PASS; old ME-8 pin fails as designed) | — |
| M-4 | Envelope lacks ok/reqId/errorCode/retryable | **FIXED** (`bf54d08` adopted §7.1; HTTP-INT-1/2 PASS; ENV-2 pin fails as designed) | — |
| M-5 | Re-unlink → 400; C/2 wants 409 | **FIXED** (now 409 `conflict.link_conflict`; UNL-6 pin fails on fixture, not status) | — |
| M-6 | Device-key race: `CreateDevice` UNIQUE(19) escapes `LoginOrLink` → 500 (7/8 threads) | **OPEN (CONFIRMED by RACE-DEV)** → catch + converge like `UpsertLink`, map to 4xx | `Auth/GitHubOAuth.cs` |
| M-7 | No endpoint carries the `api` rate-limit policy (260 hits, zero 429) | **OPEN (CONFIRMED by RL-3)** → attach `RequireRateLimiting("api")` | `Program.cs` |
| M-8 | 401 root-cause codes conflated | **FIXED** (`DeadSessionCode`: revoked-vs-expired on the wire; ME-7 pin fails as designed) | — |
| M-9 | Second session revoke → 401; C/2 SES-2b pins idempotent 200 | **OPEN (owner call)** — security property (no resurrect) holds | `Program.cs` revoke endpoint |
| M-10 | 409 `conflict.link_conflict` bodies carry `conflict=null`; §6.2 requires the CURRENT server resource (+ version) attached so the client can re-apply or drop deterministically | **OPEN (new, tracked by `e3b6432` ledger, ENV-3)** | `Program.cs` 409 sites |
| M-11 | `/v1/account/providers/{provider}/complete` hardcodes `auth.session_expired` for a REVOKED binding session while every other endpoint distinguishes revocation via `DeadSessionCode` | **OPEN (new, tracked by `e3b6432` ledger, PRV-9)** | `Program.cs` link-complete |
| F-1 | start returned state redacted | **RESOLVED** — state now RAW (HTTP-INT-1); F-1 pin inverted, ledger must drop it | suite only |
| F-2 | Missing-token 401 code inconsistent | **RESOLVED** — uniform `auth.session_expired` (R2 §6.3 table) | suite only |
| F-3 | revoke-all counts expired-but-unrevoked sessions | **OPEN (minor)** — harmless overcount | `Data/Database.cs` `RevokeAllForAccount` |
| F-4 | No transport-failure catch in exchange | **RESOLVED at envelope level** — callback catch-all → 500 `server.internal` retryable | — |
| S-1 | C/2 HTTP suite reclassification | **RESOLVED** — reclassified and committed in `e3b6432` (99 tests, 99/99 PASS, deterministic across 3 consecutive runs); ledger verdict: M-3/M-4/M-5/M-8 FIXED, M-1/M-2/M-6/M-7/M-9 still violated, M-10/M-11 newly tracked | `Duluka/Duluka.Http.Integration.Tests/` (committed) |
| S-2 | Sync slice | **NOT STARTED** (COMP-2 tripwire green: `/v1/sync*` → 404); gated on O-1/O-2; SYN/428 matrix must be enabled when it lands | new endpoints |
| S-3 | Admin boundary (O-7) / device liveness window (O-3a) | **OPEN (owner)** — no admin surface exists; suspension is a DB-operator action today | owner decision |

FROZEN document owner: C/1. R3 is a docs-only amendment committed as
`docs: reconcile Duluka v0.1 contract amendment`; production code is untouched by it
(all §14 fixes belong to C/5). Changes after R3 require a new evidence citation per rule.
