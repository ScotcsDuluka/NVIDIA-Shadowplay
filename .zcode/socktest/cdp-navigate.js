// CDP: navigate the live host's page to our staged osc (file://) and inspect.
const http = require('http');
const fs = require('fs');
const WebSocket = require('ws');

const port = '9222';
const LOG = 'C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\cdp-navigate-result.txt';
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
    out('=== navigate probe start ===');
    const targets = await getJson('/json/list');
    const page = targets.find(t => t.type === 'page');
    if (!page) { out('NO-PAGE'); return; }

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
            out('CONSOLE[' + msg.params.type + '] ' + msg.params.args.map(a => a.value !== undefined ? String(a.value) : (a.description || a.type)).join(' | ').slice(0, 220));
        } else if (msg.method === 'Runtime.exceptionThrown') {
            out('EXCEPTION: ' + JSON.stringify(msg.params.exceptionDetails).slice(0, 300));
        } else if (msg.method === 'Log.entryAdded') {
            out('LOG[' + msg.params.entry.level + '] ' + String(msg.params.entry.text).slice(0, 220) + ' @ ' + String(msg.params.entry.url || '').slice(0, 120));
        } else if (msg.method === 'Page.loadEventFired') {
            out('PAGE-LOADED');
        }
    });
    await new Promise(r => ws.on('open', r));
    await send('Runtime.enable');
    await send('Log.enable');
    await send('Page.enable');

    const fileUrl = 'file:///C:/My%20Project/NVIDIA-Shadowplay/Build/NVIDIA%20ShadowPlay/NvOverlay/Cef/Resources/osc/index.html';
    out('NAVIGATE: ' + fileUrl);
    await send('Page.navigate', { url: fileUrl });
    await wait(6000);

    const ready = await send('Runtime.evaluate', { expression: 'document.readyState', returnByValue: true });
    out('readyState: ' + JSON.stringify(ready.result && ready.result.result));
    const info = await send('Runtime.evaluate', {
        expression: 'JSON.stringify({htmlLen: document.documentElement.outerHTML.length, bodyChildren: document.body ? document.body.children.length : -1, scripts: document.scripts.length, angular: typeof angular !== "undefined"})',
        returnByValue: true,
    });
    out('info: ' + JSON.stringify(info.result && info.result.result && info.result.result.value));

    await wait(2500);
    const shot = await send('Page.captureScreenshot', { format: 'png' });
    if (shot.result && shot.result.data) {
        const png = 'C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\cdp-screenshot-file.png';
        fs.writeFileSync(png, Buffer.from(shot.result.data, 'base64'));
        out('SCREENSHOT-SAVED: ' + png + ' (' + Math.round(shot.result.data.length * 3 / 4 / 1024) + 'KB)');
    } else {
        out('SCREENSHOT-FAILED');
    }
    out('=== end ===');
    ws.close();
    process.exit(0);
})().catch(e => { out('CRASH: ' + (e && e.stack ? e.stack : e)); process.exit(2); });
