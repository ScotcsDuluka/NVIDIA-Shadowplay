Write-Host "NVIDIA ShadowPlay - Baseline Recording #1"
Write-Host "==========================================="
Write-Host "FPS: 60"
Write-Host "Duration: 10 seconds"
Write-Host "10ms timeline delay: ON"
Write-Host ""

$driverPath = "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows\CaptureEngine.Recording.ConsoleDriver.exe"
$outputPath = "C:\My Project\NVIDIA-Shadowplay\test-recordings\baseline_01.mp4"

Write-Host "Starting baseline recording #1..."
Write-Host "================================="

& $driverPath --fps 60 --duration 10 --output $outputPath --log-level Info --enable-timing-instrumentation

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Recording #1 completed!"
    Write-Host "Generating analysis..."
    Write-Host ""
    
    # Generate ffprobe analysis
    $analysisFile = "C:\My Project\NVIDIA-Shadowplay\test-recordings\baseline_01_analysis.txt"
    "=== FFPROBE ANALYSIS - baseline_01.mp4 ===" | Out-File $analysisFile
    ffprobe -v error -select_streams v:0 -show_format -show_streams -show_data $outputPath | Out-File $analysisFile -Append
    "=== FRAME TIMING ANALYSIS - baseline_01.mp4 ===" | Out-File $analysisFile -Append
    ffprobe -v error -select_streams v:0 -show_entries frame=pkt_pts_time,pkt_dts_time,best_effort_timestamp_time,pkt_duration_time -read_intervals "%+#60" $outputPath | Out-File $analysisFile -Append
    
    Write-Host "Analysis complete! Check baseline_01_analysis.txt"
} else {
    Write-Host "Recording failed with exit code: $LASTEXITCODE"
}