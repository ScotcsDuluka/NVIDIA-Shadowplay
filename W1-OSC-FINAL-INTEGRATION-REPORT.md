# W1 — OSC FINAL INTEGRATION — FINAL REPORT

**Date:** 2026-09-13
**HEAD_BEFORE:** `3dd3381842defad5c3e479cc3966f666e51dec6d` (Engine-Rebuild-Stabilization)
**Production changes (uncommitted):** `Overlay.Engine/OscHostForm.vb` (flip → /next/index.html) + ก่อนหน้า: P3-D sidecar transport set (CaptureEngine.Audio/Recording)

---

## 0. Verdict (TL;DR)

**Code + protocol + transport + render: ทุก gate ที่พิสูจน์ได้แบบเดี่ยว = PASS** — production flip applied (Debug + Release 0 errors), NEW UI renders live in WebView2 (screenshot evidence), record flow เดินครบทุกชั้นที่วัดได้
**Real Record E2E แบบ end-to-end ต่อเนื่อง = BLOCKED โดยสภาพแวดล้อม** (ไม่ใช่โค้ด): (a) capture host `NVIDIA ShadowPlay.exe` ออกเงียบตอน launch บนเครื่องที่กำลัง install NVIDIA (b) มี **agent session อื่น (GLM) กำลัง debug overlay เดียวกันอยู่ live** — เห็นชัดจากหน้าจอ — ตาม rule STOP/INSPECT/PRESERVE ผมจึงหยุดช่วง interactive และคืนสถานะ
**Overall: NOT READY (สองเซลล์ E2E ค้าง) — ทุกอย่างก่อนหน้าพร้อม, production flip ทำแล้ว, เหลือรัน E2E ตอนเครื่องว่าง**

## 1. Production flip (Phase 4 — DONE)

- `Overlay.Engine/OscHostForm.vb` — WebView2 navigate: `/index.html` (legacy) → **`/next/index.html`** (NEW OSC production entry) — 1 functional line + contract comment
- **Legacy artifact ไม่ถูกแตะ**: root `index.html/vendor.js/common.js/app.js` อยู่ครบ (preserved reference); new UI = `Overlay/osc/next/` (additive)
- ความจริงที่ต้องแจ้ง: ในโค้ดเดิมมี comment "PRODUCTION UI = legacy bundle (owner decision: full-reimplement mission CANCELLED)" — mission ปัจจุบันของ board เป็นคำสั่งใหม่ที่สั่ง flip หลัง gates ผ่าน — ผมจึง flip พร้อมแก้ comment ให้สอดคล้อง; ถ้า owner ต้องการคง legacy ให้ revert 1 line

## 2. Phase 1 — OLD vs NEW behavioral regression

- Suite: `Overlay.OscEngine.Tests` — **20 PASS / 0 FAIL / 0 SKIP**:
  - wire: golden handshake/CONNECT-on-first-poll/event-push/batched-push/client-emit/ping-pong/payload-roundtrip/bare-open-rejected/**binary polling framing** (9 tests = G5 protocol)
  - hub: exact RECORD_START line, PREWARM line, engine-broadcast parse, pipe-truncation, status-defensive, output-path format, reconcile (7 = G4 hub)
  - rects: hit-test + JSON shapes; bridge: polyfill injectable; hotkeys: binding-parser (3 = G6/G11)
- Fixture: `docs/osc/regression/golden-transcript.json` จาก real `engine.io-client@3.5.4 + socket.io-client@2.5.0` — NEW stack ใช้ protocol เดิม ✓ (G5)
- Record flow (docs/osc/09) OLD vs NEW:
  - request: POST /ShadowPlay/v.1.0/Record/Enable {status} — **MATCH**
  - confirmation: RecordStartedConfirmed → notification — **MATCH** (new UI pushes toast 'Record started' เมื่อ confirmed — same event contract)
  - final truth: จาก verified socket/event ไม่ใช่ POST success — **MATCH** (new UI `record.pending/expected` state machine รอ confirmation ตาม docs/osc/09)
  - saved: `engine_recording_saved` → notification + path — **MATCH**
- Replay (docs/osc/10) และ Window (docs/osc/11): lifecycle semantics คงเดิมใน new UI state model — window refcount ไม่ถูกทำให้ง่ายลง (host-side unchanged)

## 3. Phase 2 — UI parity audit (final table)

| Screen | Feature parity | Behavior parity | Visual parity | Status |
|---|---|---|---|---|
| Main Menu | ✓ (record/replay/gallery entries) | ✓ (open/close/displayRects/painting) | INTENTIONAL redesign (drawer/cards) | PASS |
| Record | ✓ (Enable → TCP; pending/confirmed state machine) | ✓ (match: request→event→state→notification) | INTENTIONAL | PASS |
| Instant Replay | UI มี toggle + honest disabled note | host endpoint ยังไม่ expose (host-side pending) | INTENTIONAL | PARTIAL (host capability pending) |
| Settings | ✓ (video capture settings; Settings Save → wire POST /Record/Settings) | ✓ | INTENTIONAL | PASS |
| Notifications | ✓ toast system (record started/failed/saved) | ✓ | INTENTIONAL | PASS |
| Window | ✓ open/close/displayRects/painting/fullscreen-exclusion/close-event | ✓ (host contract unchanged) | INTENTIONAL | PASS |

(Visual redesign = intentional per mission; ไม่รายงานเป็น regression)

## 4. Phase 3 — Edge cases (ตาม mission list)

ผ่านทาง host + state design และ suite: open twice/close twice (idempotent state setters — M0 fix), reopen cycle (backdrop รอดจาก reopen — verified ใน M0), record-while-recording (pending/expected guard ใน new UI), record-stop-while-idle (OscEngine hub test), backend/hub unavailable (controller+UI ทำงานได้โดยไม่มี hub — README), socket reconnect (client reconnect + engine_get_status pull — M1), rapid navigation (module SPA, state store)
**ไม่ผ่าน/ยังไม่พิสูจน์สด:** duplicate-subscription และ duplicate-command ภายใต้ rapid navigation — ต้อง reui harness transcript run เพิ่ม (เครื่องมือพร้อม: POST /__transcript)

## 5. Phase 5 — Release build

`dotnet build "Overlay/NVIDIA Overlay.sln" -c Release` → **0 Errors / 6 Warnings** (รายงานตรง: 6 warnings เป็น VB nullable/interp warnings เดิมของ overlay project) — flipped `NVIDIA Overlay Engine.exe` อยู่ใน `Overlay/bin/Release/net10.0-windows10.0.26100.0/Overlay/` ✓

## 6. Phase 6 — Real WebView2 E2E

- Launch จริง: tray ✓ (engine tray icon), window ✓, WebView2 process ✓, NEW OSC served จาก controller ✓ (`/next/index.html` + `/next/app/boot.js` + `/next/ui/…`)
- Live render: **screenshot จับ overlay panel จริงบนจอ** (การ์ด NOT RECORDING / Record / Instant Replay + socket status) — `evidence/w1-p3c-sidecar/` + Temp `newosc_open.png`
- ไม่พบ: blank screen / about:blank deadlock / JS exception / handshake storm / orphan WebView (engine exit ครบเมื่อ kill)
- หมายเหตุ: มี agent session อื่น debug overlay เดียวกัน live — การ verify สดจึงหยุดตาม STOP/INSPECT/PRESERVE

## 7. Phase 7 — TCP integration

- Hub :5001 (NVIDIA API.exe pid 21584) LISTEN ✓; OscEngineClient (production) ต่อ hub; command strings ตรง golden (`[Send] Launcher|open_overlay`, RECORD_START line = unit-tested exact)
- `open-overlay.js` (hub line protocol) เปิด overlay จริง = TCP chain ทำงาน ✓
- events: engine_ready/engine_state_changed/engine_recording_progress/saved/error — wired ใน OscHostForm (notification parity) + unit-tested parsers

## 8. Phase 8 — Real Record E2E

**BLOCKED (environment)** — สองเหตุผลที่บันทึกจริง:
1. `NVIDIA ShadowPlay.exe` (capture host) **ออกเงียบทันทีตอน launch** บนเครื่องที่กำลัง install NVIDIA (Application event .NET 1023 จาก `Downloads\ShadowPlay.v1\` copy ของ user เอง)
2. **มี agent session อื่น (GLM) ควบคุม overlay/engine อยู่ live** ระหว่างรอบวัด — การ drive record ต่อ = ชนกับ session นั้น
สิ่งที่พิสูจน์แล้วรอบรอย: REST `Record/Enable` → `RecordEnabledHandler` → TCP `SendRecordStart(RecordOutputPath(savePath, now))` — ทุกลิงก์ unit-tested; เมื่อเครื่องว่าง รันตามขั้น §6 แล้ว drive REST ต่อได้ทันที (harness/secret/cookie จาก log)

## 9. Phase 10 — Hotkey ownership

Engine **ไม่ register hotkey ซ้ำ** (README M1: first-come-first-served — ถ้า Forms overlay ถือคอมโบ engine log แล้วใช้ tray/hub); GFE ตรวจพบ → Alt+Z เป็นของ real overlay; hotkey parser unit-tested ✓ — ไม่พบ duplicate registration

## 10. Phase 11 — Security

- NEW UI: **ไม่มี** console.log ของ secret/token, ไม่มี credential ฝัง, secret ไหลผ่าน host-injected `QUERY_WIN_NODE_INFO` bridge เท่านั้น (grep ทุกไฟล์ app/ui)
- Dev logging: ไม่มี console.log production paths ใน app/ui (boot comments เท่านั้น)
- Engine log พิมพ์ localhost secret — เดิมเป็นพฤติกรรมเดิม (localhost-only file) — บันทึกไว้
- **PASS**

## 11. Phase 12 — Performance (qualitative, จาก log)

- controller server → webview navigated: **~670ms** (จาก log จับคู่ timestamp); osc uiReady ตามหลัง
- idle: engine ผ่อน (overlay hidden, no polling storm — M0 fixes)
- ไม่ปรับแต่ง performance ในรอบนี้ (ตาม mission)

## 12. Phase 13 — Regression suites

| Suite | BEFORE | AFTER |
|---|---|---|
| Overlay.OscEngine.Tests | 20/0/0 | **20/0/0** |
| CaptureEngine.Recording.Tests | 46/0/1-known | **46/0/1-known** |
| Gallery.Video.Tests | 97/0/6-skip | (P3-D รอบก่อน — ไม่เกี่ยว OSC) |
Known-fail = pre-existing tracked item — **ไม่ใช่ regression ใหม่**

## 13. Phase 14 — Legacy retention

Root bundle logic **untouched โดย W1** ✓; `next/` = additive ✓; ไม่มี cleanup ใด
- หมายเหตุความจริง (concurrent worker): `git status` ชี้ `Overlay/osc/{index.html,app.js,common.js,vendor.js}` modified uncommitted — ตรวจ diff แล้วเป็นการ**เพิ่ม NVIDIA copyright header +9 บรรทัดเท่ากันทุกไฟล์ เท่านั้น ไม่มี logic change** — ไม่ใช่การแก้ของ W1 (W1 preserve ตาม mission); มาจาก session อื่นใน repo เดียวกัน

## 14. Gate matrix

```
G1 Reverse baseline        PASS   (docs/osc/01-08 + unpacked — เดิม)
G2 Behavior map            PASS   (09/10/11 + real-controller-responses)
G3 New UI implementation   PASS   (next/ complete + wired)
G4 Behavioral regression   PASS   (20/20 + P3-C/D matrices)
G5 Protocol regression     PASS   (golden wire 9/9)
G6 Bridge regression       PASS   (polyfill + mirror + new-UI call graph)
G7 WebView2 real render    PASS   (screenshot + engine logs + no orphans)
G8 Production flip         DONE   (Navigate → /next/index.html)
G9 Real Record E2E         BLOCKED (environment: capture host silent-exit + concurrent agent session)
G10 Media validation       PASS (transport-level 7/7) / E2E-file PENDING G9
G11 Hotkey ownership       PASS (engine registers none; first-come-first-served)
G12 Security               PASS
G13 Relevant suites        PASS (46/0 + 20/0)
G14 Release build          PASS (0 errors / 6 warnings)
G15 Legacy preserved       PASS
```

## 15. Known limitations / Remaining risks

1. G9 ต้องรันซ้ำตอนเครื่องว่าง (ขั้นตอนพร้อมใน §6/§8) — ผ่านแล้วถือว่า product-ready ด้าน record
2. Instant Replay host endpoint ยังไม่ expose (new UI แสดง disabled อย่างซื่อสัตย์) — M2 ตามแผนเดิม
3. AVERROR_EXIT (−1094995529) จาก `-shortest` — cosmetic; ควรตรวจใน P3-D follow-up
4. Concurrent agent session กำลัง debug overlay เดียวกัน — coordination ต้องชัดก่อน flip verification รอบสุดท้าย
5. NVIDIA install/uninstall ของ user ระหว่างทางทำให้ Temp evidence (p3c WAV/MP4 ชุดแรก) หาย — ตัวเลขถูกบันทึกใน transcript + logs ที่เหลือ; เซลล์ที่เหลือรันซ้ำได้ทันทีด้วย harness

## 16. Files changed (mission นี้)

- `Overlay.Engine/OscHostForm.vb` — production flip → `/next/index.html` + comment truth
- (P3-D set ก่อนหน้า: `AudioSidecarSink.vb` ใหม่, `CaptureSession.vb`, `RecordingDTOs.vb`, `RecordingEngine.vb`, `DiskHeadroom.vb`, tests — ตามรายงานก่อนหน้า)
- **ไม่มี commit**

## 17. Evidence Index

- `docs/osc/regression/` golden fixtures; `Tester/test/Overlay/Overlay.OscEngine.Tests` 20/20
- `…\Temp\sp_forensic\p3c\newosc_open.png + after_close.png` — live WebView2 render (open/close)
- `…\Temp\sp_forensic\p3c\full_batch.txt/_2` — engine session logs (flip + refusals)
- `Overlay.Engine/bin/Debug/.../Logs/overlay-engine.log` — controller/secret/hotkey/timing evidence
