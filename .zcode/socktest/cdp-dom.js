// DOM state probe: what does the page actually have visible?
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
const LOG = 'C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\cdp-dom-result.txt';
const lines = [];
function out(s) { lines.push(s); try { fs.writeFileSync(LOG, lines.join('\r\n')); } catch (e) {} }

(async () => {
    const targets = await getJson('/json/list');
    const page = targets.find(t => t.type === 'page');
    if (!page) { out('NO-PAGE'); return; }
    out('PAGE: ' + page.url);
    const ws = new WebSocket(page.webSocketDebuggerUrl, { perMessageDeflate: false });
    let id = 0; const pending = {};
    ws.on('message', raw => { const m = JSON.parse(raw); if (m.id && pending[m.id]) { pending[m.id](m); delete pending[m.id]; } });
    await new Promise(r => ws.on('open', r));
    function send(expression) {
        return new Promise(res => { const i = ++id; pending[i] = res; ws.send(JSON.stringify({ id: i, method: 'Runtime.evaluate', params: { expression, returnByValue: true } })); });
    }

    const win = await send('JSON.stringify({iw: innerWidth, ih: innerHeight, dpr: devicePixelRatio, vis: document.visibilityState})');
    out('window: ' + JSON.stringify(win.result && win.result.result && win.result.result.value));

    const structs = await send(
        'JSON.stringify([' +
        "['body', getComputedStyle(document.body).backgroundColor, document.body.className, document.body.childElementCount]," +
        "Array.from(document.body.children).map(function(el){var cs=getComputedStyle(el);return [el.tagName+'.'+String(el.className).slice(0,40), cs.display, cs.visibility, cs.opacity, el.offsetWidth+'x'+el.offsetHeight];})" +
        '])');
    out('structure: ' + JSON.stringify(structs.result && structs.result.result && structs.result.result.value, null, 1).slice(0, 1500));

    const views = await send(
        'JSON.stringify(Array.from(document.querySelectorAll("[ui-view],.view,.main-view,#base,.osc,.container")).slice(0,8).map(function(el){var cs=getComputedStyle(el);var r=el.getBoundingClientRect();return [el.tagName+"#"+el.id+"."+String(el.className).slice(0,30), cs.display, cs.opacity, Math.round(r.width)+"x"+Math.round(r.height), String(el.innerText||"").slice(0,60)];}))');
    out('views: ' + JSON.stringify(views.result && views.result.result && views.result.result.value, null, 1).slice(0, 1200));

    ws.close(); process.exit(0);
})().catch(e => { out('CRASH: ' + (e && e.stack ? e.stack : e)); process.exit(2); });
