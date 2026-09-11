@echo off
echo NVIDIA ShadowPlay Full FPS Matrix Testing
echo =====================================

REM Configuration
set DURATION=5
set OUTPUT_DIR=C:\My Project\NVIDIA-Shadowplay\test-recordings
set LOG_DIR=%OUTPUT_DIR%\fps-matrix-logs

REM Create output directories
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

echo Starting FPS matrix testing...
echo Duration: %DURATION% seconds
echo Output directory: %OUTPUT_DIR%
echo.

REM Define FPS values to test
set FPS_VALUES=30 60 120 144 240

REM Run tests for each FPS
for %%f in (%FPS_VALUES%) do (
    echo Testing %%f FPS...
    echo ====================================
    
    set LOG_FILE=%LOG_DIR%\fps-%%f-test-%DATE:/=-%-%TIME::=-%.log
    set OUTPUT_FILE=%OUTPUT_DIR%\fps-%%f-test-%DATE:/=-%-%TIME::=-%.mp4
    
    echo Log: %LOG_FILE%
    echo Output: %OUTPUT_FILE%
    echo.
    
    REM Run the console driver
    cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"
    
    .\CaptureEngine.Recording.ConsoleDriver.exe ^
        --fps %%f ^
        --duration %DURATION% ^
        --output "%OUTPUT_FILE%" ^
        --log-level Info ^
        --enable-timing-instrumentation
    
    echo.
    echo %%f FPS test completed.
    echo.
)

echo All FPS matrix tests completed!
echo Check the log files in: %LOG_DIR%
echo.