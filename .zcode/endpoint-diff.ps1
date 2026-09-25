# Endpoint parity: real NvShadowPlayAPI.js vs our routes + osc page usage.
$ErrorActionPreference = 'Continue'
$scratch = 'C:\My Project\NVIDIA-Shadowplay\.zcode'

# 1) real endpoints
$real = Select-String -Path "$scratch\..\..\gfe-3.28.0.412-extract\nodejs\NvShadowPlayAPI.js" -Pattern "app\.(get|post|put|delete)\('([^']+)'"
$realList = $real | ForEach-Object {
    $m = $_.Line | Select-String -Pattern "app\.(get|post|put|delete)\('([^']+)'"
    ($m.Matches[0].Groups[1].Value.ToUpper() + ' ' + $m.Matches[0].Groups[2].Value)
} | Sort-Object -Unique
$realList | Set-Content "$scratch\real-endpoints.txt"
Write-Host ('REAL TOTAL: ' + @($realList).Count)

# 2) our endpoints (all route files)
$oursDir = 'C:\My Project\NVIDIA-Shadowplay\Project\NvBackend\NVIDIA Web Helper.exe\Backend\routes'
$oursList = @()
Get-ChildItem $oursDir -Filter '*.js' | ForEach-Object {
    $fn = $_.Name
    $hits = Select-String -Path $_.FullName -Pattern "\.(get|post|put|delete)\(\s*'([^']+)'"
    foreach ($h in $hits) {
        $m = $h.Line | Select-String -Pattern "\.(get|post|put|delete)\(\s*'([^']+)'"
        if ($m) { $oursList += ($m.Matches[0].Groups[1].Value.ToUpper() + ' ' + $m.Matches[0].Groups[2].Value + '   [' + $fn + ']') }
    }
}
$oursList = $oursList | Sort-Object -Unique
$oursList | Set-Content "$scratch\our-endpoints.txt"
Write-Host ('OURS TOTAL: ' + @($oursList).Count)

# 3) what the osc page actually calls (from minified app.js, both copies identical)
$oscJs = 'C:\My Project\NVIDIA-Shadowplay\Build\NVIDIA ShadowPlay\NvOverlay\Cef\Resources\osc\app.js'
$usage = Select-String -Path $oscJs -Pattern '"/ShadowPlay/[^"]+"|''/ShadowPlay/[^'']+''|"/ShadowPlay/v[^"?]*' -AllMatches
$used = @()
foreach ($u in $usage) {
    foreach ($mm in $u.Matches) { $used += $mm.Value.Trim('"', "'") }
}
$used = $used | Sort-Object -Unique
$used | Set-Content "$scratch\osc-usage.txt"
Write-Host ('OSC-PAGE CALLS: ' + @($used).Count)
$used | ForEach-Object { Write-Host ('  ' + $_) }
