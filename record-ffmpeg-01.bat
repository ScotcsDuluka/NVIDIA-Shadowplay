@echo off
echo NVIDIA ShadowPlay - FFmpeg Direct Recording
echo ============================================

cd "C:\My Project\NVIDIA-Shadowplay"

echo Creating FFmpeg recording command...
echo ====================================

echo Command: .\Overlay\API-Core\ffmpeg.exe -f gdigrab -framerate 60 -i desktop -c:v libx264 -preset ultrafast -crf 23 -f mp4 "test-recordings\ffmpeg_01.mp4"
echo.

.\Overlay\API-Core\ffmpeg.exe -f gdigrab -framerate 60 -i desktop -c:v libx264 -preset ultrafast -crf 23 -f mp4 "test-recordings\ffmpeg_01.mp4"

echo.
echo FFmpeg recording completed!
echo Exit code: %ERRORLEVEL%
echo.

if exist "test-recordings\ffmpeg_01.mp4" (
    echo SUCCESS: MP4 file created
    dir "test-recordings\ffmpeg_01.mp4"
) else (
    echo FAILED: MP4 file not created
)

echo.
echo Analyzing recording...
call generate-analysis.bat ffmpeg_01