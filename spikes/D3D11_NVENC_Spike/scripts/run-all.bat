@echo off
REM
REM run-all.bat — Run all 5 phases of the D3D11/NVENC spike.
REM
REM Prerequisites:
REM   - Build succeeded (run scripts\build.bat first)
REM   - nvEncodeAPI.dll is in the output directory (see README.md)
REM   - The current user has access to the desktop (NOT a service, NOT RDP without /admin)
REM
REM Usage:
REM   scripts\run-all.bat                # run all phases, output to console
REM   scripts\run-all.bat report.md      # run all phases, tee output to report.md
REM

setlocal

set CONFIG=%2
if "%CONFIG%"=="" set CONFIG=Debug

set LOG=%1
set EXE=%~dp0\..\bin\x64\%CONFIG%\net8.0-windows\CaptureEngine.Video.Spike.D3D11.exe

if not exist "%EXE%" (
    echo.
    echo FAIL: Spike executable not found at:
    echo   %EXE%
    echo.
    echo Did you run scripts\build.bat first?
    exit /b 1
)

if not "%LOG%"=="" (
    echo Running spike with output to: %LOG%
    "%EXE%" --log "%LOG%"
) else (
    "%EXE%"
)

set EXITCODE=%ERRORLEVEL%
echo.
echo Spike exit code: %EXITCODE%
exit /b %EXITCODE%

endlocal
