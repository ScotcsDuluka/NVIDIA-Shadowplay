// CDP probe: connect to the host's DevTools, evaluate page state, screenshot.
// usage: node cdp-probe.js [debugPort=9222]
const http = require('http');
const fs = require('fs');
const WebSocket = require('ws');

const port = process.argv[2] || '9222';
const LOG = 'C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\cdp-probe-result.txt';
const lines = [];
function out(s) { lines.push(s); try { fs.writeFileSync(LOG, lines.join('\r\n')); } catch (e) {} }

function getJson(path) {
    return new Promise((resolve, reject) => {
        http.get('http://127.0.0.1:' + port + path, res => {
            let b = '';
            res.on('data', d => b += d);
            res.on('end', () => { try { resolve(JSON.parse(b)); } catch (e) { resolve(b); } });
        }).on('error', reject);
    });
}
const wait = ms => new Promise(r => setTimeout(r, ms));

(async () => {
    out('=== CDP probe start ===');
    let targets;
    try {
        targets = await getJson('/json/list');
    } catch (e) {
        out('DEBUG-PORT-NOT-REACHABLE: ' + e.message);
        return;
    }
    out('targets: ' + JSON.stringify(targets.map(t => ({ type: t.type, title: t.title, url: t.url }))));
    const page = targets.find(t => t.type === 'page');
    if (!page) { out('NO-PAGE-TARGET'); return; }

    const ws = new WebSocket(page.webSocketDebuggerUrl, { perMessageDeflate: false });
    let id = 0;
    const pending = {};
    function send(method, params) {
        return new Promise(resolve => {
            const mid = ++id;
            pending[mid] = resolve;
            ws.send(JSON.stringify({ id: mid, method, params: params || {} }));
        });
    }
    ws.on('message', raw => {
        const msg = JSON.parse(raw);
        if (msg.id && pending[msg.id]) { pending[msg.id](msg); delete pending[msg.id]; return; }
        if (msg.method === 'Runtime.consoleAPICalled') {
            out('CONSOLE[' + msg.params.type + '] ' + msg.params.args.map(a => a.value !== undefined ? String(a.value) : (a.description || a.type)).join(' | ').slice(0, 200));
        } else if (msg.method === 'Runtime.exceptionThrown') {
            out('EXCEPTION: ' + JSON.stringify(msg.params.exceptionDetails).slice(0, 400));
        } else if (msg.method === 'Log.entryAdded') {
            out('LOG[' + msg.params.entry.level + '] ' + String(msg.params.entry.text).slice(0, 200) + ' @ ' + String(msg.params.entry.url || '').slice(0, 100));
        }
    });

    await new Promise(r => ws.on('open', r));
    out('CDP-OPEN: ' + page.title + '  ' + page.url);

    await send('Runtime.enable');
    await send('Log.enable');
    await send('Page.enable');

    const ready = await send('Runtime.evaluate', { expression: 'document.readyState', returnByValue: true });
    out('readyState: ' + JSON.stringify(ready.result && ready.result.result));

    const bodyInfo = await send('Runtime.evaluate', {
        expression: 'JSON.stringify({bodyChildren: document.body ? document.body.children.length : -1, htmlLen: document.documentElement ? document.documentElement.outerHTML.length : -1, title: document.title, bg: getComputedStyle(document.body).backgroundColor})',
        returnByValue: true,
    });
    out('pageInfo: ' + JSON.stringify(bodyInfo.result && bodyInfo.result.result && bodyInfo.result.result.value));

    await wait(1500);
    const shot = await send('Page.captureScreenshot', { format: 'png' });
    if (shot.result && shot.result.data) {
        const png = 'C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\cdp-screenshot.png';
        fs.writeFileSync(png, Buffer.from(shot.result.data, 'base64'));
        out('SCREENSHOT-SAVED: ' + png + ' (' + Math.round(shot.result.data.length * 3 / 4 / 1024) + 'KB)');
    } else {
        out('SCREENSHOT-FAILED: ' + JSON.stringify(shot).slice(0, 200));
    }

    const ng = await send('Runtime.evaluate', { expression: 'typeof ng !== "undefined" ? "angular-present" : "no-angular"', returnByValue: true });
    out('angular: ' + JSON.stringify(ng.result && ng.result.result && ng.result.result.value));

    out('=== probe end ===');
    ws.close();
    process.exit(0);
})().catch(e => { out('CRASH: ' + (e && e.stack ? e.stack : e)); process.exit(2); });
