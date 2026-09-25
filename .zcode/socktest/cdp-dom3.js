// Deep probe: .base view contents + hidden-state analysis.
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
const LOG = 'C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\cdp-dom3-result.txt';
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

    const tagprobe = await send('JSON.stringify({tag: (function(){var b=document.querySelector(".base");return b?b.tagName:"none"})(), attrs: (function(){var b=document.querySelector(".base");return b?Array.from(b.attributes).map(function(a){return a.name+"="+a.value.slice(0,30)}):[]})(), ngApp: !!document.querySelector("[ng-app]"), injector: (function(){try{return !!angular.element(document.body).injector()}catch(e){return "err:"+e.message}})(), modules: (function(){try{return angular.module("main").requires.slice(0,20)}catch(e){return "err"}})()})');out('tagprobe: ' + JSON.stringify(tagprobe.result && tagprobe.result.result && tagprobe.result.result.value, null, 1).slice(0, 1200)); const deep = await send(
        '(function(){' +
        'var b = document.querySelector(".base");' +
        'if (!b) return {err:"no .base"};' +
        'var all = b.querySelectorAll("*");' +
        'var visible = 0; var hidden = 0; var visSample = [];' +
        'for (var i = 0; i < all.length; i++) {' +
        '  var cs = getComputedStyle(all[i]);' +
        '  if (cs.display === "none" || cs.visibility === "hidden") { hidden++; continue; }' +
        '  if (all[i].offsetWidth > 0 && all[i].offsetHeight > 0) {' +
        '    visible++;' +
        '    if (visSample.length < 6) visSample.push(all[i].tagName + "." + String(all[i].className).slice(0, 30) + " " + all[i].offsetWidth + "x" + all[i].offsetHeight);' +
        '  }' +
        '}' +
        'return {baseChildren: b.children.length, baseHTMLLen: b.innerHTML.length,' +
        '        totalDescendants: all.length, visible: visible, hidden: hidden,' +
        '        visSample: visSample,' +
        '        sample: b.innerHTML.slice(0, 500)};' +
        '})()');
    out('deep: ' + JSON.stringify(deep.result && deep.result.result && deep.result.result.value, null, 1).slice(0, 2000));

    ws.close(); process.exit(0);
})().catch(e => { out('CRASH: ' + (e && e.stack ? e.stack : e)); process.exit(2); });

