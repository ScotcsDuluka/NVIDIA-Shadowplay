#!/usr/bin/env node
/**
 * osc-reui test harness — TEST TOOLING ONLY, never shipped.
 *
 * Implements the SAME page-facing contract as the production host
 * (Overlay.Engine/OscControllerServer.vb + CefQueryBridge.vb) plus a
 * GFE-backend-style REST simulation so BOTH the legacy page (index.html)
 * and the new UI (/next/index.html) can be driven against one backend and
 * behaviorally compared (W2 regression).
 *
 *   static        /next/… , /l10n/… , legacy files (cefQuery polyfill injected
 *                 as the first script — AddScriptToExecuteOnDocumentCreated parity)
 *   cefQuery      POST /__cef → CefQueryBridge dispatch mirror
 *   REST          M1 routes + simulated GFE routes (legacy service-prefixed
 *                 paths are normalized onto the same handlers)
 *   socket.io v2  /socket.io/ → engine.io v3 polling (golden-transcript framing)
 *   scenarios     POST /__scenario/<name>
 *   transcript    GET /__transcript — every rest/cef/sio-emit/push entry
 */

import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const OSC_ROOT = path.resolve(__dirname, '../../Overlay/osc');
const PORT = Number(process.env.HARNESS_PORT || 8123);
const SECRET = process.env.HARNESS_SECRET || 'HARNESSSECRET1';
// real socket.io@2.5.0 server lib (same family as the NVIDIA Web Helper)
const req = process.env.DEPS_DIR
    ? createRequire(path.join(process.env.DEPS_DIR, 'socket.io', 'package.json'))
    : null;

// ── engine state (OscHostForm fields) + transcript ───────────────────────
const engine = {
    recording: false,
    replay: false,
    elapsedSec: 0,
    displayRects: [],
    painting: false,
    bridgeDown: false,
    log: [],
    transcript: [],
};
const T0 = Date.now();
function T(entry) {
    entry.t = Date.now() - T0;
    engine.transcript.push(entry);
}
const storage = new Map();
let elapsedTimer = null;
let closeWaiter = null; // held /__cef response for REGISTER_CLOSE_EVENT

function stateJson() {
    return JSON.stringify({
        record: engine.recording,
        instantReplay: engine.replay,
        broadcast: false,
        elapsedSec: engine.elapsedSec,
    });
}
function startRecording() {
    engine.recording = true;
    if (!elapsedTimer) elapsedTimer = setInterval(() => { engine.elapsedSec += 1; }, 1000);
}
function stopRecording() {
    engine.recording = false;
    engine.elapsedSec = 0;
    clearInterval(elapsedTimer);
    elapsedTimer = null;
}
function savedFile(prefix) {
    return `${prefix}_${new Date().toISOString().slice(0, 19).replace(/[-:T]/g, '-')}.mp4`;
}

// ── cefQuery dispatch (mirror of CefQueryBridge.Dispatch) ───────────────
const okR = (response) => ({ ok: true, response });
const failR = (errorCode, response) => ({ ok: false, errorCode, response });

function cefDispatch(req) {
    switch (req.command) {
        case 'QUERY_WIN_NODE_INFO':
            return okR(JSON.stringify({ port: PORT, secret: SECRET }));
        case 'QUERY_FULLSCREEN_STATE':
            return okR(JSON.stringify({ fullscreen: false, hdractive: false, borderlessMode: null }));
        case 'QUERY_OSC_DISPLAY_IS_DESKTOP_MODE':
            return okR('false');
        case 'QUERY_OSC_SET_DISPLAY_RECTS':
            engine.displayRects = Array.isArray(req.displayRects) ? req.displayRects : [];
            engine.log.push(`displayRects=${engine.displayRects.length}`);
            return okR('true');
        case 'QUERY_OSC_SET_PAINTING':
            engine.painting = !!req.enablePainting;
            return okR('true');
        case 'QUERY_OSC_SET_EXPERIMENTAL':
            return okR('true');
        case 'QUERY_OSC_REGISTER_CLOSE_EVENT':
            // HOST PARITY: no immediate answer — hold until /__scenario/close-push
            engine.log.push('registerCloseEvent');
            return null;
        case 'QUERY_WIN_OPEN_OSC':
            engine.log.push(`openOsc(${req.enableInput})`);
            return okR('true');
        case 'QUERY_WIN_CLOSE_OSC':
            engine.log.push('closeOsc');
            return okR('true');
        case 'QUERY_READ_SHARED_STORAGE':
            return okR(storage.get(req.path) ?? '');
        case 'QUERY_WRITE_SHARED_STORAGE':
            storage.set(req.path, req.data ?? '');
            return okR('true');
        case 'QUERY_LOAD_STRING_TABLE':
            return okR('true');
        case 'QUERY_WIN_COPY_TO_CLIPBOARD':
            engine.log.push('clipboard');
            return okR('true');
        case 'QUERY_HTTPSERVER_START':
            return failR(-1, 'oauth_not_implemented');
        default:
            engine.log.push(`cef not implemented: ${req.command}`);
            return failR(-1, 'not_implemented');
    }
}

// ── cefQuery browser polyfill (document-created parity) ─────────────────
const CEF_POLYFILL = `<script>
(function(){
  if (window.cefQuery) return;
  // pre-bundle XHR trace (W2 diagnostics): every socket.io request the page makes
  window.__xhrLog = [];
  window.__console = [];
  (function(){
    ['log','info','warn','error'].forEach(function(k){
      var orig = console[k].bind(console);
      console[k] = function(){
        try {
          var a = Array.prototype.slice.call(arguments).map(function(x){
            try { return typeof x === 'string' ? x : JSON.stringify(x).slice(0,120); } catch(e){ return String(x); }
          }).join(' ');
          window.__console.push(k + ': ' + a.slice(0, 160));
          if (window.__console.length > 120) window.__console.shift();
        } catch(e){}
        return orig.apply(null, arguments);
      };
    });
  })();
  (function(){
    var O = XMLHttpRequest.prototype.open, S = XMLHttpRequest.prototype.send;
    XMLHttpRequest.prototype.open = function(m, u){ this.__m=m; this.__u=String(u); return O.apply(this, arguments); };
    XMLHttpRequest.prototype.send = function(){
      var x = this, url = this.__u || '';
      if (url.indexOf('/socket.io/') >= 0) {
        var entry = { m: this.__m, u: url.slice(url.indexOf('/socket.io'), url.indexOf('/socket.io') + 60) };
        x.addEventListener('load', function(){ entry.st = x.status; entry.body = (x.responseText||'').slice(0, 60); window.__xhrLog.push(entry); if (window.__xhrLog.length>40) window.__xhrLog.shift(); });
        x.addEventListener('error', function(){ entry.st = 'ERR'; window.__xhrLog.push(entry); if (window.__xhrLog.length>40) window.__xhrLog.shift(); });
      }
      return S.apply(this, arguments);
    };
  })();
  var seq=0;
  window.cefQuery=function(o){
    var id=++seq;
    var xhr=new XMLHttpRequest();
    xhr.open('POST','/__cef',true);
    xhr.setRequestHeader('Content-Type','application/json');
    xhr.onload=function(){
      try{
        var m=JSON.parse(xhr.responseText);
        if(m.ok){ o.onSuccess && o.onSuccess(m.response); }
        else { o.onFailure && o.onFailure(m.errorCode, m.response||''); }
      }catch(e){ o.onFailure && o.onFailure(-3, String(e)); }
    };
    xhr.onerror=function(){ o.onFailure && o.onFailure(-2,'network'); };
    xhr.send(JSON.stringify({__cef:1,id:id,request:(typeof o.request==='string'?o.request:JSON.stringify(o.request||{})),persistent:!!o.persistent}));
    return {id:id,cancel:function(){}};
  };
})();
</script>`;

// ── socket.io v2 server (REAL socket.io@2.5.0 lib — same family as the
//    NVIDIA Web Helper server, zero protocol drift) ────────────────────────
let io = null;
function attachRealSocketIo(httpServer) {
    io = req('socket.io')(httpServer, {
        transports: ['polling', 'websocket'], // WS enabled for legacy-client test stability (M1 host is polling-only; documented in regression report)
        origins: '*:*',
        pingInterval: 25000,
        pingTimeout: 60000,
    });
    // real-server parity: security cookie checked at handshake (index.js:216)
    io.use((socket, next) => {
        if (socket.handshake.query.X_LOCAL_SECURITY_COOKIE !== SECRET) {
            next(new Error('Security token is invalid'));
            return;
        }
        next();
    });
    io.on('connection', (sock) => {
        const q = sock.handshake.query;
        engine.log.push(`sio CONN origin=${sock.handshake.headers.origin} ua=${(sock.handshake.headers['user-agent'] || '').slice(0, 30)} cookie=${q.X_LOCAL_SECURITY_COOKIE ? 'ok' : 'MISSING'} sid=${sock.id.slice(0, 6)}`);
        sock.on('disconnect', (r) => engine.log.push('socket.io disconnect ' + sock.id + ' ' + r));
        sock.use((packet, next) => {
            if (packet[0]?.startsWith('42')) T({ kind: 'sio-emit', packet: JSON.stringify(packet).slice(0, 160) });
            next();
        });
    });
}
function pushToAll(channel, payload) {
    engine.log.push(`push ${channel} ${JSON.stringify(payload)}`);
    T({ kind: 'push', channel, payload });
    if (io) io.emit(channel, payload);
    else engine.log.push('push skipped: socket.io not attached');
}

// ── static ───────────────────────────────────────────────────────────────
const MIME = {
    '.html': 'text/html; charset=UTF-8', '.js': 'application/javascript; charset=UTF-8',
    '.css': 'text/css; charset=UTF-8', '.json': 'application/json; charset=UTF-8',
    '.png': 'image/png', '.svg': 'image/svg+xml', '.woff2': 'font/woff2', '.ico': 'image/x-icon',
};

function serveStatic(rel, res) {
    const full = path.resolve(OSC_ROOT, rel);
    if (!full.startsWith(OSC_ROOT) || !fs.existsSync(full) || !fs.statSync(full).isFile()) {
        res.writeHead(404); res.end('not found'); return;
    }
    const type = MIME[path.extname(full).toLowerCase()] ?? 'application/octet-stream';
    if (type.startsWith('text/html')) {
        const html = fs.readFileSync(full, 'utf8').replace(/<head(\s[^>]*)?>/, (m) => m + CEF_POLYFILL);
        res.writeHead(200, { 'Content-Type': type });
        res.end(html);
    } else {
        res.writeHead(200, { 'Content-Type': type });
        res.end(fs.readFileSync(full));
    }
}

function readBody(req) {
    return new Promise((resolve) => {
        let b = '';
        req.on('data', (c) => { b += c; });
        req.on('end', () => resolve(b));
    });
}

const json = (res, body = '{}', status = 200) =>
    res.writeHead(status, { 'Content-Type': 'application/json' }).end(body);

// ── server ───────────────────────────────────────────────────────────────
const server = http.createServer(async (req, res) => {
    const url = new URL(req.url, `http://127.0.0.1:${PORT}`);
    const p = url.pathname;
    // TEST-ONLY CORS: legacy osc calls http://localhost:<port> from a
    // http://127.0.0.1:<port> origin (LOCALHOST_ADDR literal). The production
    // host is same-origin and needs none of this.
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Headers', 'X_LOCAL_SECURITY_COOKIE, Content-Type');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
    if (req.method === 'OPTIONS') { res.writeHead(204); res.end(); return; }
    // /socket.io/* is owned by the real socket.io server — never touch it here
    if (p.startsWith('/socket.io/')) return;
    try {
        if (p === '/__cef' && req.method === 'POST') {
            if (engine.bridgeDown) {
                return json(res, '{}', 500); // scenario: bridge unavailable
            }
            const body = await readBody(req);
            let cmd = {};
            try { cmd = JSON.parse(JSON.parse(body).request); } catch { cmd = {}; }
            T({ kind: 'cef', command: cmd.command ?? '(none)',
                ...(cmd.command === 'QUERY_OSC_SET_DISPLAY_RECTS' ? { displayRects: cmd.displayRects } : {}),
                ...(cmd.command === 'QUERY_WRITE_SHARED_STORAGE' ? { path: cmd.path } : {}) });
            const reply = cefDispatch(cmd);
            if (reply === null) {
                closeWaiter = res; // hold like the host (persistent close event)
                return;
            }
            return json(res, JSON.stringify(reply));
        }

        if (p.startsWith('/__scenario/') && req.method === 'POST') {
            const name = p.slice('/__scenario/'.length);
            const body = await readBody(req);
            switch (name) {
                case 'engine-state': {
                    const s = JSON.parse(body || '{}');
                    if (typeof s.recording === 'boolean') s.recording ? startRecording() : stopRecording();
                    if (typeof s.replay === 'boolean') engine.replay = s.replay;
                    if (typeof s.elapsedSec === 'number') engine.elapsedSec = s.elapsedSec;
                    break;
                }
                case 'record-started':
                    startRecording();
                    pushToAll('/ShadowPlay/v.1.0/Record/Enable', { status: true });
                    break;
                case 'record-stopped':
                    stopRecording();
                    pushToAll('/ShadowPlay/v.1.0/Record/Enable', { status: false });
                    break;
                case 'notification': {
                    const n = JSON.parse(body || '{"title":"Recording saved","message":"Clip.mp4"}');
                    pushToAll('/ShadowPlay/v.1.0/Notification', n);
                    break;
                }
                case 'recording-saved':
                    pushToAll('/ShadowPlay/v.1.0/Notification',
                        { notification: 'recordingSaved', result: 0, file: savedFile('Record') });
                    break;
                case 'fs-transition':
                    pushToAll('/ShadowPlay/v.1.0/WindowState', { windowMsg: 'fullscreenTransition' });
                    break;
                case 'ir-enabled':
                    engine.replay = true;
                    pushToAll('/ShadowPlay/v.1.0/InstantReplay/Enable', { status: true });
                    pushToAll('/ShadowPlay/v.1.0/InstantReplay/Started', { started: true });
                    break;
                case 'ir-disabled':
                    engine.replay = false;
                    pushToAll('/ShadowPlay/v.1.0/InstantReplay/Enable', { status: false });
                    break;
                case 'overlay-toggle':
                    pushToAll('/ShadowPlay/v.1.0/WindowState', { windowMsg: 'overlayToggle' });
                    break;
                case 'dismiss':
                    pushToAll('/ShadowPlay/v.1.0/WindowState', { windowMsg: 'dismiss' });
                    break;
                case 'bridge-down':
                    engine.bridgeDown = true;
                    engine.log.push('bridge DOWN (scenario)');
                    break;
                case 'bridge-up':
                    engine.bridgeDown = false;
                    engine.log.push('bridge UP (scenario)');
                    break;
                case 'transcript-reset':
                    engine.transcript = [];
                    break;
                case 'close-push':
                    // host-parity: complete the held persistent REGISTER_CLOSE_EVENT
                    if (closeWaiter) {
                        const held = closeWaiter;
                        closeWaiter = null;
                        held.writeHead(200, { 'Content-Type': 'application/json' });
                        held.end(JSON.stringify(okR('true')));
                    }
                    engine.log.push('close pushed');
                    break;
            }
            return json(res);
        }

        if (p === '/__transcript') {
            return json(res, JSON.stringify({ transcript: engine.transcript }));
        }

        if (p === '/__inspect') {
            return json(res, JSON.stringify({
                engine: {
                    recording: engine.recording,
                    replay: engine.replay,
                    elapsedSec: engine.elapsedSec,
                    displayRects: engine.displayRects,
                    painting: engine.painting,
                    closeEventRegistered: closeWaiter !== null,
                    sessions: io ? Object.keys(io.sockets.sockets).length : 0,
                },
                log: engine.log.slice(-100),
            }));
        }

        // static GETs are cookie-free (host parity)
        const isStaticGet = req.method === 'GET' && (p === '/' || /\.[a-z0-9]+$/i.test(p));
        if (isStaticGet) {
            const rel = p === '/' ? 'next/index.html' : p.replace(/^\//, '');
            return serveStatic(rel, res);
        }

        // REST: read POST body once (for transcript + routing)
        const restBody = req.method === 'POST' ? await readBody(req) : undefined;
        const provided = req.headers['x_local_security_cookie'];
        T({ kind: 'rest', method: req.method, path: p,
            ...(restBody !== undefined ? { body: restBody.slice(0, 240) } : {}),
            auth: provided === SECRET });
        if (provided !== SECRET) return json(res, '{}', 401);

        // normalize legacy service-prefixed paths onto shared handlers
        let route = p;
        const pm = p.match(/^\/(?:ShadowPlay|Highlights|Settings)\/v\.[0-9.]+(\/.*)$/);
        if (pm) route = pm[1];
        if (route.startsWith('/Capture/ProcessInfo/')) {
            return json(res, JSON.stringify({ profileName: 'TestGame', pid: 4242 }));
        }

        switch (route) {
            case '/state':
                return json(res, stateJson());
            case '/Record/Settings':
                return json(res, req.method === 'POST' ? '{}' : JSON.stringify({
                    quality: 'custom', resolution: '2560x1440', framerate: '60',
                    bitrateBps: '50000000',
                }));
            case '/RecordPaths':
                return json(res, JSON.stringify({ savePath: 'C:\\Users\\Demo\\Videos\\ShadowPlay' }));
            case '/Record/Enable':
                if (req.method === 'POST') {
                    const enable = (restBody ?? '').toLowerCase().includes('true');
                    engine.log.push(`Record/Enable → ${enable}`);
                    setTimeout(() => (enable ? startRecording() : stopRecording()), 200);
                    // GFE-backend confirmations (both pages must react to these)
                    setTimeout(() => pushToAll('/ShadowPlay/v.1.0/Record/Enable', { status: enable }), 450);
                    if (!enable) {
                        setTimeout(() => pushToAll('/ShadowPlay/v.1.0/Notification', {
                            notification: 'recordingSaved', result: 0, file: savedFile('Record'),
                        }), 900);
                    }
                    return json(res);
                }
                return json(res, JSON.stringify({ status: engine.recording }));
            case '/Record/Running':
                return json(res, JSON.stringify({ running: engine.recording, status: engine.recording }));
            case '/Record/Concurrency/Broadcast':
            case '/Record/Concurrency/Gamestream':
                return json(res, JSON.stringify({ support: true }));
            case '/InstantReplay/Enable':
                if (req.method === 'POST') {
                    const enable = (restBody ?? '').toLowerCase().includes('true');
                    engine.replay = enable;
                    setTimeout(() => pushToAll('/ShadowPlay/v.1.0/InstantReplay/Enable', { status: enable }), 450);
                    setTimeout(() => pushToAll('/ShadowPlay/v.1.0/InstantReplay/Started', { started: enable }), 700);
                    return json(res);
                }
                return json(res, JSON.stringify({ status: engine.replay }));
            case '/InstantReplay/Running':
                return json(res, JSON.stringify({ running: engine.replay, status: engine.replay }));
            case '/InstantReplay/Save':
                if (req.method === 'POST') {
                    setTimeout(() => {
                        pushToAll('/ShadowPlay/v.1.0/InstantReplay/Save', { status: true });
                        pushToAll('/ShadowPlay/v.1.0/Notification', {
                            notification: 'recordingSaved', result: 0, file: savedFile('InstantReplay'),
                        });
                    }, 450);
                }
                return json(res);
            case '/InstantReplay/BufferLength':
                return json(res, JSON.stringify({ lengthSeconds: 300 }));
            case '/InstantReplay/Settings':
                return json(res, req.method === 'POST' ? '{}' : JSON.stringify({
                    replayLengthSeconds: 300, quality: 'custom',
                    resolution: '2560x1440', framerate: '60', bitrateBps: '50000000',
                }));
            case '/Launch':
                return json(res, req.method === 'GET' ? '{"launch":true}' : '{}');
            case '/NotifyOverlayState':
                return json(res);
            case '/Capture/State':
                return json(res, JSON.stringify({ state: engine.recording ? 'Recording' : 'Idle' }));
            case '/Language':
                return json(res, '{"language":"en-US"}');
            case '/uiReady':
                engine.log.push('uiReady');
                return json(res);
            case '/support':
                return json(res);
            default:
                engine.log.push(`REST ${req.method} ${p} → {} (catch-all)`);
                if (req.method === 'GET' || req.method === 'POST') return json(res);
                return json(res, '{}', 405);
        }
    } catch {
        try { res.writeHead(500).end('{}'); } catch { /* client gone */ }
    }
});

// bind ALL interfaces so http://localhost:<port> and http://127.0.0.1:<port>
// both resolve — legacy GFE pages call 'localhost' (production parity: the
// real Web Helper serves the page from the same localhost origin)
attachRealSocketIo(server);
server.listen(PORT, () => {
    console.log(`osc-reui harness on http://127.0.0.1:${PORT}`);
    console.log(`  new UI   → http://127.0.0.1:${PORT}/next/index.html`);
    console.log(`  legacy   → http://127.0.0.1:${PORT}/index.html`);
    console.log(`  inspect  → /__inspect   transcript → /__transcript`);
});
