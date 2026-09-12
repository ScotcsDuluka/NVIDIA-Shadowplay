/* Task Board — API acceptance tests (node:test, zero dependencies) */
import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { createApp } from '../server/app.js';

process.env.NODE_ENV = 'test';

const tmpDir = fs.mkdtempSync(path.join(os.tmpdir(), 'taskboard-test-'));
const { server } = createApp({
  dbPath: path.join(tmpDir, 'test.db'),
  staticDir: path.join(tmpDir, 'unused-static'),
  seed: true,
});
server.listen(0, '127.0.0.1');
await new Promise((resolve) => server.once('listening', resolve));
const base = `http://127.0.0.1:${server.address().port}`;

test.after(() => {
  server.closeAllConnections();
  server.close();
});

async function api(method, url, body, token, extraHeaders = {}) {
  const res = await fetch(base + url, {
    method,
    headers: {
      ...(body ? { 'Content-Type': 'application/json' } : {}),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...extraHeaders,
    },
    body: body ? JSON.stringify(body) : undefined,
  });
  const data = await res.json().catch(() => ({}));
  return { status: res.status, data };
}

async function login(username, password) {
  const r = await api('POST', '/auth/login', { username, password });
  assert.equal(r.status, 200, `login ${username}`);
  return r.data.token;
}

const tokens = {};
test.before(async () => {
  tokens.owner = await login('owner', 'owner123');
  tokens.admin = await login('admin', 'admin123');
  tokens.viewer = await login('viewer', 'viewer123');
});

test('seed: machines M1-M4 with exact statuses', async () => {
  const r = await api('GET', '/machines', null, tokens.owner);
  assert.equal(r.status, 200);
  const byName = Object.fromEntries(r.data.map((m) => [m.name, m]));
  assert.deepEqual(Object.keys(byName).sort(), ['M1', 'M2', 'M3', 'M4']);
  assert.equal(byName.M1.status, 'ONLINE');
  assert.equal(byName.M2.status, 'ONLINE');
  assert.equal(byName.M3.status, 'CLOSED');
  assert.equal(byName.M4.status, 'STANDBY');
});

test('seed: worker statuses exactly per spec', async () => {
  const r = await api('GET', '/workers', null, tokens.owner);
  assert.equal(r.status, 200);
  const byKey = Object.fromEntries(r.data.map((w) => [`${w.machine_name}/${w.name}`, w]));
  assert.equal(byKey['M1/W1'].status, 'WORKING');
  assert.equal(byKey['M1/W2'].status, 'END TASK');
  assert.equal(byKey['M1/W3'].status, 'END TASK');
  for (const n of ['W1', 'W2', 'W3']) assert.equal(byKey[`M2/${n}`].status, 'END TASK');
  for (const n of ['W1', 'W2', 'W3']) assert.equal(byKey[`M3/${n}`].status, 'CLOSED');
  for (const n of ['W1', 'W2', 'W3']) assert.equal(byKey[`M4/${n}`].status, 'NO SLOT');
});

test('seed: example task on M1/W1 with spec fields', async () => {
  const r = await api('GET', '/tasks', null, tokens.owner);
  const t = r.data.find((x) => x.title === 'Gallery.Video Finalization');
  assert.ok(t, 'seeded task exists');
  assert.equal(t.status, 'IN_PROGRESS');
  assert.equal(t.priority, 'P1');
  assert.equal(t.progress, 78);
  assert.equal(t.tests, '97 PASS / 0 FAIL / 6 SKIP');
  assert.equal(t.blocker, '240fps architectural limit');
  assert.match(t.files_changed, /Gallery\.Video\/FfmpegDecodeWorker\.vb/);
  assert.match(t.files_changed, /Gallery\.Video\/PlaybackSession\.vb/);
  const detail = await api('GET', `/tasks/${t.id}`, null, tokens.owner);
  assert.equal(detail.data.machine_name, 'M1');
  assert.equal(detail.data.worker_name, 'W1');
  assert.ok(detail.data.history.some((h) => h.event === 'started'));
});

test('auth: wrong password rejected', async () => {
  const r = await api('POST', '/auth/login', { username: 'owner', password: 'nope' });
  assert.equal(r.status, 401);
});

test('open access: anonymous reads and writes are allowed; X-Actor is recorded', async () => {
  const r = await api('GET', '/machines');
  assert.equal(r.status, 200);
  const me = await api('GET', '/auth/me');
  assert.equal(me.data.openAccess, true);
  assert.equal(me.data.user.username, 'anonymous');
  const t = (await api('POST', '/tasks', { title: 'Created without login' }, null, { 'X-Actor': 'GPT' })).data;
  assert.equal(t.status, 'CREATED');
  const act = (await api('GET', `/activity?action=task.created&entity_id=${t.id}`, null, tokens.owner)).data;
  assert.ok(act.some((a) => a.actor === 'GPT'), 'X-Actor identity recorded in the audit trail');
  const del = await api('DELETE', `/tasks/${t.id}`, { reason: 'cleanup' }, null);
  assert.equal(del.status, 200);
});

test('closed access mode rejects anonymous requests (TASKBOARD_OPEN_ACCESS=false)', async () => {
  const { server: closed } = createApp({
    dbPath: path.join(tmpDir, 'closed.db'),
    staticDir: path.join(tmpDir, 'unused-static'),
    seed: true,
    openAccess: false,
  });
  closed.listen(0, '127.0.0.1');
  await new Promise((resolve) => closed.once('listening', resolve));
  try {
    const base2 = `http://127.0.0.1:${closed.address().port}`;
    const anon = await fetch(base2 + '/machines');
    assert.equal(anon.status, 401);
    const login = await fetch(base2 + '/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: 'owner', password: 'owner123' }),
    });
    assert.equal(login.status, 200);
    const { token } = await login.json();
    const authed = await fetch(base2 + '/machines', { headers: { Authorization: `Bearer ${token}` } });
    assert.equal(authed.status, 200);
  } finally {
    closed.closeAllConnections();
    closed.close();
  }
});

test('VIEWER is read-only: cannot change status', async () => {
  const workers = (await api('GET', '/workers', null, tokens.viewer)).data;
  const w = workers.find((x) => x.machine_name === 'M1' && x.name === 'W2');
  const r = await api('PATCH', `/workers/${w.id}/status`, { status: 'STANDBY', reason: 'viewer try', version: w.version }, tokens.viewer);
  assert.equal(r.status, 403);
});

test('invalid transition rejected: END TASK -> WORKING (must go through ACTIVE)', async () => {
  const workers = (await api('GET', '/workers', null, tokens.owner)).data;
  const w = workers.find((x) => x.machine_name === 'M1' && x.name === 'W2');
  const r = await api('PATCH', `/workers/${w.id}/status`, { status: 'WORKING', reason: 'skip ahead', version: w.version }, tokens.owner);
  assert.equal(r.status, 400);
  assert.ok(r.data.allowed.includes('ACTIVE'));
});

test('missing reason rejected (audit trail required)', async () => {
  const workers = (await api('GET', '/workers', null, tokens.owner)).data;
  const w = workers.find((x) => x.machine_name === 'M1' && x.name === 'W3');
  const r = await api('PATCH', `/workers/${w.id}/status`, { status: 'STANDBY', version: w.version }, tokens.owner);
  assert.equal(r.status, 400);
});

test('valid worker transition recorded with old/new/who/reason/timestamp', async () => {
  const workers = (await api('GET', '/workers', null, tokens.owner)).data;
  const w = workers.find((x) => x.machine_name === 'M1' && x.name === 'W3');
  const r = await api('PATCH', `/workers/${w.id}/status`, { status: 'STANDBY', reason: 'rotation maintenance', version: w.version }, tokens.owner);
  assert.equal(r.status, 200);
  assert.equal(r.data.status, 'STANDBY');
  const hist = (await api('GET', `/history?type=worker&worker_id=${w.id}`, null, tokens.owner)).data.worker;
  const entry = hist.find((h) => h.new_status === 'STANDBY');
  assert.ok(entry, 'history row exists');
  assert.equal(entry.old_status, 'END TASK');
  assert.equal(entry.changed_by, 'owner');
  assert.equal(entry.reason, 'rotation maintenance');
  assert.ok(entry.timestamp);
  // restore
  const back = await api('PATCH', `/workers/${w.id}/status`, { status: 'END TASK', reason: 'back to duty', version: r.data.version }, tokens.owner);
  assert.equal(back.status, 200);
});

test('machine transition validated and logged', async () => {
  const machines = (await api('GET', '/machines', null, tokens.owner)).data;
  const m4 = machines.find((m) => m.name === 'M4');
  const bad = await api('PATCH', `/machines/${m4.id}/status`, { status: 'BOGUS', reason: 'x', version: m4.version }, tokens.owner);
  assert.equal(bad.status, 400);
  const ok = await api('PATCH', `/machines/${m4.id}/status`, { status: 'ONLINE', reason: 'capacity needed', version: m4.version }, tokens.owner);
  assert.equal(ok.status, 200);
  assert.equal(ok.data.status, 'ONLINE');
  const act = (await api('GET', '/activity?action=machine.status_changed', null, tokens.owner)).data;
  assert.ok(act.some((a) => a.details.includes('STANDBY -> ONLINE')));
});

test('task lifecycle: create -> assign -> start -> report -> complete', async () => {
  // 1. create (unassigned, CREATED)
  const created = await api('POST', '/tasks', {
    title: 'Integration QA sweep',
    objective: 'Verify end-to-end flows',
    priority: 'P0',
    repository: 'NVIDIA-Shadowplay',
    branch: 'Engine-Rebuild-Stabilization',
  }, tokens.admin);
  assert.equal(created.status, 200);
  const task = created.data;
  assert.equal(task.code, `T-${String(task.id).padStart(4, '0')}`);
  assert.equal(task.status, 'CREATED');

  // 2. assign -> task ASSIGNED, worker END TASK -> ACTIVE (never WORKING automatically)
  const workers = (await api('GET', '/workers', null, tokens.owner)).data;
  const w1 = workers.find((x) => x.machine_name === 'M2' && x.name === 'W1');
  const assigned = await api('POST', `/tasks/${task.id}/assign`, { worker_id: w1.id, reason: 'QA window opened', revision: task.revision }, tokens.admin);
  assert.equal(assigned.status, 200);
  assert.equal(assigned.data.status, 'ASSIGNED');
  assert.equal(assigned.data.machine_name, 'M2');
  const wAfterAssign = (await api('GET', '/workers', null, tokens.owner)).data.find((x) => x.id === w1.id);
  assert.equal(wAfterAssign.status, 'ACTIVE');
  assert.equal(wAfterAssign.current_task.id, task.id);

  // 3. start -> task IN_PROGRESS, worker ACTIVE -> WORKING, both explicit
  const started = await api('POST', `/tasks/${task.id}/start`, { reason: 'kicked off', revision: assigned.data.revision }, tokens.admin);
  assert.equal(started.status, 200);
  assert.equal(started.data.status, 'IN_PROGRESS');
  assert.ok(started.data.started_at);
  const wAfterStart = (await api('GET', '/workers', null, tokens.owner)).data.find((x) => x.id === w1.id);
  assert.equal(wAfterStart.status, 'WORKING');

  // 4. CRITICAL RULE: a "report" (progress + tests update) must NOT change any status
  const report = await api('PATCH', `/tasks/${task.id}`, { progress: 50, tests: '11 PASS / 0 FAIL', notes: '11/11 PASS report', revision: started.data.revision }, tokens.admin);
  assert.equal(report.status, 200);
  assert.equal(report.data.status, 'IN_PROGRESS', 'task status unchanged by report');
  const wAfterReport = (await api('GET', '/workers', null, tokens.owner)).data.find((x) => x.id === w1.id);
  assert.equal(wAfterReport.status, 'WORKING', 'worker status unchanged by report');

  // 5. PATCH cannot smuggle a status change
  const smuggle = await api('PATCH', `/tasks/${task.id}`, { status: 'COMPLETED', revision: report.data.revision }, tokens.admin);
  assert.equal(smuggle.status, 400);

  // 6. explicit complete -> task COMPLETED, worker WORKING -> END TASK
  const done = await api('POST', `/tasks/${task.id}/complete`, { reason: 'all criteria met', revision: report.data.revision }, tokens.admin);
  assert.equal(done.status, 200);
  assert.equal(done.data.status, 'COMPLETED');
  assert.equal(done.data.progress, 100);
  assert.ok(done.data.completed_at);
  const wAfterDone = (await api('GET', '/workers', null, tokens.owner)).data.find((x) => x.id === w1.id);
  assert.equal(wAfterDone.status, 'END TASK');
  assert.equal(wAfterDone.current_task, null);

  // 7. full history trail (history endpoint returns newest-first)
  const events = (await api('GET', `/history?type=task&task_id=${task.id}`, null, tokens.owner)).data.task.map((h) => h.event);
  assert.deepEqual(events, ['completed', 'updated', 'started', 'assigned', 'created']);
});

test('1 worker = 1 task; parallel only with explicit flag', async () => {
  const mk = (title) => api('POST', '/tasks', { title, priority: 'P2' }, tokens.admin);
  const a = (await mk('Parallel check A')).data;
  const b = (await mk('Parallel check B')).data;
  const workers = (await api('GET', '/workers', null, tokens.owner)).data;
  const w = workers.find((x) => x.machine_name === 'M2' && x.name === 'W2');

  const okA = await api('POST', `/tasks/${a.id}/assign`, { worker_id: w.id, reason: 'first', revision: a.revision }, tokens.admin);
  assert.equal(okA.status, 200);

  const reject = await api('POST', `/tasks/${b.id}/assign`, { worker_id: w.id, reason: 'second', revision: b.revision }, tokens.admin);
  assert.equal(reject.status, 409);

  const parallel = await api('POST', `/tasks/${b.id}/assign`, { worker_id: w.id, reason: 'explicit parallel task mode', revision: b.revision, parallel: true }, tokens.owner);
  assert.equal(parallel.status, 200);
  const act = (await api('GET', '/activity?action=task.assigned.parallel', null, tokens.owner)).data;
  assert.ok(act.length >= 1, 'parallel assignment is logged');
});

test('assign rejected when worker is not END TASK', async () => {
  const t = (await api('POST', '/tasks', { title: 'Needs a free worker' }, tokens.admin)).data;
  const workers = (await api('GET', '/workers', null, tokens.owner)).data;
  const w = workers.find((x) => x.machine_name === 'M4' && x.name === 'W1'); // NO SLOT
  const r = await api('POST', `/tasks/${t.id}/assign`, { worker_id: w.id, reason: 'try', revision: t.revision }, tokens.admin);
  assert.equal(r.status, 409);
  assert.match(r.data.error, /END TASK/);
  assert.equal((await api('GET', `/tasks/${t.id}`, null, tokens.owner)).data.status, 'CREATED');
});

test('optimistic concurrency: stale task revision -> 409', async () => {
  const t = (await api('POST', '/tasks', { title: 'Concurrency probe' }, tokens.admin)).data;
  const stale = await api('PATCH', `/tasks/${t.id}`, { notes: 'based on old read', revision: t.revision }, tokens.admin);
  assert.equal(stale.status, 200);
  const older = await api('PATCH', `/tasks/${t.id}`, { notes: 'lost update', revision: t.revision }, tokens.admin);
  assert.equal(older.status, 409);
  assert.ok(older.data.current_version >= 2);
});

test('optimistic concurrency: stale worker version -> 409', async () => {
  const workers = (await api('GET', '/workers', null, tokens.owner)).data;
  const w = workers.find((x) => x.machine_name === 'M2' && x.name === 'W3');
  const first = await api('PATCH', `/workers/${w.id}/status`, { status: 'STANDBY', reason: 'a', version: w.version }, tokens.owner);
  assert.equal(first.status, 200);
  const stale = await api('PATCH', `/workers/${w.id}/status`, { status: 'END TASK', reason: 'b', version: w.version }, tokens.owner);
  assert.equal(stale.status, 409);
});

test('filters: by status, priority, machine', async () => {
  const inProgress = (await api('GET', '/tasks?status=IN_PROGRESS', null, tokens.owner)).data;
  assert.ok(inProgress.length >= 1);
  assert.ok(inProgress.every((t) => t.status === 'IN_PROGRESS'));
  const p0 = (await api('GET', '/tasks?priority=P0', null, tokens.owner)).data;
  assert.ok(p0.every((t) => t.priority === 'P0'));
  const m2workers = (await api('GET', '/workers?machine_id=' + (await api('GET', '/machines', null, tokens.owner)).data.find((m) => m.name === 'M2').id, null, tokens.owner)).data;
  assert.equal(m2workers.length, 3);
});

test('global search: task title, file, blocker, machine, worker', async () => {
  const s1 = (await api('GET', '/search?q=Gallery', null, tokens.owner)).data;
  assert.ok(s1.tasks.some((t) => t.title === 'Gallery.Video Finalization'));
  const s2 = (await api('GET', '/search?q=FfmpegDecodeWorker', null, tokens.owner)).data;
  assert.ok(s2.tasks.length >= 1, 'file content finds task');
  const s3 = (await api('GET', '/search?q=architectural+limit', null, tokens.owner)).data;
  assert.ok(s3.tasks.length >= 1, 'blocker finds task');
  const s4 = (await api('GET', '/search?q=M2', null, tokens.owner)).data;
  assert.ok(s4.machines.some((m) => m.name === 'M2'));
  assert.ok(s4.workers.some((w) => w.machine_name === 'M2'));
});

test('activity log captures who did what', async () => {
  const act = (await api('GET', '/activity?limit=500', null, tokens.owner)).data;
  const actions = new Set(act.map((a) => a.action));
  for (const expected of ['auth.login', 'machine.created', 'worker.status_changed', 'task.created', 'task.assigned', 'task.started', 'task.completed']) {
    assert.ok(actions.has(expected), `activity contains ${expected}`);
  }
  const started = act.find((a) => a.action === 'task.started');
  assert.equal(started.actor, 'admin');
});

test('cancel does not auto-change worker status (explicit discipline)', async () => {
  const t = (await api('POST', '/tasks', { title: 'Will be cancelled' }, tokens.admin)).data;
  let w = (await api('GET', '/workers', null, tokens.owner)).data.find((x) => x.machine_name === 'M2' && x.name === 'W3');
  // W3 was left STANDBY by an earlier test; the operator must free it explicitly first
  if (w.status !== 'END TASK') {
    const freed = await api('PATCH', `/workers/${w.id}/status`, { status: 'END TASK', reason: 'free for reassignment', version: w.version }, tokens.owner);
    assert.equal(freed.status, 200);
    w = freed.data;
  }
  const assigned = await api('POST', `/tasks/${t.id}/assign`, { worker_id: w.id, reason: 'temp', revision: t.revision }, tokens.admin);
  assert.equal(assigned.status, 200);
  const cancelled = await api('POST', `/tasks/${t.id}/cancel`, { reason: 'changed plans', revision: assigned.data.revision }, tokens.admin);
  assert.equal(cancelled.status, 200);
  assert.equal(cancelled.data.status, 'CANCELLED');
  const wAfter = (await api('GET', '/workers', null, tokens.owner)).data.find((x) => x.id === w.id);
  assert.equal(wAfter.status, 'ACTIVE', 'worker status untouched by cancel');
});

test('reopen returns completed task to CREATED for explicit reassignment', async () => {
  const t = (await api('POST', '/tasks', { title: 'Reopen probe' }, tokens.admin)).data;
  const workers = (await api('GET', '/workers', null, tokens.owner)).data;
  const w = workers.find((x) => x.machine_name === 'M1' && x.name === 'W2');
  await api('POST', `/tasks/${t.id}/assign`, { worker_id: w.id, reason: 'go', revision: t.revision }, tokens.admin);
  await api('POST', `/tasks/${t.id}/start`, { reason: 'go', revision: 2 }, tokens.admin);
  await api('POST', `/tasks/${t.id}/complete`, { reason: 'done', revision: 3 }, tokens.admin);
  const reopened = await api('POST', `/tasks/${t.id}/reopen`, { reason: 'regression found', revision: 4 }, tokens.admin);
  assert.equal(reopened.status, 200);
  assert.equal(reopened.data.status, 'CREATED');
  assert.equal(reopened.data.worker_id, null);
});

test('soft delete hides task but preserves history', async () => {
  const t = (await api('POST', '/tasks', { title: 'Doomed task' }, tokens.admin)).data;
  const del = await api('DELETE', `/tasks/${t.id}`, { reason: 'duplicate' }, tokens.owner);
  assert.equal(del.status, 200);
  const list = (await api('GET', '/tasks', null, tokens.owner)).data;
  assert.ok(!list.some((x) => x.id === t.id), 'hidden from default list');
  const detail = await api('GET', `/tasks/${t.id}`, null, tokens.owner);
  assert.equal(detail.status, 200, 'detail still reachable (history intact)');
  assert.ok(detail.data.deleted_at);
  assert.ok(detail.data.history.some((h) => h.event === 'deleted'));
});

test('extensibility: add machine + workers via API', async () => {
  const m = (await api('POST', '/machines', { name: 'M5', notes: 'added later' }, tokens.owner)).data;
  assert.equal(m.status, 'STANDBY');
  const w = (await api('POST', '/workers', { machine_id: m.id, name: 'W1' }, tokens.owner)).data;
  assert.equal(w.status, 'NO SLOT');
  const dup = await api('POST', '/machines', { name: 'M5' }, tokens.owner);
  assert.equal(dup.status, 409);
});

test('SSE broadcasts status changes to connected clients', async () => {
  const controller = new AbortController();
  const res = await fetch(`${base}/events`, { signal: controller.signal, headers: { Authorization: `Bearer ${tokens.owner}` } });
  assert.equal(res.status, 200);
  assert.match(res.headers.get('content-type') || '', /text\/event-stream/);

  const reader = res.body.getReader();
  let received = '';
  const reading = (async () => {
    try {
      for (;;) {
        const { done, value } = await reader.read();
        if (done) break;
        received += Buffer.from(value).toString('utf8');
        if (received.includes('worker.updated')) break;
      }
    } catch { /* aborted */ }
  })();

  await new Promise((r) => setTimeout(r, 150)); // let the subscription register
  const workers = (await api('GET', '/workers', null, tokens.owner)).data;
  const w = workers.find((x) => x.machine_name === 'M1' && x.name === 'W2');
  const r1 = await api('PATCH', `/workers/${w.id}/status`, { status: 'STANDBY', reason: 'sse probe', version: w.version }, tokens.owner);
  assert.equal(r1.status, 200);

  const withTimeout = await Promise.race([
    reading.then(() => true),
    new Promise((r) => setTimeout(() => r(false), 3000)),
  ]);
  controller.abort();
  assert.ok(withTimeout, 'client received worker.updated event over SSE');
});
