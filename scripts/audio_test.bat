@echo off
chcp 65001 >nul
setlocal

set FFMPEG=C:\My Project\NVIDIA-Shadowplay\Overlay\bin\Release\net8.0-windows10.0.26100.0\api-core\ffmpeg.exe
if not exist "%FFMPEG%" set FFMPEG=C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe

echo ═══════════════════════════════════════════════════
echo  FFmpeg Audio Device Tester
echo  FFmpeg: %FFMPEG%
echo ═══════════════════════════════════════════════════
echo.

echo [1] ffmpeg -version
echo ─────────────────────────────────────
"%FFMPEG%" -version 2>&1
echo.
echo.

echo [2] ffmpeg -devices (all input/output formats)
echo ─────────────────────────────────────
"%FFMPEG%" -devices -hide_banner 2>&1
echo.
echo.

echo [3] ffmpeg -list_devices true -f dshow -i dummy (DirectShow devices)
echo ─────────────────────────────────────
"%FFMPEG%" -list_devices true -f dshow -i dummy 2>&1
echo.
echo.

echo [4] ffmpeg -list_devices true -f wasapi -i dummy (WASAPI devices — if supported)
echo ─────────────────────────────────────
"%FFMPEG%" -list_devices true -f wasapi -i dummy 2>&1
echo.
echo.

echo [5] ffmpeg -encoders (audio encoders only)
echo ─────────────────────────────────────
"%FFMPEG%" -encoders -hide_banner 2>&1 | findstr /I "aac opus mp3 flac pcm"
echo.
echo.

echo [6] ffmpeg -filters (audio filters we use)
echo ─────────────────────────────────────
"%FFMPEG%" -filters -hide_banner 2>&1 | findstr /I "volume highpass lowpass afftdn aresample aformat amix loudnorm anull"
echo.
echo.

echo [7] ffmpeg -formats (check for wasapi/dshow)
echo ─────────────────────────────────────
"%FFMPEG%" -formats -hide_banner 2>&1 | findstr /I "wasapi dshow"
echo.
echo.

echo [8] Windows Sound Devices (PowerShell)
echo ─────────────────────────────────────
powershell -NoProfile -Command "Get-CimInstance Win32_SoundDevice | Select-Object Name, Status | Format-Table -AutoSize" 2>&1
echo.
echo.

echo [9] Windows Audio Endpoints (PowerShell — COM)
echo ─────────────────────────────────────
powershell -NoProfile -Command "Get-WmiObject Win32_SoundDevice | Select-Object Name, Manufacturer, Status" 2>&1
echo.
echo.

echo [10] Stereo Mix check (PowerShell — registry)
echo ─────────────────────────────────────
powershell -NoProfile -Command "Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture' -ErrorAction SilentlyContinue | ForEach-Object { $p = Get-ItemProperty $_.PSPath -ErrorAction SilentlyContinue; if ($p) { Write-Host $_.PSChildName ':' $p.FriendlyName } }" 2>&1
echo.
echo.

echo [11] Recording test: video only (5s, no audio)
echo ─────────────────────────────────────
"%FFMPEG%" -y -f lavfi -t 5 -i "ddagrab=output_idx=0:framerate=60" -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v 10000000 -minrate 10000000 -maxrate 10000000 -bufsize 10000000 -g 60 -fps_mode cfr -movflags +faststart "C:\Users\ScotcsDuluka\Desktop\audio_test_video_only.mp4" 2>&1
echo.
echo.

echo [12] Recording test: video + dshow audio "Stereo Mix" (5s)
echo ─────────────────────────────────────
"%FFMPEG%" -y -f lavfi -t 5 -i "ddagrab=output_idx=0:framerate=60" -thread_queue_size 4096 -f dshow -i audio="Stereo Mix" -map 0:v -map 1:a -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v 10000000 -minrate 10000000 -maxrate 10000000 -bufsize 10000000 -g 60 -fps_mode cfr -c:a aac -b:a 320k -ar 48000 -movflags +faststart "C:\Users\ScotcsDuluka\Desktop\audio_test_stereo_mix.mp4" 2>&1
echo.
echo.

echo [13] Recording test: video + dshow audio "Microphone" (5s)
echo ─ Run this one — it will show available device names in the error
echo ─────────────────────────────────────
"%FFMPEG%" -y -f lavfi -t 5 -i "ddagrab=output_idx=0:framerate=60" -thread_queue_size 4096 -f dshow -i audio="Microphone" -map 0:v -map 1:a -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v 10000000 -minrate 10000000 -maxrate 10000000 -bufsize 10000000 -g 60 -fps_mode cfr -c:a aac -b:a 320k -ar 48000 -movflags +faststart "C:\Users\ScotcsDuluka\Desktop\audio_test_mic.mp4" 2>&1
echo.
echo.

echo [14] Recording test: video + wasapi "default" (5s — will fail if no WASAPI)
echo ─────────────────────────────────────
"%FFMPEG%" -y -f lavfi -t 5 -i "ddagrab=output_idx=0:framerate=60" -thread_queue_size 4096 -f wasapi -i "default" -map 0:v -map 1:a -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v 10000000 -minrate 10000000 -maxrate 10000000 -bufsize 10000000 -g 60 -fps_mode cfr -c:a aac -b:a 320k -ar 48000 -movflags +faststart "C:\Users\ScotcsDuluka\Desktop\audio_test_wasapi.mp4" 2>&1
echo.
echo.

echo [15] Recording test: video + openal audio (5s — alternative to dshow)
echo ─────────────────────────────────────
"%FFMPEG%" -y -f lavfi -t 5 -i "ddagrab=output_idx=0:framerate=60" -thread_queue_size 4096 -f openal -i "default" -map 0:v -map 1:a -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v 10000000 -minrate 10000000 -maxrate 10000000 -bufsize 10000000 -g 60 -fps_mode cfr -c:a aac -b:a 320k -ar 48000 -movflags +faststart "C:\Users\ScotcsDuluka\Desktop\audio_test_openal.mp4" 2>&1
echo.
echo.

echo ═══════════════════════════════════════════════════
echo  DONE — all tests completed
echo  Check which tests produced output files on Desktop.
echo ═══════════════════════════════════════════════════
echo.

dir "C:\Users\ScotcsDuluka\Desktop\audio_test_*.mp4" 2>nul

echo.
pause
