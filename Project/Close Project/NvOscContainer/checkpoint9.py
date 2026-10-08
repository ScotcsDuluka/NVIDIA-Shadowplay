import io
p = r'C:\My Project\OSC-ShadowPlay\HANDOFF-NEW-CHAT.md'
s = io.open(p, encoding='utf-8').read()
old = 'งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'
new = '''**⏳ จุดสุดท้าย (05:5x — ยืนยัน: socket หน้าต่อแล้ว 13 ESTABLISHED แต่ fire ไม่ขับเคลื่อน):** event จาก node ถึงหน้าแล้วแต่**โซ่ภายใน bundle ไม่เดิน** — โซ่: socket event `/ShadowPlay/v.1.0/Hotkey` → hotkeyService (u.init: register f → m="hotkeyEvent" → handler d keyMatch) → hotKeyMapping.OpenShare → displayService A(e)/T() → openOSC → WIN_OPEN — **งานต่อไป (ชัด):** อ่านโค้ดแท้ decompile — `docs/osc/unpacked/src/app/` (socketService + hotkeyService + displayService) — หา (1) payload shape ที่ handler d ต้องการ (2) event อื่นที่ node แท้ emit คู่กันตอน toggle (Window State / OSC state) (3) แล้วให้ shim fire() emit เป๊ะตามนั้น (ตอนนี้ shim ยิง `{hotKeyName:"OpenShare"}` ผ่าน '/ShadowPlay/v.1.0/Hotkey' — อาจยังขาด event คู่/field) · สคริปต์วัดพร้อม: reload + ดัก XHR + netstat ESTABLISHED (อยู่ Close Project\\NvOscContainer\\osc_hack.py)

งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'''
print('anchor found:', old in s)
s = s.replace(old, new)
io.open(p, 'w', encoding='utf-8').write(s)
print('saved')
