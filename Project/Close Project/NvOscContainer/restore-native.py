import io, shutil, os

# 1) คืน osc_main.cpp = ตัวเต็ม (เก็บตัวโง่ไว้)
base = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA OSC'
full = os.path.join(base, 'osc_main.full.cpp')
cur = os.path.join(base, 'osc_main.cpp')
shutil.copy(cur, os.path.join(base, 'osc_main.dumb.cpp'))
shutil.copy(full, cur)
print('osc_main.cpp = full version restored')

# 2) ย้าย appdata กลับ (shared storage ของหน้า)
native = r'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC Native'
old_app = r'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC\appdata'
new_app = os.path.join(native, 'appdata')
os.makedirs(native, exist_ok=True)
if os.path.exists(old_app) and not os.path.exists(new_app):
    shutil.move(old_app, new_app)
    print('appdata moved back')

# 3) runtime CEF73 + blobs กลับไป Native dir (จาก NVIDIA OSC\ ที่ deploy ไว้)
src_dir = r'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC'
files = ['libcef.dll','chrome_elf.dll','libEGL.dll','libGLESv2.dll','d3dcompiler_47.dll',
         'd3dcompiler_43.dll','icudtl.dat','v8_context_snapshot.bin','natives_blob.bin',
         'snapshot_blob.bin','cef.pak','cef_100_percent.pak','cef_200_percent.pak']
for fn in files:
    src = os.path.join(src_dir, fn)
    dst = os.path.join(native, fn)
    if os.path.exists(src) and not os.path.exists(dst):
        shutil.copy(src, dst)
print('runtime files ensured')

# 4) locales
src_loc = os.path.join(src_dir, 'locales')
dst_loc = os.path.join(native, 'locales')
if os.path.exists(src_loc) and not os.path.exists(dst_loc):
    shutil.copytree(src_loc, dst_loc)
print('locales ensured')
