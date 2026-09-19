# 18 — SESSION HANDOFF (2026-09-19 ผ่าตัดคืนนี้)

## STATUS: NVIDIA ShadowPlay จริง 100% บน 1080 Ti ✅ + Intel wire-through ✅

## WHAT WORKS NOW (1080 Ti)
- Alt+Z overlay เปิดได้, IR running, ทุกหน้า settings
- Backend: original NVIDIA index.js (post-repair, ไม่มี patch)
- osc files: pristine (app.js == app.js.orig)
- Share.json: pristine (nv-url-relative=osc/index.html)
- Hotkey edge-detection patch อยู่ใน NvShadowPlayAPI.js (prevent key repeat)

## INTEL MACHINE (HUAWEI-PC)
- Overlay เปิดได้ผ่าน wire-through (hotkey-listener.ps1 → POST → emit WindowState)
- Alt+Z ใช้ได้ ✅ (user ยืนยัน)
- Settings pages: floor ครอบคลุมแล้วแต่บางหน้าอาจยังขาดข้อมูล
- hardware-floor.json: per-machine WMI (อยู่ใน install-host.ps1)
- Duluka Server: รันที่ :5115, Tailscale: desktop-duluka.taile6314b.ts.net (100.93.212.28)

## DULUKA INTEGRATION (รอทำ)
- piplConfig.json ปัจจุบันยังชี้ NVIDIA cloud
- ต้องแก้: jarvis.server → http://127.0.0.1:5115 (หรือ Tailscale URL)
- Duluka.Server: .NET 10, :5115, build ผ่าน, DB schema v4
- DulukaModsAPI.js + DulukaAPI.js: พร้อมใน WebHelper/ (ยังไม่ deploy บน 1080 Ti)

## FILES MODIFIED ON 1080 Ti (all pristine now)
- osc/app.js = original (restore แล้วหลัง build experiments)
- osc/index.html = original (ไม่มี bridge/inject)
- osc/user-config.js = original
- NvNode/index.js = NVIDIA original (ไม่มี Duluka patch)
- NvShadowPlayAPI.js = original + edge-detection patch
- NvNode/nvnodejslauncher.exe = restored (เคยหาย!)
- NVIDIA Share.json = clean (ไม่มี debug switch)

## KEY LESSONS (ห้ามลืม)
1. app.js 1.6MB minified = ห้ามแก้ตรง (พังทั้ง overlay)
2. Share.json: `\` ต้องเป็น `\` ใน JSON + ห้ามใส่ CEF switches ที่ host ไม่รู้จัก
3. nvnodejslauncher.exe สำคัญ — ถ้าหาย CEF โหลด file:// = ตายทั้งระบบ
4. NvNode registry keys ต้องมีทั้ง 2 view (SOFTWARE + WOW6432Node)
5. osc pages = raw escaped form แก้ยาก — ใช้ mods/ system แทน
6. nodejs.json = installer สร้างเอง (dispatch table API versions)
7. context bridge ต้อง inline ก่อน DOCTYPE ไม่ได้ (quirks mode)

## NEXT STEPS
1. Intel: ทดสอบ settings ทุกหน้า + floor state store (persistent POST)
2. piplConfig: ชี้ jarvis.server → Duluka Server
3. Mods: Duluka panel, Game Filter, Theme Engine (ใช้ mods/ system)
4. CaptureEngine: ต่อผ่าน engine-endpoint.json เมื่อพร้อม

## KEY FILES
- NVIDIA-Host-Portable/ (Downloads): portable package + install script
- docs/osc/16, 17: blueprints
- docs/osc/real-floor/: real backend captures
- WebHelper/: patched backend files (git)
