import io
p = r'C:\My Project\OSC-ShadowPlay\HANDOFF-NEW-CHAT.md'
s = io.open(p, encoding='utf-8').read()
old = 'งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'
new = '''**⏳ จุดสุดท้ายจริง (05:2x — ไล่ครบทุกชั้นแล้ว):** server สมบูรณ์ (fire → node emit `/ShadowPlay/v.1.0/Hotkey` shape ใหม่ `{hotKeyName:"OpenShare"}` ยืนยันใน log · socket ESTABLISHED ×4 ตอนบูตเอง) — **event หลุดในหน้า**: โซ่หน้า = socket event → hotkeyService (u.init register f='/ShadowPlay/v.1.0/Hotkey' → re-emit m='hotkeyEvent') → handler d (keyMatch) → mapping hotKeyMapping.OpenShare → displayService A(e) (อ่าน e.windowMsg!) → T() → openOSC → cefQuery WIN_OPEN → จอ · จุดที่ต้องตรวจต่อ: (1) hotkeyService.init ถูกเรียกไหม (boot chain "Init UI" → T.connect() → y.init() = hotkeyService ✓ ปรากฏใน log bundle) (2) handler d รับ `{hotKeyName}` แล้ว match `hotKeyMapping.OpenShare` ไหม (3) A(e) อ่าน `e.windowMsg` — shim emit มีแค่ hotKeyName ไม่มี windowMsg → A อาจเมิน — **แก้: shim emit ทั้งสอง shape รวดกัน** `{hotKeyName:name, windowMsg:'overlayToggle'}` ลองก่อนไล่ต่อ

งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'''
print('anchor found:', old in s)
s = s.replace(old, new)
io.open(p, 'w', encoding='utf-8').write(s)
print('saved')
