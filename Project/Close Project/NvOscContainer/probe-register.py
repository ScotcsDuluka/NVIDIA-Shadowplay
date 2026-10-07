# probe-register.py — พิสูจน์ชั้น raw socket: register channel เองผ่าน socketService.register
import json, sys, time, urllib.request
from osc_hack import CDP, http_get

def find_page():
    pages = json.loads(http_get("http://127.0.0.1:59099/json"))
    for p in pages:
        if p.get("type") == "page" and "/index.html" in p.get("url", ""):
            return p.get("webSocketDebuggerUrl")
    return None

ws = find_page()
if not ws:
    print("no page"); sys.exit(2)
c = CDP(ws)

ATTACH = """(function(){ try {
  var inj = angular.element(document).injector();
  var rs = inj.get('$rootScope');
  var ss = inj.get('socketService');
  window.__raw = [];
  ss.register('/ShadowPlay/v.1.0/Hotkey', function(d){
    try { window.__raw.push(JSON.stringify(d)); } catch(e){}
  });
  return 'registered raw listener on /ShadowPlay/v.1.0/Hotkey';
} catch(e) { return 'ERR: ' + e.message; } })()"""
print("ATTACH:", c.eval(ATTACH))

print("FIRE:", urllib.request.urlopen(urllib.request.Request(
    "http://127.0.0.1:59011/?hk=OpenShare", data=b""), timeout=8).status)
time.sleep(2.5)

READ = """(function(){ try {
  return JSON.stringify({raw: window.__raw, hash: location.hash});
} catch(e) { return 'ERR: ' + e.message; } })()"""
print("READ:", c.eval(READ))
