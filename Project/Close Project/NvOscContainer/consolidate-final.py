import io, os, shutil, subprocess

# 1) osc_main.cpp: boot text
f = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA OSC\osc_main.cpp'
s = io.open(f, encoding='utf-8').read()
old = 'Log("=== NVIDIA OSC Native boot (OSR, CEF 73 = engine เดียวกับ Share.exe) === CreateBrowserSync="'
new = 'Log("=== NVIDIA OSC boot (windowed, CEF 73 = engine เดียวกับ Share.exe) === CreateBrowserSync="'
print('boot text found:', old in s)
s = s.replace(old, new)
io.open(f, 'w', encoding='utf-8', newline='').write(s)
print('boot text updated')

# 2) rename vcxproj (เอาชื่อ Native ออก)
src = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA OSC\NVIDIA OSC Native.vcxproj'
dst = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA OSC\NVIDIA OSC.vcxproj'
if os.path.exists(src):
    if os.path.exists(dst): os.remove(dst)
    os.rename(src, dst)
    print('vcxproj renamed → NVIDIA OSC.vcxproj')

# 3) post-build event: copy exe + osc ลง deploy dir (ใน vcxproj ใหม่)
s = io.open(dst, encoding='utf-8-sig').read()
if 'PostBuildEvent' not in s:
    pb = '''  <PropertyGroup>
    <PostBuildEvent>copy /Y "$(TargetPath)" "C:\\My Project\\NVIDIA-Shadowplay\\build\\NVIDIA ShadowPlay\\Overlay OSC\\NVIDIA OSC\\NVIDIA OSC.exe"
xcopy /E /I /Y "$(ProjectDir)osc" "C:\\My Project\\NVIDIA-Shadowplay\\build\\NVIDIA ShadowPlay\\Overlay OSC\\NVIDIA OSC\\osc\\"</PostBuildEvent>
  </PropertyGroup>
'''
    s = s.replace('  <Import Project="$(VCTargetsPath)\\Microsoft.Cpp.props" />',
                  '  <Import Project="$(VCTargetsPath)\\Microsoft.Cpp.props" />\n' + pb)
    io.open(dst, 'w', encoding='utf-8-sig').write(s)
    print('post-build added')

# 4) ย้าย appdata (shared storage ของหน้า) จาก dir เก่า → dir ใหม่
old_app = r'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC Native\appdata'
new_app = r'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC\appdata'
if os.path.exists(old_app) and not os.path.exists(new_app):
    shutil.move(old_app, new_app)
    print('appdata moved')
elif os.path.exists(new_app):
    print('appdata ใหม่มีอยู่แล้ว — คงไว้')

# 5) start-nvnode.ps1: OSC_DIR → deploy dir
ps = r'C:\My Project\NVIDIA-Shadowplay\Project\NvOscContainer\start-nvnode.ps1'
io.open(ps, 'w', encoding='utf-8').write(
    "$env:OSC_DIR = 'C:\\My Project\\NVIDIA-Shadowplay\\build\\NVIDIA ShadowPlay\\Overlay OSC\\NVIDIA OSC\\osc'\n"
    "Start-Process -FilePath 'C:\\My Project\\NVIDIA-Shadowplay\\Project\\Overlay OSC\\NVIDIA NodeAPI\\NvNode.exe' -ArgumentList 'index.js' -WorkingDirectory 'C:\\My Project\\NVIDIA-Shadowplay\\Project\\Overlay OSC\\NVIDIA NodeAPI'\n")
print('start-nvnode.ps1 → OSC_DIR = deploy osc')

# 6) ลบ deploy dir เก่า (NVIDIA OSC Native) — appdata ย้ายแล้ว
old_dir = r'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC Native'
if os.path.exists(old_dir):
    shutil.rmtree(old_dir)
    print('old dir NVIDIA OSC Native removed')
else:
    print('old dir ไม่มีอยู่แล้ว')
