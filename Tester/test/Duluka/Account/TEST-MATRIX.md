# Duluka v0.1 — Account/Sync Test Matrix & Security Regression Plan

C/2 deliverable. The contract lives as executable code in
`Duluka.Account.Tests/Contract/` — `DulukaContracts.vb` (interface + status/
error codes) and `InMemoryDulukaAccountService.vb` (deterministic reference
spec, injected clock). The HTTP harness (`DulukaHttpContractProbe.vb`)
re-runs the same matrix against a live service once `DULUKA_BASE_URL` exists.

Run: `dotnet run --project Duluka.Account.Tests -c Release`
Current: **19 passed / 0 failed** (HTTP probe group skipped until service exists).

## v0.1 identity model

```
Account 1..N ←→ ProviderIdentity(provider, providerUserId)   [≥1 required]
Device  N..1 → Account
Session N..1 → Device        (device revoke ⇒ cascade kill of its sessions)
SyncDoc  1..1 → Account      (monotonic version, If-Match writes)
```

Policy separations (deliberate):
- `CreateOrLinkAccount` resolves/creates ONE provider identity → one account.
  Growing an account to N credentials goes through authenticated
  `LinkProvider` — never silent merging of identities bound elsewhere.
- OBS/`savedReplayPath` input-path policy and Engine `RECORD_START`
  output-path policy are unrelated chains (see prior C/2 reports).

## Matrix

| ID | Scenario | Contract | Priority |
|----|----------|----------|----------|
| ACC-1 | create new identity | 201 + accountId | P0 |
| ACC-2 | **same provider identity → same account** (incl. case/space variants) | 200 + identical accountId — never a 2nd account | P0 |
| ACC-3 | same provider, different user | distinct accounts | P0 |
| UNL-1 | unlink one of two providers | 200, remaining survives | P0 |
| UNL-2 | **last-provider unlink** | 409 `last_provider_cannot_unlink`, account intact | P0 |
| UNL-3 | link identity already owned by another account | 409 `provider_linked_to_other_account` (no silent merge) | P0 |
| SES-1a/b | live use / **expired session** | 200 / **401 `session_expired`** | P0 |
| SES-2a/b | **revoke session** → use after / revoke twice | 401 `session_revoked` / idempotent 200 | P0 |
| SES-3 | unknown session | 401 `unknown_session` | P0 |
| DEV-1 | **device revoke cascade** → all its sessions die, no collateral, no new sessions on revoked device | 401 `device_revoked` (precedence over session_revoked) | P0 |
| SYN-1 | If-Match current | 200, version+1, read-back matches | P0 |
| SYN-2 | **stale If-Match** | 409 `stale_version` + current version revealed, loser must not overwrite | P0 |
| SYN-3 | missing If-Match | 428 `missing_if_match` (lost-update protection is not optional) | P0 |
| AUTH-1 | all authenticated surfaces × unknown session | 401 everywhere | P0 |
| AUTH-2 | cross-account probes (see/unlink/merge foreign identity) | refused; no leak, no merge | P0 |
| AUTH-3 | revocation outlives TTL | still 401 after expiry window | P1 |
| RACE-1 | 8 concurrent creates, same identity | exactly ONE account | P0 |
| RACE-2 | 8 concurrent If-Match writes at same version | exactly ONE winner, rest 409, version advances by 1 | P0 |

HTTP mapping: statuses are literal HTTP codes; bodies carry
`{"errorCode": "<stable code>"}` — never free-form errors, never stack traces.

## Security regression plan

Invariants carried from this repo's forensic history — each maps to a gate:

| # | Invariant | Origin | Gate |
|---|-----------|--------|------|
| R1 | Local IPC binds loopback only (`127.0.0.1`, `::1`), dual-stack can never widen it | F-04 | API.Hub.Boundary.Tests (10 real-socket cases) before any release |
| R2 | OAuth callback reflects only encoded values; no raw query text in HTML | reflected-XSS proof | Overlay.OAuth.Tests XSS-1 (6/6, Debug+Release) |
| R3 | No `client_secret` / `code_verifier` / `access_token` in any log line | secret-logging audit | Overlay.OAuth.Tests SEC-1..3 + grep gate: `Debug.WriteLine` lines must not interpolate `verifier`, `requestBody`, `json` |
| R4 | OAuth tokens at rest under DPAPI; legacy plaintext migrated/scrubbed | owner commit 42c8d2e | existing DPAPI contract tests |
| R5 | Device/session revocation is authoritative: revoked ⇒ no surviving usable credential, across TTL and restarts | this matrix | SES-2/3, DEV-1, AUTH-3 + live-probe re-run when DULUKA_BASE_URL exists |
| R6 | Sync lost-update protection mandatory: no write path may bypass If-Match | this matrix | SYN-3, RACE-2 on both adapters |
| R7 | Error bodies expose stable codes only (no internals/PII/stacks) | hub audit M5 | HTTP probe bodies regex once live |

Release gate order: `Duluka.Account.Tests` (in-memory) → HTTP probe with
`DULUKA_BASE_URL` against the staging service → R1/R2 suites → build.

## Known spec decisions (v0.1)

- 401 vs 403: authentication failures (expired/revoked/unknown session) are
  401; cross-account *resource* access is 403 with
  `cross_account_forbidden`. Unlinking a provider not on your account is
  409 `provider_not_linked` (resource-shape mismatch).
- 428 (Precondition Required) for missing If-Match on sync writes.
- Precedence on a dead session: unknown → `device_revoked` →
  `session_revoked` → `expired` (root cause wins — measured UX decision).
- Production must serialize the two races at the STORE (unique index on
  provider identity; optimistic concurrency on sync version), not with an
  application lock like the in-memory reference.
