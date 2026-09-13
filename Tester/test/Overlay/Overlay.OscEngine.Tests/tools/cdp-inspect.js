'use strict';
/* cdp-inspect.js — attach to the running WebView2 page via CDP, capture
 * console messages for N seconds, then evaluate a probe expression.
 * Usage: node cdp-inspect.js <cdpPort> <seconds>
 */
const WebSocket = require('ws');
const http = require('http');

const cdpPort = process.argv[2] || '9223';
const seconds = parseInt(process.argv[3] || '6', 10);

http.get('http://127.0.0.1:' + cdpPort + '/json', (res) => {
  let b = '';
  res.on('data', c => b += c);
  res.on('end', () => {
    const targets = JSON.parse(b).filter(t => t.type === 'page');
    if (!targets.length) { console.log('NO PAGE TARGET'); process.exit(2); }
    const ws = new WebSocket(targets[0].webSocketDebuggerUrl, { perMessageDeflate: false });
    let id = 0;
    const send = (method, params) => ws.send(JSON.stringify({ id: ++id, method, params }));
    ws.on('open', () => {
      send('Runtime.enable');
      send('Log.enable');
      send('Page.enable');
      const probe = `JSON.stringify((function(){
        return {
          href: location.href,
          ready: document.readyState,
          hasConfig: typeof OSC_CONFIG,
          hasUserConfig: typeof OSCCLIENT_USER_CONFIG,
          cef: typeof window.cefQuery,
          bodyChildren: document.body ? document.body.childElementCount : -1,
          uiView: document.querySelector('div[ui-view]') ? document.querySelector('div[ui-view]').childElementCount : -2,
          injectorBooted: !!angular && !!angular.element(document).injector()
        };
      })())`;
      setTimeout(() => send('Runtime.evaluate', { expression: probe, returnByValue: true }), seconds * 1000);
    });
    ws.on('message', (m) => {
      const msg = JSON.parse(m);
      if (msg.method === 'Runtime.consoleAPICalled') {
        const args = (msg.params.args || []).map(a => a.value !== undefined ? JSON.stringify(a.value) : (a.description || a.type)).join(' ');
        console.log('[console.' + msg.params.type + '] ' + args.slice(0, 400));
      } else if (msg.method === 'Log.entryAdded') {
        const e = msg.params.entry;
        console.log('[log.' + e.level + '] ' + (e.url || '') + ' ' + e.text.slice(0, 300));
      } else if (msg.method === 'Runtime.exceptionThrown') {
        const d = msg.params.exceptionDetails;
        console.log('[exception] ' + (d.exception && d.exception.description ? d.exception.description.slice(0, 500) : d.text));
      } else if (msg.id && msg.result && msg.result.result && msg.result.result.value !== undefined) {
        console.log('[probe] ' + msg.result.result.value);
        process.exit(0);
      }
    });
    ws.on('error', (e) => { console.log('WS ERROR ' + e.message); process.exit(2); });
  });
}).on('error', (e) => { console.log('HTTP ERROR ' + e.message); process.exit(2); });

setTimeout(() => { console.log('[probe-timeout]'); process.exit(0); }, (seconds + 5) * 1000);
