@echo off
echo NVIDIA ShadowPlay 10ms Delay Removal Experiment
echo ===============================================

REM Configuration
set FPS=60
set DURATION=5
set OUTPUT_DIR=C:\My Project\NVIDIA-Shadowplay\test-recordings
set EXPERIMENT_LOG=%OUTPUT_DIR%\timing-experiment-%FPS%fps-nodelay-%DATE:/=-%-%TIME::=-%.log

REM Create output directory if it doesn't exist
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo Starting experiment: Remove 10ms timeline delay
echo FPS: %FPS%
echo Duration: %DURATION% seconds
echo Output: %EXPERIMENT_LOG%
echo.

REM IMPORTANT: This script requires modifying CaptureSession.vb to remove the 10ms delay
REM Before running this, modify the line:
REM   _timelineStartTicks = Stopwatch.GetTimestamp() + Math.Max(1L, Stopwatch.Frequency \ 10L)
REM To:
REM   _timelineStartTicks = Stopwatch.GetTimestamp()

echo WARNING: This is an EXPERIMENTAL test
echo =====================================
echo This test will remove the 10ms timeline delay from CaptureSession.vb
echo After running, you MUST restore the original code to maintain baseline.
echo.

echo Running experiment test...
echo =========================

cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps %FPS% ^
    --duration %DURATION% ^
    --output "%OUTPUT_DIR%\experiment-%FPS%fps-nodelay.mp4" ^
    --log-level Info ^
    --enable-timing-instrumentation

echo.
echo Experiment completed!
echo.
echo Results comparison:
echo ===================
echo Baseline (10ms delay): test-60fps-baseline.bat
echo Experiment (no delay): this test
echo.
echo Key metrics to compare:
echo 1. First-frame presentation time
echo 2. Overall recording start delay
echo 3. A/V sync accuracy
echo 4. Frame timing consistency
echo.
echo After analysis, remember to restore the 10ms delay in CaptureSession.vb!
echo Use: git checkout CaptureEngine.Recording/CaptureSession.vb