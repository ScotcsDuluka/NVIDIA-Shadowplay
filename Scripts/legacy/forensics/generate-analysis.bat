@echo off
echo Generating analysis for %1 recording
echo ======================================

set recording=%1
set output_dir=C:\My Project\NVIDIA-Shadowplay\test-recordings

echo "=== FFPROBE ANALYSIS - %recording%.mp4 ===" > "%output_dir%\%recording%_analysis.txt"
"%output_dir%\..\Overlay\API-Core\ffprobe.exe" -v error -select_streams v:0 -show_format -show_streams -show_data "%output_dir%\%recording%.mp4" >> "%output_dir%\%recording%_analysis.txt"

echo "=== FRAME TIMING ANALYSIS - %recording%.mp4 ===" >> "%output_dir%\%recording%_analysis.txt"
"%output_dir%\..\Overlay\API-Core\ffprobe.exe" -v error -select_streams v:0 -show_entries frame=pkt_pts_time,pkt_dts_time,best_effort_timestamp_time,pkt_duration_time -read_intervals "%+#60" "%output_dir%\%recording%.mp4" >> "%output_dir%\%recording%_analysis.txt"

echo "=== TIMING INTERVAL CALCULATIONS ===" >> "%output_dir%\%recording%_analysis.txt"
echo "Calculating frame intervals..." >> "%output_dir%\%recording%_analysis.txt"

REM Extract frame timings and calculate intervals
"%output_dir%\..\Overlay\API-Core\ffprobe.exe" -v error -select_streams v:0 -show_entries frame=pkt_dts_time -read_intervals "%+#10" "%output_dir%\%recording%.mp4" | findstr "pkt_dts_time" > "%output_dir%\%recording%_temp.txt"

REM Calculate intervals
set prev_time=
for /f "tokens=2 delims==" %%t in ('type "%output_dir%\%recording%_temp.txt"') do (
    set current_time=%%t
    if defined prev_time (
        set /a interval_ms=(!current_time!*1000 - !prev_time!*1000)/1
        echo Frame interval: !prev_time! -> !current_time! = !interval_ms!ms >> "%output_dir%\%recording%_analysis.txt"
    )
    set prev_time=!current_time!
)

echo Analysis complete! Check %recording%_analysis.txt