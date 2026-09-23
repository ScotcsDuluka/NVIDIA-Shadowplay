'use strict'
// test/parity-validation.js — the narrowest end-to-end validation for the
// node-derived parity surface (docs/OSC-NODE-API-EXTRACTION.md §A/§B,
// docs/OSC-GALLERY-IPC-EXTRACTION.md §B).
//
// Boots the real backend on a test port (child process), connects a REAL
// socket.io v2 client (socket.io-client 2.5.0 — the same major the osc page
// ships), and exercises every implemented channel/route:
//   node test/parity-validation.js            # full run
//   node test/parity-validation.js --socket   # socket-parity gate only
//
// Exit 0 = all pass; failures print with expected vs actual.

const path = require('path');
const http = require('http');
const { spawn } = require('child_process');

const ROOT = path.join(__dirname, '..');
const PORT = 59191;
const BASE = 'http://127.0.0.1:' + PORT;
const ONLY_SOCKET = process.argv.includes('--socket');

// ── tiny http helpers ───────────────────────────────────────────────
function request(method, url, body) {
    return new Promise(function (resolve, reject) {
        const data = body === undefined ? null : String(body);
        const req = http.request(url, {
            method: method,
            headers: Object.assign({}, data !== null ? {
                'Content-Type': 'application/json',
                'Content-Length': Buffer.byteLength(data)
            } : {})
        }, function (res) {
            const chunks = [];
            res.on('data', function (c) { chunks.push(c); });
            res.on('end', function () {
                const text = Buffer.concat(chunks).toString('utf8');
                let json;
                try { json = JSON.parse(text); } catch (e) { json = undefined; }
                resolve({ status: res.statusCode, text: text, json: json });
            });
        });
        req.on('error', reject);
        if (data !== null) req.write(data);
        req.end();
    });
}
const GET = function (url) { return request('GET', url); };
const POSTJSON = function (url, obj) { return request('POST', url, JSON.stringify(obj || {})); };
const POSTRAW = function (url, text) { return request('POST', url, text); };
const DEL = function (url) { return request('DELETE', url); };

function waitHealth() {
    return new Promise(function (resolve, reject) {
        let tries = 0;
        (function poll() {
            GET(BASE + '/Backend/v.1.0/health').then(function (r) {
                if (r.status === 200) return resolve();
                retry();
            }, retry);
            function retry() {
                if (++tries > 50) return reject(new Error('backend did not come up'));
                setTimeout(poll, 200);
            }
        })();
    });
}

// ── socket.io v2 client: all-events tap via the onevent dispatcher ──
// (reserved events like 'connect' bypass onevent, so overriding it does not
// break the connect handshake)
function connectSocket() {
    const io = require(path.join(ROOT, 'node_modules', 'socket.io-client'));
    return new Promise(function (resolve, reject) {
        const sock = io(BASE, { transports: ['polling'], reconnection: false });
        const received = [];
        sock.onevent = function (packet) {
            received.push({
                channel: packet.data ? packet.data[0] : '(unknown)',
                payload: packet.data && packet.data.length > 1 ? packet.data[1] : undefined
            });
        };
        sock.on('connect', function () { resolve({ sock: sock, received: received }); });
        sock.on('connect_error', function (err) { reject(new Error('connect_error: ' + err)); });
        setTimeout(function () { reject(new Error('socket connect timeout')); }, 8000);
    });
}

function waitForEvent(received, channel, timeoutMs) {
    const deadline = Date.now() + (timeoutMs || 3000);
    return new Promise(function (resolve) {
        (function poll() {
            const hit = received.filter(function (e) { return e.channel === channel; }).pop();
            if (hit) return resolve(hit);
            if (Date.now() > deadline) return resolve(null);
            setTimeout(poll, 60);
        })();
    });
}

// ── assertions ──────────────────────────────────────────────────────
const results = [];
function check(name, ok, detail) {
    results.push({ name: name, ok: ok, detail: detail || '' });
    console.log((ok ? 'PASS' : 'FAIL') + '  ' + name + (ok ? '' : '  — ' + detail));
}
function eq(actual, expected) {
    return JSON.stringify(actual) === JSON.stringify(expected);
}

// ── main ────────────────────────────────────────────────────────────
(async function main() {
    const dataDir = path.join(ROOT, 'test', '.tmp-data');
    const child = spawn(process.execPath, ['index.js'], {
        cwd: ROOT,
        env: Object.assign({}, process.env, {
            NVSP_PORT: String(PORT),
            NVSP_DATA_DIR: dataDir,
            NVSP_LOG_LEVEL: 'error'
        }),
        stdio: ['ignore', 'pipe', 'pipe']
    });
    child.stderr.on('data', function (d) { process.stderr.write('[backend] ' + d); });

    let received = null;
    let backendUp = false;
    try { await waitHealth(); backendUp = true; }
    catch (err) { console.log('FATAL  backend boot failed: ' + err.message); }

    if (backendUp) {
        check('boot: /Backend/v.1.0/health 200', true);

        // ── existing OSC boot surface still works ───────────────────
        const init = await GET(BASE + '/ShadowPlay/v.1.0/OSC/Init');
        check('boot: OSC/Init 200', init.status === 200, 'status ' + init.status);
        const rs = await GET(BASE + '/ShadowPlay/v.1.0/Record/Settings');
        check('boot: Record/Settings 200 JSON', rs.status === 200 && rs.json && typeof rs.json === 'object', rs.text.slice(0, 80));
        const oscPost = await POSTJSON(BASE + '/ShadowPlay/v.1.0/Osc', { ready: true });
        check('boot: POST /Osc 200', oscPost.status === 200, 'status ' + oscPost.status);

        // ── socket.io v2 client + production channels ───────────────
        const sockCtx = await connectSocket();
        received = sockCtx.received;
        check('socket: v2 client connected (EIO=3)', true);

        async function expectChannel(name, channel, trigger, payload) {
            const res = await trigger();
            const hit = await waitForEvent(received, channel, 3000);
            const ok = !!hit && (payload === undefined || eq(hit.payload, payload));
            check(name, ok, hit ? ('payload=' + JSON.stringify(hit.payload)) : ('no event; http=' + (res && res.status)));
        }

        // production-trigger channels (HTTP state change → socket)
        await expectChannel('socket: Record/Enable POST → channel {status}',
            '/ShadowPlay/v.1.0/Record/Enable',
            function () { return POSTJSON(BASE + '/ShadowPlay/v.1.0/Record/Enable', { status: true }); },
            { status: true });
        await expectChannel('socket: InstantReplay/Save POST → channel {status:true}',
            '/ShadowPlay/v.1.0/InstantReplay/Save',
            function () { return POSTJSON(BASE + '/ShadowPlay/v.1.0/InstantReplay/Save', {}); },
            { status: true });
        await expectChannel('socket: Broadcast/Enable POST → channel {status}',
            '/ShadowPlay/v.1.0/Broadcast/Enable',
            function () { return POSTJSON(BASE + '/ShadowPlay/v.1.0/Broadcast/Enable', { status: true }); },
            { status: true });
        await expectChannel('socket: Broadcast/Pause POST → channel {status:true=paused}',
            '/ShadowPlay/v.1.0/Broadcast/Pause',
            function () { return POSTJSON(BASE + '/ShadowPlay/v.1.0/Broadcast/Pause', { pause: true }); },
            { status: true });
        await expectChannel('socket: OpenOscPreferences POST → DisplayOsc echo',
            '/ShadowPlay/v.1.0/DisplayOscPreferences',
            function () { return POSTJSON(BASE + '/ShadowPlay/v.1.0/OpenOscPreferences', { open: true }); },
            { open: true });
        await expectChannel('socket: OscNotification POST → DisplayOsc echo',
            '/ShadowPlay/v.1.0/DisplayOscNotification',
            function () { return POSTJSON(BASE + '/ShadowPlay/v.1.0/OscNotification', { title: 'x' }); },
            { title: 'x' });
        await expectChannel('socket: Language POST → {language}',
            '/Settings/v.1.0/Language',
            function () { return POSTJSON(BASE + '/Settings/v.1.0/Language', { language: 'en-US' }); },
            { language: 'en-US' });
        await expectChannel('socket: Account UserToken POST → {userToken, userInfo}',
            '/Account/v.1.0/UserToken',
            function () { return POSTJSON(BASE + '/Account/v.1.0/UserToken', { userToken: 'tok-1', userInfo: { userId: 'u1' } }); },
            { userToken: 'tok-1', userInfo: { userId: 'u1', displayName: '' } });

        // every proven channel via the validation hook (representative payload)
        const channels = [
            ['/ShadowPlay/v.1.0/InstantReplay/Enable', { status: true }],
            ['/ShadowPlay/v.1.0/InstantReplay/Started', { started: true }],
            ['/ShadowPlay/v.1.0/Hotkey', { hotkeyName: 'openshare' }],
            ['/ShadowPlay/v.1.0/Notification', { sessionData: true }],
            ['/ShadowPlay/v.1.0/DisplayOscState', { state: 'open' }],
            ['/ShadowPlay/v.1.0/Broadcast/SessionEvent', { sessionEvent: true }],
            ['/GameShare/v.1.0/CreateSession', { inviteMode: 'email' }],
            ['/GameShare/v.1.0/SessionUpdate', { session: 1 }],
            ['/SDK/v.1.0/Notification', { highlight: 1 }],
            ['/QuietMode2/v.1.0/state', { enabled: true }],
            ['/QuietMode2/v.1.0/support', { supported: true }],
            ['/Account/v.1.0/PrivacySettings', { consentSettings: {} }],
            ['/abHubAPI/v.0.1/Status', true],
            ['/PiplConfig/v.1.0/update', { configData: {} }]
        ];
        for (const pair of channels) {
            await expectChannel('socket: ' + pair[0] + ' (hook)',
                pair[0],
                function (p) { return function () { return POSTJSON(BASE + '/Debug/SocketEmit', { channel: p[0], payload: p[1] }); }; }(pair),
                pair[1]);
        }
        sockCtx.sock.disconnect();
    }

    // ── route families + gallery (skipped on the socket-only gate) ─────
    if (!ONLY_SOCKET && backendUp) {
        const gallery = await POSTJSON(BASE + '/Gallery/v.1.2/GetFolderListing', { directory: process.env.SystemDrive || 'C:\\', shouldWatch: false });
        check('http: GetFolderListing 200 {directories, files}',
            gallery.status === 200 && gallery.json && Array.isArray(gallery.json.directories) && Array.isArray(gallery.json.files),
            gallery.text.slice(0, 120));
        const drives = await GET(BASE + '/Gallery/v.1.0/EnumerateDrives');
        check('http: EnumerateDrives 200 {drives:[{name,type}]}',
            drives.status === 200 && drives.json && Array.isArray(drives.json.drives) &&
            drives.json.drives.length > 0 && typeof drives.json.drives[0].name === 'string' &&
            typeof drives.json.drives[0].type === 'string',
            drives.text.slice(0, 120));
        const writable = await POSTJSON(BASE + '/Gallery/v.1.0/isDirectoryWritable', { directory: dataDir });
        check('http: isDirectoryWritable boolean body true',
            writable.status === 200 && writable.json === true, writable.text.slice(0, 60));
        const readOnly = await POSTJSON(BASE + '/Gallery/v.1.0/isDirectoryWritable', { directory: 'Q:\\definitely\\missing' });
        check('http: isDirectoryWritable boolean body false (missing dir)',
            readOnly.status === 200 && readOnly.json === false, readOnly.text.slice(0, 60));

        // route families (production status codes)
        const qm = await GET(BASE + '/QuietMode2/v.1.0/state');
        check('http: QuietMode2 GET state 200', qm.status === 200, 'status ' + qm.status);
        const qmp = await POSTJSON(BASE + '/QuietMode2/v.1.0/state', { enabled: true });
        check('http: QuietMode2 POST state 200 empty', qmp.status === 200 && qmp.text === '', 'status ' + qmp.status + ' body ' + qmp.text.slice(0, 40));
        const nis = await GET(BASE + '/Nis2/v.1.0/0/state');
        check('http: Nis2 GET cmsId=0 200', nis.status === 200, 'status ' + nis.status);
        const dd = await POSTJSON(BASE + '/DeepDVC/v.1.0/state', { cmsId: '0', vibrance: 50 });
        check('http: DeepDVC POST state 200 empty', dd.status === 200 && dd.text === '', 'status ' + dd.status);
        const fb = await POSTJSON(BASE + '/Feedback/v.0.1/', { category: 'bug', message: 'x' });
        check('http: Feedback 202 empty', fb.status === 202 && fb.text === '', 'status ' + fb.status);
        const abPost = await POSTJSON(BASE + '/abHubAPI/v.0.1/Post', { clientName: 'osc', experiments: [] });
        check('http: abHub Post 200 empty', abPost.status === 200 && abPost.text === '', 'status ' + abPost.status + ' body ' + abPost.text.slice(0, 40));
        const abStatus = await GET(BASE + '/abHubAPI/v.0.1/Status');
        check('http: abHub GET Status 200 empty (production quirk)', abStatus.status === 200 && abStatus.text === '', 'status ' + abStatus.status);
        const hw = await GET(BASE + '/HardwareInformation/v.0.2/generic');
        check('http: HardwareInformation v.0.2/generic 200', hw.status === 200, 'status ' + hw.status);

        // gfeupdate: GET raw text, POST raw text (Content-Type ignored) + emits
        const gfeGet = await GET(BASE + '/gfeupdate/autoGFEDownload/autoGFEbeta');
        check('http: gfeupdate GET raw text 200', gfeGet.status === 200, 'status ' + gfeGet.status);
        await POSTRAW(BASE + '/gfeupdate/autoGFEDownload/autoGFEbeta', '1');
        const gfeGet2 = await GET(BASE + '/gfeupdate/autoGFEDownload/autoGFEbeta');
        check('http: gfeupdate POST→GET roundtrip "1"', gfeGet2.text === '1', 'body ' + JSON.stringify(gfeGet2.text.slice(0, 20)));
        if (received) {
            const beta = await waitForEvent(received, 'GFEbetaValue', 2000);
            check('socket: GFEbetaValue emit (POST side-effect)', !!beta && beta.payload === '1',
                beta ? JSON.stringify(beta.payload) : 'no event');
            const urlEv = await waitForEvent(received, '/gfeupdate/autoGFEDownload/autoGFEbeta', 2000);
            check('socket: req.url emit with raw text payload', !!urlEv && urlEv.payload === '1',
                urlEv ? JSON.stringify(urlEv.payload) : 'no event');
        }

        // GameShare status codes
        const gsCreate = await POSTJSON(BASE + '/GameShare/v.1.0/CreateSession', { activeWindow: 'game', inviteMode: 'email' });
        check('http: GameShare CreateSession 202 empty', gsCreate.status === 202 && gsCreate.text === '', 'status ' + gsCreate.status);
        const gsPut = await request('PUT', BASE + '/GameShare/v.1.0/ModifySession/abc', '{}');
        check('http: GameShare ModifySession PUT 200', gsPut.status === 200, 'status ' + gsPut.status);
        const gsDel = await DEL(BASE + '/GameShare/v.1.0/Session/abc');
        check('http: GameShare Session DELETE 200', gsDel.status === 200, 'status ' + gsDel.status);
        const gs400 = await DEL(BASE + '/GameShare/v.1.0/Session/');
        check('http: GameShare Session DELETE no-id 400', gs400.status === 400, 'status ' + gs400.status);

        // dead channels must stay dead (404 — production parity)
        const dead1 = await POSTJSON(BASE + '/ShadowPlay/v.1.0/InstantReplay/Upload', {});
        check('http: InstantReplay/Upload stays 404', dead1.status === 404, 'status ' + dead1.status);
        const dead2 = await POSTJSON(BASE + '/ShadowPlay/v.1.0/Hotkey/DynamicToggle', { enable: true });
        check('http: Hotkey/DynamicToggle stays 404', dead2.status === 404, 'status ' + dead2.status);
    }

    try { child.kill(); } catch (e) { /* already gone */ }

    const failed = results.filter(function (r) { return !r.ok; });
    console.log('\n' + (results.length - failed.length) + '/' + results.length + ' checks passed');
    process.exit(failed.length ? 1 : 0);
})().catch(function (err) {
    console.log('FATAL ' + err.stack);
    process.exit(1);
});
