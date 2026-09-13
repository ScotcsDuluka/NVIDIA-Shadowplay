'use strict';
/* cdp-net.js — watch network requests + drive the page's own
 * cefService.localNodeInfo() to see exactly where the boot stalls.
 * Usage: node cdp-net.js <cdpPort> <seconds>
 */
const WebSocket = require('ws');
const http = require('http');

const cdpPort = process.argv[2] || '9223';
const seconds = parseInt(process.argv[3] || '8', 10);

http.get('http://127.0.0.1:' + cdpPort + '/json', (res) => {
  let b = '';
  res.on('data', c => b += c);
  res.on('end', () => {
    const targets = JSON.parse(b).filter(t => t.type === 'page');
    const ws = new WebSocket(targets[0].webSocketDebuggerUrl, { perMessageDeflate: false });
    let id = 0;
    const send = (method, params, cb) => ws.send(JSON.stringify({ id: ++id, method, params }));
    const pending = {};
    ws.on('open', () => {
      send('Network.enable');
      send('Runtime.enable');
      setTimeout(() => {
        // drive the real service the boot resolve uses
        send('Runtime.evaluate', { expression: `
          (function(){
            var out = {probe:'start'};
            try {
              var inj = angular.element(document).injector();
              if (!inj) { out.probe='no injector'; return JSON.stringify(out); }
              var cs = inj.get('cefService');
              cs.localNodeInfo().then(function(v){ window.__nodeInfo = v; }, function(e){ window.__nodeErr = String(e); });
              out.probe = 'called';
            } catch(e) { out.probe = 'err: ' + e; }
            return JSON.stringify(out);
          })()`, returnByValue: true });
        setTimeout(() => {
          send('Runtime.evaluate', { expression: `JSON.stringify({nodeInfo: window.__nodeInfo || null, nodeErr: window.__nodeErr || null})`, returnByValue: true });
        }, 4000);
      }, 500);
    });
    ws.on('message', (m) => {
      const msg = JSON.parse(m);
      if (msg.method === 'Network.requestWillBeSent') {
        const u = msg.params.request.url;
        if (u.indexOf('omtrdc') < 0 && u.indexOf('static.nvidiagrid') < 0) {
          console.log('[net] ' + msg.params.request.method + ' ' + u.slice(0, 140));
        }
      } else if (msg.method === 'Network.loadingFailed') {
        console.log('[net-FAIL] ' + msg.params.errorText + ' (' + msg.params.type + ')');
      } else if (msg.id && msg.result && msg.result.result) {
        console.log('[eval] ' + (msg.result.result.value !== undefined ? msg.result.result.value : JSON.stringify(msg.result)));
      }
    });
    ws.on('error', (e) => { console.log('WS ERROR ' + e.message); process.exit(2); });
    setTimeout(() => process.exit(0), (seconds + 6) * 1000);
  });
}).on('error', (e) => { console.log('HTTP ERROR ' + e.message); process.exit(2); });
