# osc-connect.py — สั่งหน้า connect socket แล้ว fire OpenShare (ลำดับมือ checkpoint เดิม)
# ใช้: python osc-connect.py
import json, sys, time, urllib.request
from osc_hack import CDP, http_get

CDP_HTTP = "http://127.0.0.1:59099"

def find_page():
    info = http_get(CDP_HTTP + "/json")
    if not info:
        return None
    pages = json.loads(info)
    for p in pages:
        if p.get("type") == "page" and "/index.html" in p.get("url", ""):
            return p.get("webSocketDebuggerUrl")
    return None

def fire_openshare():
    req = urllib.request.Request("http://127.0.0.1:59011/?hk=OpenShare")
    r = urllib.request.urlopen(req, timeout=8)
    print("fire :59011 ->", r.status)

def main():
    ws = find_page()
    if not ws:
        print("ไม่เจอหน้า /index.html บน CDP :59099"); sys.exit(2)
    c = CDP(ws)

    # 1) สถานะก่อน
    pre = c.eval("""(function(){ try {
      var inj = angular.element(document).injector();
      if (!inj) return 'no injector';
      var names = ['socketService','oscSocketService','socket'];
      var found = names.filter(function(n){ try { return !!inj.get(n); } catch(e){ return false; } });
      return JSON.stringify({hash: location.hash, services: found});
    } catch(e) { return 'ERR: ' + e.message; } })()""")
    print("PRE:", pre)

    # 2) สั่ง connect
    res = c.eval("""(function(){ try {
      var inj = angular.element(document).injector();
      var s = inj.get('socketService');
      if (!s) return 'no socketService';
      var before = 'n/a';
      try { before = s.socket ? (s.socket.connected ? 'connected' : 'disconnected') : 'no-socket-field'; } catch(e){}
      s.connect();
      return 'connect() called, before=' + before;
    } catch(e) { return 'ERR: ' + e.message; } })()""")
    print("CONNECT:", res)
    time.sleep(2.5)

    # 3) ยืนยัน socket ต่อแล้ว (ฝั่ง node)
    import subprocess
    ns = subprocess.run(["netstat", "-ano"], capture_output=True, text=True).stdout
    est = [l for l in ns.splitlines() if ":59011" in l and "ESTABLISHED" in l]
    print("ESTABLISHED :59011 =", len(est))

    # 4) fire OpenShare
    fire_openshare()
    time.sleep(3)

    # 5) สถานะหน้าหลัง fire
    post = c.eval("""(function(){ try {
      return JSON.stringify({hash: location.hash,
        rects: !!document.querySelector('[ng-style],.nv-osc,.osc-content,.main-menu-container'),
        menu: !!document.querySelector('.list,.menu-bar,[class*=main-menu]')});
    } catch(e) { return 'ERR: ' + e.message; } })()""")
    print("POST:", post)
    try:
        c.screenshot("osc-after-fire.png")
        print("saved: osc-after-fire.png")
    except Exception as e:
        print("shot:", e)

main()
