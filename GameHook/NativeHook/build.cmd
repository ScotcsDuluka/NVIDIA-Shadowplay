@echo off
set VSTOOLS=C:\Visual Studio\VC\Tools\MSVC\14.51.36231
set SDK=D:\SDK
set SDKVER=10.0.28000.0
set INC=/I"%VSTOOLS%\include" /I"%SDK%\Include\%SDKVER%\um" /I"%SDK%\Include\%SDKVER%\ucrt" /I"%SDK%\Include\%SDKVER%\shared"
set LIBS=/LIBPATH:"%VSTOOLS%\lib\x64" /LIBPATH:"%SDK%\Lib\%SDKVER%\um\x64" /LIBPATH:"%SDK%\Lib\%SDKVER%\ucrt\x64"
"%VSTOOLS%\bin\Hostx64\x64\cl.exe" /LD /EHsc /O2 nvhook.cpp %INC% /link %LIBS% user32.lib gdi32.lib dxgi.lib d3d11.lib d3dcompiler.lib uuid.lib /OUT:NvidiaShareHook.dll
