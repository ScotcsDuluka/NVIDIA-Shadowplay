import io
p = r'C:\My Project\OSC-ShadowPlay\HANDOFF-NEW-CHAT.md'
s = io.open(p, encoding='utf-8').read()
old_start = '**⏳ งานต่อจากนี้ (ชัดแล้ว วัดจาก log):**'
old_end = 'จาก PID NVIDIA OSC'
i1 = s.find(old_start)
i2 = s.find(old_end) + len(old_end)
print('region found:', i1 > -1 and i2 > i1)
new = '''**⏳ งานต่อจากนี้ (checkpoint ฉบับสมบูรณ์ 2026-10-07 05:1x — เซสชันใหม่เริ่มตรงนี้):**

**สถานะสาย NVIDIA OSC (สแตนด์อโลน พอร์ตล็อกชุดใหม่ แยกขาดจากสายแท้):**
- รันอยู่: `NVIDIA Web Helper.exe` (shim .NET, โฟลเดอร์ NVIDIA ShadowPlay API) spawn **NvNode.exe** (node v11.13 portable) รัน index.js ต้นไม้ shim → **:59011 เดียวจบ** (REST + socket.io v2 + fire route /?hk=<Name> ผ่าน global.__nvFire) · host `NVIDIA OSC.exe` (CEF 73, :3000 หน้า + :59013 toggle + CDP :59099) · helper `nvsphelper.exe` (**Alt+X** ยิงคู่ :59011 + :59013/toggle)
- ✅ วาดแล้ว: main.main-menu + 10 tiles (แก้ getMainMenuShortcuts คืน default shortcuts — เดิมคืน undefined ทำ V() โยน modsOpen ตาย) — ยืนยันหน้าจอจริง + ring 3px
- ✅ เอนจิน: CEF 73 = เดียวกับ Share.exe (แทน 138) · CSS แท้ล้วน (ถอด injection) · ring 3px/shift 0
- ✅ identity: ไอคอน NVIDIA ShadowPlay.ico + FileDescription (OSC=On-Screen Controls Services / helper=ShadowPlay Helper Services / Web Helper=Web Helper Services) · node.exe เปลี่ยนชื่อ → **NvNode.exe** (NodeRuntime.vb มอง NvNode.exe ก่อน)
- ✅ NODE_INFO: host ตอบ {"port":59011,"disableSecurity":true,gfwsl/jarvis/gxtarget} — หน้า PARSE แล้ว (เดิม fallback 59001 เพราะไม่มีใครตอบ)

**งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):**
1. **หน้าไม่ connect socket เองตอนบูต** — socketService.connect() ต้องสั่งมือ (สั่งแล้ว fire ผ่านทันที ยืนยัน) → แก้ 2 ทาง: Backend/index.js (หรือ socket.js) on-connection → emit initial state แบบ node แท้ + หาว่า app gated connect ด้วยอะไร (oscService onlineState?)
2. **ระหว่างทดสอบใช้ลำดับมือ:** reload หน้า → CDP socketService.connect() → POST :59011/?hk=OpenShare → POST :59013/toggle → เมนูลงจอ

**ข้อควรระวัง:** ห้าม taskkill node.exe รวบ ๆ (โดน node แท้สายแท้ด้วย — ผมพลาดแล้ว สายแท้พังต้อง NvPlugins boot กู้) · eval CDP ห้าม bare return (ห่อ IIFE) · osc_hack filter หา osc/index.html แต่หน้าเรา = /index.html (bypass ด้วย CDP(wsurl) ตรง) · build exe ตอนตัวเองยังรัน = copy ล็อค (kill ก่อน build เสมอ) · NvContainer.cs คืน port 59001 แล้ว (ห้ามแตะอีก — ของสายแท้) · สายแท้พังตอนนี้ (node ถูกฆ่า + PF NvNode หาย) → รอ OWNER รัน NvPlugins boot'''
s = s[:i1] + new + s[i2:]
io.open(p, 'w', encoding='utf-8').write(s)
print('checkpoint written, region replaced')
