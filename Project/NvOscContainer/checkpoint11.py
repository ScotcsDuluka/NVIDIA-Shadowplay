import io
p = r'C:\My Project\OSC-ShadowPlay\HANDOFF-NEW-CHAT.md'
s = io.open(p, encoding='utf-8').read()
old = 'งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'
new = '''**🛑 สถานะ ณ ปิดเซสชัน (06:5x):** host ตัวใหม่ (deploy ที่ NVIDIA OSC Native\\ — build จากซอร์สที่คืนค่า + ไอคอน) **renderer crash-loop** (`[renderer] TERMINATED status=2` วนซ้ำ + ERR_ABORTED) → หน้าโหลดไม่จบ → ไม่มี UI บนจอ · backend :59011 รันอยู่ ✓ (NvNode console เปิดโชว์อยู่) · helper รันอยู่ (Alt+Shift+X fallback, Alt+X โดน NVIDIA App ถือ) · **สาเหตุ renderer loop ยังไม่ชี้ขาด** — ต่างจากบูตที่เวิร์ก (05:2x): (a) exe ใหม่มี app.rc (ไอคอน+VERSIONINFO) (b) ซอร์ส = ฉบับ restore (osc_main.full.cpp) ซึ่งอาจไม่มี merge ของ OWNER (gdi_compositor + NODE_INFO case ที่สอง port 59001+secret — ดูได้จาก build log เดิม) — **ทางแก้ที่ลองตามลำดับ:** (1) ถอด app.rc ออก build ใหม่ (2) เทียบ diff กับ osc_main.dumb.cpp (ตัวที่บูตได้จริง — dumb host บูตผ่าน crash ไม่ loop) (3) ถ้ายัง loop = จับ crash dump (procdump -ma) · ห้ามลืม: renderer loop ทำให้ CDP eval timeout ปกติ

งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'''
print('anchor found:', old in s)
s = s.replace(old, new)
io.open(p, 'w', encoding='utf-8').write(s)
print('saved')
