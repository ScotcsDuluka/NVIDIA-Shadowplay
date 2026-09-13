#!/usr/bin/env node
/**
 * Golden-vector test for the NEW UI socket client (app/bridge/socket.js).
 *
 * Vectors come from the measured transcript captured with the REAL client
 * libraries (Tester/test/Overlay/Overlay.OscEngine.Tests/golden/
 * golden-transcript.json — engine.io-client@3.5.4 + socket.io-client@2.5.0):
 *
 *   1. handshake GET  /socket.io/?X_LOCAL_SECURITY_COOKIE=<s>&EIO=3&transport=polling&t=…&b64=1
 *   2. open response  <len>:0{"sid":…,"upgrades":[],"pingInterval":…,"pingTimeout":…}
 *   3. first poll     → server CONNECT "40"          (v2 client never sends it)
 *   4. event batch    → <len>:42["<channel>",<payload>]
 *   5. client ping    POST "1:2" → server answers "3" in a later poll
 *   6. emit           POST <len>:42["<channel>",<payload>]
 *   7. transport failure → client reconnects (1s→5s backoff)
 *
 * Usage: node tools/osc-reui/test-socket-client.mjs
 */

import http from 'node:http';
import assert from 'node:assert/strict';
import { createSocketClient } from '../../Overlay/osc/next/app/bridge/socket.js';

const PORT = 8199;
const SECRET = 'GOLDENSECRET42';
const events = [];

function frame(p) { return `${p.length}:${p}`; }
function eventPacket(channel, payload) {
    const p = `42${JSON.stringify([channel, payload])}`;
    return frame(p);
}

const server = http.createServer((req, res) => {
    const url = new URL(req.url, `http://127.0.0.1:${PORT}`);
    const fail = (code, body) => { res.writeHead(code); res.end(body); };

    if (url.pathname !== '/socket.io/') return fail(404, '');
    if (url.searchParams.get('X_LOCAL_SECURITY_COOKIE') !== SECRET) return fail(401, 'unauthorized');

    // 1. handshake URL shape
    events.push(['handshake', url.search]);
    if (!url.searchParams.get('EIO')?.startsWith('3')) return fail(400, 'bad EIO');
    if (url.searchParams.get('transport') !== 'polling') return fail(400, 'bad transport');
    if (url.searchParams.get('b64') !== '1') return fail(400, 'bad b64');

    const sid = url.searchParams.get('sid');
    if (req.method === 'GET' && !sid) {
        const open = `0${JSON.stringify({ sid: 'TESTSID', upgrades: [], pingInterval: 1000, pingTimeout: 60000 })}`;
        res.writeHead(200, { 'Content-Type': 'text/plain' });
        return res.end(frame(open)); // 2. length-prefixed open
    }
    if (req.method === 'GET' && sid) {
        events.push(['poll', url.search]);
        if (!server.sawConnect) {
            server.sawConnect = true;
            res.writeHead(200, { 'Content-Type': 'text/plain' });
            return res.end(frame('40')); // 3. server-side CONNECT
        }
        if (server.queued.length) {
            const batch = server.queued.join('');
            server.queued = [];
            res.writeHead(200, { 'Content-Type': 'text/plain' });
            return res.end(batch);
        }
        if (server.queuedPongs.length) {
            const batch = server.queuedPongs.join('');
            server.queuedPongs = [];
            res.writeHead(200, { 'Content-Type': 'text/plain' });
            return res.end(batch);
        }
        // hold, then NOOP
        setTimeout(() => {
            res.writeHead(200, { 'Content-Type': 'text/plain' });
            res.end(frame('6'));
        }, 500);
        return;
    }
    if (req.method === 'POST') {
        let body = '';
        req.on('data', (c) => { body += c; });
        req.on('end', () => {
            events.push(['post', body]);
            // 5. client PING "1:2" → answer PONG in a later poll
            if (body === '1:2') server.queuedPongs.push(frame('3'));
            // 6. emit → record the packet
            if (body.includes(':42')) server.emits.push(body);
            res.writeHead(200, { 'Content-Type': 'text/plain' });
            res.end('ok');
        });
        return;
    }
    fail(400, 'bad request');
});
server.sawConnect = false;
server.queued = [];
server.queuedPongs = [];
server.emits = [];

async function listen() {
    await new Promise((r) => server.listen(PORT, '127.0.0.1', r));
}

async function waitFor(fn, ms = 8000, label = 'condition') {
    const start = Date.now();
    while (Date.now() - start < ms) {
        if (fn()) return;
        await new Promise((r) => setTimeout(r, 25));
    }
    throw new Error(`timeout waiting for ${label}`);
}

async function main() {
    await listen();
    let failed = 0;

    // kill switch to end the client's ping/reconnect cycles between phases
    let killSwitch = false;
    const client = createSocketClient({
        baseUrl: `http://127.0.0.1:${PORT}`,
        secret: SECRET,
        log: (...a) => { if (process.env.VERBOSE) console.log('[client]', ...a); },
    });
    const origFetch = globalThis.fetch;

    let sawConnect = false;
    let gotEvent = null;
    client.on('connect', () => { sawConnect = true; });
    client.on('/ShadowPlay/v.1.0/WindowState', (payload) => { gotEvent = payload; });

    client.connect();
    await waitFor(() => sawConnect, 8000, 'connect');
    console.log('PASS  connect after server-side "40"');

    server.queued.push(eventPacket('/ShadowPlay/v.1.0/WindowState', { windowMsg: 'overlayToggle' }));
    await waitFor(() => gotEvent !== null, 8000, 'channel event');
    assert.equal(gotEvent.windowMsg, 'overlayToggle');
    console.log('PASS  channel event decoded from length-prefixed batch');

    await waitFor(() => events.some((e) => e[0] === 'post' && e[1] === '1:2'), 8000, 'client ping');
    console.log('PASS  client pings "1:2" (engine.io v3 direction)');

    client.emit('/ShadowPlay/v.1.0/Hotkey', { key: 'Alt+Z' });
    await waitFor(() => server.emits.length > 0, 8000, 'emit POST');
    const emitBody = server.emits[0];
    const colon = emitBody.indexOf(':');
    const len = parseInt(emitBody.slice(0, colon), 10);
    const packet = emitBody.slice(colon + 1);
    assert.equal(len, packet.length, 'emit body must be length-prefixed');
    const parsed = JSON.parse(packet.slice(2));
    assert.equal(parsed[0], '/ShadowPlay/v.1.0/Hotkey');
    assert.deepEqual(parsed[1], { key: 'Alt+Z' });
    console.log('PASS  emit sends <len>:42["channel",payload]');

    // 7. transport failure → reconnect with backoff
    sawConnect = false;
    server.sawConnect = false;
    server.closeAllConnections?.();
    server.close();
    // restart fresh server, client must re-handshake
    server.queued = [];
    server.emits = [];
    await new Promise((r) => server.listen(PORT, '127.0.0.1', r));
    await waitFor(() => sawConnect, 15000, 'reconnect');
    console.log('PASS  reconnects after transport failure (bounded backoff)');

    client.disconnect();
    server.closeAllConnections?.();
    server.close();
    console.log('\nALL SOCKET-CLIENT GOLDEN TESTS PASS');
    process.exit(failed);
}

main().catch((err) => {
    console.error('FAIL', err.message);
    process.exit(1);
});
