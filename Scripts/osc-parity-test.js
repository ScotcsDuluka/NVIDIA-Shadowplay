// Live parity test: osc-page HTTP surface + socket notification channels.
// usage: node parity-test.js <port>
const fs = require('fs');
const http = require('http');
const _ioci = require('socket.io-client');
const io = (typeof _ioci === 'function') ? _ioci : _ioci.io;
const LOG = 'C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\parity-result.txt';
const lines = [];
function out(s) { lines.push(s); try { fs.writeFileSync(LOG, lines.join('\r\n')); } catch (e) {} }

const port = process.argv[2] || '59002';
const base = 'http://127.0.0.1:' + port;
out('STARTED base=' + base);
const received = [];

const socket = io(base, { transports: ['websocket'], reconnection: false, timeout: 8000 });
socket.on('connect', () => out('EVENT:connect sid=' + socket.id));
socket.on('connect_error', err => out('EVENT:connect_error ' + err));
socket.on('disconnect', reason => out('EVENT:disconnect ' + reason));
setTimeout(() => {
    if (!lines.some(l => l.indexOf('EVENT:connect') === 0)) {
        out('GUARD: no connect within 8s — exiting');
        fs.writeFileSync(LOG, lines.join('\r\n'));
        process.exit(3);
    }
}, 8000);

const channels = [
    '/ShadowPlay/v.1.0/DisplayOscState',
    '/ShadowPlay/v.1.0/DisplayOscPreferences',
    '/ShadowPlay/v.1.0/DisplayOscNotification',
    '/ShadowPlay/v.1.0/WindowState',
    '/ShadowPlay/v.1.0/InstantReplay/Enable',
    '/ShadowPlay/v.1.0/Record/Enable',
    '/ShadowPlay/v.1.0/Hotkey',
];
channels.forEach(ch => socket.on(ch, data => received.push({ ch, data })));

function post(path, body) {
    return new Promise((resolve, reject) => {
        const payload = JSON.stringify(body || {});
        const req = http.request(base + path, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Content-Length': Buffer.byteLength(payload) },
        }, res => {
            let buf = '';
            res.on('data', d => buf += d);
            res.on('end', () => resolve({ code: res.statusCode, body: buf }));
        });
        req.on('error', reject);
        req.end(payload);
    });
}
function get(path) {
    return new Promise((resolve, reject) => {
        http.get(base + path, res => {
            let buf = '';
            res.on('data', d => buf += d);
            res.on('end', () => resolve({ code: res.statusCode, body: buf }));
        }).on('error', reject);
    });
}
const wait = ms => new Promise(r => setTimeout(r, ms));

(async () => {
    await new Promise(r => socket.on('connect', r));
    out('CONNECTED to ' + base);

    const checks = [];
    function check(name, cond, detail) {
        checks.push({ name, ok: !!cond, detail: detail || '' });
        out((cond ? 'PASS ' : 'FAIL ') + name + (detail ? '  [' + detail + ']' : ''));
    }

    // 1. OpenOscState -> DisplayOscState echo
    await post('/ShadowPlay/v.1.0/OpenOscState', { foo: 'bar-state' });
    await wait(250);
    check('OpenOscState -> DisplayOscState', received.some(r => r.ch === '/ShadowPlay/v.1.0/DisplayOscState' && r.data && r.data.foo === 'bar-state'));

    // 2. OpenOscPreferences -> DisplayOscPreferences
    await post('/ShadowPlay/v.1.0/OpenOscPreferences', { pref: 1 });
    await wait(250);
    check('OpenOscPreferences -> DisplayOscPreferences', received.some(r => r.ch === '/ShadowPlay/v.1.0/DisplayOscPreferences' && r.data && r.data.pref === 1));

    // 3. OscNotification -> DisplayOscNotification
    await post('/ShadowPlay/v.1.0/OscNotification', { note: 'hi' });
    await wait(250);
    check('OscNotification -> DisplayOscNotification', received.some(r => r.ch === '/ShadowPlay/v.1.0/DisplayOscNotification' && r.data && r.data.note === 'hi'));

    // 4. Hotkey/Toggle -> WindowState{overlayToggle}
    await post('/ShadowPlay/v.1.0/Hotkey/Toggle', {});
    await wait(250);
    check('Hotkey/Toggle -> WindowState{overlayToggle}', received.some(r => r.ch === '/ShadowPlay/v.1.0/WindowState' && r.data && r.data.windowMsg === 'overlayToggle'));

    // 5. InstantReplay/Enable POST {status:true} -> channel echo
    const ir = await post('/ShadowPlay/v.1.0/InstantReplay/Enable', { status: true });
    await wait(250);
    check('InstantReplay/Enable POST 200', ir.code === 200, 'code=' + ir.code);
    check('InstantReplay/Enable -> channel echo', received.some(r => r.ch === '/ShadowPlay/v.1.0/InstantReplay/Enable' && r.data && r.data.status === true));
    const irGet = await get('/ShadowPlay/v.1.0/InstantReplay/Enable');
    check('InstantReplay/Enable GET returns status', irGet.code === 200 && /status/.test(irGet.body), irGet.body.slice(0, 60));

    // 6. Record/Enable POST {status:true} -> channel echo
    const rec = await post('/ShadowPlay/v.1.0/Record/Enable', { status: true });
    await wait(250);
    check('Record/Enable POST 200', rec.code === 200, 'code=' + rec.code);
    check('Record/Enable -> channel echo', received.some(r => r.ch === '/ShadowPlay/v.1.0/Record/Enable' && r.data && r.data.status === true));
    const recGet = await get('/ShadowPlay/v.1.0/Record/Enable');
    check('Record/Enable GET returns status', recGet.code === 200 && /status/.test(recGet.body), recGet.body.slice(0, 60));

    // 7. toggle debounce: two rapid Hotkey/Toggle -> only one WindowState within cooldown
    await post('/ShadowPlay/v.1.0/Hotkey/Toggle', {});
    const wsCount = received.filter(r => r.ch === '/ShadowPlay/v.1.0/WindowState').length;
    check('Hotkey debounce (single toggle per press)', wsCount >= 1, 'WindowState seen x' + wsCount);

    const failed = checks.filter(c => !c.ok).length;
    out('----------------------------------------');
    out('RESULT: ' + (checks.length - failed) + ' passed, ' + failed + ' failed, ' + checks.length + ' total');
    out('DONE');
    process.exit(failed ? 1 : 0);
})().catch(err => { out('TEST-CRASH: ' + (err && err.stack ? err.stack : err)); process.exit(2); });
