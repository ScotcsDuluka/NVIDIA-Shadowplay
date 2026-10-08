import io
p = r'C:\My Project\OSC-ShadowPlay\HANDOFF-NEW-CHAT.md'
s = io.open(p, encoding='utf-8').read()
old = 'งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'
new = '''**⏳ จุดสุดท้ายที่แท้จริง (05:4x วัดครบทุกชั้นแล้ว):** fire :59011 → node emit `/ShadowPlay/v.1.0/Hotkey` `{hotKeyName:"OpenShare"}` ถูกต้อง (ยืนยัน log shim) · socket หน้า ESTABLISHED ✓ — **แต่หน้าไม่เริ่ม openOSC** (log ไม่มี SET_DISPLAY_RECTS หลัง fire เลย) = event หลุดในหน้า ก่อนถึง displayService.openOSC — โซ่ที่ต้องไล่ใน bundle: hotkeyService (`u.init` register f='/ShadowPlay/v.1.0/Hotkey' → m='hotkeyEvent' → handler d keyMatch) → d รับ `{hotKeyName:'OpenShare'}` → match `hotKeyMapping.OpenShare='openshare'` → internal event 'openshare' → ใครฟัง 'openshare' → openOSC? — งานต่อ: เติม console hook ที่ handler d (ผ่าน pre-document script — addScriptToEvaluateOnNewDocument) ดูว่า event มาถึง d ไหม / match ไหม / หลุดขั้นไหน · **เบาะแส:** อาจต้อง emit เพิ่ม event อื่นที่หน้ารอ (WINDOW_STATE channel ของ node แท้ — node แท้ emit หลาย event คู่กัน: Hotkey + Window State + OSC state) — เทียบกับ decompile node แท้ (A:/ShadowPlay_V1 หรือ docs/osc/unpacked 0020.socketService.js) ว่าหน้าฟัง event อะไรบ้าง

งานที่เหลือ 2 จุด (จบ = Alt+X ครบวงจร):'''
print('anchor found:', old in s)
s = s.replace(old, new)
io.open(p, 'w', encoding='utf-8').write(s)
print('saved')
