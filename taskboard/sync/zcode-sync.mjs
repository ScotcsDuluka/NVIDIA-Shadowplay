#!/usr/bin/env node
/*
 * zcode-sync — pulls REAL ZCode sessions from this machine into the Task Board.
 *
 * Source: ~/.zcode/v2/tasks-index.sqlite (the same data the ZCode Web Remote
 * Control page shows: W1, W2, W3, ...). Read from a temp copy; the live app
 * files are never touched.
 *
 * Rules respected (TASK != STATUS / REPORT != STATUS):
 *   - Synced sessions become board TASKS with ZCode's reported state written
 *     as a REPORT in notes + activity log.
 *   - The sync NEVER changes board task status, and NEVER changes worker
 *     status. Assign/start/complete remain explicit operator actions.
 *
 * Usage:  node sync/zcode-sync.mjs [--url http://localhost:15242] [--include-archived]
 */
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { DatabaseSync } from 'node:sqlite';

/* ---------------- args ---------------- */
const args = process.argv.slice(2);
function argOf(flag, fallback) {
  const i = args.indexOf(flag);
  return i >= 0 && args[i + 1] ? args[i + 1] : fallback;
}
const BOARD_URL = argOf('--url', process.env.TASKBOARD_URL || 'http://localhost:15242');
const INCLUDE_ARCHIVED = args.includes('--include-archived');
const WORKSPACE = argOf('--workspace', process.env.TASKBOARD_WORKSPACE || '');
const HOME = os.homedir();
const TASKS_DB = path.join(HOME, '.zcode', 'v2', 'tasks-index.sqlite');

/* ---------------- helpers ---------------- */
async function api(method, urlPath, body) {
  const res = await fetch(BOARD_URL + urlPath, {
    method,
    headers: {
      ...(body ? { 'Content-Type': 'application/json' } : {}),
      'X-Actor': 'zcode-sync',
    },
    body: body ? JSON.stringify(body) : undefined,
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(`${method} ${urlPath} -> ${res.status}: ${data.error || res.statusText}`);
  return data;
}

function readZcodeTasks() {
  if (!fs.existsSync(TASKS_DB)) {
    throw new Error(`ZCode tasks index not found: ${TASKS_DB}`);
  }
  // copy db (+ wal/shm if present) so we never touch the live app files
  const stamp = `zcode-tasks-${Date.now()}`;
  const tmp = path.join(os.tmpdir(), stamp + '.sqlite');
  for (const ext of ['', '-wal', '-shm']) {
    const src = TASKS_DB + ext;
    if (fs.existsSync(src)) fs.copyFileSync(src, tmp + ext);
  }
  try {
    const db = new DatabaseSync(tmp);
    const rows = db.prepare(`
      SELECT workspace_path, task_id, title, task_status, mode, model,
             created_at, updated_at, archived, deleted
      FROM tasks
      WHERE deleted = 0
      ORDER BY updated_at DESC
    `).all();
    db.close();
    return rows.map((r) => ({ ...r, created_iso: new Date(r.created_at).toISOString(), updated_iso: new Date(r.updated_at).toISOString() }));
  } finally {
    for (const ext of ['', '-wal', '-shm']) {
      try { fs.rmSync(tmp + ext, { force: true }); } catch { /* ignore */ }
    }
  }
}

function splitNotes(notes) {
  const lines = String(notes || '').split('\n');
  const operator = lines.filter((l) => !l.startsWith('[zcode-sync]')).join('\n').trim();
  return operator;
}

function buildNotes(task, machine) {
  const operator = splitNotes(task.notes);
  return [
    operator,
    operator ? '' : null,
    `[zcode-sync] session: ${task.task_id}`,
    `[zcode-sync] state: ${task.task_status} (reported ${new Date().toISOString()})`,
    `[zcode-sync] workspace: ${task.workspace_path}`,
    `[zcode-sync] machine: ${machine}`,
    `[zcode-sync] zcode-created: ${task.created_iso} | zcode-updated: ${task.updated_iso}`,
    `[zcode-sync] model: ${task.model || '?'} | mode: ${task.mode || '?'}`,
  ].filter((l) => l !== null).join('\n');
}

/* ---------------- main ---------------- */
const machine = process.env.COMPUTERNAME || os.hostname();
console.log(`zcode-sync: reading ${TASKS_DB}`);
const allTasks = readZcodeTasks();
const workspaceSet = new Set(allTasks.map((t) => t.workspace_path));
const workspaces = [...workspaceSet].filter(Boolean);
console.log(`workspaces on this machine: ${workspaces.join(' | ') || '(none)'}`);

const target = WORKSPACE
  ? (workspaces.find((w) => w.toLowerCase() === WORKSPACE.toLowerCase())
    || workspaces.find((w) => w.toLowerCase().includes(WORKSPACE.toLowerCase())))
  : workspaces.find((w) => w.toLowerCase().includes('nvidia-shadowplay')) || workspaces[0];
if (!target) throw new Error(`Workspace matching "${WORKSPACE}" not found. Available: ${workspaces.join(' | ')}`);
const selected = allTasks.filter((t) => t.workspace_path === target && (INCLUDE_ARCHIVED || !t.archived));
console.log(`syncing ${selected.length} session(s) from "${target}" (machine ${machine}) -> ${BOARD_URL}`);

let created = 0, updated = 0, skipped = 0;
for (const zt of selected) {
  const marker = zt.task_id;
  const existing = await api('GET', '/search?q=' + encodeURIComponent(marker));
  const hit = existing.tasks[0];

  if (!hit) {
    const made = await api('POST', '/tasks', {
      title: (zt.title || marker).slice(0, 120),
      objective: `Session imported from ZCode on ${machine}. ZCode reports the session state below - the board status stays explicit until an operator acts on it.`,
      notes: buildNotes({ ...zt, notes: '' }, machine),
      priority: 'P2',
      repository: path.basename(zt.workspace_path),
    });
    console.log(`  + created ${made.code} "${made.title}" [zcode state: ${zt.task_status}]`);
    created++;
    continue;
  }

  const full = await api('GET', '/tasks/' + hit.id);
  if (full.notes && full.notes.includes(`state: ${zt.task_status}`) && full.notes.includes(marker)) {
    skipped++;
    continue; // nothing changed since last sync
  }
  await api('PATCH', '/tasks/' + full.id, {
    revision: full.revision,
    notes: buildNotes({ ...zt, notes: full.notes }, machine),
  });
  console.log(`  ~ updated ${full.code} "${full.title}" [zcode state: ${zt.task_status}] (status untouched: ${full.status})`);
  updated++;
}

console.log(`done: ${created} created, ${updated} updated, ${skipped} unchanged.`);
console.log('Note: board task/worker statuses were NOT modified - sync only imports tasks and reports.');
