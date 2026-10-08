import io
p = r'C:\My Project\OSC-ShadowPlay\HANDOFF-NEW-CHAT.md'
s = io.open(p, encoding='utf-8').read()
old = 'งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'
new = '''**✅ สถานะปัจจุบัน (07:2x — สาย NVIDIA OSC ใช้งานได้):** host ตัวโง่ (windowed, `build\\...\\Overlay OSC\\NVIDIA OSC\\NVIDIA OSC.exe`) = **renderer เสถียร** (OSR loop เป็นของตัวเต็มเท่านั้น) · เมนูวาดบนจอจริง (Alt+G toggle ✓) · socket ต่อ node ✓ · NODE_INFO ผ่าน /cefquery ✓ · ทุก exe มีไอคอน+Description ✓

**🅿️ ตัวเต็ม (OSR) = PARKED:** renderer crash-loop บน CEF 73 (`TERMINATED status=2` วน) — ต่างจากตัวโง่ (windowed, renderer เสถียร) — **แผน debug:** (1) procdump -ma จับ dump ตอน renderer ตาย (2) ลองปิด shared_texture_enabled ในซอร์สตัวเต็ม (3) เช็ค Windhawk mod (อยู่ใน module list ทุก crash) (4) เทียบ osc_main.dumb.cpp กับ osc_main.full.cpp ทีละระบบ · deploy dir เก่า (NVIDIA OSC Native) = ลบไปแล้ว appdata ย้ายไป NVIDIA OSC\\appdata แล้ว

งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'''
print('anchor found:', old in s)
s = s.replace(old, new)
io.open(p, 'w', encoding='utf-8').write(s)
print('saved')
