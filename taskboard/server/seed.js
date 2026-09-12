import crypto from 'node:crypto';
import { tx } from './db.js';

export function hashPassword(password) {
  const salt = crypto.randomBytes(16).toString('hex');
  const hash = crypto.scryptSync(password, salt, 64).toString('hex');
  return `${salt}:${hash}`;
}

export function verifyPassword(password, stored) {
  const [salt, hash] = String(stored).split(':');
  if (!salt || !hash) return false;
  const check = crypto.scryptSync(password, salt, 64).toString('hex');
  return crypto.timingSafeEqual(Buffer.from(hash, 'hex'), Buffer.from(check, 'hex'));
}

/**
 * Seed exactly the example data from the spec:
 *   M1 ONLINE  : W1 WORKING (task "Gallery.Video Finalization"), W2 END TASK, W3 END TASK
 *   M2 ONLINE  : W1/W2/W3 END TASK
 *   M3 CLOSED  : workers CLOSED
 *   M4 STANDBY : workers NO SLOT
 * Timestamps are relative to first boot so the dashboard shows honest relative times.
 */
export function seedIfEmpty(db) {
  const count = db.prepare('SELECT COUNT(*) AS n FROM machines').get().n;
  if (count > 0) return false;

  const now = Date.now();
  const MIN = 60_000;
  const HOUR = 60 * MIN;
  const iso = (msAgo) => new Date(now - msAgo).toISOString();

  tx(db, () => {
    // --- users -------------------------------------------------------------
    const insUser = db.prepare(
      'INSERT INTO users (username, password_hash, role, created_at) VALUES (?, ?, ?, ?)'
    );
    insUser.run('owner', hashPassword('owner123'), 'OWNER', iso(24 * HOUR));
    insUser.run('admin', hashPassword('admin123'), 'ADMIN', iso(24 * HOUR));
    insUser.run('viewer', hashPassword('viewer123'), 'VIEWER', iso(24 * HOUR));

    // --- machines ----------------------------------------------------------
    const insMachine = db.prepare(
      'INSERT INTO machines (name, status, notes, created_at, updated_at) VALUES (?, ?, ?, ?, ?)'
    );
    const machines = {};
    const machineSeed = [
      ['M1', 'ONLINE', 'Primary build machine', 3 * HOUR],
      ['M2', 'ONLINE', 'Secondary build machine', 3 * HOUR],
      ['M3', 'CLOSED', 'Retired machine', 3 * HOUR],
      ['M4', 'STANDBY', 'Reserved capacity, not currently in use', 3 * HOUR],
    ];
    for (const [name, status, notes, age] of machineSeed) {
      const info = insMachine.run(name, status, notes, iso(age), iso(age));
      machines[name] = Number(info.lastInsertRowid);
      db.prepare(
        'INSERT INTO activity_log (timestamp, actor, action, entity_type, entity_id, machine, details) VALUES (?,?,?,?,?,?,?)'
      ).run(iso(age), 'system', 'machine.created', 'machine', machines[name], name, `Machine ${name} registered as ${status}`);
    }

    // --- workers -----------------------------------------------------------
    const insWorker = db.prepare(
      'INSERT INTO workers (machine_id, name, status, created_at, updated_at) VALUES (?, ?, ?, ?, ?)'
    );
    const insWsh = db.prepare(
      'INSERT INTO worker_status_history (worker_id, machine, worker_name, old_status, new_status, changed_by, reason, timestamp) VALUES (?,?,?,?,?,?,?,?)'
    );
    const workers = {};
    const workerSeed = [
      ['M1', ['WORKING', 'END TASK', 'END TASK']],
      ['M2', ['END TASK', 'END TASK', 'END TASK']],
      ['M3', ['CLOSED', 'CLOSED', 'CLOSED']],
      ['M4', ['NO SLOT', 'NO SLOT', 'NO SLOT']],
    ];
    for (const [mName, statuses] of workerSeed) {
      statuses.forEach((status, i) => {
        const wName = `W${i + 1}`;
        const info = insWorker.run(machines[mName], wName, status, iso(3 * HOUR), iso(3 * HOUR));
        const wid = Number(info.lastInsertRowid);
        workers[`${mName}/${wName}`] = wid;
        // baseline history row so the history page has an anchor for every worker
        insWsh.run(wid, mName, wName, null, status, 'system', 'initial state (seed)', iso(3 * HOUR));
      });
    }

    // --- the seeded task (spec example) -------------------------------------
    const taskTitle = 'Gallery.Video Finalization';
    const filesChanged = [
      'Gallery.Video/FfmpegDecodeWorker.vb',
      'Gallery.Video/PlaybackSession.vb',
    ].join('\n');
    const tests = '97 PASS / 0 FAIL / 6 SKIP';

    const insTask = db.prepare(`
      INSERT INTO tasks (code, title, objective, scope, out_of_scope, acceptance_criteria,
        machine_id, worker_id, priority, status, started_at, progress, blocker,
        files_changed, tests, notes, repository, branch, created_by, created_at, updated_at, revision)
      VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)
    `);
    const tInfo = insTask.run(
      'T-PLACEHOLDER',
      taskTitle,
      'Finalize the rebuilt Gallery.Video playback engine: decode pipeline, audio clock, and completion gate stable under stress.',
      'Decode worker liveness, playback session timing, gallery library loading, watchdog behavior.',
      'Overlay rendering, recording pipeline, anything outside Gallery.Video.',
      'Full test suite green; 240fps clip plays without freeze; no decode stalls in 30-minute soak.',
      machines.M1, workers['M1/W1'],
      'P1', 'IN_PROGRESS',
      iso(50 * MIN),
      78,
      '240fps architectural limit',
      filesChanged, tests,
      'Starved-present guard added; completion gate now requires explicit liveness signal.',
      'NVIDIA-Shadowplay', 'Engine-Rebuild-Stabilization',
      'owner', iso(3 * HOUR), iso(2 * MIN), 1
    );
    const taskId = Number(tInfo.lastInsertRowid);
    const code = `T-${String(taskId).padStart(4, '0')}`;
    db.prepare('UPDATE tasks SET code = ? WHERE id = ?').run(code, taskId);
    db.prepare('UPDATE workers SET current_task_id = ? WHERE id = ?').run(taskId, workers['M1/W1']);

    const insTh = db.prepare(
      'INSERT INTO task_history (task_id, event, old_status, new_status, changed_by, details, timestamp) VALUES (?,?,?,?,?,?,?)'
    );
    insTh.run(taskId, 'created', null, 'CREATED', 'owner', 'Task created', iso(3 * HOUR));
    insTh.run(taskId, 'assigned', 'CREATED', 'ASSIGNED', 'owner', `Assigned to M1/W1 (${code})`, iso(55 * MIN));
    insTh.run(taskId, 'started', 'ASSIGNED', 'IN_PROGRESS', 'owner', 'Worker started the task', iso(50 * MIN));
    insTh.run(taskId, 'updated', 'IN_PROGRESS', 'IN_PROGRESS', 'owner', 'Build passed', iso(37 * MIN));
    insTh.run(taskId, 'updated', 'IN_PROGRESS', 'IN_PROGRESS', 'owner', '240fps freeze reproduced - blocker raised', iso(24 * MIN));
    insTh.run(taskId, 'updated', 'IN_PROGRESS', 'IN_PROGRESS', 'owner', 'Starved-present guard added', iso(10 * MIN));
    insTh.run(taskId, 'updated', 'IN_PROGRESS', 'IN_PROGRESS', 'owner', `Full suite ${tests}`, iso(2 * MIN));

    // worker history for W1: END TASK -> ACTIVE (assigned), ACTIVE -> WORKING (started)
    insWsh.run(workers['M1/W1'], 'M1', 'W1', 'END TASK', 'ACTIVE', 'owner', `Task assigned: ${code} ${taskTitle}`, iso(55 * MIN));
    insWsh.run(workers['M1/W1'], 'M1', 'W1', 'ACTIVE', 'WORKING', 'owner', `Worker started ${code} ${taskTitle}`, iso(50 * MIN));

    const insAct = db.prepare(
      'INSERT INTO activity_log (timestamp, actor, action, entity_type, entity_id, machine, worker, details) VALUES (?,?,?,?,?,?,?,?)'
    );
    insAct.run(iso(55 * MIN), 'owner', 'task.assigned', 'task', taskId, 'M1', 'W1', `${code} ${taskTitle} -> M1/W1 (worker END TASK -> ACTIVE)`);
    insAct.run(iso(50 * MIN), 'owner', 'task.started', 'task', taskId, 'M1', 'W1', `${code} started (worker ACTIVE -> WORKING)`);
    insAct.run(iso(37 * MIN), 'owner', 'task.updated', 'task', taskId, 'M1', 'W1', 'Build passed');
    insAct.run(iso(24 * MIN), 'owner', 'task.updated', 'task', taskId, 'M1', 'W1', '240fps freeze reproduced');
    insAct.run(iso(10 * MIN), 'owner', 'task.updated', 'task', taskId, 'M1', 'W1', 'Starved-present guard added');
    insAct.run(iso(2 * MIN), 'owner', 'task.updated', 'task', taskId, 'M1', 'W1', `Full suite ${tests}`);
  });

  return true;
}
