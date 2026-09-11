@echo off
echo NVIDIA ShadowPlay - Baseline Recording #2 with Full Instrumentation
echo ====================================================================
echo FPS: 60
echo Duration: 12 seconds
echo 10ms timeline delay: ON
echo Output: baseline_02.mp4
echo.

cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

echo Killing any hanging processes...
taskkill /IM "NVIDIA Capture.exe" /F 2>nul
taskkill /IM "NVIDIA ShadowPlay.exe" /F 2>nul
timeout /t 2 /nobreak >nul

echo Starting baseline recording #2...
echo ==================================

.\CaptureEngine.Recording.ConsoleDriver.exe --fps 60 --duration 12 --output "..\..\test-recordings\baseline_02.mp4" --log-level Info --enable-timing-instrumentation

echo.
echo Recording #2 completed!
echo Exit code: %ERRORLEVEL%
echo.

if exist "..\..\test-recordings\baseline_02.mp4" (
    echo SUCCESS: MP4 file created
    dir "..\..\test-recordings\baseline_02.mp4"
) else (
    echo FAILED: MP4 file not created
)

echo.
echo Generating analysis...
call "..\..\generate-analysis.bat" baseline_02

echo.
echo Checking for timing logs...
if exist "*.log" (
    echo Found timing logs:
    dir *.log
) else (
    echo No timing logs found
)