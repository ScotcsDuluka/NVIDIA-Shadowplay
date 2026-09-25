# Extract the AugmentationSource JS from share_script.h and syntax-check it.
$hdr = Get-Content 'C:\My Project\NVIDIA-Shadowplay\Project\NvOverlay\Cef\src\share_script.h' -Raw
$start = $hdr.IndexOf('inline const char* AugmentationSource()')
$end = $hdr.IndexOf('inline const char* ProofDriverSource()')
$body = $hdr.Substring($start, $end - $start)

$lines = $body -split "`r?`n"
$jsParts = New-Object System.Collections.Generic.List[string]
foreach ($ln in $lines) {
    $t = $ln.Trim()
    if ($t.StartsWith('"') -and ($t.EndsWith('"') -or $t.EndsWith('";'))) {
        $s = $t.TrimStart('"').TrimEnd(';').TrimEnd('"')
        $jsParts.Add($s)
    }
}
$js = ($jsParts -join "`n")
# unescape C++ escapes
$js = $js.Replace('\"', '"').Replace('\\', '\')
[System.IO.File]::WriteAllText('C:\My Project\NVIDIA-Shadowplay\.zcode\augment-extracted.js', $js)
Write-Host ('EXTRACTED ' + $js.Length + ' chars')
