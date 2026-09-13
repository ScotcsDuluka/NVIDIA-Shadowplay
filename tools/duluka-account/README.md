# Duluka Account — mod tools (in-place, reversible)

Mod ชั้น account ของ GFE/OSC ที่ติดตั้งอยู่: เปลี่ยน NVIDIA Account เป็น
**Duluka Account** โดยไม่แก้ไฟล์ osc เลย — ใช้จุด injection ที่ NVIDIA
ออกแบบไว้เอง (`piplConfig.json` → `/PiplConfig/v.1.0/update`)

## ไฟล์

| ไฟล์ | หน้าที่ |
|---|---|
| `apply.ps1` | แก้ `piplConfig.json`: `jarvis.server` → Duluka (`127.0.0.1:59870`), ปิด gfwsl/aem/vrs/jsEvents/telemetry, ต่ออายุ expiry |
| `revert.ps1` | คืนค่าเดิมจาก backup |
| `piplConfig.backup.json` | ของเดิม (auto-backup ครั้งแรก) |
| `stub-listener.ps1` | Duluka stub — log ทุก request ที่ osc/GFE ยิงมา = **สัญญา API ที่ Duluka server จริงต้อง implement** |

## การใช้

```powershell
powershell -File apply.ps1          # ใช้ mod
powershell -File stub-listener.ps1  # รัน Duluka stub (จับ shapes)
powershell -File revert.ps1         # คืนของเดิม
```

## ข้อควรรู้

- NvNode **refresh ไฟล์นี้เอง** (daysToExpire) — ถ้าถูกเขียนทับ รัน apply.ps1 ใหม่ (idempotent)
- apply แล้วให้รัน stub-listener ก่อนเปิด GFE/overlay เพื่อจับ shapes
- การ login ของ osc เปิด browser OAuth → loopback capture
  (QUERY_HTTPSERVER_START) — stub ต้องขยายรับ flow นี้เมื่อรู้ shapes แล้ว
- ไม่มีไฟล์ NVIDIA ถูก commit ลง repo — เก็บแค่ scripts + backup config
