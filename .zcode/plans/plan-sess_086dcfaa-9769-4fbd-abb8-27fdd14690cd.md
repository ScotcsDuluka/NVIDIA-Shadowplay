# NVIDIA OSC Native — host C++ เหมือน Share.exe แท้ทุกประเภท

## หลักการ (เลียนของแท้จากหลักฐาน import + log)
- Native C++ เรียก libcef ตรง ๆ (ไม่มี .NET/WPF ในทาง render)
- **GPU raster เปิด** (`nv-gpu-accel=true` คือค่าแท้)
- Offscreen rendering → composite ขึ้น **layered window** (UpdateLayeredWindow/AlphaBlend เหมือน import ของแท้)
- เรนเดอร์ที่ scale ต่ำกว่าจอแล้วขยาย (เหมือน OSD scale factor 1.14 ของแท้)
- ซ่อน = หยุดวาด (WasHidden) → CPU ~0

## ที่ยืมจากของเดิมได้เลย (Launcher.Cef)
cefQuery router + query handler / loopback HTTP server (จะเสิร์ฟ osc ที่ :3000) / Win32 window code / JSON config / deploy script / โครง vcxproj

## ของใหม่ที่ต้องเขียน (ส่วน "Share.exe แท้")
1. `CefRenderHandler` (GetViewRect ตาม scale, OnPaint, popup)
2. Compositor: buffer → premultiply → UpdateLayeredWindow (+ทาง D3D11 readback ถ้าได้ shared texture)
3. Input forwarding: WndProc mouse/key → SendMouseEvent/SendKeyEvent (หน้าที่ของ offscreen_window ของแท้)
4. Query handler ชุดคำสั่ง OSC: QUERY_OSC_REGISTER_CLOSE_EVENT(persistent)/FULLSCREEN_STATE/WIN_CLOSE_OSC/WIN_OPEN_OSC/SET_DISPLAY_RECTS/SET_EXPERIMENTAL
5. Toggle listener :59003 + ยิง close-event success ตอนซ่อน

## Phase
- **A. รากฐาน**: ดาวน์โหลด CEF binary รุ่นใหม่ (Chromium 120+) → cef-sdk → build libcef_dll_wrapper → โปรเจกต์ใหม่ "NVIDIA OSC Native" (clone vcxproj) เปิดหน้า osc แบบ windowed ให้ได้ก่อน (พิสูจน์ toolchain + หน้าเว็บรันบน Chromium ใหม่)
- **B. แท้ลักษณะ**: สลับเป็น OSR + layered compositor + input forwarding + โปร่งใส per-pixel
- **C. ต่อระบบ**: page server :3000, toggle :59003, cefQuery ชุด OSC, อ่าน NvConfig\nvidia-osc.json, แม่ (NvContainer-USERS) spawn ตัว native แทน CefSharp
- **D. วัดผล + สลับขั้นสุดท้าย**: เทียบ CPU native vs CefSharp, เก็บ CefSharp WPF เป็น fallback

## ผลลัพธ์สุดท้าย
`build\...\Overlay OSC\NVIDIA OSC\NVIDIA OSC.exe` เป็น native C++ (หน้าตา/พฤติกรรม/สถาปัตยกรรม = Share.exe แท้) — แม่ของเรา spawn, CefSharp เก็บไว้ fallback

## เดิมพัน/ความเสี่ยง
- build wrapper CEF + vcxproj ปรับหลายจุด (cmake + MSBuild) — Phase A เสี่ยงสุด
- ถ้า Chromium ใหม่โหลด bundle ไม่ได้จะเห็นทันทีใน Phase A (แก้ที่ bundle ได้ เพราะเป็นสำเนาของเรา)
- input forwarding OSR ต้องละเอียด (drag/wheel/key) — ทำเป็นชุดย่อยก่อน แล้วเติม