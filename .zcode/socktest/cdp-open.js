// Quick: screenshot the CURRENT page in the live host.
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

(async () => {
    const targets = await getJson('/json/list');
    const page = targets.find(t => t.type === 'page');
    if (!page) { console.log('NO-PAGE'); process.exit(1); }
    console.log('PAGE: ' + page.url);
    const ws = new WebSocket(page.webSocketDebuggerUrl, { perMessageDeflate: false });
    let id = 0; const pending = {};
    ws.on('message', raw => { const m = JSON.parse(raw); if (m.id && pending[m.id]) { pending[m.id](m); delete pending[m.id]; } });
    await new Promise(r => ws.on('open', r));
    function send(method, params) { return new Promise(res => { const i = ++id; pending[i] = res; ws.send(JSON.stringify({ id: i, method, params: params || {} })); }); }
    await send('Runtime.evaluate', { expression: 'document.documentElement.classList.add("oscengine-open")' }); const shot = await send('Page.captureScreenshot', { format: 'png' });
    if (shot.result && shot.result.data) {
        fs.writeFileSync('C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\cdp-current.png', Buffer.from(shot.result.data, 'base64'));
        console.log('SAVED cdp-current.png');
    } else { console.log('SHOT-FAIL'); }
    ws.close(); process.exit(0);
})().catch(e => { console.log('ERR ' + e.message); process.exit(2); });

