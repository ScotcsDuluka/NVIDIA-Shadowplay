'use strict';
/* cdp-sio.js — capture every /socket.io/ request/response pair the page
 * makes (status + first bytes of body) to find where polling dies.
 * Usage: node cdp-sio.js <cdpPort> <seconds>
 */
const WebSocket = require('ws');
const http = require('http');

const cdpPort = process.argv[2] || '9223';
const seconds = parseInt(process.argv[3] || '8', 10);
const seen = {};

http.get('http://127.0.0.1:' + cdpPort + '/json', (res) => {
  let b = '';
  res.on('data', c => b += c);
  res.on('end', () => {
    const targets = JSON.parse(b).filter(t => t.type === 'page');
    const ws = new WebSocket(targets[0].webSocketDebuggerUrl, { perMessageDeflate: false });
    let id = 0;
    const pending = {};
    ws.on('open', () => {
      ws.send(JSON.stringify({ id: ++id, method: 'Network.enable', params: {} }));
    });
    ws.on('message', (m) => {
      const msg = JSON.parse(m);
      if (msg.method === 'Network.requestWillBeSent') {
        const u = msg.params.request.url;
        if (u.indexOf('/socket.io/') >= 0) {
          pending[msg.params.requestId] = u;
          const q = u.split('?')[1] || '';
          const sid = (q.match(/sid=([^&]+)/) || [null, 'NOSID'])[1].slice(0, 8);
          console.log('[req] ' + msg.params.request.method + ' sid=' + sid + ' ' + q.replace(/&t=[^&]*/, '').slice(0, 120));
        }
      } else if (msg.method === 'Network.responseReceived') {
        if (pending[msg.params.requestId]) {
          console.log('[res] status=' + msg.params.response.status + ' mime=' + msg.params.response.mimeType);
          delete pending[msg.params.requestId];
          ws.send(JSON.stringify({
            id: ++id, method: 'Network.getResponseBody',
            params: { requestId: msg.params.requestId }
          }));
        }
      } else if (msg.method === 'Network.loadingFailed') {
        if (pending[msg.params.requestId]) {
          console.log('[fail] ' + msg.params.errorText + ' canceled=' + msg.params.canceled);
          delete pending[msg.params.requestId];
        }
      } else if (msg.id && msg.result && msg.result.body !== undefined) {
        console.log('[body] ' + String(msg.result.body).slice(0, 120));
      }
    });
    ws.on('error', (e) => { console.log('WS ERROR ' + e.message); process.exit(2); });
    setTimeout(() => process.exit(0), seconds * 1000 + 2000);
  });
}).on('error', (e) => { console.log('HTTP ERROR ' + e.message); process.exit(2); });
