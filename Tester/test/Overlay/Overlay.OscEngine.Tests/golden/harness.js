'use strict';
/*
 * Golden-transcript capture for the osc-compatible controller server. v2
 *
 * Order observed for socket.io-client v2 on engine.io v3 polling:
 *   1) GET  /socket.io/?EIO=3&transport=polling&t=..&X_LOCAL_SECURITY_COOKIE=..   → open packet
 *   2) POST /socket.io/?...&sid=..    body "40"   (namespace CONNECT, client→server)
 *   3) GET  /socket.io/?...&sid=..    (long-poll)  ← server replies "40" (CONNECT ack)
 *   4) GET  ... (long-poll) ← server pushes "42[event,args]" batches / "2" pings
 *   5) POST ... body "3" (engine.io pong) or "42[..]" (client events)
 */
const http = require('http');
const fs = require('fs');
const io = require('socket.io-client');

const steps = [];
const clientEvents = [];

function log(dir, entry) { steps.push(Object.assign({ dir }, entry)); }

const PORT = 4123;
const SECRET = 'GOLDENSECRET42';
let phase = 0;                       // 0 wait-handshake, 1 wait-POST40, 2 send-CONNECT, 3 streaming
let held = null;                     // held long-poll response
const queue = [];                    // packets waiting for next poll

function respond(res, status, body) {
  res.writeHead(status, { 'Content-Type': 'text/plain; charset=UTF-8' });
  res.end(body);
  log('s2c', { kind: 'response', status, body });
}
function encodeBatch(pkts) { return pkts.map(p => p.length + ':' + p).join(''); }
function push(pkts) {
  if (held) { const r = held; held = null; respond(r, 200, encodeBatch(pkts)); }
  else queue.push(...pkts);
}

const server = http.createServer((req, res) => {
  let body = '';
  req.on('data', c => body += c);
  req.on('end', () => {
    log('c2s', { kind: 'request', method: req.method, url: req.url, body });
    if (phase === 0) {
      if (req.method === 'GET') {
        phase = 1;
        const open = '0{"sid":"GOLDENSID","upgrades":[],"pingInterval":2000,"pingTimeout":60000}';
        respond(res, 200, open.length + ':' + open);
      } else { respond(res, 200, 'ok'); }
      return;
    }
    if (req.method === 'POST') {
      // client→server frames: "1:2" engine.io PING (client pings in v3!),
      // "3" pong, "42[..]" events. Respond pong via next poll.
      if (/(^|:|,)2$/.test(body.trim())) queue.push('3');
      respond(res, 200, 'ok');
      return;
    }
    // GET = long-poll
    if (phase === 1) { phase = 2; respond(res, 200, '2:40'); return; }
    if (queue.length > 0) { respond(res, 200, encodeBatch(queue.splice(0))); return; }
    held = res;
    const myRes = res;
    setTimeout(() => { if (held === myRes) { held = null; respond(myRes, 200, '1:6'); } }, 1500);
  });
});

async function waitEvent(ev, timeoutMs) {
  return new Promise((resolve, reject) => {
    const t = setTimeout(() => reject(new Error('timeout waiting ' + ev)), timeoutMs || 8000);
    const iv = setInterval(() => {
      if (clientEvents.some(e => e.ev === ev)) { clearInterval(iv); clearTimeout(t); resolve(); }
    }, 30);
  });
}

async function main() {
  await new Promise(r => server.listen(PORT, '127.0.0.1', r));

  const sio = io('http://127.0.0.1:' + PORT, {
    query: { X_LOCAL_SECURITY_COOKIE: SECRET },
    transports: ['polling'], upgrade: false, reconnection: false, timeout: 8000
  });
  sio.on('connect', () => clientEvents.push({ ev: 'connect' }));
  sio.on('connect_error', (err) => clientEvents.push({ ev: 'connect_error', msg: String(err && err.message) }));
  sio.on('disconnect', (reason) => clientEvents.push({ ev: 'disconnect', reason }));
  sio.on('/ShadowPlay/v.1.0/WindowState', (data) => clientEvents.push({ ev: '/ShadowPlay/v.1.0/WindowState', data }));
  sio.on('/ShadowPlay/v.1.0/Notification', (data) => clientEvents.push({ ev: '/ShadowPlay/v.1.0/Notification', data }));
  sio.on('/ShadowPlay/v.1.0/Hotkey', (data) => clientEvents.push({ ev: '/ShadowPlay/v.1.0/Hotkey', data }));

  await waitEvent('connect');
  await new Promise(r => setTimeout(r, 300));   // let first poll be held

  // push a channel event (the way osc gets windowMsg)
  push(['42["/ShadowPlay/v.1.0/WindowState",{"windowMsg":"overlayToggle"}]']);
  await waitEvent('/ShadowPlay/v.1.0/WindowState');

  // let the client ping (pingInterval=2000) and receive our queued pong "3"
  await new Promise(r => setTimeout(r, 4500));

  // client → server emit (control event)
  sio.emit('/ShadowPlay/v.1.0/Hotkey', { hotkey: 'Alt+F9' });
  await new Promise(r => setTimeout(r, 600));

  // batched push: TWO packets in one poll response
  push([
    '42["/ShadowPlay/v.1.0/Notification",{"id":"n1","title":"Recording saved"}]',
    '42["/ShadowPlay/v.1.0/Hotkey",{"hotkey":"Alt+F10"}]'
  ]);
  await waitEvent('/ShadowPlay/v.1.0/Notification', 10000);
  await waitEvent('/ShadowPlay/v.1.0/Hotkey', 10000);

  sio.close();
  server.close();

  const out = process.argv[2] || 'golden-transcript.json';
  fs.writeFileSync(out, JSON.stringify({
    meta: {
      captured: new Date().toISOString(),
      engineio_client: require('engine.io-client/package.json').version,
      socketio_client: require('socket.io-client/package.json').version,
      secret: SECRET
    },
    steps, clientEvents
  }, null, 2));
  console.log('clientEvents:', JSON.stringify(clientEvents));
  console.log('steps:', steps.length, '→', out);
  process.exit(0);
}

main().catch(e => {
  console.error('HARNESS FAIL:', e.message);
  try { fs.writeFileSync('fail-transcript.json', JSON.stringify({ steps, clientEvents }, null, 2)); } catch (_) {}
  process.exit(1);
});
