@echo off
echo Testing minimal console driver command...
echo.

cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

echo Current directory:
dir
echo.

echo Testing if executable exists:
if exist "CaptureEngine.Recording.ConsoleDriver.exe" (
    echo Executable found
    echo Running minimal command...
    .\CaptureEngine.Recording.ConsoleDriver.exe --fps 1 --duration 2 --output "..\..\test-recordings\minimal.mp4"
    echo Exit code: %ERRORLEVEL%
) else (
    echo Executable not found
)

echo.
echo Checking output directory:
if exist "..\..\test-recordings\minimal.mp4" (
    echo SUCCESS: MP4 created
    dir "..\..\test-recordings\minimal.mp4"
) else (
    echo FAILED: MP4 not created
)