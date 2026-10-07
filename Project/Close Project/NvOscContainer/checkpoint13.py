# checkpoint13.py — สถานะหลังทำ Host ใหม่ + ไล่โซ่ Alt+X (2026-10-07 09:4x)

## ✅ Host ใหม่ (NVIDIA OSC.exe) — เสร็จ ใช้งานได้
- โมเดลใหม่ตามสั่ง OWNER: โปร่งใส Topmost เต็มจอ **ค้างบนจอตลอด ไม่ปิด ไม่ซ่อน**
  - ปิด = WS_EX_TRANSPARENT+NOACTIVATE (คลิกทะลุทั้งจอ) · เปิด = interactive — สลับผ่าน
    QUERY_WIN_OPEN/CLOSE_OSC (ดูจาก openshare ผ่านหน้า) — ทดสอบผ่านทั้งคู่ (log "[win] click-through ON/OFF")
  - index.html โหลดค้างตั้งแต่บูต (serve จากซอร์ส osc\ ผ่าน :3000) · zoom แท้คงเดิม
    (log: fit=0.875000 level=-0.732395) · ULW composite OK · TERMINATED = 0
- **ROOT CAUSE ปม renderer ตายวน (ที่ PARKED มา): v8_context_snapshot.bin ใน deploy เป็นของ
  CEF 138 (699,577B, V8 13.8) ทับ libcef 73 (V8 7.3) → "Version mismatch between V8 binary and
  snapshot" → renderer ตาย exit=3 ทุกตัว** — แก้: copy จาก cef73\Release (688,952B)
  ⚠ ไฟล์นี้ต้องตรวจทุกครั้งที่ deploy (natives/snapshot ที่เคยเติม 06:14 ถูก แต่ลืม v8_context)
- แก้เพิ่ม: watchdog shared-texture ตรวจเฉพาะโหมด GPU (เดิม ExitProcess ทุก 10 วิในโหมด gdi =
  อาการ "ตายรัว ๆ") · CefExecuteProcess ย้ายขึ้นบรรทัดแรกของ wWinMain (child ไม่รันโค้ดเรา)
  · NODE_INFO: jarvis.server "" → "https://accounts.nvgs.nvidia.com" (ค่าแท้จาก piplConfig.json)
- path cache/subprocess/log รวมศูนย์ที่ nvidia-osc.json → deploy ปัจจุบัน
  build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC\ แล้ว (CefCache ใหม่ ไม่ปน cache 138)
- commit ท้องถิ่นแล้ว (Stable, ไม่ push)

## 🔍 โซ่ Alt+X — ไล่ได้ลึกถึงจุดพังตัวจริง
1. **Alt+G ไม่มีในระบบ** — nvsphelper.json จับแค่ Alt+X (OpenShare) — กด Alt+G = เงียบ (ปกติ)
2. helper Alt+X → **POST** /?hk=OpenShare (POST เท่านั้น! GET โดน express.static กิน —
   static ถูก register ก่อน hkHandler ใน index.js แล้วตอบ index.html ให้ "/" — ทดสอบต้องใช้ POST)
3. node __nvFire → HotkeyCallback → EmitNotification → io.emit('/ShadowPlay/v.1.0/Hotkey') ✓
   พิสูจน์ด้วย raw socket.io client (probe-sio.py): ได้รับ 42["/ShadowPlay/v.1.0/Hotkey",{hotKeyName}]
4. **หน้า connect socket เองได้แล้ว** (node log "Socket ... connected" ตอน boot หลัง NODE_INFO ถูก)!
5. **จุดพังสุดท้าย (ในหน้า): openOSC โยน "Cannot read property 'indexOf' of undefined"**
   - stack: vendor i/l/s/g = getClientTelemetryConsent (NvAccount HTTP endpoint) ← k getAppInFocus ← openOSC
   - boot log: "No valid logged in user detected, falling back to client consent setting"
   - runTimeConfigService.getConfig() = EMPTY — **หน้าไม่มี piplConfig data**
   - node เสิร์ฟ GET /PiplConfig/v.1.0/data 200 แล้ว (cache ProgramData\NVIDIA Corporation\NvNode\piplConfig.json)
     แต่ **ไม่มี socket push '/PiplConfig/v.1.0/update' ตอน boot** (NvPiplConfig push เฉพาะตอน cloud-fetch
     เปลี่ยน — cache path ไม่ push) — genuine ได้ทั้ง HTTP + socket push ตอน node ออนไลน์

## ⏭ งานต่อ (Alt+X ครบวงจร)
1. NvPiplConfig: push '/PiplConfig/v.1.0/update' (cache data) ตอน io connection / boot — ให้หน้ามี
   jarvis/account server + clientId → consent ไม่ undefined → openOSC ไม่โยน
2. reload หน้า → fire POST /?hk=OpenShare → เมนูลงจอ (host พร้อม — WIN_OPEN ทำงานแล้ว)
3. เครื่องมือ: probe-sio.py (client ทดสอบ broadcast), probe-register.py, osc-manual-chain.py,
   probe-ws.py, osc-connect.py (Close Project\NvOscContainer) · childwrap.cpp = จับ exit code child
4. ทดสอบมือชั่วคราว: CDP `inj.get('oscDisplayService').openOSC()` ยัง throw จนกว่าข้อ 1 เสร็จ
