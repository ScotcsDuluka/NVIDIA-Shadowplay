import io

# 1) osc_main.cpp: defaults → NVIDIA OSC\ + boot text CEF 73
f = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA OSC\osc_main.cpp'
s = io.open(f, encoding='utf-8').read()
old1 = r'std::wstring cache_path = L"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC Native\CefCache";'
new1 = r'std::wstring cache_path = L"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC\CefCache";'
print('cache default found:', old1 in s)
s = s.replace(old1, new1)
old2 = r'std::wstring subprocess_path = L"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC Native\NVIDIA OSC.exe";'
new2 = r'std::wstring subprocess_path = L"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC\NVIDIA OSC.exe";'
print('subprocess default found:', old2 in s)
s = s.replace(old2, new2)
old3 = 'Log("=== NVIDIA OSC Native boot (OSR + DComp GPU, Chromium 138) === CreateBrowserSync="'
new3 = 'Log("=== NVIDIA OSC Native boot (OSR, CEF 73 = engine เดียวกับ Share.exe) === CreateBrowserSync="'
print('boot text found:', old3 in s)
s = s.replace(old3, new3)
io.open(f, 'w', encoding='utf-8', newline='').write(s)
print('osc_main.cpp updated')

# 2) nvidia-osc.json: cef.cachePath + subprocessPath → NVIDIA OSC\
p = r'C:\My Project\NVIDIA-Shadowplay\Project\NvConfig\nvidia-osc.json'
s = io.open(p, encoding='utf-8-sig').read()
s = s.replace(r'Overlay OSC\NVIDIA OSC Native\CefCache', r'Overlay OSC\NVIDIA OSC\CefCache')
s = s.replace(r'Overlay OSC\NVIDIA OSC Native\NVIDIA OSC.exe', r'Overlay OSC\NVIDIA OSC\NVIDIA OSC.exe')
io.open(p, 'w', encoding='utf-8-sig').write(s)
print('json paths updated')
