/* TASK BOARD — frontend SPA (vanilla JS, no build step) */
'use strict';

const GLYPHS = {
  'WORKING': '●', 'ACTIVE': '◑', 'END TASK': '○', 'NO SLOT': '⊘', 'STANDBY': '◌', 'CLOSED': '✕',
  'ONLINE': '●', 'OFFLINE': '○',
  'CREATED': '○', 'ASSIGNED': '◑', 'IN_PROGRESS': '●', 'BLOCKED': '⊘', 'COMPLETED': '✔', 'CANCELLED': '✕',
};

const TASK_ACTIONS = {
  assign:   { title: 'ASSIGN TASK', desc: 'Assign this task to a free worker. The worker goes END TASK → ACTIVE. It will NOT go to WORKING automatically — that is a separate explicit action.' },
  start:    { title: 'START TASK', desc: 'Worker explicitly starts working: task ASSIGNED → IN_PROGRESS, worker ACTIVE → WORKING.' },
  pause:    { title: 'PAUSE TASK', desc: 'Back to ASSIGNED; worker WORKING → ACTIVE (still reserved, not processing).' },
  complete: { title: 'COMPLETE TASK', desc: 'Task IN_PROGRESS → COMPLETED and worker WORKING → END TASK. Reports saying "done" never do this — only this explicit action.' },
  block:    { title: 'BLOCK TASK', desc: 'Task IN_PROGRESS → BLOCKED. Worker status is deliberately left unchanged — change it explicitly if needed.' },
  unblock:  { title: 'UNBLOCK TASK', desc: 'Task BLOCKED → IN_PROGRESS.' },
  cancel:   { title: 'CANCEL TASK', desc: 'Task → CANCELLED. The task is released from the worker; worker status is NOT changed automatically.' },
  reopen:   { title: 'REOPEN TASK', desc: 'Task → CREATED (unassigned). Assign it again explicitly.' },
};

const state = {
  user: null, meta: null, board: null, poll: null, es: null, openAccess: false,
  filters: { statuses: new Set(), machine: '', worker: '', priority: '' },
  search: '', historyTab: 'worker', noticeHidden: localStorage.getItem('tb_notice') === '1',
};

/* ---------------- utilities ---------------- */
const $ = (sel) => document.querySelector(sel);

function esc(s) {
  return String(s ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

function fmtAgo(iso) {
  if (!iso) return '—';
  const diff = (Date.now() - new Date(iso).getTime()) / 1000;
  const abs = new Date(iso).toLocaleString();
  let out;
  if (diff < 10) out = 'just now';
  else if (diff < 60) out = `${Math.floor(diff)}s ago`;
  else if (diff < 3600) out = `${Math.floor(diff / 60)} min ago`;
  else if (diff < 86400) out = `${Math.floor(diff / 3600)}h ago`;
  else out = `${Math.floor(diff / 86400)}d ago`;
  return `<span title="${esc(abs)}">${esc(out)}</span>`;
}

function chip(status) {
  if (!status) return '';
  const cls = 'st-' + status.replace(/\s+/g, '');
  return `<span class="chip ${cls}">${GLYPHS[status] || '•'} ${esc(status)}</span>`;
}
const prio = (p) => `<span class="prio prio-${esc(p)}">${esc(p)}</span>`;

function canEdit() { return state.user && (state.user.role === 'OWNER' || state.user.role === 'ADMIN'); }

async function api(method, path, body) {
  const res = await fetch(path, {
    method,
    headers: body ? { 'Content-Type': 'application/json' } : undefined,
    body: body ? JSON.stringify(body) : undefined,
    credentials: 'same-origin',
  });
  const data = await res.json().catch(() => ({}));
  if (res.status === 401 && !path.startsWith('/auth')) { showLogin(); throw new Error(data.error || 'Not signed in'); }
  if (!res.ok) {
    const err = new Error(data.error || res.statusText);
    err.status = res.status; err.data = data;
    throw err;
  }
  return data;
}

function toast(msg, isErr) {
  const el = document.createElement('div');
  el.className = 'toast' + (isErr ? ' err' : '');
  el.textContent = msg;
  $('#toasts').appendChild(el);
  setTimeout(() => el.remove(), 5000);
}

/* ---------------- modal ---------------- */
function openModal(html) {
  $('#modalBox').innerHTML = html;
  $('#modalOverlay').classList.remove('hidden');
}
function closeModal() { $('#modalOverlay').classList.add('hidden'); }
function modalError(msg) {
  const box = $('#modalError');
  if (box) { box.outerHTML = `<div id="modalError" class="form-error">${esc(msg)}</div>`; }
  else toast(msg, true);
}
function apiInModal(promise) {
  return promise.then((data) => { closeModal(); return data; })
    .catch((err) => { modalError(err.message); throw err; });
}

/* ---------------- auth ---------------- */
function showLogin() {
  stopPolling();
  if (state.es) { state.es.close(); state.es = null; }
  $('#app').classList.add('hidden');
  $('#login').classList.remove('hidden');
}
async function enterApp() {
  $('#login').classList.add('hidden');
  $('#app').classList.remove('hidden');
  $('#userLabel').innerHTML = `<b>${esc(state.user.username)}</b> · ${esc(state.user.role)}` +
    (state.openAccess ? ` · <span style="color:var(--warn)" title="No login required — anyone can view and act. Identity via X-Actor header on the API.">OPEN ACCESS</span>` : '');
  $('#btnNewTask').classList.toggle('hidden', !canEdit());
  $('#btnLogout').classList.toggle('hidden', state.openAccess);
  if (!state.meta) state.meta = await api('GET', '/meta');
  await route();
  connectSSE();
}

/* ---------------- routing ---------------- */
async function route() {
  const hash = location.hash || '#/';
  document.querySelectorAll('#mainnav a').forEach((a) => {
    a.classList.toggle('active', a.getAttribute('href') === (hash.startsWith('#/task') ? '#/' : hash));
  });
  const fb = $('#filterbar');
  if (hash === '#/' || hash === '') {
    fb.classList.remove('hidden');
    state.board = await api('GET', '/board');
    if (state.search) renderSearchPage();
    else { renderFilterbar(); renderBoard(); }
  } else if (hash.startsWith('#/task/')) {
    fb.classList.add('hidden');
    await renderTaskPage(hash.split('/')[2]);
  } else if (hash === '#/activity') {
    fb.classList.add('hidden');
    await renderActivityPage();
  } else if (hash === '#/history') {
    fb.classList.add('hidden');
    await renderHistoryPage();
  } else {
    location.hash = '#/';
  }
}

/* ---------------- filter bar ---------------- */
function renderFilterbar() {
  const f = state.filters;
  const statuses = state.meta.worker_statuses;
  const machines = state.board.machines.map((m) => `<option value="${m.id}" ${String(f.machine) === String(m.id) ? 'selected' : ''}>${esc(m.name)}</option>`).join('');
  const workers = state.board.machines.flatMap((m) => m.workers)
    .map((w) => `<option value="${w.id}" ${String(f.worker) === String(w.id) ? 'selected' : ''}>${esc(w.machine_name)}/${esc(w.name)}</option>`).join('');
  $('#filterbar').innerHTML = `
    <span class="fl">FILTER</span>
    <span>
      ${statuses.map((s) => `<label class="fchk"><input type="checkbox" ${f.statuses.has(s) ? 'checked' : ''} onchange="TB.toggleStatusFilter('${s}')">${chip(s)}</label>`).join(' ')}
    </span>
    <span class="fl">MACHINE</span>
    <select onchange="TB.setMachineFilter(this.value)"><option value="">all</option>${machines}</select>
    <span class="fl">WORKER</span>
    <select onchange="TB.setWorkerFilter(this.value)"><option value="">all</option>${workers}</select>
    <span class="fl">PRIORITY</span>
    <select onchange="TB.setPriorityFilter(this.value)">
      <option value="">all</option>
      ${['P0', 'P1', 'P2', 'P3'].map((p) => `<option value="${p}" ${f.priority === p ? 'selected' : ''}>${p}</option>`).join('')}
    </select>
    <button class="btn ghost small" onclick="TB.clearFilters()">CLEAR</button>
    <span class="fl" style="margin-left:auto">LIVE: ${'SSE'}</span>`;
}

/* ---------------- board ---------------- */
function workerMatchesFilters(w) {
  const f = state.filters;
  if (f.statuses.size && !f.statuses.has(w.status)) return false;
  if (f.priority && (!w.current_task || w.current_task.priority !== f.priority)) return false;
  return true;
}

async function renderBoard() {
  const f = state.filters;
  let machines = state.board.machines;
  if (f.machine) machines = machines.filter((m) => String(m.id) === String(f.machine));
  if (f.worker) machines = machines.filter((m) => m.workers.some((w) => String(w.id) === String(f.worker)));

  const html = machines.map((m) => {
    let workers = m.workers;
    if (f.worker) workers = workers.filter((w) => String(w.id) === String(f.worker));
    if (f.statuses.size || f.priority) workers = workers.filter(workerMatchesFilters);
    return `
    <section class="machine">
      <div class="machine-head">
        <span class="machine-name">${esc(m.name)}</span>
        ${chip(m.status)}
        <span class="machine-notes">${esc(m.notes || '')}</span>
        <span class="spacer"></span>
        ${canEdit() ? `
          <button class="btn small" onclick="TB.machineStatusModal(${m.id})">STATUS ▾</button>
          ${m.status !== 'CLOSED' ? `<button class="btn small" onclick="TB.addWorkerModal(${m.id})">+ WORKER</button>` : ''}` : ''}
      </div>
      ${workers.length
        ? `<div class="worker-grid">${workers.map((w) => workerCard(m, w)).join('')}</div>`
        : `<div class="wc-empty">No workers match the current filter.</div>`}
    </section>`;
  }).join('');

  const c = state.board.counts;
  // backlog: CREATED (unassigned) tasks - e.g. sessions pulled in by zcode-sync - waiting for an operator
  const backlog = await api('GET', '/tasks?status=CREATED');
  const backlogRows = backlog.map((t) => `
    <tr>
      <td class="mono"><a href="#/task/${t.id}">${esc(t.code)}</a></td>
      <td><a href="#/task/${t.id}">${esc(t.title)}</a></td>
      <td>${prio(t.priority)}</td>
      <td class="dim">${fmtAgo(t.updated_at)}</td>
      <td>${canEdit() ? `<button class="btn small" onclick="TB.taskActionModal(${t.id},'assign')">ASSIGN ▸</button>` : ''}</td>
    </tr>`).join('');
  const backlogSection = backlog.length ? `
    <section class="machine">
      <div class="machine-head">
        <span class="machine-name">BACKLOG</span>
        ${chip('CREATED')}
        <span class="machine-notes">${backlog.length} unassigned task(s) - imported work waits here until an operator assigns it</span>
      </div>
      <table class="grid" style="margin:8px 14px 14px">
        <tr><th>CODE</th><th>TITLE</th><th>PRIORITY</th><th>UPDATED</th><th></th></tr>
        ${backlogRows}
      </table>
    </section>` : '';

  $('#content').innerHTML = `
    <div class="board">
      <div class="page-head">
        <h1>TASK BOARD</h1>
        <span class="sub">${c.machines} machines · ${c.workers} workers · ${c.open_tasks} open tasks (${c.unassigned_tasks} unassigned)</span>
        <span class="spacer"></span>
        ${canEdit() ? `<button class="btn small" onclick="TB.addMachineModal()">+ MACHINE</button>` : ''}
      </div>
      ${backlogSection}
      ${html || '<div class="wc-empty">No machines match the current filter.</div>'}
    </div>`;
}

function workerCard(m, w) {
  const t = w.current_task;
  const taskLine = t
    ? `<div class="wc-task"><a href="#/task/${t.id}">${esc(t.title)}</a></div>
       <div class="wc-meta">
         <span class="mono">${esc(t.code)}</span> ${prio(t.priority)} ${chip(t.status)}
         ${t.tests ? `<span>▸ ${esc(t.tests)}</span>` : ''}
       </div>
       <div>
         <div class="progressbar st-${esc(t.status)}"><div style="width:${t.progress}%"></div></div>
         <div class="wc-meta"><span>Progress ${t.progress}%</span><span>·</span><span>updated ${fmtAgo(w.updated_at)}</span></div>
       </div>
       ${t.blocker ? `<div class="wc-blocker">⊘ Blocker: ${esc(t.blocker)}</div>` : ''}`
    : `<div class="wc-task"><span class="none">— no current task —</span></div>
       <div class="wc-meta"><span>updated ${fmtAgo(w.updated_at)}</span></div>`;

  return `
  <div class="worker-card">
    <div class="wc-head">
      <span class="wc-title">${esc(m.name)} / ${esc(w.name)}</span>
      ${chip(w.status)}
    </div>
    ${taskLine}
    <div class="wc-actions">
      ${t ? `<a class="btn small" href="#/task/${t.id}">DETAILS</a>` : ''}
      ${canEdit() ? `
        <button class="btn small" onclick="TB.workerStatusModal(${w.id})">STATUS ▾</button>
        ${w.status === 'END TASK' ? `<button class="btn small" onclick="TB.assignFromWorkerModal(${w.id})">ASSIGN TASK</button>` : ''}` : ''}
    </div>
  </div>`;
}

/* ---------------- search ---------------- */
async function renderSearchPage() {
  const q = state.search;
  const r = await api('GET', '/search?q=' + encodeURIComponent(q));
  const total = r.tasks.length + r.workers.length + r.machines.length;
  const taskRows = r.tasks.map((t) => `
    <tr>
      <td class="mono"><a href="#/task/${t.id}">${esc(t.code)}</a></td>
      <td><a href="#/task/${t.id}">${esc(t.title)}</a></td>
      <td>${chip(t.status)}</td>
      <td>${prio(t.priority)}</td>
      <td class="dim">${t.progress}%</td>
      <td class="dim">${fmtAgo(t.updated_at)}</td>
    </tr>`).join('');
  const workerRows = r.workers.map((w) => `
    <tr>
      <td class="mono">${esc(w.machine_name)}/${esc(w.name)}</td>
      <td>${chip(w.status)}</td>
      <td>${w.current_task ? `<a href="#/task/${w.current_task.id}">${esc(w.current_task.title)}</a>` : '<span class="faint">—</span>'}</td>
      <td class="dim">${fmtAgo(w.updated_at)}</td>
    </tr>`).join('');
  const machineRows = r.machines.map((m) => `
    <tr>
      <td class="mono">${esc(m.name)}</td>
      <td>${chip(m.status)}</td>
      <td class="dim">${m.worker_count} workers</td>
      <td class="dim">${esc(m.notes || '')}</td>
    </tr>`).join('');

  $('#content').innerHTML = `
    <div class="page">
      <div class="page-head"><h1>SEARCH</h1><span class="sub">"${esc(q)}" — ${total} result(s)</span>
        <span class="spacer"></span><button class="btn small ghost" onclick="TB.clearSearch()">CLEAR SEARCH</button></div>
      ${r.tasks.length ? `<div class="section search-section"><h3>TASKS (${r.tasks.length})</h3>
        <table class="grid"><tr><th>ID</th><th>TITLE</th><th>STATUS</th><th>PRIORITY</th><th>PROGRESS</th><th>UPDATED</th></tr>${taskRows}</table></div>` : ''}
      ${r.workers.length ? `<div class="section search-section"><h3>WORKERS (${r.workers.length})</h3>
        <table class="grid"><tr><th>WORKER</th><th>STATUS</th><th>CURRENT TASK</th><th>UPDATED</th></tr>${workerRows}</table></div>` : ''}
      ${r.machines.length ? `<div class="section search-section"><h3>MACHINES (${r.machines.length})</h3>
        <table class="grid"><tr><th>MACHINE</th><th>STATUS</th><th>WORKERS</th><th>NOTES</th></tr>${machineRows}</table></div>` : ''}
      ${!total ? '<div class="wc-empty">Nothing matched. Search covers task name/id, machine, worker, files, tests, blockers and notes.</div>' : ''}
    </div>`;
}

/* ---------------- task detail ---------------- */
async function renderTaskPage(id) {
  let t;
  try { t = await api('GET', '/tasks/' + id); }
  catch (err) {
    $('#content').innerHTML = `<div class="page"><div class="section">${esc(err.message)}</div></div>`;
    return;
  }
  const started = t.started_at ? fmtAgo(t.started_at) : '—';
  const actions = [];
  if (canEdit()) {
    if (t.status === 'CREATED') actions.push(`<button class="btn accent" onclick="TB.taskActionModal(${t.id},'assign')">ASSIGN</button>`);
    if (t.status === 'ASSIGNED') actions.push(`<button class="btn accent" onclick="TB.taskActionModal(${t.id},'start')">▶ START</button>`);
    if (t.status === 'IN_PROGRESS') {
      actions.push(`<button class="btn accent" onclick="TB.taskActionModal(${t.id},'complete')">✔ COMPLETE</button>`);
      actions.push(`<button class="btn" onclick="TB.taskActionModal(${t.id},'pause')">⏸ PAUSE</button>`);
      actions.push(`<button class="btn danger" onclick="TB.taskActionModal(${t.id},'block')">⊘ BLOCK</button>`);
    }
    if (t.status === 'BLOCKED') actions.push(`<button class="btn accent" onclick="TB.taskActionModal(${t.id},'unblock')">UNBLOCK</button>`);
    if (['CREATED', 'ASSIGNED', 'IN_PROGRESS', 'BLOCKED'].includes(t.status)) {
      actions.push(`<button class="btn danger" onclick="TB.taskActionModal(${t.id},'cancel')">✕ CANCEL</button>`);
    }
    if (['COMPLETED', 'CANCELLED'].includes(t.status)) {
      actions.push(`<button class="btn accent" onclick="TB.taskActionModal(${t.id},'reopen')">↺ REOPEN</button>`);
    }
    actions.push(`<button class="btn" onclick="TB.editTaskModal(${t.id})">EDIT FIELDS</button>`);
    actions.push(`<button class="btn danger ghost" onclick="TB.deleteTaskModal(${t.id})">DELETE</button>`);
  }

  const kv = (k, v) => `<div class="cell"><div class="k">${k}</div><div class="v">${v}</div></div>`;
  const section = (title, body) => body ? `<div class="section"><h3>${title}</h3><pre>${body}</pre></div>` : '';

  const hist = t.history.map((h) => `
    <div class="logline">
      <span class="t">${new Date(h.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
      <span class="who">${esc(h.changed_by)}</span>
      <span class="what"><span class="mono">${esc(h.event)}</span>
        ${h.old_status || h.new_status ? `<span class="transition">${esc(h.old_status || '—')} <span class="arrow">→</span> ${esc(h.new_status || '—')}</span>` : ''}
        ${h.details ? `<span class="dim"> — ${esc(h.details)}</span>` : ''}</span>
    </div>`).join('');
  const act = t.activity.map((a) => `
    <div class="logline">
      <span class="t">${new Date(a.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
      <span class="who">${esc(a.actor)}</span>
      <span class="what"><span class="mono">${esc(a.action)}</span> <span class="dim">${esc(a.details)}</span></span>
    </div>`).join('');

  $('#content').innerHTML = `
  <div class="task-wrap">
    <div class="page-head"><a href="#/">← BOARD</a></div>
    <div class="task-head">
      <span class="code">${esc(t.code)}</span>
      <h1>${esc(t.title)}</h1>
      ${chip(t.status)} ${prio(t.priority)}
    </div>
    <div class="task-actions">${actions.join('')}</div>
    <div class="kv">
      ${kv('STATUS', chip(t.status) + ' <span class="faint">— explicit field, never inferred from reports</span>')}
      ${kv('MACHINE / WORKER', t.machine_name ? `${esc(t.machine_name)} / <b>${esc(t.worker_name || '?')}</b>` : '<span class="faint">unassigned</span>')}
      ${kv('PRIORITY', prio(t.priority))}
      ${kv('STARTED', started)}
      ${kv('LAST UPDATED', fmtAgo(t.updated_at))}
      ${kv('REVISION', `<span class="mono">rev ${t.revision}</span>`)}
    </div>
    <div class="section">
      <h3>PROGRESS</h3>
      <div class="progress-row">
        <div class="progressbar st-${esc(t.status)}"><div style="width:${t.progress}%"></div></div>
        <span class="progress-pct">${t.progress}%</span>
      </div>
    </div>
    ${section('OBJECTIVE', esc(t.objective))}
    ${section('SCOPE', esc(t.scope))}
    ${section('OUT OF SCOPE', esc(t.out_of_scope))}
    ${section('ACCEPTANCE CRITERIA', esc(t.acceptance_criteria))}
    ${t.tests ? section('TESTS', esc(t.tests) + ' <span class="faint">(a passing report does NOT change status)</span>') : ''}
    ${t.blocker ? `<div class="section"><h3>BLOCKER</h3><pre class="bad">⊘ ${esc(t.blocker)}</pre></div>` : ''}
    ${t.files_changed ? section('FILES CHANGED', `<span class="mono">${esc(t.files_changed)}</span>`) : ''}
    ${t.notes ? section('NOTES', esc(t.notes)) : ''}
    ${(t.repository || t.branch || t.commit_hash) ? `<div class="section"><h3>GIT (TRACKER ONLY — THIS APP NEVER RUNS GIT)</h3>
      <pre class="mono">${esc([t.repository, t.branch, t.commit_hash].filter(Boolean).join('  ·  '))}</pre></div>` : ''}
    <div class="section"><h3>TASK HISTORY (append-only)</h3>${hist || '<span class="faint">no entries</span>'}</div>
    <div class="section"><h3>ACTIVITY</h3>${act || '<span class="faint">no entries</span>'}</div>
  </div>`;
}

/* ---------------- activity page ---------------- */
async function renderActivityPage() {
  const rows = await api('GET', '/activity?limit=200');
  const html = rows.map((a) => `
    <div class="logline">
      <span class="t">${new Date(a.timestamp).toLocaleString()}</span>
      <span class="who">${esc(a.actor)}</span>
      <span class="what">
        <span class="mono">${esc(a.action)}</span>
        ${a.machine || a.worker ? `<span class="dim">${esc(a.machine || '')}${a.worker ? '/' + esc(a.worker) : ''}</span>` : ''}
        <span class="dim"> ${esc(a.details)}</span>
      </span>
    </div>`).join('');
  $('#content').innerHTML = `
    <div class="page">
      <div class="page-head"><h1>ACTIVITY LOG</h1><span class="sub">latest ${rows.length} events — append-only</span></div>
      <div class="section">${html || '<span class="faint">no activity yet</span>'}</div>
    </div>`;
}

/* ---------------- history page ---------------- */
async function renderHistoryPage() {
  const tab = state.historyTab;
  const data = await api('GET', '/history?limit=300');
  let body;
  if (tab === 'worker') {
    const rows = (data.worker || []).map((h) => `
      <tr>
        <td class="mono dim">${new Date(h.timestamp).toLocaleString()}</td>
        <td class="mono">${esc(h.machine)}</td>
        <td class="mono">${esc(h.worker_name)}</td>
        <td class="transition">${chip(h.old_status || '—')} <span class="arrow">→</span> ${chip(h.new_status)}</td>
        <td>${esc(h.changed_by)}</td>
        <td class="dim">${esc(h.reason)}</td>
      </tr>`).join('');
    body = `<table class="grid"><tr><th>TIME</th><th>MACHINE</th><th>WORKER</th><th>TRANSITION</th><th>BY</th><th>REASON</th></tr>${rows}</table>`;
  } else {
    const rows = (data.task || []).map((h) => `
      <tr>
        <td class="mono dim">${new Date(h.timestamp).toLocaleString()}</td>
        <td class="mono"><a href="#/task/${h.task_id}">${esc(h.task_code || h.task_id)}</a></td>
        <td class="mono">${esc(h.event)}</td>
        <td class="transition">${chip(h.old_status || '—')} <span class="arrow">→</span> ${chip(h.new_status || '—')}</td>
        <td>${esc(h.changed_by)}</td>
        <td class="dim">${esc(h.details)}</td>
      </tr>`).join('');
    body = `<table class="grid"><tr><th>TIME</th><th>TASK</th><th>EVENT</th><th>TRANSITION</th><th>BY</th><th>DETAILS</th></tr>${rows}</table>`;
  }
  $('#content').innerHTML = `
    <div class="page">
      <div class="page-head">
        <h1>HISTORY</h1>
        <button class="btn small ${tab === 'worker' ? 'accent' : ''}" onclick="TB.setHistoryTab('worker')">WORKER STATUS</button>
        <button class="btn small ${tab === 'task' ? 'accent' : ''}" onclick="TB.setHistoryTab('task')">TASK HISTORY</button>
        <span class="sub">append-only, never deleted by default</span>
      </div>
      <div class="section">${body}</div>
    </div>`;
}

/* ---------------- modals ---------------- */
function reasonField(id = 'mReason') {
  return `<label>REASON (REQUIRED — recorded in the audit trail)</label>
          <textarea id="${id}" required placeholder="Why is this change happening?"></textarea>`;
}

function statusTransitionModal(kind, entity) {
  const transitions = state.meta.transitions[kind];
  const allowed = transitions[entity.status] || [];
  const id = entity.id;
  const label = kind === 'worker' ? `${entity.machine_name}/${entity.name}` : entity.name;
  const versionField = kind === 'worker' ? 'version' : 'version';
  openModal(`
    <h2>CHANGE ${kind.toUpperCase()} STATUS — ${esc(label)}</h2>
    <div class="modal-sub">Current: ${chip(entity.status)}. Only listed transitions are valid. Every change is logged with old status, new status, timestamp, actor and reason.</div>
    <form onsubmit="return TB.submitStatus(event,'${kind}',${id},${entity[versionField]})">
      <div class="radio-row">
        ${allowed.map((s, i) => `<label><input type="radio" name="mStatus" value="${esc(s)}" ${i === 0 ? 'checked' : ''}> ${chip(s)}</label>`).join('') || '<span class="faint">No transitions available.</span>'}
      </div>
      ${reasonField()}
      <div id="modalError"></div>
      <div class="modal-actions">
        <button type="button" class="btn ghost" onclick="TB.closeModal()">CANCEL</button>
        <button class="btn accent" type="submit" ${allowed.length ? '' : 'disabled'}>CHANGE STATUS</button>
      </div>
    </form>`);
}

async function assignSelectMarkup(selectedWorkerId) {
  const workers = await api('GET', '/workers?status=' + encodeURIComponent('END TASK'));
  const opts = workers.map((w) =>
    `<option value="${w.id}" ${String(w.id) === String(selectedWorkerId) ? 'selected' : ''}>${esc(w.machine_name)}/${esc(w.name)}</option>`).join('');
  return opts
    ? `<label>WORKER (must be END TASK)</label><select id="mWorker">${opts}</select>`
    : `<div class="form-info">No worker is currently END TASK. Free a worker first (explicitly), then assign.</div>`;
}

/* ---------------- SSE / polling ---------------- */
function connectSSE() {
  if (state.es) state.es.close();
  const es = new EventSource('/events');
  state.es = es;
  let timer;
  const reload = () => { clearTimeout(timer); timer = setTimeout(() => route().catch(() => {}), 300); };
  es.onmessage = (ev) => {
    try { const m = JSON.parse(ev.data); if (m.type && m.type !== 'hello') reload(); } catch { /* ignore */ }
  };
  es.onopen = () => stopPolling();
  es.onerror = () => startPolling();
}
function startPolling() {
  if (state.poll) return;
  state.poll = setInterval(() => route().catch(() => {}), 5000);
}
function stopPolling() { if (state.poll) { clearInterval(state.poll); state.poll = null; } }

/* ---------------- event handlers (TB namespace) ---------------- */
window.TB = {
  closeModal,
  toggleStatusFilter(s) {
    const set = state.filters.statuses;
    set.has(s) ? set.delete(s) : set.add(s);
    renderBoard();
  },
  setMachineFilter(v) { state.filters.machine = v; renderBoard(); },
  setWorkerFilter(v) { state.filters.worker = v; renderBoard(); },
  setPriorityFilter(v) { state.filters.priority = v; renderBoard(); },
  clearFilters() {
    state.filters = { statuses: new Set(), machine: '', worker: '', priority: '' };
    renderFilterbar(); renderBoard();
  },
  clearSearch() {
    state.search = ''; $('#globalSearch').value = '';
    renderFilterbar(); renderBoard();
  },
  setHistoryTab(tab) { state.historyTab = tab; renderHistoryPage(); },

  async submitStatus(ev, kind, id, version) {
    ev.preventDefault();
    const status = document.querySelector('input[name="mStatus"]:checked')?.value;
    if (!status) return false;
    const reason = $('#mReason').value.trim();
    try {
      await apiInModal(api('PATCH', `/${kind}s/${id}/status`, { status, reason, version }));
      toast(`${kind} status → ${status}`);
      await route();
    } catch { /* error shown in modal */ }
    return false;
  },

  workerStatusModal(id) {
    const w = state.board.machines.flatMap((m) => m.workers).find((x) => x.id === id);
    if (w) statusTransitionModal('worker', w);
  },
  machineStatusModal(id) {
    const m = state.board.machines.find((x) => x.id === id);
    if (m) statusTransitionModal('machine', m);
  },

  /* ---- tasks ---- */
  async taskActionModal(taskId, action) {
    let t;
    try { t = await api('GET', '/tasks/' + taskId); } catch (err) { toast(err.message, true); return; }
    const A = TASK_ACTIONS[action];
    let extra = '';
    if (action === 'assign') extra = await assignSelectMarkup(t.worker_id);
    openModal(`
      <h2>${A.title} — ${esc(t.code)}</h2>
      <div class="modal-sub">${esc(A.desc)}</div>
      <form onsubmit="return TB.submitTaskAction(event,${t.id},'${action}',${t.revision})">
        ${extra}
        ${reasonField()}
        ${action === 'assign' ? `<div class="checkrow"><input type="checkbox" id="mParallel"><span>Explicit <b>parallel-task mode</b> — allow assigning even though the worker already has an open task (default is 1 worker = 1 task; parallel assignment is logged).</span></div>` : ''}
        <div id="modalError"></div>
        <div class="modal-actions">
          <button type="button" class="btn ghost" onclick="TB.closeModal()">CANCEL</button>
          <button class="btn accent" type="submit">${A.title}</button>
        </div>
      </form>`);
  },
  async submitTaskAction(ev, taskId, action, revision) {
    ev.preventDefault();
    const body = { reason: $('#mReason')?.value.trim(), revision };
    if (action === 'assign') {
      body.worker_id = $('#mWorker')?.value;
      body.parallel = $('#mParallel')?.checked || false;
      if (!body.worker_id) { modalError('Select a worker.'); return false; }
    }
    try {
      await apiInModal(api('POST', `/tasks/${taskId}/${action}`, body));
      toast(`Task ${action} done`);
      await route();
    } catch { /* error shown in modal */ }
    return false;
  },

  async assignFromWorkerModal(workerId) {
    const tasks = await api('GET', '/tasks?status=CREATED');
    const opts = tasks.map((t) => `<option value="${t.id}">${esc(t.code)} — ${esc(t.title)} [${esc(t.priority)}]</option>`).join('');
    const w = state.board.machines.flatMap((m) => m.workers).find((x) => x.id === workerId);
    openModal(`
      <h2>ASSIGN TASK — ${esc(w.machine_name)}/${esc(w.name)}</h2>
      <div class="modal-sub">Assigning sets the worker END TASK → ACTIVE. Starting the work (ACTIVE → WORKING) is a separate explicit action.</div>
      <form onsubmit="return TB.submitAssignFromWorker(event,${workerId})">
        ${opts
          ? `<label>UNASSIGNED TASK</label><select id="mTask">${opts}</select>${reasonField()}`
          : `<div class="form-info">No unassigned (CREATED) tasks. Create one with + NEW TASK.</div>${reasonField()}`}
        <div id="modalError"></div>
        <div class="modal-actions">
          <button type="button" class="btn ghost" onclick="TB.closeModal()">CANCEL</button>
          <button class="btn accent" type="submit" ${opts ? '' : 'disabled'}>ASSIGN</button>
        </div>
      </form>`);
  },
  async submitAssignFromWorker(ev, workerId) {
    ev.preventDefault();
    try {
      await apiInModal(api('POST', `/tasks/${$('#mTask').value}/assign`,
        { worker_id: workerId, reason: $('#mReason').value.trim() }));
      toast('Task assigned — worker is ACTIVE');
      await route();
    } catch { /* shown in modal */ }
    return false;
  },

  async newTaskModal() {
    const workers = await api('GET', '/workers?status=' + encodeURIComponent('END TASK'));
    const workerOpts = workers.map((w) =>
      `<option value="${w.id}">${esc(w.machine_name)}/${esc(w.name)}</option>`).join('');
    openModal(`
      <h2>NEW TASK</h2>
      <div class="modal-sub">A new task starts as CREATED with no automatic status changes. Optionally assign it now — the chosen worker (END TASK) becomes ACTIVE.</div>
      <form onsubmit="return TB.submitNewTask(event)">
        <button type="button" class="btn small" onclick="TB.fillTemplate()">USE ENGINEERING TEMPLATE</button>
        <label>TITLE *</label><input id="fTitle" required>
        <div class="row">
          <div><label>PRIORITY</label>
            <select id="fPriority">${['P0', 'P1', 'P2', 'P3'].map((p) => `<option ${p === 'P1' ? 'selected' : ''}>${p}</option>`).join('')}</select>
          </div>
          <div><label>ASSIGN TO WORKER (optional — END TASK only)</label>
            <select id="fWorker"><option value="">— leave unassigned —</option>${workerOpts}</select>
          </div>
        </div>
        <label>OBJECTIVE</label><textarea id="fObjective"></textarea>
        <label>SCOPE</label><textarea id="fScope"></textarea>
        <label>OUT OF SCOPE</label><textarea id="fOutOfScope"></textarea>
        <label>ACCEPTANCE CRITERIA</label><textarea id="fAcceptance"></textarea>
        <div class="row">
          <div><label>REPOSITORY</label><input id="fRepo" class="mono" placeholder="owner/repo"></div>
          <div><label>BRANCH</label><input id="fBranch" class="mono" placeholder="branch-name"></div>
        </div>
        <label>NOTES</label><textarea id="fNotes"></textarea>
        <div id="modalError"></div>
        <div class="modal-actions">
          <button type="button" class="btn ghost" onclick="TB.closeModal()">CANCEL</button>
          <button class="btn accent" type="submit">CREATE TASK</button>
        </div>
      </form>`);
  },
  fillTemplate() {
    $('#fTitle').placeholder = 'e.g. Gallery.Video Finalization';
    $('#fObjective').value = 'Objective: what outcome is required and how do we know it is done?\n\n- ';
    $('#fScope').value = 'In scope:\n- \n\nDeliverables:\n- ';
    $('#fOutOfScope').value = 'Explicitly out of scope:\n- ';
    $('#fAcceptance').value = 'Acceptance criteria:\n- [ ] Tests pass\n- [ ] ';
    $('#fObjective').focus();
  },
  async submitNewTask(ev) {
    ev.preventDefault();
    const body = {
      title: $('#fTitle').value.trim(),
      priority: $('#fPriority').value,
      objective: $('#fObjective').value,
      scope: $('#fScope').value,
      out_of_scope: $('#fOutOfScope').value,
      acceptance_criteria: $('#fAcceptance').value,
      repository: $('#fRepo').value.trim(),
      branch: $('#fBranch').value.trim(),
      notes: $('#fNotes').value,
    };
    const w = $('#fWorker').value;
    if (w) body.worker_id = Number(w);
    try {
      const t = await apiInModal(api('POST', '/tasks', body));
      toast(`Task ${t.code} created${t.worker_id ? ' & assigned (worker ACTIVE)' : ''}`);
      location.hash = '#/task/' + t.id;
    } catch { /* shown in modal */ }
    return false;
  },

  async editTaskModal(taskId) {
    const t = await api('GET', '/tasks/' + taskId);
    const f = (id, label, val, mono) =>
      `<label>${label}</label><textarea id="${id}" class="${mono ? 'mono' : ''}">${esc(val || '')}</textarea>`;
    openModal(`
      <h2>EDIT FIELDS — ${esc(t.code)}</h2>
      <div class="modal-sub">Field edits and reports NEVER change status (TASK ≠ STATUS). Status moves only through the explicit action buttons.</div>
      <form onsubmit="return TB.submitEditTask(event,${t.id},${t.revision})">
        <label>TITLE</label><input id="eTitle" value="${esc(t.title)}" required>
        <div class="row">
          <div><label>PRIORITY</label>
            <select id="ePriority">${['P0', 'P1', 'P2', 'P3'].map((p) => `<option ${p === t.priority ? 'selected' : ''}>${p}</option>`).join('')}</select>
          </div>
          <div><label>PROGRESS %</label><input id="eProgress" type="number" min="0" max="100" value="${t.progress}"></div>
        </div>
        ${f('eObjective', 'OBJECTIVE', t.objective)}
        ${f('eScope', 'SCOPE', t.scope)}
        ${f('eOutOfScope', 'OUT OF SCOPE', t.out_of_scope)}
        ${f('eAcceptance', 'ACCEPTANCE CRITERIA', t.acceptance_criteria)}
        ${f('eFiles', 'FILES CHANGED (one per line)', t.files_changed, true)}
        ${f('eTests', 'TESTS (e.g. 97 PASS / 0 FAIL / 6 SKIP)', t.tests, true)}
        ${f('eBlocker', 'BLOCKER', t.blocker)}
        ${f('eNotes', 'NOTES', t.notes)}
        <div class="row">
          <div><label>REPOSITORY</label><input id="eRepo" class="mono" value="${esc(t.repository)}"></div>
          <div><label>BRANCH</label><input id="eBranch" class="mono" value="${esc(t.branch)}"></div>
        </div>
        <label>COMMIT</label><input id="eCommit" class="mono" value="${esc(t.commit_hash)}">
        <div id="modalError"></div>
        <div class="modal-actions">
          <button type="button" class="btn ghost" onclick="TB.closeModal()">CANCEL</button>
          <button class="btn accent" type="submit">SAVE FIELDS</button>
        </div>
      </form>`);
  },
  async submitEditTask(ev, taskId, revision) {
    ev.preventDefault();
    const body = {
      revision,
      title: $('#eTitle').value.trim(),
      priority: $('#ePriority').value,
      progress: Number($('#eProgress').value),
      objective: $('#eObjective').value,
      scope: $('#eScope').value,
      out_of_scope: $('#eOutOfScope').value,
      acceptance_criteria: $('#eAcceptance').value,
      files_changed: $('#eFiles').value,
      tests: $('#eTests').value,
      blocker: $('#eBlocker').value,
      notes: $('#eNotes').value,
      repository: $('#eRepo').value.trim(),
      branch: $('#eBranch').value.trim(),
      commit_hash: $('#eCommit').value.trim(),
    };
    try {
      await apiInModal(api('PATCH', '/tasks/' + taskId, body));
      toast('Task fields saved — status untouched');
      await route();
    } catch { /* shown in modal */ }
    return false;
  },

  deleteTaskModal(taskId) {
    openModal(`
      <h2>DELETE TASK</h2>
      <div class="modal-sub">Soft delete: the task is hidden from lists but its history is preserved (history is never deleted by default).</div>
      <form onsubmit="return TB.submitDeleteTask(event,${taskId})">
        ${reasonField()}
        <div id="modalError"></div>
        <div class="modal-actions">
          <button type="button" class="btn ghost" onclick="TB.closeModal()">CANCEL</button>
          <button class="btn danger" type="submit">DELETE</button>
        </div>
      </form>`);
  },
  async submitDeleteTask(ev, taskId) {
    ev.preventDefault();
    try {
      await apiInModal(api('DELETE', '/tasks/' + taskId, { reason: $('#mReason').value.trim() }));
      toast('Task deleted (soft) — history preserved');
      location.hash = '#/';
    } catch { /* shown in modal */ }
    return false;
  },

  /* ---- machines & workers ---- */
  addMachineModal() {
    openModal(`
      <h2>ADD MACHINE</h2>
      <div class="modal-sub">New machines start as STANDBY. The board scales to any number of machines and workers.</div>
      <form onsubmit="return TB.submitAddMachine(event)">
        <label>NAME *</label><input id="fMName" class="mono" placeholder="M5" required>
        <label>NOTES</label><textarea id="fMNotes"></textarea>
        <div id="modalError"></div>
        <div class="modal-actions">
          <button type="button" class="btn ghost" onclick="TB.closeModal()">CANCEL</button>
          <button class="btn accent" type="submit">ADD MACHINE</button>
        </div>
      </form>`);
  },
  async submitAddMachine(ev) {
    ev.preventDefault();
    try {
      await apiInModal(api('POST', '/machines', { name: $('#fMName').value.trim(), notes: $('#fMNotes').value }));
      toast('Machine added');
      await route();
    } catch { /* shown in modal */ }
    return false;
  },

  addWorkerModal(machineId) {
    const m = state.board.machines.find((x) => x.id === machineId);
    const nextIdx = m.workers.length + 1;
    openModal(`
      <h2>ADD WORKER — ${esc(m.name)}</h2>
      <div class="modal-sub">New workers default to NO SLOT until an operator explicitly grants capacity.</div>
      <form onsubmit="return TB.submitAddWorker(event,${machineId})">
        <label>NAME *</label><input id="fWName" class="mono" value="W${nextIdx}" required>
        <label>NOTES</label><textarea id="fWNotes"></textarea>
        <div id="modalError"></div>
        <div class="modal-actions">
          <button type="button" class="btn ghost" onclick="TB.closeModal()">CANCEL</button>
          <button class="btn accent" type="submit">ADD WORKER</button>
        </div>
      </form>`);
  },
  async submitAddWorker(ev, machineId) {
    ev.preventDefault();
    try {
      await apiInModal(api('POST', '/workers', { machine_id: machineId, name: $('#fWName').value.trim(), notes: $('#fWNotes').value }));
      toast('Worker added');
      await route();
    } catch { /* shown in modal */ }
    return false;
  },
};

/* ---------------- boot ---------------- */
async function boot() {
  $('#loginForm').addEventListener('submit', async (ev) => {
    ev.preventDefault();
    const box = $('#loginError');
    box.classList.add('hidden');
    try {
      const r = await api('POST', '/auth/login', { username: $('#loginUser').value.trim(), password: $('#loginPass').value });
      state.user = r.user;
      await enterApp();
    } catch (err) {
      box.textContent = err.message;
      box.classList.remove('hidden');
    }
  });
  $('#btnLogout').addEventListener('click', async () => {
    try { await api('POST', '/auth/logout'); } catch { /* ignore */ }
    state.user = null;
    showLogin();
  });
  $('#btnNewTask').addEventListener('click', () => TB.newTaskModal());
  $('#noticeClose').addEventListener('click', () => {
    $('#notice').classList.add('hidden');
    localStorage.setItem('tb_notice', '1');
  });
  if (state.noticeHidden) $('#notice').classList.add('hidden');

  let searchTimer;
  $('#globalSearch').addEventListener('input', (ev) => {
    clearTimeout(searchTimer);
    const q = ev.target.value.trim();
    searchTimer = setTimeout(async () => {
      state.search = q;
      if (!q) { if (!location.hash.startsWith('#/task')) { renderFilterbar(); renderBoard(); } return; }
      if (location.hash !== '#/' && location.hash !== '') location.hash = '#/';
      else await renderSearchPage();
    }, 300);
  });

  window.addEventListener('hashchange', () => route().catch((e) => toast(e.message, true)));

  try {
    const me = await api('GET', '/auth/me');
    state.user = me.user;
    state.openAccess = !!me.openAccess;
    await enterApp();
  } catch {
    showLogin();
  }
}

boot();
