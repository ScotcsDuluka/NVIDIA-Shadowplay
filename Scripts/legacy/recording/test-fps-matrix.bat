@echo off
echo NVIDIA ShadowPlay FPS Matrix Timing Forensics
echo =============================================

REM Configuration
set OUTPUT_DIR=C:\My Project\NVIDIA-Shadowplay\test-recordings
set DURATION=3
set BASELINE_LOG=%OUTPUT_DIR%\timing-matrix-baseline.log

REM Create output directory if it doesn't exist
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo Starting FPS matrix timing forensics...
echo Duration: %DURATION% seconds per test
echo Output directory: %OUTPUT_DIR%
echo.

cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

echo Testing FPS: 30, 60, 120, 144, 240
echo ===================================

REM Test 30 FPS
echo Testing 30 FPS...
.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps 30 ^
    --duration %DURATION% ^
    --output "%OUTPUT_DIR%\test-30fps.mp4" ^
    --log-level Info ^
    --enable-timing-instrumentation

REM Test 60 FPS  
echo Testing 60 FPS...
.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps 60 ^
    --duration %DURATION% ^
    --output "%OUTPUT_DIR%\test-60fps.mp4" ^
    --log-level Info ^
    --enable-timing-instrumentation

REM Test 120 FPS
echo Testing 120 FPS...
.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps 120 ^
    --duration %DURATION% ^
    --output "%OUTPUT_DIR%\test-120fps.mp4" ^
    --log-level Info ^
    --enable-timing-instrumentation

REM Test 144 FPS
echo Testing 144 FPS...
.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps 144 ^
    --duration %DURATION% ^
    --output "%OUTPUT_DIR%\test-144fps.mp4" ^
    --log-level Info ^
    --enable-timing-instrumentation

REM Test 240 FPS
echo Testing 240 FPS...
.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps 240 ^
    --duration %DURATION% ^
    --output "%OUTPUT_DIR%\test-240fps.mp4" ^
    --log-level Info ^
    --enable-timing-instrumentation

echo.
echo FPS matrix test completed!
echo.
echo Generated recordings:
echo - %OUTPUT_DIR%\test-30fps.mp4
echo - %OUTPUT_DIR%\test-60fps.mp4  
echo - %OUTPUT_DIR%\test-120fps.mp4
echo - %OUTPUT_DIR%\test-144fps.mp4
echo - %OUTPUT_DIR%\test-240fps.mp4
echo.
echo Check the timing logs for each FPS to analyze:
echo - First-frame anomaly timing
echo - CFR selection accuracy
echo - Encoder input/output latency
echo - Muxer packet timing
echo.
echo Key metrics to compare across FPS:
echo 1. First-frame presentation time vs timeline start
echo 2. CFR frame selection lag
echo 3. Encoder processing time
echo 4. Muxer packet feed timing
echo 5. Overall pipeline latency