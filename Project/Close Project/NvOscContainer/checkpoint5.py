import io
p = r'C:\My Project\OSC-ShadowPlay\HANDOFF-NEW-CHAT.md'
s = io.open(p, encoding='utf-8').read()
old = 'ข้อควรระวัง: eval CDP ห้าม bare `return` (ห่อ IIFE) · เช็ค socket ต่อ = netstat ESTABLISHED :59011 จาก PID NVIDIA OSC'
new = ('ข้อควรระวัง: eval CDP ห้าม bare `return` (ห่อ IIFE) · เช็ค socket ต่อ = netstat ESTABLISHED :59011 จาก PID NVIDIA OSC\n\n'
       '**⏳ จุดสุดท้ายจริง (04:5x วัดแล้ว):** fire → หน้า toggle route สำเร็จ (`#/base/main-menu` + 10 tiles ใน DOM) **แต่หน้าไม่ส่ง `QUERY_WIN_OPEN_OSC`** → หน้าต่าง host ไม่โชว์ → จอเห็นแค่ dim (R22) — ขั้นต่อไป: อ่าน `T()` ใน osc/app.js (handler `"overlayToggle"===e.windowMsg?T()`) ว่าแท้เรียก `M.openOSC/M.closeOSC` ผ่านอะไร → เติม emit ที่หายใน shim fire (`NvShadowPlayAPINode.js` fire()/`global.__nvFire`) ให้ครบตามที่ node แท้ส่งคู่กัน → จบ = Alt+X เปิด overlay เต็มวงจร · ปัจจุบันดู overlay ได้ด้วย `curl :59013/show` (บังคับโชว์ — หน้าวาด main-menu แล้ว 10 tiles) + ทดสอบ Alt+Shift+X (fallback คีย์)')
print('anchor found:', old in s)
s = s.replace(old, new)
io.open(p, 'w', encoding='utf-8').write(s)
print('saved')
