# Phase 2 — ย้ายโฟลเดอร์ build tree (ห้ามทำก่อน OSC นิ่ง 3-4 วัน)

## เงื่อนไขก่อนทำ
1. OSC บูตสำเร็จต่อเนื่อง ≥ 3 วัน (ไม่มี ud2 / watchdog respawn ใน log)
2. Record + Alt+Shift+Z + คลิกทะลุ ทดสอบผ่านครบ
3. Backup: `robocopy "build\NVIDIA ShadowPlay" "D:\backup\NVIDIA ShadowPlay" /MIR` (หรือ drive อื่น)

## จุดที่ต้องแก้เมื่อย้าย (path รวมศูนย์แล้ว = แก้ที่นี่จุดเดียว)
| ไฟล์ | คีย์ | หมายเหตุ |
|---|---|---|
| `Project\NvConfig\nvcontainer.json` | `buildRoot`, `children.*`, `nodeApiPort`, `logsDir` | แม่อ่านทุก path จากนี่; `ours` ดูแล Web Helper → NodeAPI :59011, OSC, nvsphelper (ยังไม่ start NvCapture) |
| `Project\NvConfig\nvidia-osc.json` | `page`, `logFile`, `cef.cachePath`, `cef.subprocessPath` | OSC native อ่าน |
| `Project\NvConfig\webhelper.json` | (ถ้ามี path) | node backend |
| `NvContainer-USERS\NvContainer.cs:25` | `ROOT` | คงที่ (repo root ไม่ย้าย) |
| Scheduled task `Duluka-NvContainer` | `schtasks /change /tr "<new path>"` | เปิดแม่ตอน sign-in; ต้องอยู่ใน user session เพื่อให้ OSC แสดงผล |

## Controller startup and ownership
- `NvContainer.exe` is the single-instance parent; it starts `NVIDIA Web Helper.exe`. That host owns `NvNode.exe` in its job object and serves NodeAPI on `127.0.0.1:59011`; the parent watchdog monitors the host and NodeAPI health, along with OSC and `nvsphelper.exe`.
- Register `build\NVIDIA ShadowPlay\NvContainer\NvContainer.exe` with the per-user `Duluka-NvContainer` logon task. Do not run the interactive OSC host as a boot-time service in session 0.
- Keep `children.webHelperExe` and `children.webHelperWd` pointed at the paired project NodeAPI host; it owns `NvNode.exe` and is outside `buildRoot`.

## สิ่งที่ phase 1 ทำไปแล้ว (2026-09-30)
- osc_main.cpp อ่าน `cachePath` / `subprocessPath` / `debugPort` จาก nvidia-osc.json (ไม่ hardcode แล้ว)
- OSC host ที่ใช้งานอยู่คือ `build\...\Overlay OSC\NVIDIA OSC\NVIDIA OSC.exe` และเป็น path ที่ `nvcontainer.json` ให้ NvContainer spawn
- native exe ใน `Overlay OSC\NVIDIA OSC Native\` ไม่ใช่ target ของ NvContainer

## checklist หลังย้าย (ต้องผ่านทุกข้อ)
- [ ] แม่ spawn ครบ: NVIDIA Web Helper.exe → NodeAPI :59011 + NVIDIA OSC.exe + nvsphelper.exe (ดู NvContainer.log)
- [ ] ยืนยันว่า NvCapture.exe ยังไม่ถูก spawn โดย NvContainer (รอ Phase 3)
- [ ] OSC: `CreateBrowserSync=OK` + `frame loaded` + `composite PRESENT ok`
- [ ] คลิกทะลุตอนปิด (WindowFromPoint ชี้หน้าต่างอื่น)
- [ ] Alt+Shift+Z เปิด/ปิดได้ + เมาส์ไม่ค้าง
- [ ] Record → ไฟล์เกิดใน Videos (ffmpeg ชุด DLL ครบ)
- [ ] ไม่มี ud2/0x80000003 ใน Event Viewer 15 นาทีหลังบูต

## ข้อห้าม
- ห้าม overwrite `Overlay OSC\NVIDIA OSC\NVIDIA OSC.exe` (CefSharp) ด้วย native exe
- ห้ามลบ `NvNode\ffmpeg\` (ชุด DLL อ้างอิงของ CaptureEngine)
- ห้ามแตะ `nvcontainer.exe` (lowercase — ของแท้)
