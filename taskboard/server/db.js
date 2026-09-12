import fs from 'node:fs';
import path from 'node:path';

let DatabaseSync;
try {
  ({ DatabaseSync } = await import('node:sqlite'));
} catch {
  throw new Error(
    'node:sqlite is not available. Task Board requires Node.js >= 23.4 (tested on v24). ' +
    `Current: ${process.version}`
  );
}

const SCHEMA = `
CREATE TABLE IF NOT EXISTS users (
  id            INTEGER PRIMARY KEY AUTOINCREMENT,
  username      TEXT UNIQUE NOT NULL,
  password_hash TEXT NOT NULL,
  role          TEXT NOT NULL CHECK (role IN ('OWNER','ADMIN','VIEWER')),
  created_at    TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS sessions (
  token      TEXT PRIMARY KEY,
  user_id    INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  created_at TEXT NOT NULL,
  expires_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS machines (
  id         INTEGER PRIMARY KEY AUTOINCREMENT,
  name       TEXT UNIQUE NOT NULL,
  status     TEXT NOT NULL DEFAULT 'STANDBY'
             CHECK (status IN ('ONLINE','OFFLINE','STANDBY','CLOSED')),
  notes      TEXT NOT NULL DEFAULT '',
  created_at TEXT NOT NULL,
  updated_at TEXT NOT NULL,
  version    INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS tasks (
  id                  INTEGER PRIMARY KEY AUTOINCREMENT,
  code                TEXT UNIQUE NOT NULL,
  title               TEXT NOT NULL,
  objective           TEXT NOT NULL DEFAULT '',
  scope               TEXT NOT NULL DEFAULT '',
  out_of_scope        TEXT NOT NULL DEFAULT '',
  acceptance_criteria TEXT NOT NULL DEFAULT '',
  machine_id          INTEGER REFERENCES machines(id),
  worker_id           INTEGER REFERENCES workers(id),
  priority            TEXT NOT NULL DEFAULT 'P1' CHECK (priority IN ('P0','P1','P2','P3')),
  status              TEXT NOT NULL DEFAULT 'CREATED'
                      CHECK (status IN ('CREATED','ASSIGNED','IN_PROGRESS','BLOCKED','COMPLETED','CANCELLED')),
  started_at          TEXT,
  completed_at        TEXT,
  progress            INTEGER NOT NULL DEFAULT 0 CHECK (progress BETWEEN 0 AND 100),
  blocker             TEXT NOT NULL DEFAULT '',
  files_changed       TEXT NOT NULL DEFAULT '',
  tests               TEXT NOT NULL DEFAULT '',
  notes               TEXT NOT NULL DEFAULT '',
  repository          TEXT NOT NULL DEFAULT '',
  branch              TEXT NOT NULL DEFAULT '',
  commit_hash         TEXT NOT NULL DEFAULT '',
  created_by          TEXT NOT NULL DEFAULT 'system',
  created_at          TEXT NOT NULL,
  updated_at          TEXT NOT NULL,
  revision            INTEGER NOT NULL DEFAULT 1,
  deleted_at          TEXT
);

CREATE TABLE IF NOT EXISTS workers (
  id              INTEGER PRIMARY KEY AUTOINCREMENT,
  machine_id      INTEGER NOT NULL REFERENCES machines(id),
  name            TEXT NOT NULL,
  status          TEXT NOT NULL DEFAULT 'NO SLOT'
                  CHECK (status IN ('WORKING','ACTIVE','END TASK','NO SLOT','STANDBY','CLOSED')),
  current_task_id INTEGER REFERENCES tasks(id),
  notes           TEXT NOT NULL DEFAULT '',
  created_at      TEXT NOT NULL,
  updated_at      TEXT NOT NULL,
  version         INTEGER NOT NULL DEFAULT 1,
  UNIQUE (machine_id, name)
);

CREATE INDEX IF NOT EXISTS idx_workers_machine  ON workers(machine_id);
CREATE INDEX IF NOT EXISTS idx_tasks_worker     ON tasks(worker_id);
CREATE INDEX IF NOT EXISTS idx_tasks_status     ON tasks(status);

CREATE TABLE IF NOT EXISTS worker_status_history (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  worker_id   INTEGER NOT NULL REFERENCES workers(id),
  machine     TEXT NOT NULL DEFAULT '',
  worker_name TEXT NOT NULL DEFAULT '',
  old_status  TEXT,
  new_status  TEXT NOT NULL,
  changed_by  TEXT NOT NULL DEFAULT 'system',
  reason      TEXT NOT NULL DEFAULT '',
  timestamp   TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_wsh_worker ON worker_status_history(worker_id);

CREATE TABLE IF NOT EXISTS task_history (
  id         INTEGER PRIMARY KEY AUTOINCREMENT,
  task_id    INTEGER NOT NULL REFERENCES tasks(id),
  event      TEXT NOT NULL,
  old_status TEXT,
  new_status TEXT,
  changed_by TEXT NOT NULL DEFAULT 'system',
  details    TEXT NOT NULL DEFAULT '',
  timestamp  TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_th_task ON task_history(task_id);

CREATE TABLE IF NOT EXISTS activity_log (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  timestamp   TEXT NOT NULL,
  actor       TEXT NOT NULL,
  action      TEXT NOT NULL,
  entity_type TEXT,
  entity_id   INTEGER,
  machine     TEXT,
  worker      TEXT,
  details     TEXT NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS idx_activity_ts ON activity_log(timestamp);
`;

export function openDb(dbPath) {
  fs.mkdirSync(path.dirname(dbPath), { recursive: true });
  const db = new DatabaseSync(dbPath);
  db.exec('PRAGMA journal_mode = WAL;');
  db.exec('PRAGMA foreign_keys = ON;');
  db.exec(SCHEMA);
  return db;
}

/** Run fn inside a transaction; rolls back on throw. node:sqlite is sync, so this is atomic per connection. */
export function tx(db, fn) {
  db.exec('BEGIN IMMEDIATE');
  try {
    const out = fn();
    db.exec('COMMIT');
    return out;
  } catch (err) {
    try { db.exec('ROLLBACK'); } catch { /* already rolled back */ }
    throw err;
  }
}
