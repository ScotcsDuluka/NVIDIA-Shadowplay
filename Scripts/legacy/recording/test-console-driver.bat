@echo off
echo Testing console driver directly...
echo.

cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

echo Running: .\CaptureEngine.Recording.ConsoleDriver.exe --fps 60 --duration 3 --output "..\..\test-recordings\test-short.mp4" --log-level Debug --enable-timing-instrumentation
echo.

.\CaptureEngine.Recording.ConsoleDriver.exe --fps 60 --duration 3 --output "..\..\test-recordings\test-short.mp4" --log-level Debug --enable-timing-instrumentation

echo.
echo Exit code: %ERRORLEVEL%
echo.

if exist "..\..\test-recordings\test-short.mp4" (
    echo SUCCESS: MP4 file created
    dir "..\..\test-recordings\test-short.mp4"
) else (
    echo FAILED: MP4 file not created
)

echo.
echo Checking for log files...
dir *.log 2>nul || echo No log files found