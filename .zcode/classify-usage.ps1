# Classify the 14 osc-page paths: HTTP route in real backend? socket event? implemented by ours?
$scratch = 'C:\My Project\NVIDIA-Shadowplay\.zcode'
$realJs = 'C:\My Project\gfe-3.28.0.412-extract\nodejs\NvShadowPlayAPI.js'
$realIndex = 'C:\My Project\gfe-3.28.0.412-extract\nodejs\index.js'
$oursRoutes = 'C:\My Project\NVIDIA-Shadowplay\Project\NvBackend\NVIDIA Web Helper.exe\Backend\routes'

$paths = Get-Content "$scratch\osc-usage.txt"
$oursAll = Get-Content "$scratch\our-endpoints.txt"

foreach ($p in $paths) {
    $short = $p -replace '/ShadowPlay/v\.1\.0/', ''
    $inRealHttp = Select-String -Path $realJs -SimpleMatch ("'" + $p + "'") -Quiet
    $inRealSocket = $false
    if (-not $inRealHttp) {
        $inRealSocket = Select-String -Path $realJs, $realIndex -SimpleMatch ('"' + $short + '"') -Quiet
        if (-not $inRealSocket) { $inRealSocket = Select-String -Path $realJs, $realIndex -SimpleMatch ("'" + $short + "'") -Quiet }
    }
    $inOursHttp = @($oursAll | Where-Object { $_ -like ("* " + $p + " *") -or $_ -like ("* " + $p) }).Count -gt 0
    $kind = if ($inRealHttp) { 'HTTP(real)' } elseif ($inRealSocket) { 'SOCKET/EVENT(real)' } else { 'NOT-IN-REAL' }
    $ok = if ($inOursHttp) { 'ours:HTTP-OK' } else { 'ours:MISSING' }
    Write-Host ($kind.PadRight(20) + ' ' + $ok.PadRight(14) + $p)
}

Write-Host ''
Write-Host '--- our full ShadowPlay-related endpoints ---'
$oursAll | Where-Object { $_ -match 'ShadowPlay|Hotkey|Broadcast|InstantReplay|Record|Notification|WindowState|Osc' } | ForEach-Object { Write-Host ('  ' + $_) }
