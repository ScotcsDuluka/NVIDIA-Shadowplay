# osc-manual-chain.py — ลำดับมือเต็ม checkpoint เดิม: reload -> connect -> fire -> ตรวจ
import json, sys, time, urllib.request
from osc_hack import CDP, http_get

def find_page():
    pages = json.loads(http_get("http://127.0.0.1:59099/json"))
    for p in pages:
        if p.get("type") == "page" and "/index.html" in p.get("url", ""):
            return p.get("webSocketDebuggerUrl")
    return None

def fire():
    req = urllib.request.Request("http://127.0.0.1:59011/?hk=OpenShare")
    return urllib.request.urlopen(req, timeout=8).status

ws = find_page()
if not ws:
    print("no page"); sys.exit(2)
c = CDP(ws)

# 1) reload + รอ boot chain
c.call("Page.enable")
c.call("Page.reload", {"ignoreCache": True})
print("reloaded — รอ 8 วิ boot chain")
time.sleep(8)

# 2) connect socket (หลัง NODE_INFO ตอบแล้ว)
res = c.eval("""(function(){ try {
  var inj = angular.element(document).injector();
  if (!inj) return 'no injector';
  var s = inj.get('socketService');
  if (!s) return 'no socketService';
  s.connect();
  return 'connect() ok, hash=' + location.hash;
} catch(e) { return 'ERR: ' + e.message; } })()""")
print("CONNECT:", res)
time.sleep(3)

# 3) แปะ probe ก่อน fire
c.eval("""(function(){ try {
  var inj = angular.element(document).injector();
  var rs = inj.get('$rootScope');
  window.__probe = {hotkeyEvent: [], openshare: [], hash: []};
  rs.$on('hotkeyEvent', function(ev, d){ try { window.__probe.hotkeyEvent.push(JSON.stringify(d)); } catch(e){} });
  rs.$on('openshare', function(){ window.__probe.openshare.push('fired'); });
  rs.$on('$routeChangeSuccess', function(){ window.__probe.hash.push(location.hash); });
  return 'ok';
} catch(e) { return 'ERR: ' + e.message; } })()""")

# 4) fire
print("FIRE:", fire())
time.sleep(4)

# 5) อ่านผล
post = c.eval("""(function(){ try {
  var inj = angular.element(document).injector();
  try { inj.get('$rootScope').$apply(); } catch(e){}
  return JSON.stringify({probe: window.__probe, hash: location.hash,
    tiles: document.querySelectorAll('.list .list-item, .menu-bar, [class*=tile]').length});
} catch(e) { return 'ERR: ' + e.message; } })()""")
print("POST:", post[:600])
try:
    c.screenshot("osc-chain.png")
    print("saved: osc-chain.png")
except Exception as e:
    print("shot:", e)
