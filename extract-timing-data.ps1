# PowerShell script to extract timing data from evidence files
$evidenceDir = "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows\evidence"
$outputFile = "C:\My Project\NVIDIA-Shadowplay\test-recordings\timing-analysis.csv"

# Initialize output file
"File,selectedTs,selectedLag,selectedTick,muxFeedTick,cfrTick,_timelineStartTicks" | Out-File $outputFile -Encoding utf8

# Process each evidence file
$evidenceFiles = Get-ChildItem -Path $evidenceDir -Filter "phase-12b-validation-*.md"

foreach ($file in $evidenceFiles) {
    $content = Get-Content $file.FullName
    $selectedTs = $content | Select-String -Pattern "selectedTs=" | Select-Object -First 1
    $selectedLag = $content | Select-String -Pattern "selectedLag=" | Select-Object -First 1
    $selectedTick = $content | Select-String -Pattern "selectedTick=" | Select-Object -First 1
    $muxFeedTick = $content | Select-String -Pattern "muxFeedTick=" | Select-Object -First 1
    $cfrTick = $content | Select-String -Pattern "cfrTick=" | Select-Object -First 1
    $timelineStart = $content | Select-String -Pattern "_timelineStartTicks=" | Select-Object -First 1
    
    if ($selectedTs) {
        $selectedTs = $selectedTs -replace ".*selectedTs=", ""
    }
    if ($selectedLag) {
        $selectedLag = $selectedLag -replace ".*selectedLag=", ""
    }
    if ($selectedTick) {
        $selectedTick = $selectedTick -replace ".*selectedTick=", ""
    }
    if ($muxFeedTick) {
        $muxFeedTick = $muxFeedTick -replace ".*muxFeedTick=", ""
    }
    if ($cfrTick) {
        $cfrTick = $cfrTick -replace ".*cfrTick=", ""
    }
    if ($timelineStart) {
        $timelineStart = $timelineStart -replace ".*_timelineStartTicks=", ""
    }
    
    "$($file.Name),$selectedTs,$selectedLag,$selectedTick,$muxFeedTick,$cfrTick,$timelineStart" | Out-File $outputFile -Encoding utf8 -Append
}

Write-Host "Timing data extracted to: $outputFile"