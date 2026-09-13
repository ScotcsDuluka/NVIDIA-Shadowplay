# apply.ps1 — Duluka Account mod (in-place, reversible)
#
# Point the installed GFE/OSC account layer at the DULUKA server and
# disable NVIDIA cloud/telemetry by rewriting piplConfig.json — the file
# NVIDIA's NvNode service feeds the osc page via /PiplConfig/v.1.0/update.
#
# NOTE: NvNode refreshes this file (daysToExpire) — if NVIDIA rewrites it,
# re-run this script (idempotent). revert.ps1 restores the original.

$ErrorActionPreference = 'Stop'
$pipl = 'C:\ProgramData\NVIDIA Corporation\NvNode\piplConfig.json'
$backup = Join-Path $PSScriptRoot 'piplConfig.backup.json'
$dulukaServer = 'http://127.0.0.1:59870'   # Duluka Account stub/server

# keep a fresh backup if none exists yet
if (-not (Test-Path $backup)) {
    Copy-Item $pipl $backup
    Write-Output 'backup created'
}

$cfg = Get-Content $pipl -Raw | ConvertFrom-Json
$cfg.data.configData.jarvis.server = $dulukaServer
$cfg.data.configData.gfwsl.server = ''
$cfg.data.configData.aem.server = ''
$cfg.data.configData.vrs.server = ''
$cfg.data.configData.jsEvents.server = ''
if ($cfg.data.configData.nvTelemetry) {
    $cfg.data.configData.nvTelemetry.eventsServer = ''
    $cfg.data.configData.nvTelemetry.feedbackServer = ''
    $cfg.data.configData.nvTelemetry.feedbackAttachmentServer = ''
}
# extend expiry so NvNode does not immediately re-fetch over our mod
$cfg.expiryTime = [DateTimeOffset]::UtcNow.AddYears(1).ToUnixTimeMilliseconds()

$json = $cfg | ConvertTo-Json -Depth 10
$tmp = "$pipl.duluka.tmp"
Set-Content -Path $tmp -Value $json -Encoding UTF8
Move-Item $tmp $pipl -Force

Write-Output 'Duluka Account mod APPLIED:'
Write-Output ("  jarvis.server → " + (Get-Content $pipl -Raw | ConvertFrom-Json).data.configData.jarvis.server)
Write-Output '  gfwsl/aem/vrs/jsEvents/telemetry → empty (NVIDIA cloud off)'
