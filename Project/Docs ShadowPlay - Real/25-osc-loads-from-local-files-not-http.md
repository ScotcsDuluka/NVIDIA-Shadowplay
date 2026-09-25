# ⭐ osc loads from LOCAL FILES (file://), not from the backend — the black-screen root cause

> ข้อค้นพบปิดเคสจอดำ — 2026-09-25 ยืนยันด้วย CDP (remote debugging) บน
> host ของเราที่รันด้วย libcef NVIDIA + --remote-debugging-port=9222

## ข้อเท็จจริงที่วัดได้

1. **Genuine Web Helper (:59001) เป็น API-only** — ไม่เสิร์ฟ static files:
   - `/index.html` → 404, `/osc/index.html` → 404, `/vendor.js` → 404, `/` → 404
   - แต่ API routes ตอบปกติ (`POST /ShadowPlay/v.1.0/Hotkey/Toggle` → 200)
2. `NVIDIA Share.json` ตัวจริง: `nv-url-relative=osc/index.html` = **relative
   กับโฟลเดอร์ของ Share.exe** (GFExperience\osc\) → โหลดผ่าน **file://**
   (แปลที่ผิดมาตลอด: คิดว่า relative กับ HTTP root)
3. ที่มาของ CORS/`disableSecurity=1`: หน้าจาก file:// (origin null) ต้องเรียก
   API ข้าม origin ที่ 127.0.0.1:59001 — backend เลยต้องเปิด CORS + ปิด cookie
4. **host เราเคยพาไป `http://127.0.0.1:59001/index.html`** → backend จริงตอบ
   404 error page (DOM 549 ตัวอักษร, ไม่มี Angular, readyState=complete) —
   **นี่คือกล่องดำทั้งหมด** ไม่เกี่ยวกับ libcef/render/GPU เลย
   (libcef NVIDIA vs stock ยังเป็นข้อสังเกตที่ถูกของตัวมันเอง — stock ค้าง
   "Not responding", NVIDIA ตอบสนอง — แต่จอดำเป็นเรื่อง URL)

## การพิสูจน์ (CDP กับ host เรา, libcef NVIDIA)

Page.navigate → `file:///.../Build/NVIDIA ShadowPlay/NvOverlay/Cef/Resources/osc/index.html`:
- `angular: true`, `htmlLen: 776,043`, `scripts: 5`
- services บูตครบ (jsEvents, PiplConfig → ดึง config จาก Duluka server
  192.168.1.254:5115 ได้จริง, telemetryService, ugcService, gamepadService…)
- `socketService: "socket connect"` → **"socket io event connect"** — socket.io
  v2 client ในหน้า เชื่อม backend 59001 ได้จริง
- uiRouter resolve ไป `#/base`
- error ที่เหลือเป็น API-level (404 HardwareInformation/v.0.1 บน genuine,
  401 Account/UserToken — หน้า handle ต่อได้ ไม่ถึงขั้นพัง)

## สิ่งที่ต้องแก้ใน host เรา (เมื่อจะกลับมาทำต่อ)

`share_main.cpp` — URL construction ให้เป็น file:// ตาม nv-url-relative:
```
url = file:// + <exe dir>\ + nv-url-relative     (จาก NVIDIA Share.json ของเรา
                                                  ปรับเป็น "osc/index.html" และ
                                                  stage osc ไว้ข้าง exe)
```
+ backend เรา (NvBackend) ต้องเปิด CORS ให้ origin null (file://) — ตอนนี้
  backend เราเสิร์ฟ osc เองผ่าน HTTP อยู่ (ต่างจากของจริง) — ทางเลือก:
  (ก) host โหลด file:// + backend API-only+CORS (ตรงแบบ GFE) หรือ
  (ข) คง HTTP-serving แล้ว host โหลด http:// — แต่ต้องรู้ว่าต่างจากของแท้

## สถานะการใช้งาน

- **เครื่องนี้ใช้ของ NVIDIA (GFE ported) เป็น osc หลัก** — Alt+Z + osc รันปกติ
- lane เรา: build เขียว + parity ครบ + รู้สาเหตุจอดำแล้ว (เหลือแก้ URL เป็น
  file:// บรรทัดเดียว) — พร้อมเปิดใหม่ตามจังหวะ "ค่อย ๆ port"
