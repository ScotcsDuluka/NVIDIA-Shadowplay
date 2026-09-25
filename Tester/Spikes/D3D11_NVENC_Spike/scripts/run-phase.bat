@echo off
REM
REM run-phase.bat — Run a single phase of the D3D11/NVENC spike.
REM
REM Usage:
REM   scripts\run-phase.bat 4            # run Phase 4 (NVENC) only
REM   scripts\run-phase.bat 4 report.md  # tee to report.md
REM
REM Note: Phases 1-3 must run before Phase 4-5. Use scripts\run-all.bat for full run.

setlocal

set PHASE=%1
set LOG=%2
set CONFIG=%3
if "%CONFIG%"=="" set CONFIG=Debug

if "%PHASE%"=="" (
    echo Usage: scripts\run-phase.bat ^<phase^> [logfile] [config]
    exit /b 2
)

set EXE=%~dp0\..\bin\x64\%CONFIG%\net8.0-windows\CaptureEngine.Video.Spike.D3D11.exe

if not exist "%EXE%" (
    echo FAIL: Spike executable not found. Run scripts\build.bat first.
    exit /b 1
)

if not "%LOG%"=="" (
    "%EXE%" phase%PHASE% --log "%LOG%"
) else (
    "%EXE%" phase%PHASE%
)

exit /b %ERRORLEVEL%

endlocal
