# PowerShell script to extract timing data from forensic log
$logFile = "C:\My Project\NVIDIA-Shadowplay\test-recordings\forensic-60fps-Fri 09-11-2026-16-55-19.32.log"
$outputFile = "C:\My Project\NVIDIA-Shadowplay\test-recordings\forensic-timing-data.csv"

# Initialize output file
"timestamp,selectedTs,selectedLag,selectedTick,muxFeedTick,cfrTick,_timelineStartTicks" | Out-File $outputFile -Encoding utf8

# Read log file
$logContent = Get-Content $logFile

# Extract timing data
$selectedTs = $logContent | Select-String -Pattern "selectedTs=" | Select-Object -First 10
$selectedLag = $logContent | Select-String -Pattern "selectedLag=" | Select-Object -First 10
$selectedTick = $logContent | Select-String -Pattern "selectedTick=" | Select-Object -First 10
$muxFeedTick = $logContent | Select-String -Pattern "muxFeedTick=" | Select-Object -First 10
$cfrTick = $logContent | Select-String -Pattern "cfrTick=" | Select-Object -First 10
$timelineStart = $logContent | Select-String -Pattern "_timelineStartTicks=" | Select-Object -First 10

# Process and save data
for ($i = 0; $i -lt $selectedTs.Count; $i++) {
    $ts = $selectedTs[$i] -replace ".*selectedTs=", ""
    $lag = $selectedLag[$i] -replace ".*selectedLag=", ""
    $tick = $selectedTick[$i] -replace ".*selectedTick=", ""
    $mux = $muxFeedTick[$i] -replace ".*muxFeedTick=", ""
    $cfr = $cfrTick[$i] -replace ".*cfrTick=", ""
    $timeline = $timelineStart[$i] -replace ".*_timelineStartTicks=", ""
    
    "$ts,$lag,$tick,$mux,$cfr,$timeline" | Out-File $outputFile -Encoding utf8 -Append
}

Write-Host "Timing data extracted to: $outputFile"