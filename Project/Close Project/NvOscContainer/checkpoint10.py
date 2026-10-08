import io
p = r'C:\My Project\OSC-ShadowPlay\HANDOFF-NEW-CHAT.md'
s = io.open(p, encoding='utf-8').read()
old = 'งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'
new = '''**🛑 สรุปปิดเซสชัน 2026-10-07 06:2x (ตรวจครบทุกชั้น — จุดพังเดียวที่เหลือชัดเจน):**
- server :59011 ครบ 100% (REST 200 + handshake CORS ถูก origin + fire route + NODE_INFO port=59011) · host CEF 73 บูตได้ (หลังเติม natives_blob.bin + snapshot_blob.bin — CEF 73 ต้องมี 2 ไฟล์นี้ ขาด = crash 0x80000003 ตอน init — ข้อควรจำ deploy!) · socket หน้าต่อได้ (ESTABLISHED ยืนยัน) · Alt+X ยิงถูก shape
- **จุดพังเดียวที่เหลือ: หน้าโหลดใหม่แล้วไม่มี socket.io XHR เลย** (ยืนยันด้วย pre-document XHR instrumentation) = app ไม่ connect socket ตอนบูตบน backend เรา — boot chain ของแอป (log bundle: "Init UI" → T.connect() → N.initialize() → j() → y.init() → ... → h.setOscReady()) **ค้าง/สะดุดก่อน T.connect หรือ connect แล้วดีดกลับ** — งานต่อ: (1) ใช้ pre-document hook console.log/info ดู boot chain ว่าไปถึงไหน (2) เทียบ emission ตอน connect กับ node แท้ (node แท้ตอน connection ส่ง initial state อะไร — ดูจาก decompile 0020.socketService) (3) เติมให้ครบ = Alt+X ครบวงจร
- สถานะดิสก์หลัง OWNER จัดโครง: backend = `Project\\Overlay OSC\\NVIDIA NodeAPI\\` (rename จาก NVIDIA ShadowPlay API) · ซอร์ส+osc = `Project\\Overlay OSC\\NVIDIA OSC\\` (merge แบนแล้ว — osc_main.cpp มี fix ครบ + gdi_compositor ตัวใหม่) · deploy host = `build\\...\\Overlay OSC\\NVIDIA OSC\\` (CEF 73 runtime + blobs ครบ) · เครื่องมือเก่า = `Project\\Close Project\\NvOscContainer\\` (osc_hack.py)
- สายแท้: node แท้ถูกฆ่า + PF\\NvNode หาย — รอ OWNER รัน NvPlugins boot (admin) กู้
- ตัวช่วยทดสอบ: helper รันอยู่ (Alt+Shift+X fallback — Alt+X โดน NVIDIA App ถือ) · node console โชว์ = รัน NvNode.exe index.js ตรงจาก NVIDIA NodeAPI\\ · ทดสอบ fire = POST :59011/?hk=OpenShare + :59013/show

งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'''
print('anchor found:', old in s)
s = s.replace(old, new)
io.open(p, 'w', encoding='utf-8').write(s)
print('saved')
