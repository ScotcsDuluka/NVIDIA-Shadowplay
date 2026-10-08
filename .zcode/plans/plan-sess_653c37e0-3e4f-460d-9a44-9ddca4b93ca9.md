พัฒนา BuildTool (tools\BuildTool) — แตะแค่ Bridge.cs / MainForm.cs / app.js / app.css:

**แก้ให้ทำงานจริง**
1. MainForm.cs ArgsStr — ไข JSON string element ด้วย GetString() แทน GetRawText() (แก้ saveVersion/saveVersions ที่ encode ซ้อน 2 ชั้นแล้ว Deserialize throw ทุกครั้ง) และใช้ ArgsStr กับ saveVersions
2. MainForm.cs RpcReceived — message ธรรมดา (JSERR/DOM) ที่ไม่ใช่ JSON ให้ log เป็น note แทน exception "envelope failed" รัวใน ui-log
3. Bridge.cs StartBuild/Exited — คืนเลข build ที่กำลังจะได้ (ตอนนี้คืนเลขเก่า), log "BUILD OK" เฉพาะ exit=0, log "BUILD FAILED (exit N)" เมื่อล้ม; app.js pollLog แสดง FAILED สีแดง + ตอนเริ่มแสดง "กำลัง build #N"
4. app.js Dashboard — เปลี่ยน process ที่ไม่มีจริง (NvShadowPlay/NvNotifier) เป็นชุด staging จริง: NvContainer, nvsphelper64, NvBackend, NVIDIA Web Helper, NVIDIA Share (การ์ด OVERLAY → NVIDIA Share)

**เพิ่มความสามารถ**
5. ปุ่ม CANCEL BUILD (โผล่เฉพาะตอนรัน) + Bridge.CancelBuild() = Kill(entireProcessTree:true) + ไม่เพิ่ม counter
6. MainForm OnFormClosing — ถ้า build กำลังรัน ยืนยันก่อนปิด + kill process tree ตอนปิด (กัน orphan powershell)
7. RootLocator — หา repo root ด้วย marker Directory.Build.props (เดินขึ้นจาก BaseDirectory) — dev-run จาก bin\Debug ก็ถูก
8. Cache git commit 60 วินาที — เลิก spawn git ทุก poll 5 วินาที

**ความแข็งแรง**
9. Bridge.cs RegenVersionsProps — ใช้ XmlEsc เต็ม (contract _Bl1ProjectVersion/HardcoreVersion คงเดิม, ไม่แตะ Directory.Build.* / build-dev.ps1)
10. app.js esc() escape คำพูดด้วย + ปุ่มคัดลอก log; app.css เพิ่ม class .red

ตามสั่งเจ้าของ: **ไม่รัน build ทดสอบใดๆ** (ไม่ dotnet build / ไม่ build-dev.ps1 / ไม่เปิดโปรแกรม) — ตรวจด้วยการอ่านโค้ดเท่านั้น ไฟล์ config ที่ generate (Build-Config\*) ไม่ถูกแตะตอน implement