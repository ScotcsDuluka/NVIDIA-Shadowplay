@echo off
echo NVIDIA ShadowPlay 38ms Gap Analysis Test
echo ===========================================

REM Configuration
set FPS=60
set DURATION=5
set OUTPUT_DIR=C:\My Project\NVIDIA-Shadowplay\test-recordings
set LOG_FILE=%OUTPUT_DIR%\timing-forensic-%FPS%fps-%DATE:/=-%-%TIME::=-%.log
set OUTPUT_FILE=%OUTPUT_DIR%\baseline-%FPS%fps.mp4

REM Create output directory if it doesn't exist
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo Starting %FPS% FPS recording test...
echo Duration: %DURATION% seconds
echo Output: %LOG_FILE%
echo.

REM Run the console driver with timing instrumentation
cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

echo Running baseline test with 10ms timeline delay ON...
echo ====================================================

.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps %FPS% ^
    --duration %DURATION% ^
    --output "%OUTPUT_FILE%" ^
    --log-level Info ^
    --enable-timing-instrumentation

echo.
echo Test completed. Check the log file for timing data:
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