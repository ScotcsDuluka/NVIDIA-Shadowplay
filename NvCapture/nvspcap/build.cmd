@echo off
REM build.cmd — NvCapture minimal-safe nvspcap shim, freestanding (no CRT,
REM no Windows SDK: this machine has none). Generates its own minimal import
REM libraries from kernel32_min*.def via lib.exe, links /NODEFAULTLIB.
REM Outputs: build\nvspcap64.dll (x64) and build\nvspcap.dll (x86) — naming
REM follows the reference: no-suffix = x86, "64" = x64.
setlocal
cd /d "%~dp0."
set VSTOOLS=C:\Visual Studio\VC\Tools\MSVC\14.51.36231
set BINX64=%VSTOOLS%\bin\Hostx64\x64
set BINX86=%VSTOOLS%\bin\Hostx64\x86

if not exist build mkdir build

REM ---- minimal import libraries (kernel32 is the only real dependency) ----
if not exist build\kernel32_min_x64.lib (
  "%BINX64%\lib.exe" /nologo /DEF:kernel32_min.def /MACHINE:X64 /OUT:build\kernel32_min_x64.lib
)
if not exist build\kernel32_min_x86.lib (
  "%BINX64%\lib.exe" /nologo /DEF:kernel32_min_x86.def /MACHINE:X86 /OUT:build\kernel32_min_x86.lib
)

echo == x64 shim ==
"%BINX64%\cl.exe" /nologo /c /O2 /Zl /GS- /Gs9999999 nvspcap.cpp /Fobuild\nvspcap64.obj
if errorlevel 1 (echo BUILD FAILED: compile x64 & exit /b 1)
"%BINX64%\link.exe" /nologo /NODEFAULTLIB /DLL /MACHINE:X64 /ENTRY:DllMain ^
  /DEF:nvspcap.def build\nvspcap64.obj build\kernel32_min_x64.lib ^
  /OUT:build\nvspcap64.dll /PDB:build\nvspcap64.pdb
if errorlevel 1 (echo BUILD FAILED: link x64 & exit /b 1)

echo == x86 shim ==
"%BINX86%\cl.exe" /nologo /c /O2 /Zl /GS- /Gs9999999 nvspcap.cpp /Fobuild\nvspcap86.obj
if errorlevel 1 (echo BUILD FAILED: compile x86 & exit /b 1)
"%BINX86%\link.exe" /nologo /NODEFAULTLIB /DLL /MACHINE:X86 /ENTRY:DllMain@12 ^
  /DEF:nvspcap.def build\nvspcap86.obj build\kernel32_min_x86.lib ^
  /OUT:build\nvspcap.dll /PDB:build\nvspcap.pdb
if errorlevel 1 (echo BUILD FAILED: link x86 & exit /b 1)

echo BUILD OK
endlocal
