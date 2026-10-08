import io, os, shutil

# 1) osc_main.cpp: defaults กลับมา NVIDIA OSC Native\
f = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA OSC\osc_main.cpp'
s = io.open(f, encoding='utf-8').read()
s = s.replace(
    r'Overlay OSC\NVIDIA OSC\CefCache',
    r'Overlay OSC\NVIDIA OSC Native\CefCache')
s = s.replace(
    r'Overlay OSC\NVIDIA OSC\NVIDIA OSC.exe',
    r'Overlay OSC\NVIDIA OSC Native\NVIDIA OSC.exe')
io.open(f, 'w', encoding='utf-8', newline='').write(s)
print('osc_main.cpp paths → NVIDIA OSC Native')

# 2) nvidia-osc.json: cef paths กลับมาเหมือนกัน
p = r'C:\My Project\NVIDIA-Shadowplay\Project\NvConfig\nvidia-osc.json'
s = io.open(p, encoding='utf-8-sig').read()
s = s.replace(
    r'Overlay OSC\NVIDIA OSC\CefCache',
    r'Overlay OSC\NVIDIA OSC Native\CefCache')
s = s.replace(
    r'Overlay OSC\NVIDIA OSC\NVIDIA OSC.exe',
    r'Overlay OSC\NVIDIA OSC Native\NVIDIA OSC.exe')
io.open(p, 'w', encoding='utf-8-sig').write(s)
print('json paths → NVIDIA OSC Native')

# 3) deploy dir: สร้างคืน + runtime + blobs + appdata (จาก NVIDIA OSC\ ที่ยังครบ)
old = r'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC'
new = r'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC Native'
os.makedirs(new, exist_ok=True)
for fn in ['libcef.dll','chrome_elf.dll','libEGL.dll','libGLESv2.dll','d3dcompiler_47.dll',
           'd3dcompiler_43.dll','icudtl.dat','v8_context_snapshot.bin','natives_blob.bin',
           'snapshot_blob.bin','cef.pak','cef_100_percent.pak','cef_200_percent.pak']:
    src = os.path.join(old, fn)
    dst = os.path.join(new, fn)
    if os.path.exists(src) and not os.path.exists(dst):
        shutil.copy(src, dst)
if os.path.exists(os.path.join(old, 'locales')) and not os.path.exists(os.path.join(new, 'locales')):
    shutil.copytree(os.path.join(old, 'locales'), os.path.join(new, 'locales'))
if os.path.exists(os.path.join(old, 'appdata')) and not os.path.exists(os.path.join(new, 'appdata')):
    shutil.copytree(os.path.join(old, 'appdata'), os.path.join(new, 'appdata'))
# exe จาก build ล่าสุด (ตัวเต็ม)
exe_src = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA OSC\bin\x64\Release\NVIDIA OSC.exe'
if os.path.exists(exe_src):
    shutil.copy(exe_src, os.path.join(new, 'NVIDIA OSC.exe'))
print('deploy dir NVIDIA OSC Native ready (runtime + exe)')
