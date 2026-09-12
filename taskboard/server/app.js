import http from 'node:http';
import crypto from 'node:crypto';
import fs from 'node:fs';
import path from 'node:path';
import { openDb, tx } from './db.js';
import { seedIfEmpty, verifyPassword } from './seed.js';
import {
  WORKER_STATUSES, WORKER_TRANSITIONS,
  MACHINE_STATUSES, MACHINE_TRANSITIONS,
  TASK_STATUSES, TASK_TRANSITIONS,
  assertTransition,
} from './transitions.js';

const SESSION_TTL_MS = 7 * 24 * 60 * 60 * 1000;
const COOKIE_NAME = 'tb_token';
const MUTATING_ROLES = ['OWNER', 'ADMIN'];

function nowISO() { return new Date().toISOString(); }

function httpError(status, message, extra) {
  const err = new Error(message);
  err.status = status;
  err.expose = true;
  if (extra) Object.assign(err, extra);
  return err;
}

/* ------------------------------------------------------------------ *
 * App factory
 * ------------------------------------------------------------------ */
export function createApp({ dbPath, staticDir, seed = true, openAccess = String(process.env.TASKBOARD_OPEN_ACCESS ?? 'true') !== 'false' }) {
  const db = openDb(dbPath);
  const freshlySeeded = seed ? seedIfEmpty(db) : false;

  // --- SSE bus ---------------------------------------------------------
  const sseClients = new Set();
  let sseSeq = 0;
  function publish(type, data) {
    const payload = `data: ${JSON.stringify({ type, seq: ++sseSeq, data, ts: nowISO() })}\n\n`;
    for (const client of sseClients) {
      try { client.res.write(payload); } catch { sseClients.delete(client); }
    }
  }
  setInterval(() => {
    for (const client of sseClients) {
      try { client.res.write(': ping\n\n'); } catch { sseClients.delete(client); }
    }
  }, 25_000).unref();

  // --- small helpers ----------------------------------------------------
  const q = (sql) => db.prepare(sql);

  function recordActivity({ actor, action, entity_type = null, entity_id = null, machine = null, worker = null, details = '', ts = nowISO() }) {
    q(`INSERT INTO activity_log (timestamp, actor, action, entity_type, entity_id, machine, worker, details)
       VALUES (?,?,?,?,?,?,?,?)`)
      .run(ts, actor, action, entity_type, entity_id, machine, worker, details);
    publish('activity', { actor, action, entity_type, entity_id, machine, worker, details, ts });
  }

  function recordWorkerStatusChange(worker, oldStatus, newStatus, actor, reason, ts) {
    const machine = q('SELECT name FROM machines WHERE id = ?').get(worker.machine_id);
    q(`INSERT INTO worker_status_history (worker_id, machine, worker_name, old_status, new_status, changed_by, reason, timestamp)
       VALUES (?,?,?,?,?,?,?,?)`)
      .run(worker.id, machine?.name ?? '', worker.name, oldStatus, newStatus, actor, reason, ts);
  }

  function recordTaskHistory(taskId, event, oldStatus, newStatus, actor, details, ts) {
    q(`INSERT INTO task_history (task_id, event, old_status, new_status, changed_by, details, timestamp)
       VALUES (?,?,?,?,?,?,?)`)
      .run(taskId, event, oldStatus, newStatus, actor, details, ts);
  }

  function getMachine(id) {
    const row = q('SELECT * FROM machines WHERE id = ?').get(Number(id));
    if (!row) throw httpError(404, 'Machine not found');
    return row;
  }
  function getWorker(id) {
    const row = q('SELECT * FROM workers WHERE id = ?').get(Number(id));
    if (!row) throw httpError(404, 'Worker not found');
    return row;
  }
  function getTask(id) {
    const row = q('SELECT * FROM tasks WHERE id = ?').get(Number(id));
    if (!row) throw httpError(404, 'Task not found');
    return row;
  }
  function getLiveTask(id) {
    const row = getTask(id);
    if (row.deleted_at) throw httpError(404, 'Task not found (deleted)');
    return row;
  }

  function requireReason(reason, what = 'status change') {
    if (typeof reason !== 'string' || !reason.trim()) {
      throw httpError(400, `A reason is required for every ${what} (audit trail).`);
    }
    return reason.trim();
  }

  function checkVersion(row, body, field, label) {
    const expected = body[field];
    if (expected === undefined || expected === null) {
      throw httpError(400, `Missing "${field}" - optimistic concurrency protection requires the ${label} version you read.`);
    }
    if (Number(expected) !== row[field]) {
      throw httpError(409, `${label} was modified by someone else (version conflict: you sent ${field}=${expected}, current is ${row[field]}). Reload and retry.`, { current_version: row[field] });
    }
  }

  function taskSummary(row) {
    if (!row) return null;
    return {
      id: row.id, code: row.code, title: row.title, status: row.status,
      priority: row.priority, progress: row.progress, tests: row.tests,
      blocker: row.blocker, files_changed: row.files_changed,
      machine_id: row.machine_id, worker_id: row.worker_id,
      started_at: row.started_at, updated_at: row.updated_at, revision: row.revision,
    };
  }

  function workerView(w) {
    const machine = q('SELECT id, name, status FROM machines WHERE id = ?').get(w.machine_id);
    let current_task = null;
    if (w.current_task_id) {
      const t = q('SELECT * FROM tasks WHERE id = ? AND deleted_at IS NULL').get(w.current_task_id);
      if (t && ['ASSIGNED', 'IN_PROGRESS', 'BLOCKED'].includes(t.status)) current_task = taskSummary(t);
    }
    return {
      id: w.id, name: w.name, status: w.status, notes: w.notes,
      machine_id: w.machine_id, machine_name: machine?.name ?? '',
      machine_status: machine?.status ?? '',
      current_task_id: w.current_task_id, current_task,
      created_at: w.created_at, updated_at: w.updated_at, version: w.version,
    };
  }

  function machineView(m) {
    const workers = q('SELECT * FROM workers WHERE machine_id = ? ORDER BY name').all(m.id);
    return {
      id: m.id, name: m.name, status: m.status, notes: m.notes,
      created_at: m.created_at, updated_at: m.updated_at, version: m.version,
      worker_count: workers.length,
      workers: workers.map(workerView),
    };
  }

  // --- auth -------------------------------------------------------------
  function parseCookies(req) {
    const out = {};
    const raw = req.headers.cookie;
    if (!raw) return out;
    for (const part of raw.split(';')) {
      const idx = part.indexOf('=');
      if (idx > 0) out[part.slice(0, idx).trim()] = decodeURIComponent(part.slice(idx + 1).trim());
    }
    return out;
  }

  function resolveUser(req) {
    let token = null;
    const authz = req.headers.authorization;
    if (authz?.startsWith('Bearer ')) token = authz.slice(7).trim();
    if (!token) token = parseCookies(req)[COOKIE_NAME] || null;
    if (!token) return null;
    const row = q(`
      SELECT u.id, u.username, u.role, s.expires_at
      FROM sessions s JOIN users u ON u.id = s.user_id
      WHERE s.token = ?`).get(token);
    if (!row) return null;
    if (new Date(row.expires_at).getTime() < Date.now()) {
      q('DELETE FROM sessions WHERE token = ?').run(token);
      return null;
    }
    return { id: row.id, username: row.username, role: row.role, token };
  }

  // --- routing table ------------------------------------------------------
  const routes = [];
  function route(method, pattern, handler, opts = {}) {
    const keys = [];
    const regex = new RegExp('^' + pattern.replace(/:[^/]+/g, (m) => {
      keys.push(m.slice(1));
      return '([^/]+)';
    }) + '$');
    routes.push({ method, regex, keys, handler, ...opts });
  }

  /* ================= AUTH ================= */
  route('POST', '/auth/login', (ctx) => {
    const { username, password } = ctx.body || {};
    const user = q('SELECT * FROM users WHERE username = ?').get(String(username || ''));
    if (!user || !verifyPassword(String(password || ''), user.password_hash)) {
      throw httpError(401, 'Invalid username or password');
    }
    const token = crypto.randomBytes(32).toString('hex');
    const ts = nowISO();
    q('INSERT INTO sessions (token, user_id, created_at, expires_at) VALUES (?,?,?,?)')
      .run(token, user.id, ts, new Date(Date.now() + SESSION_TTL_MS).toISOString());
    recordActivity({ actor: user.username, action: 'auth.login', details: `role=${user.role}`, ts });
    return {
      token,
      user: { id: user.id, username: user.username, role: user.role },
      // cookie is set by the transport layer via ctx.res
      cookie: `${COOKIE_NAME}=${token}; HttpOnly; Path=/; SameSite=Lax; Max-Age=${SESSION_TTL_MS / 1000}`,
    };
  }, { public: true });

  route('POST', '/auth/logout', (ctx) => {
    if (ctx.user?.token) q('DELETE FROM sessions WHERE token = ?').run(ctx.user.token);
    recordActivity({ actor: ctx.user?.username ?? 'anonymous', action: 'auth.logout', ts: nowISO() });
    return { ok: true, cookie: `${COOKIE_NAME}=; HttpOnly; Path=/; SameSite=Lax; Max-Age=0` };
  });

  route('GET', '/auth/me', (ctx) => ({
    user: { id: ctx.user.id, username: ctx.user.username, role: ctx.user.role },
    openAccess,
  }));

  /* ================= BOARD (dashboard aggregate) ================= */
  route('GET', '/board', (ctx) => {
    const machines = q('SELECT * FROM machines ORDER BY name').all().map(machineView);
    const counts = {
      machines: machines.length,
      workers: q('SELECT COUNT(*) AS n FROM workers').get().n,
      open_tasks: q(`SELECT COUNT(*) AS n FROM tasks WHERE deleted_at IS NULL AND status IN ('CREATED','ASSIGNED','IN_PROGRESS','BLOCKED')`).get().n,
      unassigned_tasks: q(`SELECT COUNT(*) AS n FROM tasks WHERE deleted_at IS NULL AND status = 'CREATED'`).get().n,
    };
    return { machines, counts, transitions: { worker: WORKER_TRANSITIONS, machine: MACHINE_TRANSITIONS, task: TASK_TRANSITIONS } };
  });

  /* ================= MACHINES ================= */
  route('GET', '/machines', () => {
    const rows = q('SELECT * FROM machines ORDER BY name').all();
    return rows.map(machineView);
  });

  route('POST', '/machines', (ctx) => {
    const name = String(ctx.body?.name || '').trim();
    if (!name) throw httpError(400, 'Machine name is required');
    const status = ctx.body?.status ?? 'STANDBY';
    if (!MACHINE_STATUSES.includes(status)) throw httpError(400, `status must be one of ${MACHINE_STATUSES.join(', ')}`);
    if (q('SELECT id FROM machines WHERE name = ?').get(name)) throw httpError(409, `Machine "${name}" already exists`);
    const ts = nowISO();
    const info = q('INSERT INTO machines (name, status, notes, created_at, updated_at) VALUES (?,?,?,?,?)')
      .run(name, status, String(ctx.body?.notes || ''), ts, ts);
    const m = getMachine(info.lastInsertRowid);
    recordActivity({ actor: ctx.user.username, action: 'machine.created', entity_type: 'machine', entity_id: m.id, machine: m.name, details: `registered as ${status}`, ts });
    publish('machine.updated', machineView(m));
    return machineView(m);
  });

  route('PATCH', '/machines/:id', (ctx) => {
    const m = getMachine(ctx.params.id);
    checkVersion(m, ctx.body || {}, 'version', 'Machine');
    const next = { ...m };
    if (ctx.body?.name !== undefined) {
      const name = String(ctx.body.name).trim();
      if (!name) throw httpError(400, 'Machine name cannot be empty');
      const dup = q('SELECT id FROM machines WHERE name = ? AND id != ?').get(name, m.id);
      if (dup) throw httpError(409, `Machine "${name}" already exists`);
      next.name = name;
    }
    if (ctx.body?.notes !== undefined) next.notes = String(ctx.body.notes);
    const ts = nowISO();
    q('UPDATE machines SET name = ?, notes = ?, updated_at = ?, version = version + 1 WHERE id = ?')
      .run(next.name, next.notes, ts, m.id);
    const out = getMachine(m.id);
    recordActivity({ actor: ctx.user.username, action: 'machine.updated', entity_type: 'machine', entity_id: m.id, machine: out.name, details: `fields: ${Object.keys(ctx.body).filter((k) => !['version'].includes(k)).join(', ')}`, ts });
    publish('machine.updated', machineView(out));
    return machineView(out);
  });

  route('PATCH', '/machines/:id/status', (ctx) => {
    const m = getMachine(ctx.params.id);
    checkVersion(m, ctx.body || {}, 'version', 'Machine');
    const to = String(ctx.body?.status || '');
    if (!MACHINE_STATUSES.includes(to)) throw httpError(400, `status must be one of ${MACHINE_STATUSES.join(', ')}`);
    const reason = requireReason(ctx.body?.reason, 'machine status change');
    assertTransition(MACHINE_TRANSITIONS, m.status, to, 'Machine');
    const ts = nowISO();
    q('UPDATE machines SET status = ?, updated_at = ?, version = version + 1 WHERE id = ?').run(to, ts, m.id);
    recordActivity({
      actor: ctx.user.username, action: 'machine.status_changed', entity_type: 'machine', entity_id: m.id,
      machine: m.name, details: `${m.status} -> ${to} | reason: ${reason}`, ts,
    });
    const out = getMachine(m.id);
    publish('machine.updated', machineView(out));
    return machineView(out);
  });

  /* ================= WORKERS ================= */
  route('GET', '/workers', (ctx) => {
    let rows = q(`SELECT w.* FROM workers w
                  JOIN machines m ON m.id = w.machine_id
                  ORDER BY m.name, w.name`).all();
    const { machine_id, status, q: query } = ctx.query;
    if (machine_id) rows = rows.filter((w) => w.machine_id === Number(machine_id));
    if (status) {
      const set = new Set(String(status).split(',').map((s) => s.trim()));
      rows = rows.filter((w) => set.has(w.status));
    }
    if (query) {
      const needle = String(query).toLowerCase();
      rows = rows.filter((w) => {
        const m = q('SELECT name FROM machines WHERE id = ?').get(w.machine_id);
        return w.name.toLowerCase().includes(needle) || (m?.name || '').toLowerCase().includes(needle);
      });
    }
    return rows.map(workerView);
  });

  route('POST', '/workers', (ctx) => {
    const machine = getMachine(ctx.body?.machine_id);
    const name = String(ctx.body?.name || '').trim();
    if (!name) throw httpError(400, 'Worker name is required');
    if (machine.status === 'CLOSED') throw httpError(409, `Machine ${machine.name} is CLOSED - cannot add workers to a closed machine`);
    const status = ctx.body?.status ?? 'NO SLOT';
    if (!WORKER_STATUSES.includes(status)) throw httpError(400, `status must be one of ${WORKER_STATUSES.join(', ')}`);
    if (q('SELECT id FROM workers WHERE machine_id = ? AND name = ?').get(machine.id, name)) {
      throw httpError(409, `Worker ${machine.name}/${name} already exists`);
    }
    const ts = nowISO();
    tx(db, () => {
      const info = q('INSERT INTO workers (machine_id, name, status, notes, created_at, updated_at) VALUES (?,?,?,?,?,?)')
        .run(machine.id, name, status, String(ctx.body?.notes || ''), ts, ts);
      const wid = Number(info.lastInsertRowid);
      q(`INSERT INTO worker_status_history (worker_id, machine, worker_name, old_status, new_status, changed_by, reason, timestamp)
         VALUES (?,?,?,?,?,?,?,?)`)
        .run(wid, machine.name, name, null, status, ctx.user.username, 'worker created', ts);
      recordActivity({ actor: ctx.user.username, action: 'worker.created', entity_type: 'worker', entity_id: wid, machine: machine.name, worker: name, details: `initial status ${status}`, ts });
      publish('worker.updated', workerView(getWorker(wid)));
      return workerView(getWorker(wid));
    });
    return workerView(getWorker(q('SELECT id FROM workers WHERE machine_id = ? AND name = ?').get(machine.id, name).id));
  });

  route('PATCH', '/workers/:id', (ctx) => {
    const w = getWorker(ctx.params.id);
    checkVersion(w, ctx.body || {}, 'version', 'Worker');
    const next = { ...w };
    if (ctx.body?.name !== undefined) {
      const name = String(ctx.body.name).trim();
      if (!name) throw httpError(400, 'Worker name cannot be empty');
      next.name = name;
    }
    if (ctx.body?.notes !== undefined) next.notes = String(ctx.body.notes);
    const ts = nowISO();
    q('UPDATE workers SET name = ?, notes = ?, updated_at = ?, version = version + 1 WHERE id = ?')
      .run(next.name, next.notes, ts, w.id);
    const out = getWorker(w.id);
    recordActivity({ actor: ctx.user.username, action: 'worker.updated', entity_type: 'worker', entity_id: w.id, machine: workerView(out).machine_name, worker: out.name, details: `fields: ${Object.keys(ctx.body).filter((k) => !['version'].includes(k)).join(', ')}`, ts });
    publish('worker.updated', workerView(out));
    return workerView(out);
  });

  route('PATCH', '/workers/:id/status', (ctx) => {
    const w = getWorker(ctx.params.id);
    checkVersion(w, ctx.body || {}, 'version', 'Worker');
    const to = String(ctx.body?.status || '');
    if (!WORKER_STATUSES.includes(to)) throw httpError(400, `status must be one of ${WORKER_STATUSES.join(', ')}`);
    const reason = requireReason(ctx.body?.reason, 'worker status change');
    assertTransition(WORKER_TRANSITIONS, w.status, to, 'Worker');
    const ts = nowISO();
    tx(db, () => {
      q('UPDATE workers SET status = ?, updated_at = ?, version = version + 1 WHERE id = ?').run(to, ts, w.id);
      recordWorkerStatusChange(w, w.status, to, ctx.user.username, reason, ts);
      // Explicit worker status changes NEVER touch tasks. Warn if a task is still open.
      if (w.current_task_id) {
        const t = q('SELECT * FROM tasks WHERE id = ? AND deleted_at IS NULL').get(w.current_task_id);
        if (t && ['ASSIGNED', 'IN_PROGRESS', 'BLOCKED'].includes(t.status) && !['WORKING', 'ACTIVE'].includes(to)) {
          recordActivity({
            actor: ctx.user.username, action: 'worker.warning', entity_type: 'worker', entity_id: w.id,
            machine: workerView(w).machine_name, worker: w.name,
            details: `Worker moved to ${to} while task ${t.code} "${t.title}" is still ${t.status}. Task status was NOT changed - reassign or close it explicitly.`,
            ts,
          });
        }
      }
      recordActivity({
        actor: ctx.user.username, action: 'worker.status_changed', entity_type: 'worker', entity_id: w.id,
        machine: workerView(w).machine_name, worker: w.name, details: `${w.status} -> ${to} | reason: ${reason}`, ts,
      });
      publish('worker.updated', workerView(getWorker(w.id)));
    });
    return workerView(getWorker(w.id));
  });

  /* ================= TASKS ================= */
  function taskFull(t) {
    const machine = t.machine_id ? q('SELECT name FROM machines WHERE id = ?').get(t.machine_id) : null;
    const worker = t.worker_id ? q('SELECT name FROM workers WHERE id = ?').get(t.worker_id) : null;
    const history = q('SELECT * FROM task_history WHERE task_id = ? ORDER BY timestamp ASC, id ASC').all(t.id);
    const activity = q(`SELECT * FROM activity_log WHERE entity_type = 'task' AND entity_id = ? ORDER BY timestamp DESC, id DESC LIMIT 100`).all(t.id);
    return {
      ...t,
      machine_name: machine?.name ?? null,
      worker_name: worker?.name ?? null,
      history,
      activity,
    };
  }

  route('GET', '/tasks', (ctx) => {
    let rows = q('SELECT * FROM tasks WHERE deleted_at IS NULL ORDER BY updated_at DESC').all();
    const { status, priority, machine_id, worker_id, q: query } = ctx.query;
    if (status) {
      const set = new Set(String(status).split(',').map((s) => s.trim()));
      rows = rows.filter((t) => set.has(t.status));
    }
    if (priority) {
      const set = new Set(String(priority).split(',').map((s) => s.trim()));
      rows = rows.filter((t) => set.has(t.priority));
    }
    if (machine_id) rows = rows.filter((t) => t.machine_id === Number(machine_id));
    if (worker_id) rows = rows.filter((t) => t.worker_id === Number(worker_id));
    if (query) {
      const needle = String(query).toLowerCase();
      rows = rows.filter((t) =>
        [t.code, t.title, t.objective, t.scope, t.notes, t.files_changed, t.tests, t.blocker, t.repository, t.branch]
          .some((f) => (f || '').toLowerCase().includes(needle)));
    }
    return rows.map(taskSummary);
  });

  route('GET', '/tasks/:id', (ctx) => taskFull(getTask(ctx.params.id)));

  route('POST', '/tasks', (ctx) => {
    const b = ctx.body || {};
    const title = String(b.title || '').trim();
    if (!title) throw httpError(400, 'Task title is required');
    const priority = b.priority ?? 'P1';
    if (!['P0', 'P1', 'P2', 'P3'].includes(priority)) throw httpError(400, 'priority must be P0, P1, P2 or P3');
    const progress = b.progress === undefined ? 0 : Number(b.progress);
    if (!Number.isInteger(progress) || progress < 0 || progress > 100) throw httpError(400, 'progress must be an integer 0-100');

    let worker = null;
    if (b.worker_id !== undefined && b.worker_id !== null && b.worker_id !== '') {
      worker = getWorker(b.worker_id);
      if (getMachine(worker.machine_id).status === 'CLOSED') throw httpError(409, 'Cannot assign to a machine that is CLOSED');
      if (worker.status === 'CLOSED') throw httpError(409, 'Cannot assign to a CLOSED worker');
      if (worker.status !== 'END TASK') {
        throw httpError(409, `Worker must be END TASK to accept a task (current: ${worker.status}). Change status explicitly first.`);
      }
      const openCount = q(`SELECT COUNT(*) AS n FROM tasks WHERE worker_id = ? AND deleted_at IS NULL AND status IN ('ASSIGNED','IN_PROGRESS','BLOCKED')`).get(worker.id).n;
      if (openCount > 0 && b.parallel !== true) {
        throw httpError(409, '1 worker = 1 task: this worker already has an open task. Pass parallel:true only as an explicit parallel-task mode.', { hint: 'parallel task mode' });
      }
    }

    const ts = nowISO();
    const id = tx(db, () => {
      const info = q(`INSERT INTO tasks (code, title, objective, scope, out_of_scope, acceptance_criteria,
          machine_id, worker_id, priority, status, progress, blocker, files_changed, tests, notes,
          repository, branch, commit_hash, created_by, created_at, updated_at)
        VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)`)
        .run('T-PENDING', title, String(b.objective || ''), String(b.scope || ''), String(b.out_of_scope || ''),
          String(b.acceptance_criteria || ''), worker ? worker.machine_id : null,
          worker ? worker.id : null, priority, worker ? 'ASSIGNED' : 'CREATED', progress,
          String(b.blocker || ''), String(b.files_changed || ''), String(b.tests || ''), String(b.notes || ''),
          String(b.repository || ''), String(b.branch || ''), String(b.commit_hash || ''),
          ctx.user.username, ts, ts);
      const tid = Number(info.lastInsertRowid);
      const code = `T-${String(tid).padStart(4, '0')}`;
      q('UPDATE tasks SET code = ? WHERE id = ?').run(code, tid);
      recordTaskHistory(tid, 'created', null, 'CREATED', ctx.user.username, `Task created (${code})`, ts);
      recordActivity({ actor: ctx.user.username, action: 'task.created', entity_type: 'task', entity_id: tid, machine: worker ? (q('SELECT name FROM machines WHERE id=?').get(worker.machine_id)?.name ?? null) : null, worker: worker?.name ?? null, details: `${code} "${title}" [${priority}]`, ts });
      if (worker) {
        // explicit assignment on creation: END TASK -> ACTIVE (never WORKING)
        const wBefore = getWorker(worker.id);
        assertTransition(WORKER_TRANSITIONS, wBefore.status, 'ACTIVE', 'Worker');
        q('UPDATE workers SET status = ?, current_task_id = ?, updated_at = ?, version = version + 1 WHERE id = ?')
          .run('ACTIVE', tid, ts, worker.id);
        const wName = q('SELECT name FROM machines WHERE id = ?').get(worker.machine_id)?.name ?? '';
        recordWorkerStatusChange(wBefore, wBefore.status, 'ACTIVE', ctx.user.username, `Task assigned: ${code} ${title}`, ts);
        recordTaskHistory(tid, 'assigned', 'CREATED', 'ASSIGNED', ctx.user.username, `Assigned to ${wName}/${worker.name}`, ts);
        recordActivity({ actor: ctx.user.username, action: 'task.assigned', entity_type: 'task', entity_id: tid, machine: wName, worker: worker.name, details: `${code} -> ${wName}/${worker.name} (worker END TASK -> ACTIVE)`, ts });
      }
      publish('task.updated', { id: tid, code });
      return tid;
    });
    return taskFull(getLiveTask(id));
  });

  const TASK_EDITABLE = ['title', 'objective', 'scope', 'out_of_scope', 'acceptance_criteria', 'priority', 'progress', 'blocker', 'files_changed', 'tests', 'notes', 'repository', 'branch', 'commit_hash'];

  route('PATCH', '/tasks/:id', (ctx) => {
    const t = getLiveTask(ctx.params.id);
    checkVersion(t, ctx.body || {}, 'revision', 'Task');
    const b = ctx.body || {};
    if ('status' in b) {
      throw httpError(400, 'TASK != STATUS: task status only changes via explicit action endpoints (/assign, /start, /complete, ...). Editing fields or adding reports never changes status.');
    }
    if ('worker_id' in b) {
      throw httpError(400, 'Worker assignment only changes via POST /tasks/:id/assign');
    }
    const updates = {};
    for (const field of TASK_EDITABLE) {
      if (b[field] === undefined) continue;
      let value = b[field];
      if (field === 'title') {
        value = String(value).trim();
        if (!value) throw httpError(400, 'Task title cannot be empty');
      } else if (field === 'priority') {
        if (!['P0', 'P1', 'P2', 'P3'].includes(value)) throw httpError(400, 'priority must be P0, P1, P2 or P3');
      } else if (field === 'progress') {
        value = Number(value);
        if (!Number.isInteger(value) || value < 0 || value > 100) throw httpError(400, 'progress must be an integer 0-100');
      } else {
        value = value === null ? '' : String(value);
      }
      updates[field] = value;
    }
    if (Object.keys(updates).length === 0) throw httpError(400, 'No editable fields provided');
    const ts = nowISO();
    tx(db, () => {
      const sets = Object.keys(updates).map((k) => `${k} = ?`).join(', ');
      q(`UPDATE tasks SET ${sets}, updated_at = ?, revision = revision + 1 WHERE id = ?`)
        .run(...Object.values(updates), ts, t.id);
      recordTaskHistory(t.id, 'updated', t.status, t.status, ctx.user.username, `Changed: ${Object.keys(updates).join(', ')}`, ts);
      recordActivity({ actor: ctx.user.username, action: 'task.updated', entity_type: 'task', entity_id: t.id, machine: t.machine_id ? q('SELECT name FROM machines WHERE id=?').get(t.machine_id)?.name ?? null : null, worker: t.worker_id ? q('SELECT name FROM workers WHERE id=?').get(t.worker_id)?.name ?? null : null, details: `${t.code} fields updated: ${Object.keys(updates).join(', ')} (status unchanged: ${t.status})`, ts });
      publish('task.updated', { id: t.id, code: t.code });
    });
    return taskFull(getLiveTask(t.id));
  });

  route('DELETE', '/tasks/:id', (ctx) => {
    const t = getLiveTask(ctx.params.id);
    const reason = requireReason(ctx.body?.reason, 'task deletion');
    const ts = nowISO();
    tx(db, () => {
      q('UPDATE tasks SET deleted_at = ?, updated_at = ?, revision = revision + 1 WHERE id = ?').run(ts, ts, t.id);
      if (t.worker_id) {
        q('UPDATE workers SET current_task_id = NULL WHERE current_task_id = ?').run(t.id);
      }
      recordTaskHistory(t.id, 'deleted', t.status, t.status, ctx.user.username, `Soft-deleted: ${reason} (history preserved)`, ts);
      recordActivity({ actor: ctx.user.username, action: 'task.deleted', entity_type: 'task', entity_id: t.id, details: `${t.code} "${t.title}" deleted - ${reason}`, ts });
      publish('task.updated', { id: t.id, code: t.code, deleted: true });
    });
    return { ok: true, deleted_at: ts };
  });

  // ---- explicit task lifecycle actions ------------------------------------
  function taskAction(name, fn) {
    route('POST', `/tasks/:id/${name}`, (ctx) => {
      const t = getLiveTask(ctx.params.id);
      checkVersion(t, ctx.body || {}, 'revision', 'Task');
      const reason = requireReason(ctx.body?.reason, `"${name}" action`);
      return tx(db, () => fn(t, reason, ctx));
    });
  }

  taskAction('assign', (t, reason, ctx) => {
    if (t.status !== 'CREATED') throw httpError(409, `Task is ${t.status} - only CREATED tasks can be assigned. Reopen it first if needed.`);
    const worker = getWorker(ctx.body?.worker_id);
    const machine = q('SELECT * FROM machines WHERE id = ?').get(worker.machine_id);
    if (worker.status === 'CLOSED') throw httpError(409, `Worker ${machine.name}/${worker.name} is CLOSED`);
    if (machine.status === 'CLOSED') throw httpError(409, `Machine ${machine.name} is CLOSED - cannot assign tasks to it`);
    const openCount = q(`SELECT COUNT(*) AS n FROM tasks WHERE worker_id = ? AND id != ? AND deleted_at IS NULL AND status IN ('ASSIGNED','IN_PROGRESS','BLOCKED')`).get(worker.id, t.id).n;
    const parallel = ctx.body?.parallel === true;
    if (openCount > 0 && !parallel) {
      throw httpError(409, `1 worker = 1 task: ${machine.name}/${worker.name} already has an open task. Pass parallel:true only as an explicit parallel-task mode.`, { hint: 'parallel task mode' });
    }
    const ts = nowISO();
    let workerChanged = null;
    if (worker.status === 'END TASK') {
      assertTransition(WORKER_TRANSITIONS, 'END TASK', 'ACTIVE', 'Worker');
      q('UPDATE workers SET status = ?, current_task_id = ?, updated_at = ?, version = version + 1 WHERE id = ?')
        .run('ACTIVE', t.id, ts, worker.id);
      recordWorkerStatusChange(worker, 'END TASK', 'ACTIVE', ctx.user.username, `Task assigned: ${t.code} ${t.title}`, ts);
      workerChanged = 'END TASK -> ACTIVE';
    } else if (['ACTIVE', 'WORKING'].includes(worker.status)) {
      // Reserved worker (or already working in explicit parallel-task mode): status kept, task attached.
      q('UPDATE workers SET current_task_id = ?, updated_at = ?, version = version + 1 WHERE id = ?')
        .run(t.id, ts, worker.id);
      workerChanged = `${worker.status} kept${openCount > 0 ? ' (explicit parallel-task mode)' : ''}`;
    } else {
      throw httpError(409, `Worker ${machine.name}/${worker.name} must be END TASK to accept a task (current: ${worker.status}). Change the worker status explicitly first.`);
    }
    assertTransition(TASK_TRANSITIONS, t.status, 'ASSIGNED', 'Task');
    q('UPDATE tasks SET status = ?, machine_id = ?, worker_id = ?, updated_at = ?, revision = revision + 1 WHERE id = ?')
      .run('ASSIGNED', machine.id, worker.id, ts, t.id);
    recordTaskHistory(t.id, 'assigned', t.status, 'ASSIGNED', ctx.user.username, `Assigned to ${machine.name}/${worker.name} (worker ${workerChanged})`, ts);
    recordActivity({ actor: ctx.user.username, action: openCount > 0 ? 'task.assigned.parallel' : 'task.assigned', entity_type: 'task', entity_id: t.id, machine: machine.name, worker: worker.name, details: `${t.code} "${t.title}" -> ${machine.name}/${worker.name} (worker ${workerChanged})`, ts });
    publish('task.updated', { id: t.id, code: t.code });
    publish('worker.updated', workerView(getWorker(worker.id)));
    return taskFull(getLiveTask(t.id));
  });

  taskAction('start', (t, reason, ctx) => {
    assertTransition(TASK_TRANSITIONS, t.status, 'IN_PROGRESS', 'Task');
    if (!t.worker_id) throw httpError(409, 'Task has no assigned worker');
    const worker = getWorker(t.worker_id);
    const machine = q('SELECT name FROM machines WHERE id = ?').get(worker.machine_id);
    let workerChanged = null;
    if (worker.status === 'ACTIVE') {
      assertTransition(WORKER_TRANSITIONS, 'ACTIVE', 'WORKING', 'Worker');
      q('UPDATE workers SET status = ?, updated_at = ?, version = version + 1 WHERE id = ?').run('WORKING', nowISO(), worker.id);
      recordWorkerStatusChange(worker, 'ACTIVE', 'WORKING', ctx.user.username, `Worker started ${t.code} ${t.title}`, nowISO());
      workerChanged = 'ACTIVE -> WORKING';
    } else if (worker.status !== 'WORKING') {
      throw httpError(409, `Worker ${machine.name}/${worker.name} must be ACTIVE to start (current: ${worker.status}). Fix the worker status explicitly first.`);
    }
    const ts = nowISO();
    q('UPDATE tasks SET status = ?, started_at = ?, updated_at = ?, revision = revision + 1 WHERE id = ?')
      .run('IN_PROGRESS', t.started_at ?? ts, ts, t.id);
    recordTaskHistory(t.id, 'started', t.status, 'IN_PROGRESS', ctx.user.username, `${reason}${workerChanged ? ` (${workerChanged})` : ''}`, ts);
    recordActivity({ actor: ctx.user.username, action: 'task.started', entity_type: 'task', entity_id: t.id, machine: machine.name, worker: worker.name, details: `${t.code} started${workerChanged ? ` - worker ${workerChanged}` : ''}`, ts });
    publish('task.updated', { id: t.id, code: t.code });
    publish('worker.updated', workerView(getWorker(worker.id)));
    return taskFull(getLiveTask(t.id));
  });

  taskAction('pause', (t, reason, ctx) => {
    assertTransition(TASK_TRANSITIONS, t.status, 'ASSIGNED', 'Task');
    const ts = nowISO();
    let workerChanged = null;
    if (t.worker_id) {
      const worker = getWorker(t.worker_id);
      if (worker.status === 'WORKING') {
        assertTransition(WORKER_TRANSITIONS, 'WORKING', 'ACTIVE', 'Worker');
        q('UPDATE workers SET status = ?, updated_at = ?, version = version + 1 WHERE id = ?').run('ACTIVE', ts, worker.id);
        recordWorkerStatusChange(worker, 'WORKING', 'ACTIVE', ctx.user.username, `Paused ${t.code} ${t.title}`, ts);
        workerChanged = 'WORKING -> ACTIVE';
        publish('worker.updated', workerView(getWorker(worker.id)));
      }
    }
    q('UPDATE tasks SET status = ?, updated_at = ?, revision = revision + 1 WHERE id = ?').run('ASSIGNED', ts, t.id);
    recordTaskHistory(t.id, 'paused', t.status, 'ASSIGNED', ctx.user.username, reason, ts);
    recordActivity({ actor: ctx.user.username, action: 'task.paused', entity_type: 'task', entity_id: t.id, details: `${t.code} paused${workerChanged ? ` - worker ${workerChanged}` : ''}`, ts });
    publish('task.updated', { id: t.id, code: t.code });
    return taskFull(getLiveTask(t.id));
  });

  taskAction('complete', (t, reason, ctx) => {
    assertTransition(TASK_TRANSITIONS, t.status, 'COMPLETED', 'Task');
    const ts = nowISO();
    let workerChanged = null;
    if (t.worker_id) {
      const worker = getWorker(t.worker_id);
      const machine = q('SELECT name FROM machines WHERE id = ?').get(worker.machine_id);
      if (worker.status === 'WORKING') {
        assertTransition(WORKER_TRANSITIONS, 'WORKING', 'END TASK', 'Worker');
        q('UPDATE workers SET status = ?, current_task_id = NULL, updated_at = ?, version = version + 1 WHERE id = ?')
          .run('END TASK', ts, worker.id);
        recordWorkerStatusChange(worker, 'WORKING', 'END TASK', ctx.user.username, `Task completed: ${t.code} ${t.title}`, ts);
        workerChanged = 'WORKING -> END TASK';
      } else if (worker.current_task_id === t.id) {
        q('UPDATE workers SET current_task_id = NULL, updated_at = ?, version = version + 1 WHERE id = ?').run(ts, worker.id);
      }
      recordActivity({ actor: ctx.user.username, action: 'task.completed', entity_type: 'task', entity_id: t.id, machine: machine.name, worker: worker.name, details: `${t.code} "${t.title}" completed${workerChanged ? ` - worker ${workerChanged}` : ''}`, ts });
      publish('worker.updated', workerView(getWorker(worker.id)));
    } else {
      recordActivity({ actor: ctx.user.username, action: 'task.completed', entity_type: 'task', entity_id: t.id, details: `${t.code} "${t.title}" completed`, ts });
    }
    q('UPDATE tasks SET status = ?, completed_at = ?, progress = 100, updated_at = ?, revision = revision + 1 WHERE id = ?')
      .run('COMPLETED', ts, ts, t.id);
    recordTaskHistory(t.id, 'completed', t.status, 'COMPLETED', ctx.user.username, reason, ts);
    publish('task.updated', { id: t.id, code: t.code });
    return taskFull(getLiveTask(t.id));
  });

  taskAction('block', (t, reason, ctx) => {
    assertTransition(TASK_TRANSITIONS, t.status, 'BLOCKED', 'Task');
    const ts = nowISO();
    q('UPDATE tasks SET status = ?, blocker = ?, updated_at = ?, revision = revision + 1 WHERE id = ?')
      .run('BLOCKED', reason, ts, t.id);
    recordTaskHistory(t.id, 'blocked', t.status, 'BLOCKED', ctx.user.username, reason, ts);
    recordActivity({ actor: ctx.user.username, action: 'task.blocked', entity_type: 'task', entity_id: t.id, details: `${t.code} blocked: ${reason} (worker status deliberately unchanged)`, ts });
    publish('task.updated', { id: t.id, code: t.code });
    return taskFull(getLiveTask(t.id));
  });

  taskAction('unblock', (t, reason, ctx) => {
    assertTransition(TASK_TRANSITIONS, t.status, 'IN_PROGRESS', 'Task');
    const ts = nowISO();
    q('UPDATE tasks SET status = ?, updated_at = ?, revision = revision + 1 WHERE id = ?').run('IN_PROGRESS', ts, t.id);
    recordTaskHistory(t.id, 'unblocked', t.status, 'IN_PROGRESS', ctx.user.username, reason, ts);
    recordActivity({ actor: ctx.user.username, action: 'task.unblocked', entity_type: 'task', entity_id: t.id, details: `${t.code} unblocked: ${reason}`, ts });
    publish('task.updated', { id: t.id, code: t.code });
    return taskFull(getLiveTask(t.id));
  });

  taskAction('cancel', (t, reason, ctx) => {
    assertTransition(TASK_TRANSITIONS, t.status, 'CANCELLED', 'Task');
    const ts = nowISO();
    q('UPDATE tasks SET status = ?, updated_at = ?, revision = revision + 1 WHERE id = ?').run('CANCELLED', ts, t.id);
    if (t.worker_id) {
      const worker = getWorker(t.worker_id);
      if (worker.current_task_id === t.id) {
        // release the task from the worker; worker STATUS is intentionally left alone
        q('UPDATE workers SET current_task_id = NULL, updated_at = ?, version = version + 1 WHERE id = ?').run(ts, worker.id);
      }
    }
    recordTaskHistory(t.id, 'cancelled', t.status, 'CANCELLED', ctx.user.username, reason, ts);
    recordActivity({ actor: ctx.user.username, action: 'task.cancelled', entity_type: 'task', entity_id: t.id, details: `${t.code} "${t.title}" cancelled: ${reason} (worker status not auto-changed)`, ts });
    if (t.worker_id) publish('worker.updated', workerView(getWorker(t.worker_id)));
    publish('task.updated', { id: t.id, code: t.code });
    return taskFull(getLiveTask(t.id));
  });

  taskAction('reopen', (t, reason, ctx) => {
    assertTransition(TASK_TRANSITIONS, t.status, 'CREATED', 'Task');
    const ts = nowISO();
    q('UPDATE tasks SET status = ?, machine_id = NULL, worker_id = NULL, updated_at = ?, revision = revision + 1 WHERE id = ?')
      .run('CREATED', ts, t.id);
    recordTaskHistory(t.id, 'reopened', t.status, 'CREATED', ctx.user.username, reason, ts);
    recordActivity({ actor: ctx.user.username, action: 'task.reopened', entity_type: 'task', entity_id: t.id, details: `${t.code} reopened: ${reason}`, ts });
    publish('task.updated', { id: t.id, code: t.code });
    return taskFull(getLiveTask(t.id));
  });

  /* ================= ACTIVITY & HISTORY ================= */
  route('GET', '/activity', (ctx) => {
    let rows = q('SELECT * FROM activity_log ORDER BY timestamp DESC, id DESC').all();
    const { entity_type, entity_id, action, machine, worker } = ctx.query;
    if (entity_type) rows = rows.filter((r) => r.entity_type === entity_type);
    if (entity_id) rows = rows.filter((r) => r.entity_id === Number(entity_id));
    if (action) rows = rows.filter((r) => r.action === action);
    if (machine) rows = rows.filter((r) => r.machine === machine);
    if (worker) rows = rows.filter((r) => r.worker === worker);
    const limit = Math.min(Number(ctx.query.limit) || 200, 500);
    return rows.slice(0, limit);
  });

  route('GET', '/history', (ctx) => {
    const out = {};
    const { type, worker_id, task_id, limit: limitRaw } = ctx.query;
    const limit = Math.min(Number(limitRaw) || 300, 1000);
    if (!type || type === 'worker') {
      let rows = q('SELECT * FROM worker_status_history ORDER BY timestamp DESC, id DESC').all();
      if (worker_id) rows = rows.filter((r) => r.worker_id === Number(worker_id));
      out.worker = rows.slice(0, limit);
    }
    if (!type || type === 'task') {
      let rows = q(`SELECT h.*, t.code AS task_code, t.title AS task_title
                    FROM task_history h LEFT JOIN tasks t ON t.id = h.task_id
                    ORDER BY h.timestamp DESC, h.id DESC`).all();
      if (task_id) rows = rows.filter((r) => r.task_id === Number(task_id));
      out.task = rows.slice(0, limit);
    }
    return out;
  });

  /* ================= SEARCH ================= */
  route('GET', '/search', (ctx) => {
    const needle = String(ctx.query.q || '').trim().toLowerCase();
    if (!needle) return { tasks: [], workers: [], machines: [] };
    const like = (f) => (f || '').toLowerCase().includes(needle);

    const tasks = q('SELECT * FROM tasks WHERE deleted_at IS NULL ORDER BY updated_at DESC LIMIT 500').all()
      .filter((t) => [t.code, t.title, t.objective, t.scope, t.out_of_scope, t.acceptance_criteria, t.notes, t.files_changed, t.tests, t.blocker, t.repository, t.branch, t.commit_hash].some(like))
      .slice(0, 25).map(taskSummary);

    const workers = q(`SELECT w.*, m.name AS machine_name FROM workers w JOIN machines m ON m.id = w.machine_id`).all()
      .filter((w) => like(w.name) || like(w.machine_name) || like(w.notes))
      .slice(0, 25).map(workerView);

    const machines = q('SELECT * FROM machines').all()
      .filter((m) => like(m.name) || like(m.notes))
      .slice(0, 25).map(machineView);

    return { tasks, workers, machines };
  });

  /* ================= META ================= */
  route('GET', '/meta', () => ({
    worker_statuses: WORKER_STATUSES,
    machine_statuses: MACHINE_STATUSES,
    task_statuses: TASK_STATUSES,
    priorities: ['P0', 'P1', 'P2', 'P3'],
    transitions: { worker: WORKER_TRANSITIONS, machine: MACHINE_TRANSITIONS, task: TASK_TRANSITIONS },
    roles: ['OWNER', 'ADMIN', 'VIEWER'],
  }));

  /* ================= SSE ================= */
  route('GET', '/events', (ctx) => {
    ctx.res.writeHead(200, {
      'Content-Type': 'text/event-stream',
      'Cache-Control': 'no-cache, no-transform',
      Connection: 'keep-alive',
      'X-Accel-Buffering': 'no',
    });
    ctx.res.write('retry: 3000\n\n');
    ctx.res.write(`data: ${JSON.stringify({ type: 'hello', ts: nowISO() })}\n\n`);
    const client = { res: ctx.res };
    sseClients.add(client);
    ctx.res.on('close', () => sseClients.delete(client));
  }, { stream: true });

  /* ------------------------------------------------------------------ *
   * HTTP plumbing
   * ------------------------------------------------------------------ */
  function readBody(req) {
    return new Promise((resolve, reject) => {
      const chunks = [];
      let size = 0;
      req.on('data', (c) => {
        size += c.length;
        if (size > 1_000_000) {
          reject(httpError(413, 'Request body too large'));
          req.destroy();
          return;
        }
        chunks.push(c);
      });
      req.on('end', () => {
        if (chunks.length === 0) return resolve({});
        try {
          resolve(JSON.parse(Buffer.concat(chunks).toString('utf8')));
        } catch {
          reject(httpError(400, 'Invalid JSON body'));
        }
      });
      req.on('error', reject);
    });
  }

  function sendJSON(res, status, obj, extraHeaders = {}) {
    const body = JSON.stringify(obj, null, 2);
    res.writeHead(status, {
      'Content-Type': 'application/json; charset=utf-8',
      'Cache-Control': 'no-store',
      ...extraHeaders,
    });
    res.end(body);
  }

  const MIME = {
    '.html': 'text/html; charset=utf-8',
    '.js': 'text/javascript; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
    '.svg': 'image/svg+xml',
    '.png': 'image/png',
    '.ico': 'image/x-icon',
    '.json': 'application/json; charset=utf-8',
    '.woff2': 'font/woff2',
  };

  function serveStatic(req, res, pathname) {
    let rel = pathname === '/' ? 'index.html' : pathname.replace(/^\/+/, '');
    const file = path.normalize(path.join(staticDir, rel));
    if (!file.startsWith(path.normalize(staticDir))) {
      sendJSON(res, 403, { error: 'Forbidden' });
      return;
    }
    fs.readFile(file, (err, data) => {
      if (err) {
        // SPA fallback: unknown non-API GET paths get the app shell
        fs.readFile(path.join(staticDir, 'index.html'), (err2, index) => {
          if (err2) return sendJSON(res, 404, { error: 'Not found' });
          res.writeHead(200, { 'Content-Type': MIME['.html'], 'Cache-Control': 'no-store' });
          res.end(index);
        });
        return;
      }
      res.writeHead(200, { 'Content-Type': MIME[path.extname(file)] || 'application/octet-stream', 'Cache-Control': 'no-store' });
      res.end(data);
    });
  }

  const server = http.createServer(async (req, res) => {
    const url = new URL(req.url, 'http://localhost');
    const pathname = decodeURIComponent(url.pathname);
    try {
      const match = routes.find((r) => r.method === req.method && r.regex.test(pathname));
      if (match) {
        const m = pathname.match(match.regex);
        const params = {};
        match.keys.forEach((k, i) => { params[k] = m[i + 1]; });

      let user = resolveUser(req);
      if (!user && openAccess) {
        // Open access mode: no login required. Anonymous callers get full rights;
        // identity for the audit trail can be supplied via the X-Actor header.
        user = {
          id: null,
          username: String(req.headers['x-actor'] || '').trim().replace(/[^\w .\-@]/g, '').slice(0, 40) || 'anonymous',
          role: 'OWNER',
          token: null,
        };
      }
      if (!match.public && !user) {
        return sendJSON(res, 401, { error: 'Authentication required', hint: 'POST /auth/login {username, password}' });
      }
        if (!match.public && !match.stream && !MUTATING_ROLES.includes(user.role) && req.method !== 'GET') {
          return sendJSON(res, 403, { error: `Role ${user.role} is read-only. Only OWNER/ADMIN can make changes.` });
        }

        const body = ['POST', 'PATCH', 'DELETE'].includes(req.method) ? await readBody(req) : {};
        const query = Object.fromEntries(url.searchParams.entries());
        const ctx = { req, res, params, query, body, user, db, publish };

        const result = await match.handler(ctx);
        if (match.stream) return; // handler owns the response
        if (result?.cookie) {
          return sendJSON(res, 200, (({ cookie, ...rest }) => rest)(result), { 'Set-Cookie': result.cookie });
        }
        return sendJSON(res, 200, result ?? { ok: true });
      }

      if (req.method === 'GET' && !pathname.startsWith('/auth') && url.searchParams.get('token') === null) {
        // fall through to static/SPA below
      }
      if (req.method === 'GET') {
        return serveStatic(req, res, pathname);
      }
      return sendJSON(res, 404, { error: `No route: ${req.method} ${pathname}` });
    } catch (err) {
      if (err?.expose) {
        const out = { error: err.message };
        if (err.allowed) out.allowed = err.allowed;
        if (err.current_version !== undefined) out.current_version = err.current_version;
        if (err.hint) out.hint = err.hint;
        return sendJSON(res, err.status || 400, out);
      }
      if (String(err?.code || '').startsWith('SQLITE_')) {
        return sendJSON(res, 400, { error: `Database constraint: ${err.message}` });
      }
      console.error(`[taskboard] ${req.method} ${pathname} failed:`, err);
      return sendJSON(res, 500, { error: 'Internal server error' });
    }
  });

  return { server, db, publish, freshlySeeded, sseClients };
}
