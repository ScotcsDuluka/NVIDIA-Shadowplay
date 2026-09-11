@echo off
echo NVIDIA ShadowPlay - Overlay Recording with Full Instrumentation
echo ================================================================

cd "C:\My Project\NVIDIA-Shadowplay\Overlay\bin\Debug\net10.0-windows10.0.26100.0"

echo Killing any hanging processes...
taskkill /IM "NVIDIA Capture.exe" /F 2>nul
taskkill /IM "NVIDIA ShadowPlay.exe" /F 2>nul
timeout /t 2 /nobreak >nul

echo Starting Overlay application...
echo ==============================

echo Command: .\Launcher.exe --config "..\..\config\recording-config.json"
echo.

.\Launcher.exe --config "..\..\config\recording-config.json"

echo.
echo Recording completed!
echo.

echo Checking for output files...
if exist "..\..\test-recordings\overlay_01.mp4" (
    echo SUCCESS: MP4 file created
    dir "..\..\test-recordings\overlay_01.mp4"
) else (
    echo FAILED: MP4 file not created
)

echo.
echo Generating analysis...
call "..\..\generate-analysis.bat" overlay_01

echo.
echo Checking for timing logs...
if exist "*.log" (
    echo Found timing logs:
    dir *.log
) else (
    echo No timing logs found
)