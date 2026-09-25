# Dissect every PE in the official ShadowPlay payload: exports, dependents, filtered strings.
$ErrorActionPreference = 'Continue'
$dir = 'C:\My Project\gfe-3.28.0.412-extract\ShadowPlay'
$dumpbin = Get-ChildItem 'C:\Program Files\Microsoft Visual Studio\18\Community\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe' | Select-Object -First 1 -ExpandProperty FullName
$report = 'C:\My Project\NVIDIA-Shadowplay\.zcode\shadowplay-dissection.txt'
$out = New-Object System.Collections.Generic.List[string]

function Get-Strings([byte[]]$b, [int]$stride, [int]$min) {
    $out = New-Object System.Collections.Generic.List[string]
    $sb = New-Object System.Text.StringBuilder
    for ($i = 0; $i -lt $b.Length; $i += $stride) {
        $c = $b[$i]
        if (($c -ge 0x20 -and $c -le 0x7E) -or $c -ge 0x80) { [void]$sb.Append([char]$c) }
        else { if ($sb.Length -ge $min) { $out.Add($sb.ToString()) }; [void]$sb.Clear() }
    }
    if ($sb.Length -ge $min) { $out.Add($sb.ToString()) }
    return $out
}

$targets = @(
    'nvsphelper64.exe','capcore64.dll','nvspcap64.dll','nvspcap.dll',
    'nvspapi64.dll','nvspapix64.dll','ipccommon64.dll',
    '_nvspcaps64.dll','_nvspserviceplugin64.dll',
    'nvspscreenshot64.dll','NvRemux64.dll','NvRtmpStreamer64.dll',
    'nvfp64.dll','nvmf64.dll','ShadowPlayExt.dll'
)

foreach ($t in $targets) {
    $p = Join-Path $dir $t
    if (-not (Test-Path $p)) { continue }
    $f = Get-Item $p
    $out.Add('')
    $out.Add('################ ' + $t + '  (' + [Math]::Round($f.Length/1KB) + 'KB) ################')

    # exports
    $exp = & $dumpbin /exports $p 2>&1
    $names = @($exp | Where-Object { $_ -match '^\s+\d+\s+[0-9A-Fa-f]+\s+[0-9A-Fa-f]+\s+(\S+)' } | ForEach-Object { $Matches[1] })
    if ($names.Count -gt 0) {
        $out.Add('EXPORTS(' + $names.Count + '): ' + (($names | Select-Object -First 24) -join ', '))
        if ($names.Count -gt 24) { $out.Add('  ... +' + ($names.Count - 24) + ' more') }
    } else { $out.Add('EXPORTS: none') }

    # dependents
    $dep = & $dumpbin /dependents $p 2>&1
    $deps = @($dep | Where-Object { $_ -match '\.dll' } | ForEach-Object { $_.Trim() } | Where-Object { $_ -match '^[\w\.-]+\.dll$' } | Select-Object -Unique -First 14)
    $out.Add('DEPENDS: ' + ($deps -join ', '))

    # filtered strings
    $bytes = [System.IO.File]::ReadAllBytes($p)
    $strings = Get-Strings $bytes 1 6 + (Get-Strings $bytes 2 6)
    $patterns = @('127.0.0.1','59001','5001','5050','http://','https://','/ShadowPlay','/v.1.0','/v1.0',
                  'SOFTWARE\NVIDIA','MessageBus','MMF','OVLY','pipe','Pipe','GetCustomize','Toggle',
                  'nvcontainer','NvContainer','Share.exe','Web Helper','GeForce Overlay','osc','OSC',
                  'NVSPCAPS','ShadowPlayCfg','IsShadowPlayEnabled','Bitrate','fps','FPS','encoder',
                  'Encoder','NVENC','nvenc','capture','Capture','DDA','Desktop Duplication','hook',
                  'Hook','Inject','nvspcap','session','Session','remux','Remux','stream','Stream',
                  'rtmp','RTMP','Twitch','screenshot','Screenshot','InstantReplay','Instant Replay',
                  'push','Push','tmp','mp4','mp3','aac','h264','hevc','av1')
    $seen = @{}
    $count = 0
    foreach ($s in $strings) {
        if ($count -ge 55) { break }
        if ($s.Length -gt 150 -or $s.Length -lt 6) { continue }
        foreach ($pt in $patterns) {
            if ($s.IndexOf($pt, [System.StringComparison]::Ordinal) -ge 0 -and -not $seen.ContainsKey($s)) {
                $seen[$s] = $true; $out.Add('  ' + $s); $count++
                break
            }
        }
    }
}
$out | Set-Content -Path $report -Encoding UTF8
Write-Host ('REPORT-WRITTEN ' + $report + ' lines=' + $out.Count)
