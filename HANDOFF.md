# HANDOFF — NVIDIA-Shadowplay / ZCode Session Handover
> เขียน 2026-09-05 โดย ZCode session `sess_02804a48-cca4-40de-ab79-0028cb067262`
> วัตถุประสงค์: ย้ายบริบทงานทั้งหมดไปเครื่อง/แชทใหม่ — ให้ agent ตัวใหม่อ่านไฟล์นี้ก่อนทำอะไร

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

1. ~~เทสเสียง clap-sync~~ → **ทำแล้ว 2026-09-05 — เจอบั๊กจริงและแก้แล้ว (ดู §9)**
2. OBS จะเป็น capture ตัวที่สามเต็มตัวไหม (ต้องเพิ่ม app→OBS StartRecord/StopRecord
   + รับไฟล์จาก OBS เข้า Gallery)
3. Phase 13 single-clock — implement จริงหรือยัง (config ยังเป็น AudioClockMode="Legacy")
4. 5 slot capture เทาของ Duluka (WGC มี slot enum แล้ว)
5. ย้ายเครื่อง Intel — ทางลัด = FFmpeg regime + QSV (ดู §5)

## 9. เสียง — ผล clap-sync เทสจริง 2026-09-05 + บั๊กที่แก้แล้ว

**บั๊กที่เจอ (หลักฐานจากไฟล์อัดจริง):**
- อัดสั้น 58s: วิดีโอ 58.07s / เสียง **50.13s** — เสียงท้ายหาย ~8s
  (`LiveMux: dropped=1,530,240B` ≈ 7.97s พอดี)
- **สาเหตุ**: `LiveMuxSession.PipeFeed.RequestStopAndDrain` ใช้ `Join(timeoutMs)`
  (3s) — คิวเสียงค้างท้ายหลายวินาที ทำให้ pipe ถูกปิดทั้งที่ยังมีข้อมูล
  → ไบต์หายทั้งก้อน แล้ว ffmpeg EOF ช่วงท้ายจึงไม่มีเสียง
- **บั๊กซ้อน**: `SessionResult.Pass` ไม่เห็น drop ฝั่ง mux (sidecar รายงาน dropped=0
  ขณะ mux ทิ้งจริง) → `pass=True` หลอกตลอด 4 ปี
- **แก้แล้ว 3 ไฟล์** (validate: build 0/0, ConfigTruth 30/30, อัด 3 นาทีจริง
  `dropped=0B`, เสียง 176.87s = วิดีโอ 176.89s, decode -xerror 0 errors):
  1. `CaptureEngine.FFmpegBackend/LiveMuxSession.vb` — RequestStopAndDrain รอ
     drain คิวจนหมด (bounded by timeout) ก่อนปิด pipe
  2. `CaptureEngine.Recording/RecordingDTOs.vb` — เพิ่ม `MuxDroppedBytes`;
     `Pass` ต้องการ dropped ทั้งสามทาง = 0 (mux + sidecar + mic)
  3. `CaptureEngine.Recording/CaptureSession.vb` — map `liveRes.DroppedBytes`
     → `result.MuxDroppedBytes` + Warning log เมื่อ > 0

**การค้นพบเรื่อง endpoint (สำคัญ — ยังไม่แก้):**
- engine จับ system audio จาก endpoint `{6ec9c7cb-…}` = **"FxSound Audio Enhancer"
  (APO ของ FxSound, process กำลังรัน pid 9052)** — ไม่ใช่ลำโพงจริง
- โทนทดสอบที่เล่น (SoundPlayer) ออกทาง default endpoint จริง → ไฟล์เงียบทั้งเทป
  (-91dB ทั้งไฟล์ ทั้งที่ dropped=0) — งานที่ค้างต่อ: (a) ยิงเสียงเข้า endpoint
  ที่ FxSound คุม หรือ (b) ปิด FxSound ชั่วคราวแล้วเทสซ้ำ หรือ (c) ใช้ ffmpeg
  `-filter_complex "[1:a]aloop"` ผ่าน `dshow` จะคุม endpoint ได้ตรง ๆ
- ตัวนับในไฟล์อัด 3 นาที: `dropped=0B` หมายถึง "ไปป์ไลน์ไม่ทิ้งข้อมูล" —
  ส่วนว่าไฟล์มีเสียงหรือไม่ต้องวัดด้วย silencedetect/volumedetect แยกต่างหาก

## 10. สิ่งที่ค้างต่อจาก §9 ทันที (สำหรับแชทใหม่)

1. **ตรวจว่า fix เสียงเข้า HEAD แล้วหรือยัง** (`git log --oneline -3` — ถ้ายังไม่มี
   commit ใหม่เรื่อง audio ให้ทำตาม §9 รายการไฟล์ 2)
2. **เทสโทนซ้ำให้เห็นเสียงจริงในไฟล์**: ปิด FxSound ก่อน (kill pid หรือ service)
   → ยิงโทน → อัด → silencedetect ต้องเห็นโทน; ถ้ายังเงียบ ให้เช็คว่า default
   render device ตรงกับ endpoint ที่ engine จับ
3. ตัดสินใจนโยบาย endpoint: จับ "default render" ตอนเริ่มเซสชัน หรือให้ผู้ใช้เลือกได้
   (FxSound/APO เป็นเรื่องปกติของเครื่องผู้ใช้จริง)
