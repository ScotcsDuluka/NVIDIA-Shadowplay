@echo off
echo NVIDIA ShadowPlay No-Delay Experiment (10ms timeline delay OFF)
echo =========================================================

REM Configuration
set FPS=60
set DURATION=5
set OUTPUT_DIR=C:\My Project\NVIDIA-Shadowplay\test-recordings
set LOG_FILE=%OUTPUT_DIR%\no-delay-experiment-60fps-%DATE:/=-%-%TIME::=-%.log
set OUTPUT_FILE=%OUTPUT_DIR%\no-delay-experiment-60fps-%DATE:/=-%-%TIME::=-%.mp4

REM Create output directory if it doesn't exist
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo Starting no-delay experiment recording...
echo FPS: %FPS%
echo Duration: %DURATION% seconds
echo Output: %OUTPUT_FILE%
echo Log: %LOG_FILE%
echo.

REM IMPORTANT: This experiment requires modifying CaptureSession.vb to remove the 10ms delay
REM Before running, replace the line:
REM   _timelineStartTicks = Stopwatch.GetTimestamp() + Math.Max(1L, Stopwatch.Frequency \ 10L)
REM With:
REM   _timelineStartTicks = Stopwatch.GetTimestamp()
REM
REM After running, restore the original code.

echo Running experiment with 10ms timeline delay OFF...
echo ====================================================

cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps %FPS% ^
    --duration %DURATION% ^
    --output "%OUTPUT_FILE%" ^
    --log-level Debug ^
    --enable-timing-instrumentation

echo.
echo Experiment completed. Check the log file for timing data:
echo %LOG_FILE%
echo.
echo Timing instrumentation will trace:
echo - Frame capture timing (DdagrabBackend)
echo - Queue dequeue timing (CaptureSession)
echo - CFR selection timing (CaptureSession)
echo - Encoder input/output timing (NvencEncoderBackend)
echo - Muxer feed timing (CaptureSession)
echo.
echo Look for "FORENSIC INSTRUMENTATION" entries in the log.
echo.
echo Remember to restore the 10ms delay in CaptureSession.vb after analysis!