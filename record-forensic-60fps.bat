@echo off
echo NVIDIA ShadowPlay Forensic 60 FPS Recording
echo =======================================

REM Configuration
set FPS=60
set DURATION=5
set OUTPUT_DIR=C:\My Project\NVIDIA-Shadowplay\test-recordings
set LOG_FILE=%OUTPUT_DIR%\forensic-60fps-%DATE:/=-%-%TIME::=-%.log
set OUTPUT_FILE=%OUTPUT_DIR%\forensic-60fps-%DATE:/=-%-%TIME::=-%.mp4

REM Create output directory if it doesn't exist
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo Starting forensic recording...
echo FPS: %FPS%
echo Duration: %DURATION% seconds
echo Output: %OUTPUT_FILE%
echo Log: %LOG_FILE%
echo.

REM Run the console driver with timing instrumentation
cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

echo Running forensic test with 10ms timeline delay ON...
echo ====================================================

.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps %FPS% ^
    --duration %DURATION% ^
    --output "%OUTPUT_FILE%" ^
    --log-level Debug ^
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