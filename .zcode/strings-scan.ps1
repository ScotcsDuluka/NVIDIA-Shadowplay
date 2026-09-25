# Crude strings scan (ASCII + UTF-16LE) filtered for hotkey/IPC/HTTP tokens.
$path = 'C:\My Project\gfe-3.28.0.412-extract\ShadowPlay\nvsphelperplugin64.dll'
$bytes = [System.IO.File]::ReadAllBytes($path)

function Get-Strings([byte[]]$b, [int]$stride, [int]$min) {
    $out = New-Object System.Collections.Generic.List[string]
    $sb = New-Object System.Text.StringBuilder
    for ($i = 0; $i -lt $b.Length; $i += $stride) {
        $c = $b[$i]
        if (($c -ge 0x20 -and $c -le 0x7E) -or $c -ge 0x80) {
            [void]$sb.Append([char]$c)
        } else {
            if ($sb.Length -ge $min) { $out.Add($sb.ToString()) }
            [void]$sb.Clear()
        }
    }
    if ($sb.Length -ge $min) { $out.Add($sb.ToString()) }
    return $out
}

$ascii = Get-Strings $bytes 1 6
$uni   = Get-Strings $bytes 2 6
Write-Host ('ascii-strings: ' + $ascii.Count + '  utf16-strings: ' + $uni.Count)

$patterns = @('Hotkey','HotKey','hotkey','Toggle','toggle','RegisterHotKey','WM_HOTKEY',
              'ShadowPlay','127.0.0.1','59001','http','Http','POST','/v.1.0','registry',
              'SOFTWARE','NVIDIA','MessageBus','ipc','IPC','osc','Osc','OSC','Alt','VK_','keybd',
              'GetAsyncKeyState','Z','overlay','Overlay','plugin','Plugin','state','State')
$seen = @{}
foreach ($s in ($ascii + $uni)) {
    foreach ($p in $patterns) {
        if ($s.Contains($p) -and -not $seen.ContainsKey($s)) {
            if ($s.Length -le 160) { $seen[$s] = $true; Write-Host $s }
            break
        }
    }
}
