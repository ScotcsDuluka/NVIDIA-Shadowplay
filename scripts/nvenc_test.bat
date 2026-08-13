@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

set FFMPEG=C:\My Project\NVIDIA-Shadowplay\Overlay\bin\Release\net8.0-windows10.0.26100.0\api-core\ffmpeg.exe
set OUTDIR=C:\Users\ScotcsDuluka\Desktop\nvenc_test
set FPS=240
set BR=100000000
set DUR=5

mkdir "%OUTDIR%" 2>nul

echo ═══════════════════════════════════════════════════
echo  NVENC Command Tester — %FPS%fps / %BR%bps / %DUR%s each
echo ═══════════════════════════════════════════════════
echo.

set TEST=0

:: ── Test 1: CBR + bufsize=1x + tune ll ──
set /a TEST+=1
set TAG=test%TEST%_cbr_buf1_tunell
echo [!TEST!] CBR bufsize=1x tune=ll g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v %BR% -minrate %BR% -maxrate %BR% -bufsize %BR% -g %FPS% -fps_mode cfr -spatial-aq 1 -temporal-aq 1 -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Test 2: CBR + bufsize=2x + tune ll ──
set /a TEST+=1
set TAG=test%TEST%_cbr_buf2_tunell
echo [!TEST!] CBR bufsize=2x tune=ll g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v %BR% -minrate %BR% -maxrate %BR% -bufsize 200000000 -g %FPS% -fps_mode cfr -spatial-aq 1 -temporal-aq 1 -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Test 3: CBR + bufsize=1x + NO tune ──
set /a TEST+=1
set TAG=test%TEST%_cbr_buf1_notune
echo [!TEST!] CBR bufsize=1x NO tune g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p4 -rc cbr -b:v %BR% -minrate %BR% -maxrate %BR% -bufsize %BR% -g %FPS% -fps_mode cfr -spatial-aq 1 -temporal-aq 1 -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Test 4: CBR + zerolatency ──
set /a TEST+=1
set TAG=test%TEST%_cbr_zerolatency
echo [!TEST!] CBR zerolatency=1 g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v %BR% -minrate %BR% -maxrate %BR% -bufsize %BR% -g %FPS% -fps_mode cfr -zerolatency 1 -spatial-aq 1 -temporal-aq 1 -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Test 5: VBR + maxrate=target ──
set /a TEST+=1
set TAG=test%TEST%_vbr_maxtarget
echo [!TEST!] VBR maxrate=target g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p4 -tune ll -rc vbr -b:v %BR% -maxrate %BR% -bufsize %BR% -g %FPS% -fps_mode cfr -spatial-aq 1 -temporal-aq 1 -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Test 6: CQ (Constant Quality) ──
set /a TEST+=1
set TAG=test%TEST%_cq_p4
echo [!TEST!] CQ cq=25 g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p4 -tune ll -rc vbr -cq 25 -b:v %BR% -maxrate %BR% -bufsize %BR% -g %FPS% -fps_mode cfr -spatial-aq 1 -temporal-aq 1 -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Test 7: CBR p7 (max quality preset) ──
set /a TEST+=1
set TAG=test%TEST%_cbr_p7
echo [!TEST!] CBR preset=p7 g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p7 -tune ll -rc cbr -b:v %BR% -minrate %BR% -maxrate %BR% -bufsize %BR% -g %FPS% -fps_mode cfr -spatial-aq 1 -temporal-aq 1 -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Test 8: CBR p1 (fastest preset) ──
set /a TEST+=1
set TAG=test%TEST%_cbr_p1
echo [!TEST!] CBR preset=p1 g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p1 -tune ll -rc cbr -b:v %BR% -minrate %BR% -maxrate %BR% -bufsize %BR% -g %FPS% -fps_mode cfr -spatial-aq 1 -temporal-aq 1 -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Test 9: CBR NO spatial-aq NO temporal-aq ──
set /a TEST+=1
set TAG=test%TEST%_cbr_noaq
echo [!TEST!] CBR NO AQ g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v %BR% -minrate %BR% -maxrate %BR% -bufsize %BR% -g %FPS% -fps_mode cfr -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Test 10: CBR + NO -fps_mode (let FFmpeg handle fps) ──
set /a TEST+=1
set TAG=test%TEST%_cbr_nofpsmode
echo [!TEST!] CBR NO fps_mode g=%FPS%
"%FFMPEG%" -y -f lavfi -t %DUR% -i "ddagrab=output_idx=0:framerate=%FPS%" -c:v h264_nvenc -preset p4 -tune ll -rc cbr -b:v %BR% -minrate %BR% -maxrate %BR% -bufsize %BR% -g %FPS% -spatial-aq 1 -temporal-aq 1 -movflags +faststart "%OUTDIR%\!TAG!.mp4" 2>nul
for %%A in ("%OUTDIR%\!TAG!.mp4") do echo     Size: %%~zA bytes
echo.

:: ── Summary ──
echo ═══════════════════════════════════════════════════
echo  DONE — %TEST% tests completed
echo  Files saved to: %OUTDIR%
echo ═══════════════════════════════════════════════════
echo.
echo  Target size: 62,500,000 bytes (100 Mbps x 5s / 8)
echo  Compare file sizes — closest to target = best CBR.
echo.
explorer "%OUTDIR%"
pause
