# probe-sio.py — socket.io v2 (EIO=3) raw client ต่อ node :59011 พิสูจน์ broadcast
import json, sys, time, urllib.request, socket, struct, base64

def ws_connect(host, port, path):
    sock = socket.create_connection((host, port), timeout=5)
    key = base64.b64encode(b"0123456789abcdef").decode()
    req = (f"GET {path} HTTP/1.1\r\nHost: {host}:{port}\r\nUpgrade: websocket\r\n"
           f"Connection: Upgrade\r\nSec-WebSocket-Key: {key}\r\nSec-WebSocket-Version: 13\r\n\r\n")
    sock.sendall(req.encode())
    resp = b""
    while b"\r\n\r\n" not in resp:
        chunk = sock.recv(4096)
        if not chunk: raise RuntimeError("handshake failed")
        resp += chunk
    if b"101" not in resp.split(b"\r\n")[0]:
        raise RuntimeError("no upgrade: " + resp.split(b"\r\n")[0].decode())
    return sock

def ws_send(sock, payload):
    data = payload.encode()
    mask = b"\x11\x22\x33\x44"
    header = bytearray([0x81])
    n = len(data)
    if n < 126: header.append(0x80 | n)
    elif n < 65536: header += struct.pack(">BH", 0x80 | 126, n)
    else: header += struct.pack(">BQ", 0x80 | 127, n)
    header += mask
    masked = bytes(b ^ mask[i % 4] for i, b in enumerate(data))
    sock.sendall(bytes(header) + masked)

def ws_recv(sock, timeout=3):
    sock.settimeout(timeout)
    try:
        b1, b2 = sock.recv(2)
        n = b2 & 0x7F
        if n == 126: n = struct.unpack(">H", sock.recv(2))[0]
        elif n == 127: n = struct.unpack(">Q", sock.recv(8))[0]
        if b2 & 0x80:  # masked from server (rare)
            mk = sock.recv(4)
            data = b""
            while len(data) < n:
                data += sock.recv(min(65536, n - len(data)))
            data = bytes(b ^ mk[i % 4] for i, b in enumerate(data))
            return data.decode(errors="replace")
        data = b""
        while len(data) < n:
            chunk = sock.recv(n - len(data))
            if not chunk: break
            data += chunk
        return data.decode(errors="replace")
    except Exception:
        return None

sock = ws_connect("127.0.0.1", 59011, "/socket.io/?EIO=3&transport=websocket")
print("WS handshake OK")
hello = ws_recv(sock, 3)
print("engine.io open:", (hello or "?")[:80])
ws_send(sock, "40")  # join default namespace
print("sent 40 (ns connect)")
got = None
end = time.time() + 8
while time.time() < end:
    m = ws_recv(sock, 2)
    if m is None: continue
    print("recv:", m[:150])
    if "Hotkey" in m:
        got = m
        break

if got:
    print("RESULT: node broadcast ถึง client จริง ✓")
else:
    # ยิงเองระหว่างฟังถ้ายังไม่มี — probe ตอนต้นอาจพลาดช่วง
    print("RESULT: ไม่ได้รับ event ภายใน 8 วิ")
