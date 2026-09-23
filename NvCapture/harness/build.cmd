@echo off
REM build.cmd — NvCapture ABI harness, freestanding x64 (no CRT, no SDK).
REM SEH (__try) resolves through ntdll!__C_specific_handler via ..\ntdll_min.def.
setlocal
cd /d "%~dp0."
set VSTOOLS=C:\Visual Studio\VC\Tools\MSVC\14.51.36231
set BINX64=%VSTOOLS%\bin\Hostx64\x64

if not exist build mkdir build

if not exist build\kernel32_min_x64.lib (
  "%BINX64%\lib.exe" /nologo /DEF:..\nvspcap\kernel32_min.def /MACHINE:X64 /OUT:build\kernel32_min_x64.lib
)
if not exist build\ntdll_min.lib (
  "%BINX64%\lib.exe" /nologo /DEF:..\ntdll_min.def /MACHINE:X64 /OUT:build\ntdll_min.lib
)

"%BINX64%\cl.exe" /nologo /c /O2 /Zl /GS- /Gs9999999 abi_harness.cpp /Fobuild\abi_harness.obj
if errorlevel 1 (echo BUILD FAILED: compile & exit /b 1)
"%BINX64%\link.exe" /nologo /NODEFAULTLIB /MACHINE:X64 /SUBSYSTEM:CONSOLE /ENTRY:entryProc ^
  build\abi_harness.obj build\kernel32_min_x64.lib build\ntdll_min.lib ^
  /OUT:build\abi_harness.exe
if errorlevel 1 (echo BUILD FAILED: link & exit /b 1)

echo BUILD OK: build\abi_harness.exe
endlocal
