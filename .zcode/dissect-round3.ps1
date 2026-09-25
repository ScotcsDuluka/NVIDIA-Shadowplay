# Final sweep: every remaining ShadowPlay-adjacent PE.
$ErrorActionPreference = 'Continue'
$dumpbin = Get-ChildItem 'C:\Program Files\Microsoft Visual Studio\18\Community\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe' | Select-Object -First 1 -ExpandProperty FullName
$report = 'C:\My Project\NVIDIA-Shadowplay\.zcode\shadowplay-dissection3.txt'
$out = New-Object System.Collections.Generic.List[string]

function Get-Strings([byte[]]$b, [int]$stride, [int]$min) {
    $o = New-Object System.Collections.Generic.List[string]
    $sb = New-Object System.Text.StringBuilder
    for ($i = 0; $i -lt $b.Length; $i += $stride) {
        $c = $b[$i]
        if (($c -ge 0x20 -and $c -le 0x7E) -or $c -ge 0x80) { [void]$sb.Append([char]$c) }
        else { if ($sb.Length -ge $min) { $o.Add($sb.ToString()) }; [void]$sb.Clear() }
    }
    if ($sb.Length -ge $min) { $o.Add($sb.ToString()) }
    return $o
}

$base = 'C:\My Project\gfe-3.28.0.412-extract'
$targets = @(
    "$base\NvContainer\x86_64\NvContainerInternal.exe",
    "$base\NvBackend\NvBackendAPI64.dll",
    "$base\NvBackend\NvBackendExt.dll",
    "$base\NvBackend\NvBatteryBoostCheck64.dll",
    "$base\NvBackend\NvDriverUpdateCheck64.dll",
    "$base\GFExperience\GFExperienceExt.dll",
    "$base\GFExperience\OSCExt.dll",
    "$base\GFExperience\GfeXCode64.dll",
    "$base\GFExperience\NVGalleryAPINode.node",
    "$base\GFExperience.NvStreamSrv\NvStreamSrvExt.dll",
    "$base\NvTelemetry\NvTelemetry64.dll",
    "$base\NvTelemetry\NvTelemetryAPI64.dll"
)

$patterns = @('ShadowPlay','Hotkey','Toggle','osc','OSC','59001','MessageBus','MMF','OVLY',
              'NvNode','nvnode','Share.exe','Web Helper','nvcontainer','NVSPCAPS','session',
              'Session','capture','Capture','overlay','Overlay','battery','Battery','driver',
              'Driver','telemetry','Telemetry','stream','Stream','GameStream','gallery','Gallery',
              'account','Account','SOFTWARE\NVIDIA','pipe','cookie','Cookie','port')

foreach ($p in $targets) {
    if (-not (Test-Path $p)) { $out.Add('MISSING: ' + $p); continue }
    $f = Get-Item $p
    $out.Add('')
    $out.Add('################ ' + $f.Name + '  (' + [Math]::Round($f.Length/1KB) + 'KB) ################')
    $exp = & $dumpbin /exports $p 2>&1
    $names = @($exp | Where-Object { $_ -match '^\s+\d+\s+[0-9A-Fa-f]+\s+[0-9A-Fa-f]+\s+(\S+)' } | ForEach-Object { $Matches[1] })
    if ($names.Count -gt 0) {
        $out.Add('EXPORTS(' + $names.Count + '): ' + (($names | Select-Object -First 16) -join ', '))
        if ($names.Count -gt 16) { $out.Add('  ... +' + ($names.Count - 16) + ' more') }
    } else { $out.Add('EXPORTS: none') }
    $bytes = [System.IO.File]::ReadAllBytes($p)
    $strings = Get-Strings $bytes 1 6 + (Get-Strings $bytes 2 6)
    $seen = @{}; $count = 0
    foreach ($s in $strings) {
        if ($count -ge 25) { break }
        if ($s.Length -gt 130 -or $s.Length -lt 6) { continue }
        foreach ($pt in $patterns) {
            if ($s.IndexOf($pt, [System.StringComparison]::Ordinal) -ge 0 -and -not $seen.ContainsKey($s)) {
                $seen[$s] = $true; $out.Add('  ' + $s); $count++
                break
            }
        }
    }
}
$out | Set-Content -Path $report -Encoding UTF8
Write-Host ('REPORT3 ' + $out.Count)
