# Phase 2 — ย้ายโฟลเดอร์ build tree (ห้ามทำก่อน OSC นิ่ง 3-4 วัน)

## เงื่อนไขก่อนทำ
1. OSC บูตสำเร็จต่อเนื่อง ≥ 3 วัน (ไม่มี ud2 / watchdog respawn ใน log)
2. Record + Alt+Shift+Z + คลิกทะลุ ทดสอบผ่านครบ
3. Backup: `robocopy "build\NVIDIA ShadowPlay" "D:\backup\NVIDIA ShadowPlay" /MIR` (หรือ drive อื่น)

## จุดที่ต้องแก้เมื่อย้าย (path รวมศูนย์แล้ว = แก้ที่นี่จุดเดียว)
| ไฟล์ | คีย์ | หมายเหตุ |
|---|---|---|
| `Project\NvConfig\nvcontainer.json` | `buildRoot`, `children.*`, `logsDir` | แม่อ่านทุก path จากนี่ |
| `Project\NvConfig\nvidia-osc.json` | `page`, `logFile`, `cef.cachePath`, `cef.subprocessPath` | OSC native อ่าน |
| `Project\NvConfig\webhelper.json` | (ถ้ามี path) | node backend |
| `NvContainer-USERS\NvContainer.cs:25` | `ROOT` | คงที่ (repo root ไม่ย้าย) |
| Scheduled task `Duluka-NvContainer` | `schtasks /change /tr "<new path>"` | แม่ |

## สิ่งที่ phase 1 ทำไปแล้ว (2026-09-30)
- osc_main.cpp อ่าน `cachePath` / `subprocessPath` / `debugPort` จาก nvidia-osc.json (ไม่ hardcode แล้ว)
- CefSharp fallback exe คืนแล้ว (build\...\Overlay OSC\NVIDIA OSC\NVIDIA OSC.exe = .NET 622KB — ห้าม overwrite ด้วย native อีก)
- deploy native exe: คัดลอกไปที่เดียว = `Overlay OSC\NVIDIA OSC Native\` (watchdog/oscExe ชี้ที่นี่)

## checklist หลังย้าย (ต้องผ่านทุกข้อ)
- [ ] แม่ spawn ครบ: Web Helper + OSC + nvsphelper + NvCapture (ดู NvContainer.log)
- [ ] OSC: `CreateBrowserSync=OK` + `frame loaded` + `composite PRESENT ok`
- [ ] คลิกทะลุตอนปิด (WindowFromPoint ชี้หน้าต่างอื่น)
- [ ] Alt+Shift+Z เปิด/ปิดได้ + เมาส์ไม่ค้าง
- [ ] Record → ไฟล์เกิดใน Videos (ffmpeg ชุด DLL ครบ)
- [ ] ไม่มี ud2/0x80000003 ใน Event Viewer 15 นาทีหลังบูต

## ข้อห้าม
- ห้าม overwrite `Overlay OSC\NVIDIA OSC\NVIDIA OSC.exe` (CefSharp) ด้วย native exe
- ห้ามลบ `NvNode\ffmpeg\` (ชุด DLL อ้างอิงของ CaptureEngine)
- ห้ามแตะ `nvcontainer.exe` (lowercase — ของแท้)
