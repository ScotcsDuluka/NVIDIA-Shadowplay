// Status transition rules - the single authority for what may change into what.
// Status semantics (do not redefine):
//   WORKING  - worker is actually executing a task right now
//   ACTIVE   - worker is reserved/designated for use, not necessarily processing
//   END TASK - chat/worker is free, ready to receive a new task
//   NO SLOT  - no worker slot available
//   STANDBY  - machine/worker exists but is not currently in use
//   CLOSED   - machine/worker permanently closed / not allowed to be used

export const WORKER_STATUSES = ['WORKING', 'ACTIVE', 'END TASK', 'NO SLOT', 'STANDBY', 'CLOSED'];

export const WORKER_TRANSITIONS = {
  'WORKING':  ['ACTIVE', 'END TASK', 'NO SLOT', 'CLOSED'],
  'ACTIVE':   ['WORKING', 'END TASK', 'NO SLOT', 'CLOSED'],
  'END TASK': ['ACTIVE', 'STANDBY', 'NO SLOT', 'CLOSED'],
  'NO SLOT':  ['END TASK', 'STANDBY', 'CLOSED'],
  'STANDBY':  ['END TASK', 'ACTIVE', 'NO SLOT', 'CLOSED'],
  // CLOSED is terminal by definition; reopening is an explicit admin decision.
  'CLOSED':   ['STANDBY'],
};

export const MACHINE_STATUSES = ['ONLINE', 'OFFLINE', 'STANDBY', 'CLOSED'];

export const MACHINE_TRANSITIONS = {
  'ONLINE':  ['STANDBY', 'OFFLINE', 'CLOSED'],
  'OFFLINE': ['ONLINE', 'STANDBY', 'CLOSED'],
  'STANDBY': ['ONLINE', 'OFFLINE', 'CLOSED'],
  'CLOSED':  ['STANDBY'],
};

export const TASK_STATUSES = ['CREATED', 'ASSIGNED', 'IN_PROGRESS', 'BLOCKED', 'COMPLETED', 'CANCELLED'];

export const TASK_TRANSITIONS = {
  'CREATED':     ['ASSIGNED', 'CANCELLED'],
  'ASSIGNED':    ['IN_PROGRESS', 'CREATED', 'CANCELLED'],
  'IN_PROGRESS': ['ASSIGNED', 'BLOCKED', 'COMPLETED', 'CANCELLED'],
  'BLOCKED':     ['IN_PROGRESS', 'ASSIGNED', 'CANCELLED'],
  'COMPLETED':   ['CREATED'],
  'CANCELLED':   ['CREATED'],
};

export function assertTransition(map, from, to, kind) {
  if (from === to) {
    const err = new Error(`${kind} is already ${from}`);
    err.status = 400;
    err.expose = true;
    throw err;
  }
  const allowed = map[from];
  if (!allowed) {
    const err = new Error(`Unknown ${kind.toLowerCase()} status: ${from}`);
    err.status = 400;
    err.expose = true;
    throw err;
  }
  if (!allowed.includes(to)) {
    const err = new Error(
      `Invalid ${kind.toLowerCase()} transition ${from} -> ${to}. Allowed: ${allowed.join(', ')}` +
      (from === to ? '' : '')
    );
    err.status = 400;
    err.expose = true;
    err.allowed = allowed;
    throw err;
  }
}
