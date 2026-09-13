'use strict';
/* cdp-debug.js — turn on the bundled engine.io/socket.io debug logger,
 * reload the page, and dump socket-related console output.
 * Usage: node cdp-debug.js <cdpPort> <seconds>
 */
const WebSocket = require('ws');
const http = require('http');

const cdpPort = process.argv[2] || '9223';
const seconds = parseInt(process.argv[3] || '10', 10);

http.get('http://127.0.0.1:' + cdpPort + '/json', (res) => {
  let b = '';
  res.on('data', c => b += c);
  res.on('end', () => {
    const targets = JSON.parse(b).filter(t => t.type === 'page');
    const ws = new WebSocket(targets[0].webSocketDebuggerUrl, { perMessageDeflate: false });
    let id = 0;
    let phase = 0;
    ws.on('open', () => {
      ws.send(JSON.stringify({ id: ++id, method: 'Runtime.enable', params: {} }));
      ws.send(JSON.stringify({
        id: ++id, method: 'Runtime.evaluate',
        params: { expression: `try{localStorage.setItem('debug','engine.io-client:*,socket.io-client:*');}catch(e){} 'ok'`, returnByValue: true }
      }));
      setTimeout(() => {
        phase = 1;
        ws.send(JSON.stringify({ id: ++id, method: 'Page.enable', params: {} }));
        ws.send(JSON.stringify({ id: ++id, method: 'Page.reload', params: {} }));
      }, 500);
    });
    ws.on('message', (m) => {
      const msg = JSON.parse(m);
      if (msg.method === 'Runtime.consoleAPICalled' && phase === 1) {
        const args = (msg.params.args || []).map(a => a.value !== undefined ? JSON.stringify(a.value) : (a.description || a.type)).join(' ');
        if (args.indexOf('engine.io') >= 0 || args.indexOf('socket.io') >= 0 || msg.params.type === 'error' || msg.params.type === 'warning') {
          console.log('[' + msg.params.type + '] ' + args.slice(0, 300));
        }
      }
      if (msg.method === 'Runtime.exceptionThrown') {
        const d = msg.params.exceptionDetails;
        console.log('[exception] ' + (d.exception && d.exception.description ? d.exception.description.slice(0, 300) : d.text));
      }
    });
    ws.on('error', (e) => { console.log('WS ERROR ' + e.message); process.exit(2); });
    setTimeout(() => process.exit(0), seconds * 1000 + 2500);
  });
}).on('error', (e) => { console.log('HTTP ERROR ' + e.message); process.exit(2); });
