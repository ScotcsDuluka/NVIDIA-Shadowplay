#!/usr/bin/env python3
# osc_hack.py — ควบคุม/สอดส่องหน้า osc จริงที่รันอยู่ใน Share.exe ผ่าน CDP (:9222)
# ประตูนี้ NVIDIA เปิดเอง (Share.json: nv-remote-debugging-port=9222) — ไม่แตะ binary
#
# ใช้: python osc_hack.py <command>
#   state        สถานะหน้า + สภาพแวดล้อม (viewport/zoom/font)
#   tree         รายการ element โต้ตอบได้ทั้งหมด + ขนาดจริง
#   eval "<js>"  รัน JS อะไรก็ได้ในหน้า (คืนค่า JSON)
#   shot         แคปภาพหน้า osc ตามที่กำลังวาดจริง -> osc_page.png
#   click <css>  คลิก element ตาม selector (ผ่าน CDP Input)
#   open / close เปิด/ปิด overlay (ผ่าน node /OpenOsc)
#   console      สตรีม console.log ของหน้า สด ๆ (เห็นทุก event ที่หน้าได้รับ)
#   watch        state ทุก 2 วิ + console สด (Ctrl+C หยุด)
import base64, hashlib, json, socket, struct, sys, time, urllib.request

CDP_HTTP = "http://127.0.0.1:59099"

def http_get(url, timeout=4):
    try:
        return urllib.request.urlopen(url, timeout=timeout).read().decode(errors="replace")
    except Exception:
        return None

def find_page_ws():
    info = http_get(CDP_HTTP + "/json")
    if not info: return None
    pages = json.loads(info)
    for p in pages:
        if p.get("type") == "page" and "index.html" in p.get("url", ""):
            return p.get("webSocketDebuggerUrl")
    return None

class CDP:
    def __init__(self, wsurl):
        m = __import__("re").match(r"ws://127\.0\.0\.1:(\d+)(/.*)", wsurl)
        self.port, self.path = int(m.group(1)), m.group(2)
        self.sock = socket.create_connection(("127.0.0.1", self.port), timeout=8)
        key = base64.b64encode(b"0123456789abcdef").decode()
        self.sock.sendall((f"GET {self.path} HTTP/1.1\r\nHost: 127.0.0.1:{self.port}\r\n"
                           f"Upgrade: websocket\r\nConnection: Upgrade\r\n"
                           f"Sec-WebSocket-Key: {key}\r\nSec-WebSocket-Version: 13\r\n\r\n").encode())
        resp = b""
        while b"\r\n\r\n" not in resp: resp += self.sock.recv(4096)
        if b"101" not in resp.split(b"\r\n")[0]: raise Exception("CDP handshake failed")
        self._id = 0
        self.console_queue = []

    def _send_raw(self, payload):
        if isinstance(payload, str): payload = payload.encode("utf-8")
        mask = bytes([0x37, 0x13, 0x91, 0x55])
        masked = bytes(b ^ mask[i % 4] for i, b in enumerate(payload))
        hdr = b"\x81"; n = len(payload)
        if n < 126: hdr += bytes([0x80 | n])
        elif n < 65536: hdr += bytes([0x80 | 126]) + struct.pack(">H", n)
        else: hdr += bytes([0x80 | 127]) + struct.pack(">Q", n)
        self.sock.sendall(hdr + mask + masked)

    def _recv_raw(self, timeout=6):
        self.sock.settimeout(timeout)
        h = b""
        while len(h) < 2:
            c = self.sock.recv(1)
            if not c: return None, None
            h += c
        op = h[0] & 0x0F; n = h[1] & 0x7F
        if n == 126: n = struct.unpack(">H", self.sock.recv(2))[0]
        elif n == 127: n = struct.unpack(">Q", self.sock.recv(8))[0]
        data = b""
        while len(data) < n:
            c = self.sock.recv(n - len(data))
            if not c: return None, None
            data += c
        if op == 8: return None, None
        un = bytes(b ^ mask[i % 4] for i, b in enumerate(data)) if False else data
        return op, data

    def call(self, method, params=None, timeout=6):
        self._id += 1
        msg = json.dumps({"id": self._id, "method": method, "params": params or {}})
        self._send_raw(msg)
        deadline = time.time() + timeout
        while time.time() < deadline:
            op, data = self._recv_raw(max(0.2, deadline - time.time()))
            if data is None: break
            try: j = json.loads(data)
            except: continue
            if j.get("id") == self._id:
                if "error" in j: raise Exception(j["error"].get("message", "cdp error"))
                return j.get("result", {})
        raise Exception(f"CDP {method} timeout")

    def eval(self, js, timeout=6):
        r = self.call("Runtime.evaluate", {"expression": js, "returnByValue": True,
                                           "awaitPromise": False}, timeout)
        return r.get("result", {}).get("value")

    def enable_console(self):
        self.call("Runtime.enable")

    def drain_console(self):
        # non-blocking sweep of consoleAPICalled events
        got = []
        self.sock.settimeout(0.15)
        while True:
            try:
                op, data = self._recv_raw(0.15)
            except socket.timeout:
                break
            except Exception:
                break
            if data is None: break
            try: j = json.loads(data)
            except: continue
            if j.get("method") == "Runtime.consoleAPICalled":
                args = j.get("params", {}).get("args", [])
                txt = " ".join(a.get("value", a.get("description", "")) if isinstance(a.get("value"), str)
                               else json.dumps(a.get("value", a.get("preview", "")), ensure_ascii=False)[:120]
                               for a in args)
                t = time.strftime("%H:%M:%S")
                got.append(f"{t} {j['params'].get('type','log')}: {txt[:200]}")
        self.sock.settimeout(6)
        return got

    def screenshot(self, save="osc_page.png"):
        r = self.call("Page.captureScreenshot", {"format": "png"})
        import base64 as b64
        open(save, "wb").write(b64.b64decode(r["data"]))
        return save

def need_cdp():
    ws = find_page_ws()
    if not ws:
        print("หน้า osc ยังไม่โหลด — กด Alt+X เปิด overlay ก่อน (CEF โหลดหน้า = 9222 เปิด)")
        sys.exit(2)
    return CDP(ws)

def cmd_state(c):
    env = c.eval("""JSON.stringify({
  url: location.hash, innerW: innerWidth, innerH: innerHeight,
  scrollbar: innerWidth - document.documentElement.clientWidth,
  dpr: devicePixelRatio, zoom: getComputedStyle(document.body).zoom
})""")
    print("ENV:", env)
    print("RECORD/IR ฝั่ง node:", __import__("urllib.request", fromlist=["urlopen"]).urlopen(
        "http://127.0.0.1:59001/ShadowPlay/v.1.0/Record/Running", timeout=3).read().decode(),
        __import__("urllib.request", fromlist=["urlopen"]).urlopen(
        "http://127.0.0.1:59001/ShadowPlay/v.1.0/InstantReplay/Enable", timeout=3).read().decode())

def cmd_tree(c):
    js = """JSON.stringify((function(){
  var out = [];
  document.querySelectorAll('button, [ng-click], [role=button], a[href], .clickable, li[class]').forEach(function (el) {
    var r = el.getBoundingClientRect();
    if (r.width < 2 || r.height < 2) return;
    out.push({t: el.tagName, cls: (el.className||'').toString().slice(0,40),
              txt: (el.innerText||'').trim().slice(0,30),
              x: Math.round(r.x), y: Math.round(r.y), w: Math.round(r.width), h: Math.round(r.height)});
  });
  return out.slice(0, 40);
})())"""
    items = c.eval(js)
    try:
        for it in json.loads(items):
            print(f"  ({it['x']:4},{it['y']:4}) {it['w']:3}x{it['h']:3}  <{it['t']} {it['cls']}> {it['txt']}")
    except Exception as e:
        print("tree:", items[:400])

def cmd_eval(c, js):
    print(json.dumps(c.eval(js), ensure_ascii=False, default=str)[:2000])

def cmd_click(c, sel):
    js = f"""(function(){{ var el = document.querySelector({json.dumps(sel)}); if (!el) return 'not found';
      var r = el.getBoundingClientRect(); return JSON.stringify({{x: r.x + r.width/2, y: r.y + r.height/2}}); }})()"""
    pos = json.loads(c.eval(js))
    c.call("Input.dispatchMouseEvent", {"type": "mousePressed", "x": pos["x"], "y": pos["y"],
                                        "button": "left", "clickCount": 1})
    c.call("Input.dispatchMouseEvent", {"type": "mouseReleased", "x": pos["x"], "y": pos["y"],
                                        "button": "left", "clickCount": 1})
    print("clicked:", sel, "at", pos)

def cmd_open_close(c, on):
    import urllib.request
    req = urllib.request.Request("http://127.0.0.1:59001/ShadowPlay/v.1.0/OpenOsc",
                                 data=json.dumps({"open": on}).encode(),
                                 headers={"Content-Type": "application/json"}, method="POST")
    print("node resp:", urllib.request.urlopen(req, timeout=8).status)

def cmd_shot(c):
    print("saved:", c.screenshot())

def cmd_console(c):
    print("สตรีม console ของหน้า (Ctrl+C หยุด):")
    c.enable_console()
    while True:
        for line in c.drain_console(): print(" ", line)
        time.sleep(0.3)

def cmd_watch(c):
    print("watch: state ทุก 2 วิ (Ctrl+C หยุด)")
    c.enable_console()
    prev = None
    while True:
        try:
            st = c.eval("""JSON.stringify({hash: location.hash,
                rec: !!document.querySelector('[class*=record]:not([class*=off])'),
                body: document.body.className.slice(0,60)})""")
            if st != prev:
                print("STATE:", st); prev = st
        except Exception as e:
            print("page lost:", e); return
        for line in c.drain_console(): print(" ", line)
        time.sleep(2)

def main():
    if len(sys.argv) < 2 or sys.argv[1] in ("-h", "--help", "help"):
        print(__doc__); return
    cmd = sys.argv[1]
    if cmd in ("open", "close"):
        c = None
        return cmd_open_close(None, cmd == "open")
    c = need_cdp()
    c.enable_console()
    if cmd == "state": cmd_state(c)
    elif cmd == "tree": cmd_tree(c)
    elif cmd == "eval": cmd_eval(c, sys.argv[2] if len(sys.argv) > 2 else "1+1")
    elif cmd == "shot": cmd_shot(c)
    elif cmd == "click": cmd_click(c, sys.argv[2])
    elif cmd == "console": cmd_console(c)
    elif cmd == "watch": cmd_watch(c)
    else: print("unknown:", cmd); print(__doc__)

if __name__ == "__main__":
    main()
