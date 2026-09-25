# Round 2: remaining ShadowPlay-related PEs across the payload.
$ErrorActionPreference = 'Continue'
$dumpbin = Get-ChildItem 'C:\Program Files\Microsoft Visual Studio\18\Community\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe' | Select-Object -First 1 -ExpandProperty FullName
$report = 'C:\My Project\NVIDIA-Shadowplay\.zcode\shadowplay-dissection2.txt'
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

$targets = @(
    'C:\My Project\gfe-3.28.0.412-extract\nodejs\nvnodejslauncher.exe',
    'C:\My Project\gfe-3.28.0.412-extract\nodejs\NVIDIA Web Helper.exe',
    'C:\My Project\gfe-3.28.0.412-extract\NvContainer\x86_64\NvContainer.exe',
    'C:\My Project\gfe-3.28.0.412-extract\NvContainer\x86_64\MessageBus.dll',
    'C:\My Project\gfe-3.28.0.412-extract\NvContainer\x86_64\NvPluginWatchdog.dll',
    'C:\My Project\gfe-3.28.0.412-extract\GFExperience\NVIDIA Share.exe',
    'C:\My Project\gfe-3.28.0.412-extract\GFExperience\NVIDIA GeForce Experience.exe',
    'C:\My Project\gfe-3.28.0.412-extract\GFExperience\NVIDIA Notification.exe',
    'C:\My Project\gfe-3.28.0.412-extract\NvBackend\NvBackend64.dll',
    'C:\My Project\gfe-3.28.0.412-extract\NvBackend\NvTmRep.exe',
    'C:\My Project\gfe-3.28.0.412-extract\NvBackend\NvSHIM.exe'
)

$patterns = @('59001','5001','Global\NvNode','nvnodejslauncher','nodejs.json','nv-node','nv-url','nv-gpu',
              'ShadowPlay','Hotkey','Toggle','osc','OSC','MessageBus','MMF','OVLY','pipe','registry',
              'SOFTWARE\NVIDIA','Share.exe','Web Helper','GeForce Overlay','port','listen','service',
              'Service','nvsphelper','nvspcap','session','Session','NvNode','nvnode.log','security',
              'cookie','Cookie','5115','jarvis','Gallery','stream','Stream')

foreach ($p in $targets) {
    if (-not (Test-Path $p)) { $out.Add('MISSING: ' + $p); continue }
    $f = Get-Item $p
    $out.Add('')
    $out.Add('################ ' + (Split-Path $p -Leaf) + '  (' + [Math]::Round($f.Length/1KB) + 'KB) ################')

    $exp = & $dumpbin /exports $p 2>&1
    $names = @($exp | Where-Object { $_ -match '^\s+\d+\s+[0-9A-Fa-f]+\s+[0-9A-Fa-f]+\s+(\S+)' } | ForEach-Object { $Matches[1] })
    if ($names.Count -gt 0) {
        $out.Add('EXPORTS(' + $names.Count + '): ' + (($names | Select-Object -First 18) -join ', '))
        if ($names.Count -gt 18) { $out.Add('  ... +' + ($names.Count - 18) + ' more') }
    } else { $out.Add('EXPORTS: none') }

    $bytes = [System.IO.File]::ReadAllBytes($p)
    $strings = Get-Strings $bytes 1 6 + (Get-Strings $bytes 2 6)
    $seen = @{}
    $count = 0
    foreach ($s in $strings) {
        if ($count -ge 30) { break }
        if ($s.Length -gt 140 -or $s.Length -lt 6) { continue }
        foreach ($pt in $patterns) {
            if ($s.IndexOf($pt, [System.StringComparison]::Ordinal) -ge 0 -and -not $seen.ContainsKey($s)) {
                $seen[$s] = $true; $out.Add('  ' + $s); $count++
                break
            }
        }
    }
}
$out | Set-Content -Path $report -Encoding UTF8
Write-Host ('REPORT2 ' + $out.Count)
