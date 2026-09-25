# run-proof-of-life.ps1 - CEF lane proof-of-life driver.
# Runs NvOverlay\CEF\NVIDIA Share.exe --proof-of-life from the OWNER slot,
# validates the checkpoint chain, and reports PASS/FAIL with evidence
# paths. Exit code: 0 = PASS, 1 = FAIL.
#
# Usage:
#   powershell -File Overlay.Cef\run-proof-of-life.ps1
#   powershell -File Overlay.Cef\run-proof-of-life.ps1 -SelfExitMs 30000
param(
    [int]$SelfExitMs = 25000,
    # Optional: prove a different staged owner slot (e.g. the canonical
    # dist\NVIDIA ShadowPlay tree). Default = the lane slot.
    [string]$CefDir = ""
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
if ($CefDir -eq "") { $CefDir = Join-Path $repo 'dist\nvoverlay-layout\NVIDIA ShadowPlay\NvOverlay\CEF' }
$cefDir = [System.IO.Path]::GetFullPath($CefDir)
$proofDir = Join-Path $PSScriptRoot 'proof'
$exe = Join-Path $cefDir 'NVIDIA Share.exe'

if (-not (Test-Path -LiteralPath $exe)) {
    Write-Output "FAIL: owner exe missing: $exe (run deploy-cef-owner.ps1)"
    exit 1
}

# Fresh evidence per run.
$logs = Join-Path $cefDir 'Logs'
if (Test-Path -LiteralPath $logs) { Remove-Item -LiteralPath $logs -Recurse -Force }

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$proc = Start-Process -FilePath $exe -ArgumentList @('--proof-of-life', "--self-exit-ms=$SelfExitMs") -WorkingDirectory $cefDir -PassThru
Wait-Process -Id $proc.Id -Timeout ([int]($SelfExitMs / 1000) + 30) -ErrorAction SilentlyContinue
$sw.Stop()

if (-not $proc.HasExited) {
    Write-Output "FAIL: process did not exit within timeout - killing"
    Stop-Process -Id $proc.Id -Force
    exit 1
}
Write-Output ("process exit code: {0}  (wall {1} ms)" -f $proc.ExitCode, $sw.ElapsedMilliseconds)

$proof = Get-ChildItem -LiteralPath $logs -Filter 'proof-of-life-*.json' -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $proof) {
    Write-Output "FAIL: no proof-of-life-*.json produced"
    exit 1
}

$doc = Get-Content -LiteralPath $proof.FullName -Raw | ConvertFrom-Json
$required = @('PROC_START', 'HTTP_SERVER_UP', 'CEF_INIT_OK', 'BROWSER_CREATED', 'PAGE_LOADED', 'PROOF_ECHO_VERIFIED')
$seen = @{}
foreach ($c in $doc.checkpoints) { $seen[$c.name] = $c }

Write-Output ''
Write-Output '== proof-of-life checkpoint chain =='
foreach ($name in $required) {
    if ($seen.ContainsKey($name)) {
        $c = $seen[$name]
        Write-Output ("  [ok] {0,-20} {1,8:N1} ms  {2}" -f $name, $c.ms, $c.detail)
    } else {
        Write-Output ("  [MISSING] {0}" -f $name)
    }
}
if ($seen.ContainsKey('ORGANIC_QUERY')) {
    Write-Output ("  [ok] {0,-20} {1,8:N1} ms  {2}" -f 'ORGANIC_QUERY', $seen['ORGANIC_QUERY'].ms, $seen['ORGANIC_QUERY'].detail)
}
if ($seen.ContainsKey('DOM_PROBE')) {
    Write-Output ("  [ok] {0,-20} {1,8:N1} ms  {2}" -f 'DOM_PROBE', $seen['DOM_PROBE'].ms, $seen['DOM_PROBE'].detail)
}

$missing = $required | Where-Object { -not $seen.ContainsKey($_) }
$pass = ($proc.ExitCode -eq 0) -and ($missing.Count -eq 0)

# Collect evidence copies into the lane's proof\ folder.
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
Copy-Item -LiteralPath $proof.FullName -Destination (Join-Path $proofDir "proof-of-life-$stamp.json")
Get-ChildItem -LiteralPath $logs -Filter 'nvidia-share-host-*.log' -ErrorAction SilentlyContinue |
    Sort-Object Length -Descending | Select-Object -First 1 | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $proofDir "host-log-$stamp.log.txt")
    }

Write-Output ''
if ($pass) {
    Write-Output "PASS: process start -> CEF init -> page load -> cefQuery round-trip verified"
    Write-Output "evidence: $($proof.FullName)"
    exit 0
} else {
    Write-Output "FAIL: exit=$($proc.ExitCode) missing=$($missing -join ',')"
    exit 1
}
