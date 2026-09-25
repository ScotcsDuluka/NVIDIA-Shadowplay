// Diagnose augmentation state + force navigation.
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
const wait = ms => new Promise(r => setTimeout(r, ms));

(async () => {
    const targets = await getJson('/json/list');
    const page = targets.find(t => t.type === 'page');
    if (!page) { console.log('NO-PAGE'); process.exit(1); }
    const ws = new WebSocket(page.webSocketDebuggerUrl, { perMessageDeflate: false });
    let id = 0; const pending = {};
    ws.on('message', raw => { const m = JSON.parse(raw); if (m.id && pending[m.id]) { pending[m.id](m); delete pending[m.id]; } });
    await new Promise(r => ws.on('open', r));
    function send(expression) {
        return new Promise(res => { const i = ++id; pending[i] = res; ws.send(JSON.stringify({ id: i, method: 'Runtime.evaluate', params: { expression, returnByValue: true } })); });
    }

    const aug = await send('JSON.stringify({oscOpen: typeof window.__oscOpen, unlock: window.__unlockStatus, bd: window.__bdStatus, state: (function(){try{return angular.element(document.body).injector().get("$state").current.name}catch(e){return "err"}})(), angular: !!window.angular})');
    console.log('AUG: ' + JSON.stringify(aug.result && aug.result.result && aug.result.result.value));

    const nav = await send('(function(){try{var inj=angular.element(document.body).injector();inj.get("$state").go("main.main-menu");return "navigated"}catch(e){return "err:" + e.message}})()');
    console.log('NAV: ' + JSON.stringify(nav.result && nav.result.result && nav.result.result.value));
    await wait(2000);

    const shot = await send('Page.captureScreenshot', { format: 'png' });
    if (shot.result && shot.result.data) {
        fs.writeFileSync('C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\cdp-menu2.png', Buffer.from(shot.result.data, 'base64'));
        console.log('SAVED cdp-menu2.png');
    }
    ws.close(); process.exit(0);
})().catch(e => { console.log('ERR ' + e.message); process.exit(2); });
