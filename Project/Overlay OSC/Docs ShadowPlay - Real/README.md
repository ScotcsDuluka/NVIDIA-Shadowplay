# Docs ShadowPlay - Real

ความรู้จากการแกะ GFE 3.28.0.412 ตัวจริง (payload จาก installer official,
โหลดจาก `us.download.nvidia.com/GFE/GFEClient/3.28.0.412/`) — แกะวันที่
2026-09-25 บนเครื่อง Intel (HUAWEI-PC) — payload เก็บไว้ที่
`C:\My Project\gfe-3.28.0.412-extract\` (ไม่ใน git)

## สารบัญ (อ่านตามลำดับนี้)

| # | ไฟล์ | คือ |
|---|---|---|
| 20 | `20-intel-cef-osc-findings-2026-09-25.md` | payload แท้ + ต้นตอจอดำ (libcef NVIDIA vs stock) + hotkey plugin |
| 21 | `21-shadowplay-engine-dissection.md` | engine map ทั้ง `ShadowPlay\` (15 PE: capcore/nvspcap hook/API/plugins) |
| 22 | `22-nvnode-backend-and-hosts-dissection.md` | NvNode backend จริง (70+ endpoint) + 3 CEF hosts = ไบนารีเดียวกัน + NvContainer/MessageBus |
| 23 | `23-remaining-components-and-configs.md` | ชิ้นที่เหลือ (NvBackendAPI http_over_pipe, GfeXCode CUDA/NGX, telemetry) + config อ่านได้ |
| 24 | `24-gfe-architecture-understanding.md` | ⭐ **เริ่มอ่านที่นี่** — mental model สังเคราะห์ (process tree, 5 IPC planes, config hierarchy, gap map กับ gfe-rebuild, ลำดับงานต่อ) |

## หลักฐาน raw (จาก dumpbin/strings + live test)

- `raw-dissection-shadowplay-folder.txt` — strings/exports ทั้งโฟลเดอร์ ShadowPlay
- `raw-dissection-nvnode-hosts-container.txt` — nodejs/launcher/Web Helper/Container/hosts
- `raw-dissection-final-sweep.txt` — ชิ้นที่เหลือ (NvBackendAPI/telemetry/XCode ฯลฯ)
- `parity-test-result-11of11.txt` — ผลทดสอบ parity สด 11/11 PASS
  (script: `Scripts/osc-parity-test.js` — ต้องใช้ `socket.io-client@2.4.0`,
  protocol v2 ตรงกับ osc bundle; v4 คุยไม่ได้)

## สถานะเครื่อง Intel ณ วันเก็บเอกสาร

- GFE ที่ port มารันปกติ (osc + Alt+Z ผ่าน backend จริงที่ 59001)
- lane ของเรา: build เขียว + CEF runtime ของ NVIDIA + backend parity ครบ
  — เหลือ debug จอดำที่ window creation (`nv-remote-debugging-port` ใช้ได้
  ทั้งสองฝั่ง)
