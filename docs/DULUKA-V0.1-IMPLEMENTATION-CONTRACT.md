# DULUKA ACCOUNT — v0.1 IMPLEMENTATION CONTRACT

Status: **READY (contract) — implementation BLOCKED on 3 owner decisions (§10)**
Scope owner: ScotcsDuluka · Synthesized from C/4 + C/5 + C/6 discoveries against HEAD of
`Engine-Rebuild-Stabilization`. This document is the SINGLE SOURCE OF TRUTH for the
Duluka Account v0.1 client/server contract. Where a discovery dictates a rule, the rule
cites its origin. Where no discovery decides, the item is marked **OPEN (owner)** — it is
NOT invented here.

---

## 0. SCOPE / NON-GOALS

In scope: identity model, state machines, concurrency control, HTTP semantics, error
envelope, module boundaries, at-rest rules.

Out of scope (hard):
- No server implementation, no UI, no changes to ShadowPlay production code.
- **No NVIDIA implementation decisions.** The NVIDIA capture/encoder modules are bound
  only by the module boundary rules in §3; every internal of `DdagrabBackend`,
  `NvencEncoderBackend`, `LiveMuxSession`, `CaptureSession` stays sovereign.

---

## 1. DISCOVERY GROUNDING (evidence base)

| Origin | Discovery | Contract rule derived |
|---|---|---|
| C/4 (`1c3b58f`, F-01/F-02) | Honest outcomes: RUN/SKIP/FAIL are distinct; a claim ("saved/valid") is backed by real evidence (probe), never existence-only | §6.4 truthfulness rules, §7 envelope `evidence` field |
| C/5 (`de2052e`, RECORD_START H1) | ONE validator seam per input class, BEFORE anything downstream exists; fixed value vocabulary; unknown-value fallback documented (`OverlayConfig.GetEngineMode`) | §4.2 identifier validation, §5 state vocabulary, §7.2 code registry |
| C/6 (F-05/F-07/C-1/C-2, `5f4bf61`/`23c74f7`/`3cee799`) | State commit atomic with effect; failure states CONVERGE (never wedge); timeout = intermediate state, not a lying terminal; ownership for every spawned thing; soak provenance | §5 state machines, §6.5 concurrency, §8 provenance |
| C/2 (`5c9c174`, LoopbackGate, API.Hub.Boundary.Tests) | Loopback-only transport; `reqId` correlation; reflected-output encoding; secret redaction in logs; hub is an UNAUTHENTICATED relay (S1) | §3.5 transport boundary, §7 envelope, §9 at-rest/redaction |
| C/3 (`0eb33d9`, SessionEndBroadcastPolicy) | Exactly-once terminal events; expiry and async-failure are first-class terminal triggers; no double broadcast | §5.3 session terminal semantics, §6.6 retry/idempotency |
| F-S2 (`42c8d2e`, GHS suite) | OAuth token at-rest = DPAPI CurrentUser, plaintext never on disk, legacy migration scrubs (incl. `.bak`), persistence justified by startup consumer | §4.4 ProviderKey/token at-rest, §9 |

---

## 2. MODULE BOUNDARIES

```text
┌─────────────────────────────┐   ┌──────────────────────────┐
│ Duluka Account module        │   │ ShadowPlay production     │
│ (identity/sync — THIS doc)   │   │ (capture/encode/mux)      │
│                              │   │                          │
│ owns: AccountId, LinkId,     │   │ owns: SessionId (capture  │
│       DeviceId, SessionId,   │   │ recording session),       │
│       ProviderKey, states,   │   │ pipeline processes,       │
│       sync version, envelope │   │ config.json ownership     │
└──────────────┬───────────────┘   └────────────┬─────────────┘
               │  ONE interface (§3.4)          │
               └────────────┬───────────────────┘
                            ▼
              Account service (NOT specified here —
              server impl out of scope; wire format IS)
```

### 2.1 Rules
1. The Account module **never imports** capture/encoder/mux internals. It consumes
   only the §3.4 interface (and vice versa).
2. The capture pipeline **never calls the account service directly.** Session telemetry
   reaches the account module only through the §3.4 event pass-through — same shape the
   C/3 session-end broadcast already uses (`SessionEndBroadcastPolicy.Decide`).
3. `config.json` remains the ONLY user-facing store (CONFIG_OWNERSHIP_MATRIX §1 law).
   The Account module gets its OWN store (see §4.5) and never writes `config.json`.
4. NVIDIA implementation (capture/encoder internals) is out of this contract's authority.
   The contract may state WHAT the module boundary passes (SessionId, terminal action)
   and never HOW capture/encode works.

---

## 3. IDENTIFIERS

### 3.1 AccountId
- Minted server-side at account creation. Opaque, UUIDv4-shaped string.
- Globally unique, immutable, never derived from user-identifying data
  (S8 PII lesson: no hardware serials, no usernames in the ID itself).
- Client stores it in the account store (§4.5); it is NOT a secret.

### 3.2 LinkId
- Identifies one (AccountId ↔ ProviderKey) LINK — the proof that an external
  provider identity belongs to this account. Minted server-side at link time.
- Immutable. One account MAY hold multiple links (multiple providers); one
  ProviderKey MUST map to at most one account per provider (server-enforced,
  §6.2).
- Deleting a link does not delete the account.

### 3.3 DeviceId
- Minted CLIENT-SIDE at install (UUIDv4, crypto-random), stored DPAPI-protected
  in the account store (F-S2 at-rest precedent). Regenerated only on explicit
  "forget device".
- Opaque. NEVER derived from hardware serials, MACs, disk serials, usernames,
  or the GTX/machine probe strings used by the C/6 soak harness (S8 PII rule).
- The client MAY keep a separate human-readable device LABEL (user-editable);
  the label is display data, never an identifier.

### 3.4 SessionId
- **Account-session** SessionId: minted server-side at login/session creation
  (§5.2). Distinct from the capture pipeline's recording-session identifier —
  the two MUST NOT share a namespace (C/2 lesson: `appName` spoofing through
  shared namespaces; G2 lesson: recording session has its own lifecycle owner).
- Recording-session correlation: the capture pipeline's own session identifier
  is passed through §3.4 events opaquely; the account module never re-mints or
  rewrites it.

### 3.5 ProviderKey
- The stable, provider-scoped user identity AFTER a successful OAuth exchange
  (e.g. GitHub numeric user id) — NOT the OAuth access token.
- The OAuth access/refresh token is a SECRET: stored DPAPI CurrentUser only
  (F-S2 GHS-1..3 contract), never in the account store, never in logs (C/2
  redaction rule), never in the error envelope.
- ProviderKey is safe to persist server-side and to appear in envelopes.

### 3.6 Validation (C/5 seam rule)
- EVERY identifier crossing the network seam passes ONE canonical validator
  (format + charset) at the client boundary, BEFORE any request object exists —
  the same seam discipline as `IsSafeRecordingOutputPath`.
- Validator vocabulary is fixed here: identifiers are `[0-9a-f-]{36}` (UUIDv4)
  or server-issued opaque strings of printable ASCII without quotes/control
  characters. Anything else is rejected client-side with `error.client.bad_id`
  and never reaches the wire.

---

## 4. AT-REST RULES (account store)

1. Account store = its own local file, owned exclusively by the Account module
   (CONFIG_OWNERSHIP_MATRIX law: one owner per store). NOT `config.json`.
2. At-rest secrets (OAuth access/refresh tokens, anything bearer-shaped):
   DPAPI `ProtectedData(CurrentUser)` — the F-S2 GHS contract, verbatim:
   plaintext never on disk, migration scrubs legacy plaintext including `.bak`,
   plaintext never in the error envelope or logs.
3. Non-secrets (AccountId, LinkId, DeviceId, Username, avatar URL, last-login)
   persist as plain JSON.
4. Writes are atomic (temp → `.bak` = previous → rename) and the `.bak`
   must never hold a secret-bearing predecessor — secrets are encrypted BEFORE
   the file is written, so any `.bak`/`.tmp` snapshot is already safe
   (GHS-2 proved the double-save scrub; keep that invariant).

---

## 5. STATE MACHINES

Vocabulary is FIXED here (C/5 lesson: one selector, one vocabulary, unknown
values have a documented fallback). Unknown state received from the server is
treated as the safest fallback of its family and reported once — never
silently mapped.

### 5.1 Account states
```text
Created → Linked → Active → Suspended → Deleted
                  ↘ Suspended ↗        ↓
                                    (terminal)
```
- `Created`: AccountId exists, no ProviderKey link yet.
- `Linked`: at least one LinkId bound.
- `Active`: Linked + at least one active DeviceId.
- `Suspended`: server-set, reversible. Client MUST stop sync (§6) but keep
  local data.
- `Deleted`: terminal. Client behavior on encountering it: wipe local account
  store secrets, keep DeviceId, surface one terminal notice (exactly once —
  C/3 rule).

### 5.2 Session (account session) states
```text
Pending → Active → Expired
               ↘ Revoked
               ↘ Failed
```
- Exactly ONE active session per DeviceId (server-enforced). A second login on
  the same device revokes the previous session first (server-side), client
  learns via `409 session_conflict` (§6.3).
- `Expired`/`Revoked`/`Failed` are TERMINAL — the client clears in-memory
  tokens, keeps the DeviceId, and reports exactly once (C/3 exactly-once rule;
  no double toast/broadcast, mirrors `SessionEndBroadcastPolicy`).

### 5.3 Device states
```text
Registered → Active → Stale → Removed
```
- `Registered`: DeviceId known, never seen online.
- `Active`: seen within the server's liveness window (window value = OPEN,
  owner; C/2 heartbeat cadence suggests 60 s granularity as the floor).
- `Stale`: not seen within the window — informational, NOT an error; the
  client treats it as "other device offline" and never invents a failure
  (C/4 honesty: unknown/offline ≠ failed).
- `Removed`: terminal; server refuses further sync for that DeviceId with
  `404 device_removed`.

### 5.4 Transition rules (C-1/C-2 derived)
1. A state transition and its causing effect commit atomically server-side —
   no intermediate state observable that lies about what happened (C-1:
   "state commit atomic with effect"; no unconditional downgrade of a
   committed state).
2. Every failure path CONVERGES to a terminal or retryable state within a
   bounded time — no state may wedge forever (C-2: worker-death → tail
   convergence; timeouts are intermediate states, never lying terminals).
3. Terminal semantics fire EXACTLY ONCE per (SessionId, terminal kind)
   (C/3 exactly-once; retries on the transport level must be idempotent, §6.6).

---

## 6. SYNC + CONCURRENCY

### 6.1 Sync version
- Every synced resource carries a monotonically increasing integer
  `syncVersion`, owned by the server. `0` = never synced.
- The client never computes it; it echoes the last value it received.

### 6.2 If-Match / 409
- Every mutating sync request carries `If-Match: <syncVersion>`.
- Server compares against current:
  - match → apply, return new syncVersion;
  - mismatch → `409 conflict` with the CURRENT server state attached in the
    envelope (body carries the server's resource + syncVersion) — the client
    re-applies its change on top or drops it by user decision. Never silent.
- Conflict resolution policy per resource kind is an **OPEN (owner)** decision
  (last-writer-wins vs field-merge) — v0.1 ships LWW as the default and
  documents it; per-field merge must not be silently introduced.
- Uniqueness races (e.g. ProviderKey already linked to another account)
  return `409 link_conflict` — deterministic, never `403` (a link conflict is
  a state conflict, not a permission problem).

### 6.3 401 / 403 / 404 semantics (fixed — never conflated)
- **401** = identity not established/expired: missing/invalid/expired session
  token. Client behavior: silently refresh or re-auth; MUST NOT surface as an
  error toast on its own.
- **403** = identity established, action not permitted (Suspended account,
  Removed device, token lacks scope). Client surfaces ONE terminal notice.
- **404** = identity established, resource does not exist (unknown AccountId/
  LinkId/SessionId/DeviceId or `device_removed`). Client treats as terminal
  for that resource and re-syncs the list.
- Server MUST NOT answer an auth problem with 404 to hide existence UNLESS the
  resource is cross-tenant (then 404 is REQUIRED and the client contract for
  it is the 404 branch above).
- This tri-split mirrors the C/4 honesty rule: a distinct reality must map to
  a distinct outcome (RUN/SKIP/FAIL ↔ 401/403/404) — never folded together.

### 6.4 Truthfulness (C/4)
- Any success response that claims a persisted/derived effect carries
  verifiable evidence in the envelope (`resource.syncVersion`, or a
  server-issued content hash). The client may verify before surfacing "saved".
- "Environment not capable" (no network, no provider reachability) is reported
  as `error.client.env` locally and NEVER sent to the server as a server
  failure.

### 6.5 Concurrency on the client (C-1 rule)
- Client-side state transitions of the local store commit atomically with the
  effect that caused them (single lock), and a failure may never downgrade an
  already-committed state (C-1 lesson from `DdagrabBackend.Start`).

### 6.6 Retries / idempotency (C-3 rule)
- Retries carry the SAME (SessionId, requestId, syncVersion precondition).
- Terminal events are delivered exactly once per key; retried deliveries are
  deduplicated by `(SessionId, terminalKind, syncVersion)`.
- Backoff: bounded, starts 1 s, doubles, caps 30 s (matches TcpClientHelper
  proven cadence) — never infinite silent retry without surfacing state.

---

## 7. ERROR ENVELOPE (single schema)

### 7.1 Shape
```json
{
  "ok": false,
  "reqId": "client-issued UUIDv4, echoed verbatim",
  "errorCode": "registry.code",
  "httpStatus": 409,
  "retryable": false,
  "conflict": { "syncVersion": 12, "resource": { } },
  "message": "human-readable, safe-for-display, HTML-encoded at render"
}
```
- `ok=true` responses carry the same `reqId` + the resource + its
  `syncVersion` (C/4 evidence rule; C/2 `reqId` correlation preserved).
- `message` is display text: HTML-encode on render (C/2 reflected-XSS fix),
  and it NEVER contains tokens, ProviderKeys, or raw OAuth payloads
  (C/2 redaction rule).

### 7.2 Code registry (fixed vocabulary, C/5 style)
```text
client.*       bad_id, env, protocol      (never sent to server)
auth.*         401 family: session_expired, session_revoked
perm.*         403 family: account_suspended, device_removed, scope_missing
nf.*           404 family: account, link, session, device_removed
conflict.*     409 family: sync_version, link_conflict, session_conflict
server.*       5xx family: internal, unavailable (retryable=true)
```
- Unknown `errorCode` received → client renders generic per-HTTP-class message
  and logs the raw code once (C/5 unknown-value fallback rule).

### 7.3 Exactly-once (C/3)
- One request → one terminal envelope. Retries reuse `reqId`; the server
  answers retried requests from an idempotency window rather than re-executing.

---

## 8. PROVENANCE (C/6 soak/BL-1 rule)

- Every account-module client build embeds the same provenance the BL-1 work
  gave the product: product version + git sha, printed in the module's
  diagnostic banner — so any support/log evidence identifies the exact binary.
- Sync payloads never include machine-identifying data beyond DeviceId (§3.3).

---

## 9. SECRETS / REDACTION

1. OAuth tokens: DPAPI at-rest (F-S2), in-memory only after decrypt, never
   logged, never in envelopes, never in `.bak`/`.tmp` snapshots (GHS-2
   invariant).
2. Logs redact by allow-list, not deny-list: a line may print identifiers
   (AccountId, LinkId, DeviceId, SessionId, ProviderKey) and codes; anything
   token-shaped (`gh*_`, `Bearer `, base64 blobs >256 chars) is redacted
   (C/2 rule).
3. The account transport never rides the unauthenticated hub relay (S1): the
   hub stays a loopback command relay (LoopbackGate); account traffic uses its
   own authenticated channel. This is a resolved conflict — see §11.

---

## 10. OPEN DECISIONS (owner — do NOT implement from this doc)

| # | Decision | Why open |
|---|---|---|
| O-1 | What syncs (payload contents: settings? recording metadata?) | No discovery defines the payload; CONFIG_OWNERSHIP_MATRIX must rule which settings may have a cloud shadow |
| O-2 | Conflict policy per resource (LWW default vs field-merge) | §6.2 ships LWW as documented default; real policy is a product decision |
| O-3 | Device liveness window + session TTL values | C/2 evidence gives the floor (60 s heartbeat), not the product values |
| O-4 | Server stack/hosting | Explicitly out of scope |
| O-5 | Provider list for v0.1 (GitHub is the proven one; others speculative) | F-S2 proved GitHub only |

---

## 11. CONFLICTS RESOLVED (the "ชน" log)

| Conflict | Sides | Resolution |
|---|---|---|
| C/2 hub relay vs account authn | Hub = unauthenticated broadcast (S1 finding) vs Account needs 401/403 | Account traffic gets its OWN authenticated channel; hub stays loopback command relay; §9.3. Never mix. |
| C/3 exactly-once vs sync retries | Terminal events must fire once vs transport retries | Idempotency key `(SessionId, terminalKind, syncVersion)` + server idempotency window (§6.6, §7.3) |
| C/4 SKIP honesty vs HTTP errors | "environment not capable" must not masquerade as a failure | Local `error.client.env` never sent to server; 401/403/404 reserved for server-side realities (§6.3) |
| C/5 validator seam vs identifier zoo | Each identifier class needs validation but only ONE seam per class | §3.6: one canonical validator at the client boundary, fixed charset, single rejection code |
| C/6 state convergence vs async sync | Client state must converge, but server round-trips are async | §5.4 bounded convergence + §6.6 bounded backoff; async inflight = intermediate state, never a lying terminal |
| CONFIG_OWNERSHIP_MATRIX (config.json = only user store) vs cloud sync | Sync would shadow user settings | §2.1(3): account store is separate; synced settings ownership = **O-1/O-2 owner decisions** — v0.1 does not sync into config.json |
| F-S2 DPAPI persistence vs token rotation | Token persisted but must rotate | Rotation is a server-issued re-issue over the same LinkId; at-rest contract unchanged (§9.1) |

---

## 12. VERDICT

**READY** as the v0.1 contract: identifiers, state machines, concurrency,
HTTP semantics, envelope, boundaries, and at-rest rules are fully specified and
grounded in C/2/C/3/C/4/C/5/C/6 evidence.

**BLOCKED for implementation** until O-1..O-5 are decided by the owner — none
of them can be responsibly derived from existing discoveries, and each gates a
different implementation slice (O-1/O-2 gate sync endpoints, O-3 gates session
TTL, O-4 gates the server build, O-5 gates the OAuth provider matrix).
