# MISSION: NVIDIA OSC แท้ 100% จาก build path — กำจัดปัญหา nvcontainer.exe

> สำเนาสำหรับ agent ใหม่ (New Chat) — อ่านให้ครบก่อนลงมือ ห้ามข้าม "กติกา" กับ "สูตรที่พิสูจน์แล้ว"

## บริบทโปรเจกต์ (คุณไม่มี context ก่อนหน้า — อ่านส่วนนี้ก่อน)

- Repo: `C:\My Project\NVIDIA-Shadowplay` — branch `Stable`
- เป้าหมายโปรเจกต์: พอร์ต NVIDIA ShadowPlay/OSC (in-game overlay Alt+Z) ให้รันได้โดยไม่ต้องติดตั้ง GFE
- สถานะปัจจุบัน (พิสูจน์แล้ว): boot ของแท้ได้ทั้งกองโดยใช้ไฟล์จาก `build\NVIDIA ShadowPlay\` **ยกเว้น** `nvcontainer.exe` (service NvContainerLocalSystem + container ที่ Watchdog spawn) ที่ยังต้องอยู่ `C:\Program Files\NVIDIA Corporation\NvContainer\`
- คำตัดสินใจล่าสุดของ OWNER: **ใช้ของแท้ NVIDIA ทั้งหมด รันจาก build path ทั้งหมด ไม่พึ่ง Program Files** — ตัวขวางเดียวที่เชื่อกันคือ nvcontainer.exe แท้

### สิ่งสำคัญที่เปลี่ยนเกม (หลักฐานที่ต้องรู้ก่อนเริ่ม)

1. ข้อสรุปเดิม "ShadowPlayController::ValidatePID ปฏิเสธ nvcontainer ที่ไม่อยู่ PF ('Unknown executable path')" **ยังไม่เคยถูกพิสูจน์จริง** — log บรรทัด "Unknown executable path" ไม่เคยถูกเก็บมาใน repo เลยแม้แต่บรรทัดเดียว ไม่รู้ด้วยซ้ำว่ามันตรวจ path ของตัว container เอง / ของ caller (node/Share) / หรือของ Share.exe ที่ถูกสั่ง spawn
2. `Project\Overlay OSC\Docs ShadowPlay - Real\OscProvision\Logs\flip-svc-build.log` (08:18) พิสูจน์แล้วว่า **service container แท้ RUNNING จาก build path ได้จริง 3/3 ตัว**
3. การทดลองที่ "ล้มเหลว" (19:26 `grand-boot.log` — Launch 200 แต่ Share: 0) ปนเปื้อนตัวแปรอื่น 2 ตัว: (a) spawn mode ทำ Share crash 0xc0000005 offset 0x13892c — แก้ด้วย attach mode, (b) ไม่ได้ start Share ด้วยตัวเอง
4. Watchdog profiles `SessionX64` / `NetworkServiceX64` **ไม่เคยถูก flip ไป build เลย** (มีแค่ SPUserX64/UserX64 ที่เคย flip แล้ว flip กลับ)

ดังนั้นโจทย์จริง = **ทำการทดลองสะอาดที่แยกตัวแปรครบ แล้วตัดสินใจจาก log จริง** — มีโอกาสสูงที่ build path ผ่านทั้งกองอยู่แล้ว

## ไฟล์ที่ต้องอ่านก่อนเริ่ม (เรียงตามลำดับ)

1. `PROJECT_MEMORY.txt` — กติกาโปรเจกต์ + ประวัติ (อย่างน้อย: [PROJECT RULES], [NON-NEGOTIABLE DECISION RULE])
2. `Project\Overlay OSC\Docs ShadowPlay - Real\OscProvision\PATH-MANIFEST.md` — §7-§17 โดยเฉพาะ §9 (flip สูตร), §10 (6-step boot idempotent), §13 (สูตร "ทุกอย่างจาก Build ยกเว้น container")
3. `docs\GFE-STACK-GENUINE-INSTALLED.md` §5 — ข้อพิสูจน์ GetFinalPathName (junction โดนตีกลับที่ node)
4. `Project\NvPlugins\src\OscModes.cs` + `Deploy.cs` — boot code ของเรา, path ที่ hardcode
5. `Project\NvConfig\nvcontainer.json` — mode switch ("genuine" / "ours")
6. สูตร flip ที่พิสูจน์แล้ว: script ใน `Project\Overlay OSC\Docs ShadowPlay - Real\OscProvision\` — `phaseB-restore.ps1` (flip สูตร), `attach-final.ps1` (attach mode), `arm-and-fire.ps1` (boot ครบชุด), `spuser-to-pf.ps1` / `service-to-pf.ps1` (flip กลับ), `link-share-gfe.ps1` / `fix-mmf-gfe.ps1` (hardlink สูตร) + logs คู่กันใน `Logs\` (flip-svc-build.log, grand-boot.log, service-to-pf.log, attach-final.log, spuser-to-pf.log)

## กติกา (ห้ามละเมิด — มาจาก PROJECT_MEMORY)

- ใช้หลักฐานจริงเท่านั้น: real logs + real tests ห้ามเดา — UNKNOWN อยู่จนกว่าจะมี evidence
- ห้ามเปลี่ยน architecture โดยพลการ — การตัดสินใจใหญ่ต้องถาม OWNER
- Build success ≠ behavioral equivalence — ต้อง verify runtime จริง
- ห้าม mass delete โดยไม่มี evidence — cleanup แยก track
- ห้าม push ของที่ยังไม่ verify / ห้ามเขียน token ลงไฟล์หรือ git config
- รายงานเป็นภาษาไทย แยก FACT / INTERPRETATION / UNKNOWN / RISK เสมอ

## สูตรที่พิสูจน์แล้ว (ใช้ได้เลย ห้ามคิดค้นใหม่)

- แก้ registry ด้วย `Set-ItemProperty` ผ่าน registry provider — **ห้าม `sc config binPath`** (quote พังเสมอ, exit 1639)
- ก่อน stop service: เคลียร์ FailureActions ก่อนเสมอ
- หลัง flip ImagePath: start ครั้งแรกอาจตาย — start ซ้ำถือเป็นปกติ
- Share.exe ต้องเป็น **attach mode** (Share ยืนรอ แล้ว container เข้าเกาะ) — spawn mode crash ที่ offset 0x13892c
- `nvsphelper64.exe` เริ่มได้ **หลัง node ตอบ 200 ที่ :59001 เท่านั้น** (ก่อนหน้านั้น = ตายใน ~20s, HELPR 8116)
- หลัง container ทุกรอบ (restart/death): ต้อง **re-arm `POST /Launch`** ให้ hotkey receiver ใหม่
- **ห้าม** set `nv-osc=false` ใน Share.json เด็ดขาด (ทำให้ OSC แท้ตายทั้งสาย)
- หลังแก้ module ฝั่ง node: ต้อง kill `NVIDIA Web Helper.exe` ด้วย (Share จะ reuse node ตัวเก่าไม่งั้น)
- Registry ที่เกี่ยว: `Global\NvNode` (port=59001, disableSecurity=1), `Global\GFExperience\FullPath` (ทั้ง 2 view), `NvContainer\ModuleMap`, `NvContainer\MessageBus` (InstallPath x86 view ต้องชี้ x86 dir), `NvContainer\Watchdog\*` 4 profiles, service `NvContainerLocalSystem` + DACL ต้องมี read ACE ของ Interactive User
- Log ศูนย์หลักฐาน: `C:\ProgramData\NVIDIA\*.log` (NvContainerLocalSystem.log, CaptureCore.log), `%LOCALAPPDATA%\NVIDIA Corporation\{NvNode,NVIDIA Share}\`, `build\NVIDIA ShadowPlay\Logs\`

## แผนงาน 4 เฟส

### Phase 0 — เซฟงานก่อน (ทำก่อนแตะอย่างอื่น)
สายที่ใช้งานจริงทั้งหมดยัง untracked — ดิสก์พังคือหายหมด:
1. `git add` + commit: `Project/NvPlugins/`, `Project/Overlay OSC/` (ข้างในมีครบ: `Docs ShadowPlay - Real/` ที่ย้ายมาจาก `Project/Docs ShadowPlay - Real/` เดิม — เอกสาร 20-25 + OscProvision ที่ข้างในมี PATH-MANIFEST.md, Payload, Registry, Logs, scripts — และ GenuineRuntime), `Project/OscProvision/` (ต้นแยกที่ Project root: Registry + Tools), `Project/NvContainer/`, `Project/NvConfig/`, `PROJECT_MEMORY.txt`, `.gitignore`
2. ตรวจ working-tree deletions ค้าง (มาจาก commit "OSC" f1de7e5 ที่ถูก reset ทิ้ง) ทีละจุดว่าของย้ายไปไหนบนดิสก์จริง (docs ย้ายแล้ว, ตรวจ Launcher.exe เดิม) แล้ว commit เป็น restructure รอบเดียว — ถ้าหาตำแหน่งใหม่ไม่เจอ หยุดถาม OWNER ก่อน
3. สร้าง `start-osc.ps1` ใหม่ (เวอร์ชัน 6-step idempotent ตาม PATH-MANIFEST §10 — ของเดิมถูกลบจากดิสก์ เหลือแต่เวอร์ชัน 4-step เก่าใน git commit 6e9a009b51) แล้ว commit

### Phase 1 — การทดลองสะอาด: nvcontainer แท้จาก build path (โจทย์หลัก)
1. เขียน probe script ตามสูตร phaseB-restore.ps1: flip service ImagePath + Watchdog **ครบทั้ง 4 profiles** (SPUserX64, UserX64, SessionX64, NetworkServiceX64) ไป build path — ครั้งนี้ flip ให้ครบ เพราะ 2 profile หลังไม่เคยถูก flip มาก่อน
2. Boot ตามลำดับ: service (build) → node (build) รอ :59001 = 200 → Share (build, **attach mode** ตามสูตร attach-final.ps1) → `POST /Launch` → วัด process จริง (Share: 2, helper: 1 คือเกณฑ์อ้างอิงจาก service-to-pf.log)
3. **เก็บ log ทั้งชุดเป็นหลักฐาน**: grep `C:\ProgramData\NVIDIA\*.log` หา "Unknown executable path" และเก็บ log เต็มไว้ใน repo — นี่คือ artifact ที่ไม่มีใครเคยเก็บ ตัดสินใจจากมันเท่านั้น
4. ตัดสินใจ 3 ทางจาก log จริง:
   - **ไม่เจอ string + Share/helper ปกติ** → build path ผ่านทั้งกอง ข้อสรุปเดิมเป็น confound → จบ Phase 1 ไป Phase 2
   - **เจอ string ชี้ caller** (node/Share PID) → แก้ที่ path ของ caller: `GFExperience\FullPath` ทั้ง 2 view / สูตร hardlink เข้า GFE dir (พิสูจน์แล้วกับ Share.exe ด้วย link-share-gfe.ps1)
   - **เจอ string ชี้ self-path** → ลอง NTFS junction ก่อน (รู้ล่วงหน้าว่าเสี่ยง: node ใช้ GetFinalPathName ตีกลับ) → ทางสุดท้าย binary patch nvcontainer.exe — **หยุด รายงาน OWNER แล้วรออนุมัติก่อนแตะ binary** เสมอ
5. ไม่ว่าผลอะไร บันทึก evidence ลง PATH-MANIFEST.md (เฟสใหม่) พร้อม path ของ log ที่เก็บไว้

### Phase 2 — Supervisor ให้มันรอดทุกวัน
ใช้ NvPlugins.exe (มีอยู่แล้ว) เป็นแม่เดียว:
- Boot 6-step idempotent + re-arm `POST /Launch` หลัง container cycle ทุกครั้ง + helper เริ่มหลัง node ตอบ 200 เสมอ
- Container ตาย/Share หลุด → ตรวจ liveness (port :59001 + log marker) แล้ว restart Share ใน attach mode — นี่แก้ settings-sync gap เชิงปฏิบัติ (page reboot = settings ยิงใหม่)
- `start-osc.ps1` = wrapper บาง ๆ เรียก NvPlugins เท่านั้น

### Phase 3 — อัดจริงด้วย capture แท้
1. Boot สะอาด (container ตื่นก่อน page เสมอ) → เปิด Manual Record จากหน้าแท้
2. เกณฑ์ผ่าน: CreateCaptureSession สำเร็จ (m_pSettings ไม่ NULL), SPUser container spawn, MP4 จริงออกที่ RecordPaths
3. ถ้า `0x80040233` (SP Server enable) ยังตัน → แยกเป็น investigation ใหม่ต่างหาก ไม่บล็อกงานอื่น — **รายงาน OWNER ก่อนไล่ต่อ**
4. ถ้า settings-sync ยังแตกเฉพาะกรณี container ตายกลางทาง → phase ถัดไป: replay settings (เก็บ 237 SetProperty calls จาก page boot) เข้า container ตัวใหม่

### Phase 4 — รื้อความมั่ว (แยก track ไม่ mass delete)
1. `NvConfig/nvcontainer.json` mode: **"genuine" เป็นหลัก**, "ours" (shim) ลดสถานะเป็น fallback lane
2. Archive lanes ตาย: Backend clean-room, Overlay.Engine, lane CefSharp (NVIDIA OSC.exe), cef-*.js debug scripts ~25 ตัว — ย้ายเข้า `archive/` + README ป้ายกำกับ ไม่ลบ
3. ลด osc bundle ที่ซ้ำ 5 สำเนา เหลือ payload + runtime
4. parity-test (ฐาน 11/11 — `Scripts\osc-parity-test.js`, socket.io-client@2.4.0) กลายเป็น regression gate ทุกครั้งที่แตะ stack

## เกณฑ์ "รอด" (acceptance — ครบทุกข้อถึงจบ mission)
1. Alt+Z → หน้าแท้ขึ้น โดยไม่มี dependency บังคับที่ Program Files (registry ชี้ build ได้หมด)
2. Boot ซ้ำ ≥10 รอบ idempotent ไม่พัง ไม่ต้องแก้มือ
3. Kill container กลางทาง → supervisor ฟื้นเอง Alt+Z ใช้ต่อได้
4. อัดได้ MP4 จริงด้วย capture แท้

## ข้อจำกัดการทำงาน
- ขั้นที่แตะ service/registry ระดับ admin ต้องรันบนเครื่องจริง (DESKTOP-DULUKA, GTX 1080 Ti) — ถ้า permission ไม่พอ ให้เตรียมสคริปต์ + สั่ง OWNER รัน แล้วขอ log กลับมาวิเคราะห์ อย่าเดาผล
- ทุกจบเฟส: รายงาน OWNER (ภาษาไทย) สรุป FACT/INTERPRETATION/UNKNOWN/RISK + evidence path ก่อนไปเฟสถัดไป
- ถ้าเจออะไรขัดกับเอกสารที่อ่านมา ให้ถือว่าเอกสารอาจล้าสุด ใช้ log จริงตัดสิน และรายงานความขัดแย้งนั้นด้วย
