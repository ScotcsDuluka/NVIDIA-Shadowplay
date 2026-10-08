# probe-hotkey.py — ดัก event ในหน้าทุกชั้น: raw socket -> hotkeyEvent -> openshare
import json, sys, time, urllib.request
from osc_hack import CDP, http_get

def find_page():
    pages = json.loads(http_get("http://127.0.0.1:59099/json"))
    for p in pages:
        if p.get("type") == "page" and "/index.html" in p.get("url", ""):
            return p.get("webSocketDebuggerUrl")
    return None

ATTACH = """(function(){ try {
  var inj = angular.element(document).injector();
  var rs = inj.get('$rootScope');
  var ss = inj.get('socketService');
  window.__probe = {hotkeyEvent: [], openshare: [], raw: []};
  rs.$on('hotkeyEvent', function(ev, d){ try { window.__probe.hotkeyEvent.push(JSON.stringify(d)); } catch(e){} });
  rs.$on('openshare', function(){ window.__probe.openshare.push('fired'); });
  var sock = null;
  var keys = Object.keys(ss || {});
  try { sock = ss.socket || ss._socket || ss.io || null; } catch(e){}
  var rawAttached = 'no-socket-field';
  if (sock && typeof sock.on === 'function') {
    sock.on('/ShadowPlay/v.1.0/Hotkey', function(d){ try { window.__probe.raw.push(JSON.stringify(d)); } catch(e){} });
    rawAttached = 'yes';
  }
  return JSON.stringify({attached: true, ssKeys: keys.slice(0,15), raw: rawAttached});
} catch(e) { return 'ERR: ' + e.message; } })()"""

READ = """(function(){ try {
  var inj = angular.element(document).injector();
  try { inj.get('$rootScope').$apply(); } catch(e){}
  return JSON.stringify(window.__probe);
} catch(e) { return 'ERR: ' + e.message; } })()"""

def fire():
    req = urllib.request.Request("http://127.0.0.1:59011/?hk=OpenShare")
    return urllib.request.urlopen(req, timeout=8).status

ws = find_page()
if not ws:
    print("no page"); sys.exit(2)
c = CDP(ws)
print("ATTACH:", c.eval(ATTACH))
print("FIRE:", fire())
time.sleep(2.5)
print("READ:", c.eval(READ))
