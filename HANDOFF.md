# HANDOFF — NVIDIA-Shadowplay / ZCode Session Handover
> เขียน 2026-09-05 โดย ZCode session `sess_02804a48-cca4-40de-ab79-0028cb067262`
> วัตถุประสงค์: ย้ายบริบทงานทั้งหมดไปเครื่อง/แชทใหม่ — ให้ agent ตัวใหม่อ่านไฟล์นี้ก่อนทำอะไร

## 0. อัปเดตล่าสุด — 2026-09-05 (session ต่อ, อ่านก่อน §1-8)

**เครื่องที่รัน session นี้คือเครื่อง Intel** (ไม่มี NVIDIA GPU: Intel UHD 0x9B41, จอ 1920x1080)
— ต่างจากเครื่องเดิมตอนเขียน §1-8 (1680x1050) สิ่งที่ทำไปแล้วใน session นี้:

1. **ของที่หายถูกสร้างคืนหมด** (evidence/ ถูก gitignore ไฟล์อยู่บนดิสก์เท่านั้น):
   `tone_1k_60s.wav` + `make-clap-tone.ps1`, `synthetic-click.ps1`,
   `apphost-contamination-experiment.ps1` (สร้างคืนแต่ไม่ได้รัน), `diag-hotkey-probe.ps1`,
   `clap-sync-test.ps1` (driver เทสเต็มระบบ), `loopback-probe\` (net10 console probe WASAPI) —
   รายละเอียด + ผลทั้งหมดใน `evidence/clap-sync-SUMMARY-20260905.md`
2. **เทส clap-sync (§8.1) รันแล้วจบ**: pipeline ผ่านทั้งระบบบนเครื่องนี้ แต่มีข้อค้นพบเรื่อง
   environment 4 ข้อ (อ่านก่อนเทสเสียงอะไรบนเครื่องนี้):
   - ต้องแก้ config 2 ค่า (แก้ไปแล้ว, เก็บถาวร): `Paths.FFmpegPath` = ffmpeg ใน product tree
     (ไม่งั้น RECORD_START ถูก reject "FFmpegPath invalid") และ `Recording.encoder=QUICKSYNC_H264`
     (Duluka รันไม่ได้บนเครื่องนี้ — fallback legacy + QSV branch ทำงานจริง: 1080p60 CBR ผ่าน)
   - **Endpoint เครื่องนี้ถูก mute อยู่** → loopback ได้ digital silence ทั้งที่เสียงเล่น
     (พิสูจน์ด้วย loopback-probe: unmute = -8.0 dBFS, mute = -91 dBFS) — เทสเสียงต้อง unmute
     (clap-sync-test.ps1 จัดการ unmute/restore ให้เอง)
   - **Intel SST loopback start-stall**: callback แรกมาช้า 0.4-12s หลัง StartRecording
     เนื้อหาช่วงนั้นหาย (gap กลายเป็น silence แต่ timeline ต่อเนื่อง) — warmer (-40dB tone
     เล่นต่อเนื่องตั้งแต่ก่อน start) ลดเหลือ ~0.4s
   - **sync-verify วัด ms-level ไม่ได้เชิงคุณภาพบนเครื่องนี้**: offset คงที่ -0.86s (audio
     leads) ทั้งชุด มาจาก stall + skew ของ ffplay เอง (เปิดเสียงก่อนภาพ ~0.9s) — ไม่ใช่
     bias ระดับ 50ms ที่ Phase-13 ตาม; spacing ภายใน stream เป๊ะ 3.000s ทั้งสองฝั่ง
3. **ความรู้ trigger ระดับ production**: WM_HOTKEY ต้องโพสต์ไปที่ window ชื่อ "Main"
   (WinForms visible) ของ process "NVIDIA ShadowPlay" — `MainWindowHandle` ของ process นี้ = 0x0,
   window "test" 4 ตัวไม่รับ message; id=5 = ManualRecordToggle (ยืนยันจากลำดับ AllHotkeys)
4. **ลบไฟล์ .bak ที่หลุดเข้า e032343 แล้ว** (commit `f064928` + ignore patterns กันซ้ำ) และ
   แก้ `scripts/sync-verify.ps1` (ใส่ BOM แก้ parse error บน PowerShell 5.1 + Find-Ffmpeg
   หา product tree ได้)
5. §8 ข้อ 2-5 (OBS เต็มตัว, Phase-13 implement, slot เทา, ย้ายเครื่อง Intel) — **ยังรอตัดสินใจ
   เจ้าของเหมือนเดิม** โดยข้อ 5 ตอนนี้มีข้อค้นพบเพิ่ม: ทาง Intel ใช้งานได้จริงแล้วเบื้องต้น
   (legacy+QSV) แต่การวัด A/V ms-level ติดข้อจำกัด driver (ข้อ 2-3 ข้างบน)

---

## 1. สถานะ Repo ปัจจุบัน (อัปเดตสุดท้ายตอนเขียนไฟล์นี้)

- Branch: `Engine-Rebuild-Stabilization`
- HEAD: `e032343 "Capture"` — **อยู่บน origin แล้ว (push แล้ว, synced)**
- ประวัติล่าสุด: `e032343 Capture` → `b697a30 Fix overlay offline state and config write race` → `9728424 Record CFR disposer validation checkpoint`
- Working tree: **clean** — ผู้ใช้คอมมิต+push ทุกอย่างเหลือผ่าน GitHub Desktop
- `.gitignore` มี `/evidence` และ `*.mp4` เพิ่มใน e032343 → โฟลเดอร์ evidence ไม่ถูก track อีกต่อไป

## 2. สอง UX fix ที่คอมมิตใน b697a30 (ผ่านการ validate ครบ)

1. **`Sub_Record.vb` (Overlay)** — `ToggleRecording()`: เพิ่ม guard `tcp.IsConnected`
   ก่อน RECORD_START — ถ้า hub ออฟไลน์ แสดง `"Hub Offline — unable to start recording"`
   ที่ `Record_Stats` แทนการ flip เป็น "Recording" แบบ optimistic
   (เหตุผล: `TcpClientHelper.Send()` drop เงียบเมื่อ disconnect + `ShowNotifier()` ก็ขับเคลื่อน
   ผ่าน TCP เหมือนกันจึงใช้ toast ไม่ได้ตอนออฟไลน์)
2. **`Launcher Main.vb`** — ย้ายการเขียน `Overlay.UseOverlayEnabled` ออกจาก `IF_APP_Tick`
   (tick เคยเขียนทับ config ภายนอกทุกวินาที) → เขียนเฉพาะใน `Use_Overlay_ValueChanged`
   (user toggle เท่านั้น) + gate `_toggleInitializing` ตอน Load; tick เหลือ read-only

## 3. สถานะที่ validate ผ่านแล้ว (ห้ามถือว่าเสีย เว้นแต่พิสูจน์ใหม่)

- Apphost production: `Launcher.exe → NVIDIA API.exe (hub :5000) → Notifier/ShadowPlay/Capture`
  ทุกตัวเป็น apphost จริงจาก tree — PASS
- UI Start → Stop บน overlay (Alt+Z / WM_HOTKEY id=1) — PASS
- MP4: H264 1680x1050 @60fps + AAC, `start_time=0.000000` ทั้งคู่ (A/V offset 0),
  full decode `-xerror` 0 errors, FFmpeg exit 0, ไม่มี orphan ffmpeg — PASS หลายรอบ
- Engine.ConfigTruth.Tests: 30/30 PASS
- ไฟล์ทดสอบที่เคยใช้: `evidence/synthetic-click.ps1` (คลิกพิกัดจอ 1680x1050),
  `evidence/apphost-contamination-experiment.ps1`
- เทสเสียง clap-sync: **เตรียมไว้แต่ยังไม่ได้รัน** — โทนอยู่ที่
  `evidence/tone_1k_60s.wav` (1kHz/60s, ถูก ignore โดย git) — แผน: เริ่มอัด → +5s เปิดโทน
  (`ffplay tone.wav`) → 30s → ปิดโทน → +5s stop → วัด onset ด้วย
  `ffmpeg -af silencedetect` ว่าโทนเริ่ม ~5.0s หรือไม่ (วัด A/V offset จริง)

## 4. ความรู้เรื่อง Apphost/SingleInstance (สำคัญ — เคยทำให้วินิจฉัยผิด)

- `IsSingleInstance=true` ของ VB ผูก mutex กับ **assembly identity** — ดังนั้น
  `dotnet ...NVIDIA Capture.dll` กับ `Application\NVIDIA Capture.exe` **ใช้ mutex เดียวกัน**
- อินสแตนซ์ใหม่ของ exe ที่ spawn ขณะ dotnet-host ถือ mutex → exit code 0 ใน ~2s
  (ดูเหมือน crash แต่ไม่ใช่) — supervisor (`GetProcessesByName("NVIDIA Capture")`)
  มองไม่เห็นโฮสต์ชื่อ "dotnet" เลย spawn วนลูป
- **กฎเหล็ก: ก่อน spawn/ทดสอบ apphost ต้องไม่มี host ตัวอื่นของ assembly เดียวกันรันอยู่**
- Overlay menu (Alt+Z) บาง instance หลัง kill/respawn รัว ๆ อาจไม่ลงทะเบียน hotkey
  (เกิดชั่วคราว, respawn ใหม่หาย) — workaround ที่ deterministic:
  `PostMessage(mainHwnd, WM_HOTKEY=0x0312, wParam=1, lParam=0)` = ToggleOverlay

## 5. โครงระบบ 3 Capture Regime

| Regime | engine_mode | เส้นทาง | สถานะ |
|---|---|---|---|
| **Duluka** (native) | duluka/ddagrab/native | `RecordingEngineHost`(_useNewEngine=True) → RecordingEngine → DdagrabBackend(DXGI) → CFR(QPC) → NVENC native → FFmpeg live-mux | production, validate ครบ |
| **FFmpeg** (legacy) | ffmpeg/legacy | `Engine\Engine\[Capture]\CaptureEngine.vb` (2-process: video FFmpeg subprocess + AudioFileWriter WAV → mux ตอน stop) | ใช้ได้; capture ddagrab/gdigrab/gfxcapture (`FFmpegCommandBuilderV2`), **มี branch IntelQSV แล้ว** (`hwmap=derive_device=qsv`) |
| **OBS** | — | ยังเป็นสะพาน event เท่านั้น: `ObsWebSocketClient` (Notifier, obs-websocket :4455, `notifier_obs.json`) → `ObsEventMap` แปลง RecordStateChanged/ReplayBuffer* ส่งเข้าแอป — **ไม่มี** app→OBS สั่งอัด, ไม่มีไฟล์ OBS เข้าระบบ | event-forward only |

- UI เลือก API: `[5] Video Capture.vb:2254` — Duluka = 6 รายการ (เปิดจริงแค่
  `dxgi_desktop_duplication`; อีก 5 เป็น slot เทา: windows_graphics_capture,
  d3d11_native, window_capture, region_capture, native_game_capture);
  FFmpeg = ddagrab/gdigrab/gfxcapture เปิดหมด
- Duluka: config ล็อก `APICapture="ddagrab"` เสมอ; `RecordingEngine.vb:126` dispatch —
  ค่าอื่น → GAP warning แล้ววิ่ง Ddagrab ต่อ; `VideoBackendKind` มี slot `GfxCapture` รออยู่
- **ผูก NVIDIA มี 2 จุด** (สำหรับย้ายเครื่อง Intel): (a) `DdagrabBackend.vb:232` เลือก
  adapter `VendorId=&H10DE` เท่านั้น (b) NVENC native (`CaptureEngine.Encoder.Nvenc`)
  — ที่เหลือ GPU-agnostic; ทาง Intel สั้นสุด = FFmpeg regime + QSV branch

## 6. เรื่องเสียง — บทวิเคราะห์ 4 ปี (อ่าน `docs/PHASE-13-SHADOWPLAY-CLOCK.md`)

- **รากปัญหาเชิงโครงสร้าง (เอกสารเขียนไว้เอง)**: pipe เป็น raw bytes ไม่มีเวลา →
  ประดิษฐ์นาฬิกา 2 ตัว (CFR tick loop + AudioTap จาก callback arrival time) —
  stabilizer ทั้งหมด (steering, lead 50ms, silence caps, idle-gate) คือตัวชดเชย
  ที่ "ลด" ไม่ "กำจัด"
- **ทางแก้ที่วาง design ไว้ (ยังไม่ implement)**: single hardware clock —
  WASAPI `qpcPosition` (มีอยู่แล้วใน packet!) หักกับ video QPC T0 ตรง ๆ:
  `offsetSec = (videoT0Qpc − firstAudioQpc)/QpcFrequency`
- **config เสียงปัจจุบัน**: `SystemAudioEnabled=true, MicEnabled=false,
  AudioClockMode="Legacy"` (มี `AudioTapDeviceClock.vb` 388 บรรทัดรออยู่ —
  น่าจะคือ device-clock implementation ครึ่งทาง)
- การอัดสั้นวันนี้: A/V offset = 0 จริง (ได้แล้วในเงื่อนไขสั้น); รอยด่านที่เอกสารระบุ
  = อัดยาว (crystal drift), ช่วงเงียบ, lead 50ms ที่คาลิเบรตมือ

## 7. Gotchas ปฏิบัติ

- Build ผลิตภัณฑ์: `dotnet build "Overlay/NVIDIA Overlay.vbproj" -c Release`
  (สแกน product tree เอง) + `Launcher/Launcher.vbproj`; **ต้องปิดทุก process ของแอปก่อน
  build** (ไฟล์ payload ถูก lock)
- Product tree จริง = `Overlay\bin\Release\net10.0-windows10.0.26100.0\`
  (Application\ Services\ Overlay\ Engine\ Core\ … `.NET Deployment\`)
- FFmpeg/ffprobe ใช้จาก `…\net10.0-…\FFmpeg\` ใน tree
- `engine_mode` default = "ffmpeg" ถ้า config ไม่ชัด (`OverlayConfig.GetEngineMode`)
- Working dir ของ engine = product root (AppLayout เดินจาก exe leaf)
- อย่า commit `CaptureSession.vb.bak_wake/.diagbackup/.tmpwake` ซ้ำ — มันเข้าไปใน
  e032343 แล้ว (ถ้าอยากถอดออกทำ follow-up commit ลบไฟล์)
- Untracked ใหม่จะไม่โผล่ถ้าอยู่ใต้ `/evidence` (ถูก ignore แล้ว) — ไฟล์ handoff นี้
  อยู่ root จะโผล่ให้เห็นใน GitHub Desktop ให้ commit+push ต่อ

## 8. เรื่องค้างที่รอตัดสินใจเจ้าของ

1. เทสเสียง clap-sync (ดู §3) — ยังไม่รัน
2. OBS จะเป็น capture ตัวที่สามเต็มตัวไหม (ต้องเพิ่ม app→OBS StartRecord/StopRecord
   + รับไฟล์จาก OBS เข้า Gallery)
3. Phase 13 single-clock — implement จริงหรือยัง
4. 5 slot capture เทาของ Duluka (WGC มี slot enum แล้ว)
5. ย้ายเครื่อง Intel — ทางลัด = FFmpeg regime + QSV (ดู §5)
