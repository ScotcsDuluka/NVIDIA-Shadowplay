// Boot-log probe v2: reload + capture console/net failures (fixed id matching).
const http = require('http');
const fs = require('fs');
const WebSocket = require('ws');

function getJson(path) {
    return new Promise((resolve, reject) => {
        http.get('http://127.0.0.1:9222' + path, res => {
            let b = '';
            res.on('data', d => b += d);
            res.on('end', () => { try { resolve(JSON.parse(b)); } catch (e) { resolve(b); } });
        }).on('error', reject);
    });
}
const LOG = 'C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\cdp-bootlog.txt';
const lines = [];
function out(s) { lines.push(s); try { fs.writeFileSync(LOG, lines.join('\r\n')); } catch (e) {} }

(async () => {
    const targets = await getJson('/json/list');
    const page = targets.find(t => t.type === 'page');
    if (!page) { out('NO-PAGE'); return; }
    out('PAGE: ' + page.url);
    const ws = new WebSocket(page.webSocketDebuggerUrl, { perMessageDeflate: false });
    let id = 0; const pending = {};
    ws.on('message', raw => {
        const m = JSON.parse(raw);
        if (m.id && pending[m.id]) { pending[m.id](m); delete pending[m.id]; return; }
        if (m.method === 'Runtime.consoleAPICalled') {
            const t = m.params.type;
            out('CONSOLE[' + t + '] ' + m.params.args.map(a => a.value !== undefined ? String(a.value) : (a.description || a.type)).join(' | ').slice(0, 220));
        } else if (m.method === 'Runtime.exceptionThrown') {
            out('EXCEPTION: ' + JSON.stringify(m.params.exceptionDetails).slice(0, 300));
        } else if (m.method === 'Log.entryAdded') {
            out('LOG[' + m.params.entry.level + '] ' + String(m.params.entry.text).slice(0, 200) + ' @ ' + String(m.params.entry.url || '').slice(0, 130));
        } else if (m.method === 'Network.loadingFailed') {
            out('NET-FAIL: ' + (m.params.errorText || '') + ' ' + (m.params.type || ''));
        } else if (m.method === 'Network.responseReceived') {
            const r = m.params.response;
            if (r.status >= 400) out('HTTP-' + r.status + ': ' + r.url.slice(0, 130));
        } else if (m.method === 'Network.requestWillBeSent') {
            const u = m.params.request.url;
            if (u.indexOf('59003/socket.io') === -1 && u.indexOf('EIO=') === -1) {
                out('REQ: ' + m.params.request.method + ' ' + u.slice(0, 140));
            }
        }
    });
    await new Promise(r => ws.on('open', r));
    function send(method, params) {
        return new Promise(res => { const i = ++id; pending[i] = res; ws.send(JSON.stringify({ id: i, method, params: params || {} })); });
    }
    await send('Runtime.enable');
    await send('Log.enable');
    await send('Network.enable');
    await send('Page.enable');
    await send('Page.reload', { ignoreCache: true });
    await new Promise(r => setTimeout(r, 10000));
    const st = await send('Runtime.evaluate', { expression: 'JSON.stringify({url: location.href, angular: typeof angular !== "undefined", htmlLen: document.documentElement.outerHTML.length, baseLen: (function(){var b=document.querySelector(".base");return b?b.innerHTML.length:-1})()})', returnByValue: true });
    out('FINAL: ' + JSON.stringify(st.result && st.result.result && st.result.result.value));
    out('=== end ===');
    ws.close(); process.exit(0);
})().catch(e => { out('CRASH: ' + (e && e.stack ? e.stack : e)); process.exit(2); });
