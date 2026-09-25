# Build.exe → Desktop Web App (WebView2) + Hardcore Version + Preview

## สถาปัตยกรรม (เลือก WebView2 แทน CEF — ผลลัพธ์เหมือนกัน เบากว่ามาก)
`Build\Build.exe` = **แอปเดสก์ท็อปหน้าต่างเดียว** (net10.0-windows, WinExe) ที่ข้างในเป็น **WebView2** render เว็บแอป SPA ตัวเต็ม — ไม่มี console, ไม่มี port, ไม่ต้องก๊อป CEF runtime
- JS คุยกับ C# ตรง ๆ ผ่าน host object (ไม่ต้องมี HTTP server เลย — ตัดปัญหา port/CORS หมด)
- เหตุผลที่ไม่ใช้ CEF: ต้องก๊อป CEF runtime + C++ host เพิ่มทั้งชุด เพื่อ UI เหมือนกัน — WebView2 ให้ผลเดียวกันและเครื่องรองรับอยู่แล้ว
- หน้าต่าง: 1100×780, ชื่อ "NVIDIA ShadowPlay — Build", ไอคอน NVIDIA, ปิด = จบแอป

## เว็บแอป 4 หน้า (SPA, ธีม NVIDIA ดำ-เขียว, hash routing)

**1. Dashboard** — การ์ด: BuildVer (build.txt), commit, build ล่าสุด, สถานะ process
**2. Build** — ปุ่ม Build / Clean Build + **log สด real-time** (stream จาก build-dev.ps1) + ปุ่ม disabled กันกดซ้ำระหว่าง build
**3. Version (Hardcore)** — toggle "บังคับทุกโปรเจค ON/OFF" + ช่อง Version (default 3.41) + Company = Duluka Corporation + Authors = ScotcsDuluka + Product/Copyright = © 2026 Duluka Corporation + ปุ่มบันทึก + ตัวอย่าง FileVersion ที่จะได้
**4. Preview** — สแกน `Build\NVIDIA ShadowPlay`: ต้นไม้ไฟล์ + รายการ .exe/.dll พร้อม FileVersion จริง + ขนาดรวม + ปุ่ม รัน Launcher.exe / เปิดโฟลเดอร์

## Version injection (MSBuild — 2 ไฟล์)
1. **Directory.Build.props** (root): เพิ่ม `<HardcoreVersion>` default ว่าง + import `Build\Build-Config\version.props` เมื่อมีไฟล์
2. **Directory.Build.targets** line 84: `_Bl1Version` แยกเงื่อนไข — HardcoreVersion ว่าง → `3.41.<N>.61` (เดิม) / ไม่ว่าง → `<HardcoreVersion>.<N>.61`
3. ผล: ทุก .NET assembly เปลี่ยนตาม toggle ทันที (InformationalVersion แนบ +build.N — **BuildVer เพิ่มปกติ** ✓)

## Config (Build\Build-Config\)
- **`version.json`** (tracked) = ค่าจริง: hardcore on/off, version, company, authors, product, copyright
- **`version.props`** (generated, gitignored) = Build.exe เขียนก่อน build ทุกครั้ง
- **`build.txt`** = BuildVer counter (มีอยู่ — 3850)

## ขั้นตอนทำงาน
1. เก็บสถานะ dirty ปัจจุบัน (252+ ไฟล์ restructure จากเครื่อง Intel) → commit รักษาไว้
2. ซ่อม sln/slnf/build-dev.ps1 paths ให้ตรง layout ปัจจุบัน (ตรวจทุก path — ปัจจุบัน slnf 22/22 เสีย) ยืนยันด้วย restore ผ่าน
3. เขียน BuildTool ใหม่ (WinForms + WebView2 + bridge) + wwwroot SPA → publish → Build\Build.exe
4. ทดสอบ: เปิดแอป → Build → log สด → hardcore ON → build → ตรวจ FileVersion ใน Preview = เวอร์ชันที่ตั้ง → OFF → กลับ 3.41.x
5. commit + push (source + wwwroot + version.json + Directory.Build edits)

## หมายเหตุ
- Microsoft.Web.WebView2 NuGet (ดาวน์โหลดครั้งเดียว) + WebView2 Runtime (Win10/11 มีในตัว)
- Version injection ครอบคลุม .NET projects ทั้งหมด (vcxproj CEF เป็น C++ ไม่กระทบ)
- ถ้าเซสชันฝั่ง Intel push อะไรเพิ่ม → pull ก่อนทุกขั้น (workflow สองเครื่องปกติ)