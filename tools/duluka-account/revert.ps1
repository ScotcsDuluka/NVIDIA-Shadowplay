# revert.ps1 — restore the original piplConfig.json (NVIDIA defaults)

$ErrorActionPreference = 'Stop'
$pipl = 'C:\ProgramData\NVIDIA Corporation\NvNode\piplConfig.json'
$backup = Join-Path $PSScriptRoot 'piplConfig.backup.json'

if (-not (Test-Path $backup)) { Write-Output 'no backup found'; exit 1 }
Copy-Item $backup $pipl -Force
Write-Output 'Duluka Account mod REVERTED (original piplConfig restored):'
Write-Output ("  jarvis.server → " + (Get-Content $pipl -Raw | ConvertFrom-Json).data.configData.jarvis.server)
