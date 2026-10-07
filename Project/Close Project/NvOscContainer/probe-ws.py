# probe-ws.py — ดัก websocket frame ที่วิ่งเข้าหน้าผ่าน CDP Network domain
import json, sys, time, urllib.request, socket, base64, hashlib, struct
from osc_hack import http_get

GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"

def cdp_ws_connect(wsurl):
    # wsurl = ws://127.0.0.1:59099/devtools/page/<id>
    u = wsurl.replace("ws://", "")
    host, rest = u.split("/", 1)
    sock = socket.create_connection((host.split(":")[0], int(host.split(":")[1])), timeout=5)
    key = base64.b64encode(b"0123456789abcdef").decode()
    req = (f"GET /{rest} HTTP/1.1\r\nHost: {host}\r\nUpgrade: websocket\r\n"
           f"Connection: Upgrade\r\nSec-WebSocket-Key: {key}\r\nSec-WebSocket-Version: 13\r\n\r\n")
    sock.sendall(req.encode())
    resp = b""
    while b"\r\n\r\n" not in resp:
        resp += sock.recv(4096)
    return sock

def ws_send(sock, payload):
    data = payload.encode()
    header = bytearray([0x81])
    n = len(data)
    if n < 126: header.append(0x80 | n)
    elif n < 65536: header += struct.pack(">BH", 0x80 | 126, n)
    else: header += struct.pack(">BQ", 0x80 | 127, n)
    mask = b"\x00\x00\x00\x00"  # unmasked payload (ต้อง masked — ใช้ 0 mask)
    header += mask
    sock.sendall(bytes(header) + data)

def ws_recv(sock, timeout=4):
    sock.settimeout(timeout)
    try:
        b1, b2 = sock.recv(2)
        n = b2 & 0x7F
        if n == 126: n = struct.unpack(">H", sock.recv(2))[0]
        elif n == 127: n = struct.unpack(">Q", sock.recv(8))[0]
        data = b""
        while len(data) < n: data += sock.recv(n - len(data))
        return data.decode(errors="replace")
    except Exception:
        return None

pages = json.loads(http_get("http://127.0.0.1:59099/json"))
ws = None
for p in pages:
    if p.get("type") == "page" and "/index.html" in p.get("url", ""):
        ws = p.get("webSocketDebuggerUrl")
if not ws:
    print("no page"); sys.exit(2)

sock = cdp_ws_connect(ws)
mid = 0
def send(method, params=None):
    global mid
    mid += 1
    ws_send(sock, json.dumps({"id": mid, "method": method, "params": params or {}}))
    return mid

send("Network.enable")
time.sleep(0.5)
# เคลียร์ของค้าง
ws_recv(sock, 1)

print("FIRE:", urllib.request.urlopen(urllib.request.Request(
    "http://127.0.0.1:59011/?hk=OpenShare", data=b""), timeout=8).status)

frames = []
end = time.time() + 4
while time.time() < end:
    msg = ws_recv(sock, 1.5)
    if not msg: continue
    try:
        m = json.loads(msg)
    except Exception:
        continue
    meth = m.get("method", "")
    if "webSocket" in meth.lower():
        fr = m.get("params", {}).get("response", {})
        frames.append((meth, str(fr.get("payload", ""))[:200]))
    elif meth == "Network.webSocketFrameReceived":
        pass

if frames:
    for f in frames[:12]:
        print("WS:", f)
else:
    print("NO websocket frames ที่วิ่งเข้าหน้าตอน fire (มองเฉพาะ ws events)")
