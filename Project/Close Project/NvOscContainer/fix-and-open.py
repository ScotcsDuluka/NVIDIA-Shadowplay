import io, subprocess, time, urllib.request

p = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA ShadowPlay API\shims\NvShadowPlayAPINode.js'
s = io.open(p, encoding='utf-8').read()

# OpenShare branch: เติม hotkey emit (hotkeyService ของหน้าจับคู่ด้วย hotKeyName)
old = """    if (name === 'OpenShare') {
      if (oscWindowStateCallback) {
        oscWindowStateCallback({ windowMsg: 'overlayToggle' });
        console.log("[shim:hotkey] WindowState windowMsg=overlayToggle emitted");
      }
      return;
    }"""
new = """    if (name === 'OpenShare') {
      if (hotkeyCallback) hotkeyCallback(data);
      if (oscWindowStateCallback) {
        oscWindowStateCallback({ windowMsg: 'overlayToggle' });
        console.log("[shim:hotkey] WindowState windowMsg=overlayToggle emitted");
      }
      return;
    }"""
print('anchor found:', old in s)
s = s.replace(old, new)
io.open(p, 'w', encoding='utf-8', newline='').write(s)
print('shim patched')

# restart shim (node ตามไปเกิดใหม่ — โหลดไฟล์ที่แก้แล้ว)
subprocess.run(['taskkill', '/IM', 'NVIDIA Web Helper.exe', '/F'], capture_output=True)
subprocess.run(['taskkill', '/IM', 'NvNode.exe', '/F'], capture_output=True)
time.sleep(1.5)
subprocess.Popen([r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA ShadowPlay API\NVIDIA Web Helper.exe'],
                 cwd=r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA ShadowPlay API')
time.sleep(8)

# ยิง: node event (หน้า toggle) + host show (หน้าต่าง)
for url in ['http://127.0.0.1:59011/?hk=OpenShare', 'http://127.0.0.1:59013/show']:
    try:
        urllib.request.urlopen(urllib.request.Request(url, data=b'{}'), timeout=5).read()
    except Exception as e:
        print('fire err', url, e)
time.sleep(3)
print('fired + shown')
