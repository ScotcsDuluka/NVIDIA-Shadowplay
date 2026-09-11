@echo off
echo NVIDIA ShadowPlay - Baseline Recording #1
echo ===========================================
echo FPS: 60
echo Duration: 12 seconds
echo 10ms timeline delay: ON
echo.

cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

echo Starting baseline recording #1...
echo ==================================

.\CaptureEngine.Recording.ConsoleDriver.exe ^
    --fps 60 ^
    --duration 12 ^
    --output "C:\My Project\NVIDIA-Shadowplay\test-recordings\baseline_01.mp4" ^
    --log-level Info ^
    --enable-timing-instrumentation

echo.
echo Recording #1 completed!
echo Generating analysis...
echo.

REM Generate ffprobe analysis
echo "=== FFPROBE ANALYSIS - baseline_01.mp4 ===" > "C:\My Project\NVIDIA-Shadowplay\test-recordings\baseline_01_analysis.txt"
ffprobe -v error -select_streams v:0 -show_format -show_streams -show_data "C:\My Project\NVIDIA-Shadowplay\test-recordings\baseline_01.mp4" >> "C:\My Project\NVIDIA-Shadowplay\test-recordings\baseline_01_analysis.txt"

echo "=== FRAME TIMING ANALYSIS - baseline_01.mp4 ===" >> "C:\My Project\NVIDIA-Shadowplay\test-recordings\baseline_01_analysis.txt"
ffprobe -v error -select_streams v:0 -show_entries frame=pkt_pts_time,pkt_dts_time,best_effort_timestamp_time,pkt_duration_time -read_intervals "%+#60" "C:\My Project\NVIDIA-Shadowplay\test-recordings\baseline_01.mp4" >> "C:\My Project\NVIDIA-Shadowplay\test-recordings\baseline_01_analysis.txt"

echo Analysis complete! Check baseline_01_analysis.txt