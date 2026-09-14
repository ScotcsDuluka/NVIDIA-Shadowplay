@echo off
REM build-nvhook.cmd — compile the NVIDIA Share in-game hook DLL.
REM Requires: MSVC (VC\Tools\MSVC) + Windows 11 SDK (d3d11.h/dxgi.h).
setlocal
set VSTOOLS=C:\Visual Studio\VC\Tools\MSVC\14.51.36231
set SDK=C:\Program Files (x86)\Windows Kits\10
for /f "delims=" %%v in ('dir /b /ad "%SDK%\Include" 2^>nul') do set SDKVER=%%v

set INC=/I"%VSTOOLS%\include" /I"%SDK%\Include\%SDKVER%\um" /I"%SDK%\Include\%SDKVER%\ucrt" /I"%SDK%\Include\%SDKVER%\shared"
set LIB=/LIBPATH:"%VSTOOLS%\lib\x64" /LIBPATH:"%SDK%\Lib\%SDKVER%\um\x64" /LIBPATH:"%SDK%\Lib\%SDKVER%\ucrt\x64"

"%VSTOOLS%\bin\Hostx64\x64\cl.exe" /LD /EHsc /O2 nvhook.cpp %INC% ^
  /link %LIB% user32.lib gdi32.lib dxgi.lib d3d11.lib d3dcompiler.lib ^
  /OUT:NvidiaShareHook.dll

if exist NvidiaShareHook.dll (echo BUILD OK: NvidiaShareHook.dll) else (echo BUILD FAILED)
endlocal
