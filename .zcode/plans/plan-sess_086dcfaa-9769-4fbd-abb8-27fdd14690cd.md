# ขั้น 1: index.js boot จน :59001 route ครบจริง (layer โดดเดี่ยว)

## กติกา (ตามที่คุณกำหนด)
- ไม่แตะ CaptureEngine/Hotkey เข้ามาปนในขั้นนี้ — Node Backend ต้องสมบูรณ์ก่อน
- ไม่เอา runtime กลับไป NvOverlay\Cef\ (ตรงนั้น = host/source ของ CEF/OSC/NvShim)

## ขั้นตอน
1. Kill node/Web Helper เก่า (เคลียร์ port 59001)
2. `node index.js` จาก `Project\NvShadowPlayAPI\NvAPI\` (node v24 + bypass + shims ที่แก้ไว้)
3. ตรวจ boot: log จน "Initialization complete" + :59001 LISTENING
4. ทดสอบ route ครบชุดที่หน้าใช้จริง (17 endpoints จาก capture + Hotkey/:hk GETs + OSC/MainView) — ทุกเส้นต้องไม่ 404
5. เจอ error ไหนแก้ไฟล์นั้นทันที (layer โดดเดี่ยว = รู้ทันทีว่าผิดที่ Node)
6. จบขั้น 1 = :59001 ตอบครบทุก route

## ขั้นต่อไป (หลังขั้น 1 ผ่าน — ไม่ทำในขั้นนี้)
Genuine Share.exe ↔ Node → nvsphelper64 ↔ Node → Hotkey → Record → CaptureEngine → FILE

## เก็บ: commit หลังขั้น 1 ผ่าน