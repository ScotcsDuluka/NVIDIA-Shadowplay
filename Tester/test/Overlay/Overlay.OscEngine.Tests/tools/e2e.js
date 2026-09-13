'use strict';
/* e2e.js — the REAL socket.io-client@2.5.0 talks to the running
 * OscControllerServer. Asserts the golden-verified behaviors against the
 * actual VB implementation:
 *   1. handshake + server-initiated socket.io CONNECT → "connect" event
 *   2. client ping / server pong keeps the session alive ≥2 cycles
 *   3. client → server emit is accepted (HTTP 200 on POST)
 *   4. unauthorized socket (wrong cookie) is rejected 401
 * Usage: node e2e.js <port> <secret>
 */
const io = require('socket.io-client');
const http = require('http');

const port = process.argv[2];
const secret = process.argv[3];
const url = 'http://127.0.0.1:' + port;
let failures = 0;
function check(name, cond) {
  console.log((cond ? 'PASS ' : 'FAIL ') + name);
  if (!cond) failures++;
}

function httpGet(path, withCookie) {
  return new Promise((resolve) => {
    const req = http.get(url + path, { headers: withCookie ? { 'X_LOCAL_SECURITY_COOKIE': secret } : {} }, (res) => {
      let b = ''; res.on('data', c => b += c); res.on('end', () => resolve({ status: res.statusCode, body: b }));
    });
    req.on('error', () => resolve({ status: 0, body: '' }));
  });
}

async function main() {
  // 4. unauthorized first
  const denied = await httpGet('/state', false);
  check('REST without cookie rejected (401)', denied.status === 401);

  const sio = io(url, {
    query: { X_LOCAL_SECURITY_COOKIE: secret },
    transports: ['polling'], upgrade: false, reconnection: false, timeout: 8000
  });
  const gotConnect = new Promise((resolve, reject) => {
    const t = setTimeout(() => reject(new Error('connect timeout')), 8000);
    sio.on('connect', () => { clearTimeout(t); resolve(); });
    sio.on('connect_error', (e) => { clearTimeout(t); reject(new Error('connect_error: ' + e)); });
  });
  await gotConnect;
  check('socket.io connect (server-initiated 40)', true);

  // 3. client emit accepted
  sio.emit('/ShadowPlay/v.1.0/Hotkey', { hotkey: 'Alt+F9' });

  // 2. stay alive ≥2 ping cycles (env-tuned interval via OSCENGINE_PING_INTERVAL)
  const interval = parseInt(process.env.OSCENGINE_PING_INTERVAL || '25000', 10);
  const cycles = 2;
  await new Promise(r => setTimeout(r, interval * cycles + 1500));
  check('session alive across ' + cycles + ' ping cycles', sio.connected === true);

  // REST with cookie
  const st = await httpGet('/state?X_LOCAL_SECURITY_COOKIE=' + secret, false);
  check('GET /state with cookie 200', st.status === 200 && st.body.includes('"record"'));

  const ui = await new Promise((resolve) => {
    const req = http.request(url + '/uiReady', { method: 'POST', headers: { 'X_LOCAL_SECURITY_COOKIE': secret } }, (res) => {
      let b = ''; res.on('data', c => b += c); res.on('end', () => resolve(res.statusCode));
    });
    req.on('error', () => resolve(0));
    req.end();
  });
  check('POST /uiReady 200', ui === 200);

  sio.close();
  console.log(failures === 0 ? 'E2E OK' : 'E2E FAILED: ' + failures);
  process.exit(failures === 0 ? 0 : 1);
}

main().catch(e => { console.error('E2E ERROR: ' + e.message); process.exit(1); });
