# TASK BOARD — Mission Control

Single source of truth for **task state** and **worker status** across many machines and many AI workers.

> Goal: make it impossible to ever again misread "the AI said the work is done" as "the chat/worker is free".
> **TASK ≠ STATUS · REPORT ≠ STATUS** — status is an explicit field; nothing is ever inferred from reports, messages, or elapsed time.

---

## Run

Requirements: **Node.js ≥ 23.4** (tested on v24). No npm dependencies — everything uses Node built-ins.

```bash
cd taskboard
npm start          # http://localhost:3000  (PORT=8080 npm start to change)
npm test           # 23 API acceptance tests
```

The SQLite database is created and seeded on first start at `taskboard/data/taskboard.db`
(override with `TASKBOARD_DB=/path/to/file.db`).

## Access (open by default)

The board runs in **open access** mode: no login screen, anyone (including other AI agents such as GPT) can open the page, view everything, and act. Anonymous callers act with OWNER rights and are recorded in the audit trail as `anonymous`.

- Give yourself a name in the audit trail: send header `X-Actor: <name>` (e.g. `X-Actor: GPT`) on API calls.
- To restore the login wall, start with `TASKBOARD_OPEN_ACCESS=false npm start` — then all endpoints require the session cookie / Bearer token again.

The seeded accounts remain available in either mode (useful when access is closed):

| Username | Password   | Role   | Can do                          |
|----------|------------|--------|---------------------------------|
| `owner`  | `owner123` | OWNER  | everything                      |
| `admin`  | `admin123` | ADMIN  | everything                      |
| `viewer` | `viewer123`| VIEWER | read-only (all writes → 403)    |

Change these before real use (`users` table, passwords are scrypt-hashed).

Seeded example data (exactly per spec):

```
M1 ONLINE   W1 = WORKING   (T-0001 "Gallery.Video Finalization", 78%, 97 PASS / 0 FAIL / 6 SKIP,
                            blocker: 240fps architectural limit)
            W2 = END TASK  W3 = END TASK
M2 ONLINE   W1..W3 = END TASK
M3 CLOSED   W1..W3 = CLOSED
M4 STANDBY  W1..W3 = NO SLOT
```

---

## Architecture

```
taskboard/
├── server/
│   ├── index.js       entrypoint (PORT, TASKBOARD_DB env)
│   ├── app.js         HTTP router, REST API, auth, SSE bus, static files
│   ├── db.js          SQLite (node:sqlite) + relational schema + transactions
│   ├── seed.js        example data + scrypt password hashing
│   └── transitions.js THE status transition rules (single authority)
├── public/            zero-build vanilla JS SPA (dashboard, task detail, activity, history)
├── sync/zcode-sync.mjs  pulls real ZCode sessions from this machine into the board
├── test/api.test.js   node:test acceptance suite (24 tests)
└── data/taskboard.db  SQLite file (WAL mode) - gitignored
```

- **Server**: plain `node:http`, no framework. JSON REST API + Server-Sent Events.
- **DB**: SQLite via `node:sqlite` (`DatabaseSync`). Writes run inside `BEGIN IMMEDIATE` transactions; the sync single-connection model serializes writes, so transition checks + entity updates + history rows are atomic.
- **Frontend**: static SPA served by the same process. Hash routing (`#/`, `#/task/:id`, `#/activity`, `#/history`).
- **Real-time**: server pushes `machine.updated` / `worker.updated` / `task.updated` / `activity` events over **SSE** (`GET /events`). Every client re-fetches the affected view within ~300 ms. If SSE drops, the frontend automatically falls back to 5 s polling — the polling calls the same API the push events come from, so the architecture is real-time-first.
- **Tracker only**: the app stores repository/branch/commit/files/tests as text fields. It never executes git or any repository operation, and there are no destructive repository endpoints.

---

## ZCode sync (real data from this machine)

`npm run sync` (or `node sync/zcode-sync.mjs`) imports the **real** ZCode AI-worker sessions from this computer into the board — the same data shown by the ZCode Web Remote Control page:

- **Source**: `~/.zcode/v2/tasks-index.sqlite` (read from a temp copy; the live ZCode app files are never touched).
- **Target workspace**: `NVIDIA-Shadowplay` by default; choose another with `--workspace GalleryCloud` (run `npm run sync` once to list all workspaces found). Add `--include-archived` to include archived sessions.
- **What it does**: for each session it creates a board task (CREATED, unassigned) titled like the session, and writes a `[zcode-sync]` block into the task notes with the session id, ZCode's reported state (`completed`, `error`, ...), workspace, machine, model and timestamps. Re-running is idempotent — it only rewrites the notes when ZCode's reported state changed.
- **What it never does**: it does not change board task status and does not touch worker status. ZCode's state is a *report*; assigning, starting and completing stay explicit operator actions (TASK ≠ STATUS). Synced tasks wait in the **BACKLOG** section of the dashboard until someone presses ASSIGN.

Examples:

```bash
npm run sync                                   # NVIDIA-Shadowplay workspace -> http://localhost:15242
node sync/zcode-sync.mjs --url http://localhost:3000 --workspace GalleryCloud
node sync/zcode-sync.mjs --include-archived    # also import archived sessions
```

---

## Status semantics (do not redefine)

### Worker

| Status     | Meaning |
|------------|---------|
| `WORKING`  | worker is actually executing a task right now |
| `ACTIVE`   | worker is reserved/designated for use — not necessarily processing |
| `END TASK` | chat/worker is free, ready to receive a new task |
| `NO SLOT`  | no worker slot available |
| `STANDBY`  | worker exists but is not currently in use |
| `CLOSED`   | worker permanently closed / not allowed to be used |

Allowed transitions (enforced by the API, mirrored in the UI radio buttons):

```
WORKING   → ACTIVE | END TASK | NO SLOT | CLOSED
ACTIVE    → WORKING | END TASK | NO SLOT | CLOSED
END TASK  → ACTIVE | STANDBY | NO SLOT | CLOSED
NO SLOT   → END TASK | STANDBY | CLOSED
STANDBY   → END TASK | ACTIVE | NO SLOT | CLOSED
CLOSED    → STANDBY          (explicit admin reopen; otherwise terminal)
```

### Machine (independent of worker status — never implicitly bound)

```
ONLINE → STANDBY | OFFLINE | CLOSED
OFFLINE → ONLINE | STANDBY | CLOSED
STANDBY → ONLINE | OFFLINE | CLOSED
CLOSED → STANDBY                 (explicit reopen)
```

### Task

```
CREATED      → ASSIGNED | CANCELLED
ASSIGNED     → IN_PROGRESS | CREATED (unassign) | CANCELLED
IN_PROGRESS  → ASSIGNED (pause) | BLOCKED | COMPLETED | CANCELLED
BLOCKED      → IN_PROGRESS (unblock) | ASSIGNED | CANCELLED
COMPLETED    → CREATED (reopen)
CANCELLED    → CREATED (reopen)
```

### The critical rule

Nothing in the system auto-derives status:

- A report saying "11/11 PASS" (a `PATCH` of `tests`/`progress`/`notes`) **never** changes worker or task status — the PATCH endpoint rejects `status` outright and logs "status unchanged".
- A silent worker never gets guessed into any status.
- Every transition is an explicit API action by a logged-in actor with a **required reason**, recorded with old status, new status, timestamp, and who changed it.
- Editing a task's fields is deliberately a different endpoint (`PATCH /tasks/:id`) than changing its state (`POST /tasks/:id/<action>`).

---

## Task lifecycle (all steps explicit)

```
CREATE task            → status CREATED (no status side effects)
ASSIGN to worker       → task CREATED → ASSIGNED
                         worker END TASK → ACTIVE   (never WORKING automatically)
                         worker must be END TASK; machine/worker must not be CLOSED
START                  → task ASSIGNED → IN_PROGRESS (started_at set)
                         worker ACTIVE → WORKING    (separate explicit action)
PAUSE                  → task IN_PROGRESS → ASSIGNED, worker WORKING → ACTIVE
BLOCK / UNBLOCK        → task IN_PROGRESS ↔ BLOCKED
                         worker status deliberately untouched — change it explicitly if needed
COMPLETE               → task IN_PROGRESS → COMPLETED (completed_at, progress=100)
                         worker WORKING → END TASK
CANCEL                 → task → CANCELLED; task released from worker;
                         worker status NOT auto-changed (ACTIVE = "reserved, idle" is valid)
REOPEN                 → COMPLETED/CANCELLED → CREATED (unassigned); assign again explicitly
DELETE                 → soft delete (deleted_at); history is preserved
```

**1 worker = 1 task**: assigning a second open task to a worker is rejected with `409` unless the request passes `parallel: true` — explicit parallel-task mode — and the assignment is logged as `task.assigned.parallel`.

---

## Database (relational schema)

```
users(id, username, password_hash, role)
sessions(token, user_id, created_at, expires_at)
machines(id, name, status, notes, version, ...)
workers(id, machine_id → machines, name, status, current_task_id → tasks, version, ...)
        UNIQUE(machine_id, name)
tasks(id, code T-####, title, objective, scope, out_of_scope, acceptance_criteria,
      machine_id, worker_id, priority, status, started_at, completed_at, progress,
      blocker, files_changed, tests, notes, repository, branch, commit_hash,
      revision, deleted_at, ...)
worker_status_history(id, worker_id, machine, worker_name, old_status, new_status,
                      changed_by, reason, timestamp)
task_history(id, task_id, event, old_status, new_status, changed_by, details, timestamp)
activity_log(id, timestamp, actor, action, entity_type, entity_id, machine, worker, details)
```

- `machine 1→many workers`, `worker 1→many tasks`, `worker 1→many status_history`, `task 1→many history`.
- **History is append-only and never deleted by default** (even DELETE of a task only soft-deletes it).
- `task_history.event` ∈ created, assigned, started, paused, updated, blocked, unblocked, completed, cancelled, reopened, deleted.

---

## API

All endpoints are reachable without credentials in open access mode (default). With `TASKBOARD_OPEN_ACCESS=false`, everything except `/auth/login` requires a session cookie or `Authorization: Bearer <token>` from `POST /auth/login`, and writes require role OWNER or ADMIN. In open access mode an anonymous caller is treated as `anonymous` (name override via the `X-Actor` header) with OWNER rights.

```
POST   /auth/login {username, password}      → {token, user} + session cookie
POST   /auth/logout
GET    /auth/me

GET    /board                                → dashboard aggregate (machines+workers+tasks+counts+transitions)
GET    /meta                                 → statuses, priorities, transition maps

GET    /machines                             ?q=
POST   /machines                             {name, status?, notes?}                (default STANDBY)
PATCH  /machines/:id                         {name?, notes?, version}               (optimistic concurrency)
PATCH  /machines/:id/status                  {status, reason, version}

GET    /workers                              ?machine_id=&status=&q=
POST   /workers                              {machine_id, name, status?, notes?}    (default NO SLOT)
PATCH  /workers/:id                          {name?, notes?, version}
PATCH  /workers/:id/status                   {status, reason, version}              ← validated transitions

GET    /tasks                                ?status=&priority=&machine_id=&worker_id=&q=
GET    /tasks/:id                            → full detail + task_history + activity
POST   /tasks                                {title, priority?, objective?, ..., worker_id?}   (worker_id ⇒ immediate assign)
PATCH  /tasks/:id                            field edits only {…, revision} — rejects "status" with 400
DELETE /tasks/:id                            {reason} — soft delete

POST   /tasks/:id/assign                     {worker_id, reason, revision, parallel?}
POST   /tasks/:id/start | pause | complete | block | unblock | cancel | reopen    {reason, revision}

GET    /activity                             ?entity_type=&entity_id=&action=&machine=&worker=&limit=
GET    /history                              ?type=worker|task&worker_id=&task_id=&limit=
GET    /search?q=                            → {tasks, workers, machines}

GET    /events                               → SSE stream (real-time updates)
```

Conventions:

- **Reasons are mandatory** for every status change / task action / delete — the API answers 400 otherwise. Every change is logged (who, what, old → new, when, why).
- **Optimistic concurrency**: tasks carry `revision`, machines/workers carry `version`. Mutations must send the value the client read; mismatch → `409` with `current_version`. This prevents two operators (or two AI workers) from silently overwriting each other.
- Status transitions that are not in the maps above → `400` with the allowed list.

### Example: an AI worker reporting through the API

```bash
TOKEN=$(curl -s -X POST localhost:3000/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"username":"owner","password":"owner123"}' | jq -r .token)

# worker starts the assigned task (explicit)
curl -X POST localhost:3000/tasks/2/start \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"reason":"kicked off","revision":2}'

# a report NEVER changes status
curl -X PATCH localhost:3000/tasks/2 \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"revision":3,"progress":80,"tests":"40 PASS / 0 FAIL / 2 SKIP"}'

# finishing is an explicit human/operator action
curl -X POST localhost:3000/tasks/2/complete \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"reason":"all acceptance criteria met","revision":4}'
```

---

## UI

Dark, technical, developer-dashboard ("Mission Control") style. Desktop dashboard is primary; responsive down to tablet/mobile.

- **Dashboard**: every machine on one screen, machine header (status, STATUS ▾, + WORKER) and worker cards (status chip, current task, code, priority, progress bar + %, tests line, blocker, "updated X min ago").
- **Worker card actions**: `DETAILS` (task page), `STATUS ▾` (transition modal showing only valid transitions + required reason), `ASSIGN TASK` (for END TASK workers).
- **Task detail**: all fields (objective, scope, out-of-scope, acceptance criteria, progress, tests, blocker, files, notes, git info), explicit action buttons per state (ASSIGN / START / PAUSE / COMPLETE / BLOCK / UNBLOCK / CANCEL / REOPEN / EDIT FIELDS / DELETE), append-only task history, related activity.
- **Filters**: worker status (multi-select), machine, worker, priority.
- **Global search**: task name/id, machine, worker, file, test, blocker, notes — server-side across all text fields.
- **History pages**: worker status history and task history, append-only.
- **Accessibility of status**: every status is glyph + text label + color (never color alone); progress is bar + numeric %.
- **TASK ≠ STATUS banner** is always visible on the dashboard.

### Engineering safety

The Task Board is a tracker. It performs **no** `git reset / clean / revert / rebase / stash`, has no repository endpoints, and will never run git or any other process.

---

## Tests

`npm test` boots a fresh server on a temp database and verifies, among other things:

- seed shape (M1–M4, W1–W3 statuses, seeded task fields),
- invalid transitions rejected (e.g. `END TASK → WORKING` must go through `ACTIVE`),
- every transition recorded with old/new/who/reason/timestamp,
- full lifecycle create → assign → start → report → complete including the rule that a report **cannot** change status and `PATCH` cannot smuggle a status change,
- `1 worker = 1 task` (409) with explicit parallel override, logged,
- optimistic concurrency (stale `revision`/`version` → 409),
- roles (VIEWER read-only), missing reason → 400,
- filters, global search (title/file/blocker/machine/worker), activity log,
- cancel does not auto-change worker status, reopen, soft delete preserves history,
- extensibility (add machine M5 + workers), and SSE broadcast to connected clients.
