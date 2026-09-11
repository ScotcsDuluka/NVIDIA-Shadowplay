@echo off
echo Starting debug recording test...
echo.

echo ================================================================
echo TEST CONFIGURATION:
echo - Duration: 5 seconds
echo - FPS: 30
echo - Video Codec: H.264 (NVENC)
echo - Audio: Enabled
echo - Debug logging: Enabled
echo ================================================================
echo.

cd /d "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

echo Starting recording...
echo.
echo ================================================================
echo DEBUG OUTPUT (first 4 frames):
echo ================================================================

CaptureEngine.Recording.ConsoleDriver.exe

echo.
echo ================================================================
echo Recording complete.
echo ================================================================

echo.
echo Check debug-recording.log for detailed timestamp analysis.