# DULUKA ACCOUNT — v0.1 IMPLEMENTATION CONTRACT

Status: **FROZEN — R2 reconciled against the implemented C/4+C/5+C/6 code and the C/2
executable spec (2026-09-06). Conformance gaps are enumerated in §14; they gate the next
integration phase, not this document.**
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
| I-10 | **requestId (Duluka)** | **NOT part of the Duluka v0.1 wire contract.** `reqId` correlation is a ShadowPlay HUB mechanism (`req=<reqId>` in `engine_response`, Overlay TCP client). Duluka request correlation = **OPEN (O-6)** | — | — | grep: `reqId` exists only in Overlay TCP client; zero occurrences in Duluka code/tests |
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
  = distinct outcome, C/4) — current server folding into 401 is conformance gap G-5.
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
- **Multiple concurrent sessions per device are ALLOWED.** R1's "exactly ONE active
  session per DeviceId + `409 session_conflict` on second login" is RETIRED: it is
  disproven by BOTH implementations (server REVOKE-1 keeps t1/t2 alive on one device;
  C/2 reference DEV-1 creates multiple sessions per device) and no `session_conflict`
  code exists anywhere.
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
- Missing If-Match on a write → **`428 missing_if_match`** (C/2 SYN-3, executable and
  tested in the reference). R1 was silent on this case; 428 is the frozen rule.
- Mismatched If-Match → **`409 stale_version`**, and the response body MUST reveal the
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
| 400 | Malformed input / failed provider callback | `invalid_device_name`, `invalid_device_key`, `invalid_callback`, `invalid_state`, `invalid_provider`, `github_not_configured`, `provider_token_exchange_failed`, `provider_callback_rejected`, `provider_identity_fetch_failed` | Program.cs paths |
| 401 | Identity not established / not usable | `unknown_session`, `device_revoked`, `session_revoked`, `session_expired` — root-cause precedence in that order (root cause wins, TEST-MATRIX decision); `session_missing` (no Bearer) | C/2 ResolveLiveSession precedence; server folds all four into `session_invalid` today = gap G-1 |
| 403 | Identity established, action not permitted | `device_revoked` (revoked KEY presented at login — re-registration attempt), `account_suspended` (forward — see §5.1), `cross_account_forbidden` (forward, cross-account resource access) | Program.cs callback catch (403 device_revoked, implemented); C/2 TEST-MATRIX decisions |
| 404 | Identity established, resource does not exist | `link_not_found` (foreign or unknown), `device_not_found` (foreign or unknown) | Program.cs DELETE provider / device revoke |
| 409 | State conflict (deterministic, never 403) | `provider_linked_to_other_account`, `last_provider_cannot_unlink`, `provider_not_linked`, `stale_version` | C/2 codes (canonical) vs server names `identity_already_linked` / `last_provider` = rename gap G-2 |
| 428 | Missing precondition on sync writes | `missing_if_match` | C/2 SYN-3 |
| 429 | Rate limited (per-IP fixed windows: 10/min auth starts, 240/min API) | no body guaranteed | Program.cs RateLimiter, `RejectionStatusCode=429` |
| 501 | Provider reserved (known key, no flow — `nvidia` and any unimplemented key) | `provider_reserved` | Program.cs, three endpoints |
| 5xx | Unhandled server fault | `server.internal` (retryable) | envelope rule; note gap G-6 |

- **401 vs 403 vs 404 rule (kept from R1, now evidence-aligned):** 401 = re-auth may
  fix it; 403 = authenticated but forbidden; 404 = authenticated, resource absent.
  Cross-tenant resources are 404 (never leak existence): implemented — device revoke
  and link lookup of another account's row return 404, not 403.
- **Device revocation resolves R1's internal contradiction** (R1 put `device_removed`
  in BOTH the 403 and 404 families): there is no `device_removed` code. A revoked
  device still EXISTS (404 would lie). Frozen: use-with-revoked-device → `401
  device_revoked`; login-with-revoked-key → `403 device_revoked`; unknown/foreign
  DeviceId → `404 device_not_found`.
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
- **Revoke operations are idempotent**: revoking an already-revoked session returns
  `200` (no error) — C/2 SES-2b, executable and tested. Server currently returns
  `401 session_invalid` on the second revoke = gap G-3.
- Use-after-revoke remains `401 session_revoked` forever (AUTH-3: revocation outlives
  TTL).
- Future write retries (sync phase) carry the SAME (SessionId, If-Match precondition);
  terminal events are deduplicated by `(SessionId, terminalKind, syncVersion)`.
- Backoff: bounded, starts 1 s, doubles, caps 30 s (TcpClientHelper proven cadence) —
  never infinite silent retry without surfacing state.

---

## 7. ERROR ENVELOPE (single schema — R2 frozen to the implemented wire)

### 7.1 Shape (implemented, `Program.cs Err()` — evidence)
```json
HTTP <status>
{ "error": { "code": "<stable machine code, §7.2>", "message": "<human-readable, safe-for-display>" } }
```
- Success bodies are camelCase JSON (`sessionToken`, `accountId`, `deviceId`,
  `existingAccount`, `sessionExpiresAt`, `providers[]`, `devices[]`, …).
- `message` is display text: HTML-encode on render (C/2 reflected-XSS fix); it NEVER
  contains tokens, ProviderKeys, or raw OAuth payloads (C/2 redaction rule; enforced by
  `Secrets.Redact` in all log paths).
- Error bodies expose stable codes only — no internals, no PII, no stack traces
  (TEST-MATRIX R7).
- R1's envelope (`ok:false` + `reqId` echo + `errorCode` + `retryable` + `conflict`
  fields) is RETIRED: it matched no implemented artifact; `reqId` is a hub-channel
  mechanism (I-10). `retryable` semantics live in the status class (429/5xx retryable;
  4xx terminal). The sync slice MAY add a conflict payload to the 409 body (§6.2) —
  that is the only sanctioned extension.

### 7.2 Code registry (fixed vocabulary — canonical names from the C/2 executable spec)
```text
client.* (local only, never sent):  bad_id, env, protocol
400 family:    invalid_device_name, invalid_device_key, invalid_callback,
               invalid_state, invalid_provider,
               github_not_configured, provider_token_exchange_failed,
               provider_callback_rejected, provider_identity_fetch_failed
401 family:    unknown_session, device_revoked, session_revoked, session_expired,
               session_missing
403 family:    device_revoked (login path), account_suspended (forward),
               cross_account_forbidden (forward)
404 family:    link_not_found, device_not_found
409 family:    provider_linked_to_other_account, last_provider_cannot_unlink,
               provider_not_linked, stale_version
428 family:    missing_if_match
429:           (rate limited; no body guaranteed)
501 family:    provider_reserved
5xx family:    server.internal (retryable)
```
- Known aliases in the current server that MUST be renamed for conformance (gap G-2):
  `session_invalid` → root-cause 401 codes (G-1); `identity_already_linked` →
  `provider_linked_to_other_account`; `last_provider` → `last_provider_cannot_unlink`.
- Unknown `errorCode` received → client renders generic per-HTTP-class message and
  logs the raw code once (C/5 unknown-value fallback rule).
- R1's dotted registry (`auth.*`, `perm.*`, `nf.*`, `conflict.*`) is RETIRED — no
  artifact ever emitted it.

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
| O-6 | Duluka request-correlation id (hub `reqId` analog) | **NEW (R2)** — retired from the wire envelope as unevidenced; re-add via owner decision only |
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

---

## 12. VERDICT (R2)

**FROZEN.** Identifiers (§3), state machines (§5), sync CAS (§6), HTTP/error semantics
(§6.3/§7), boundaries (§2), and at-rest rules (§4) are fully specified, mutually
consistent, and grounded in the implemented, green-suite code plus the C/2 executable
spec. No UUID/ULID ambiguity remains (none exist); no API semantic contradiction remains
(§11 closes each one with evidence).

**Gates for the next integration phase** (do not block this freeze):
- §14 conformance gaps G-1..G-7 (server renames/foldings + sync endpoints + probe
  retargeting).
- Owner decisions O-1/O-2 gate the sync payload specifically; O-3a/O-6/O-7 are
  independent micro-decisions.

---

## 13. IMPLEMENTED HTTP SURFACE (R2 inventory — `Program.cs`)

| Route | Auth | Semantics (statuses per §6.3) |
|---|---|---|
| `GET /healthz`, `GET /healthz/ready` | none | liveness / readiness (schema version) |
| `POST /v1/auth/{provider}/start` | rate-limited per-IP | 501 reserved; 400 invalid device fields; 200 `{authorizationUrl, state(redacted), expiresInMinutes:10}` |
| `POST /v1/auth/{provider}/callback` | rate-limited | 400 invalid callback/state/device key/provider errors; 403 device_revoked; 200 `{sessionToken, accountId, deviceId, existingAccount, sessionExpiresAt}` |
| `POST /v1/account/providers` | Bearer | start LINK flow onto THIS account; 401 session_missing/session_invalid; 501 reserved |
| `POST /v1/account/providers/{provider}/complete` | flow-bound | 400 invalid_state; 401 session_invalid; 409 provider identity bound elsewhere; 200 `{linked, linkId, already}` |
| `POST /v1/auth/session/refresh` | Bearer | 200 `{sessionExpiresAt}` / 401 |
| `POST /v1/auth/session/revoke` | Bearer | 200 `{revoked:true}` / 401 (second revoke 401 = gap G-3) |
| `POST /v1/auth/sessions/revoke-all` | Bearer | 200 `{revokedSessions:n}` / 401 |
| `GET /v1/account/me` | Bearer | 200 `{accountId, displayName, createdAt, currentDevice}` |
| `GET /v1/account/providers` | Bearer | 200 `{providers:[{linkId, providerKey, providerEmail, status, linkedAt}]}` |
| `DELETE /v1/account/providers/{linkId}` | Bearer | 200 `{unlinked:true, revokedSessions:n}` / 409 last_provider / 404 link_not_found / 400 link_already_unlinked |
| `GET /v1/account/devices` | Bearer | 200 `{devices:[{deviceId, deviceName, createdAt, lastSeenAt, revokedAt}]}` |
| `POST /v1/account/devices/{deviceId}/revoke` | Bearer | 200 `{revoked:true, sessionsRevoked:n}` / 404 device_not_found (incl. cross-tenant) |
| (future) sync routes | Bearer + If-Match | per §6 — not implemented in v0 |

---

## 14. CONFORMANCE GAPS (server/tests vs FROZEN contract — next-phase work list)

> Read-only reconciliation: NOTHING below was changed in production code. Each gap is
> listed with the file that would need the change.

| # | Gap | Current (evidence) | Frozen requirement | File(s) |
|---|---|---|---|---|
| G-1 | 401 root-cause codes folded | All session failures → `401 session_invalid` | Emit `unknown_session` / `device_revoked` / `session_revoked` / `session_expired` with C/2 precedence | `Duluka/Duluka.Server/Program.cs` (+ `Data/Database.cs` to expose the cause) |
| G-2 | 409 code aliases | `identity_already_linked`, `last_provider` | Rename to `provider_linked_to_other_account`, `last_provider_cannot_unlink` | `Program.cs` |
| G-3 | Revoke idempotency | Second revoke → 401 | Second revoke → 200 (C/2 SES-2b) | `Program.cs` (`/v1/auth/session/revoke`) |
| G-4 | Unhandled `device_key_in_use` on login path | InvalidOperationException escapes (500) | Map to 409 `device_key_in_use` (or owner-chosen 4xx) | `Program.cs` callback catch |
| G-5 | Suspended-account folding | Suspended → 401 | 403 `account_suspended` | `Data/Database.cs` + `Program.cs` |
| G-6 | Rate-limit + 5xx bodies | 429 empty body; unhandled faults → plain 500 | Acceptable as-is; if bodies are added they must use §7.1 shape | `Program.cs` |
| G-7 | Sync slice | Only `SyncProfile` DDL exists | Implement §6 (If-Match, 428/409/429 semantics, initial version 1) | `Duluka/Duluka.Server` (new endpoints) |
| G-8 | HTTP probe route sketch | Probe targets `POST /accounts`, `PUT /sync`, … | Retarget at the §13 routes when `DULUKA_BASE_URL` testing starts (assert bodies per R7 too) | `Duluka.Account.Tests/DulukaHttpContractProbe.vb`, `Duluka.Account.Tests/TEST-MATRIX.md` |

FROZEN document owner: C/1. Changes to this document after R2 require a new evidence
citation per rule (same discipline as R1/R2).
