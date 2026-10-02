# node-restart2.ps1 — รีบูต node (container ว่างแล้ว) ให้ boot-time enable สัมผัสกับ OSC ใหม่
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\node-restart2.log' -Force
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'

Write-Host '=== [1] kill node ==='
Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue | ForEach-Object {
  Write-Host ("[KILL] NVIDIA Web Helper PID " + $_.Id)
  Stop-Process -Id $_.Id -Force
}
Start-Sleep -Seconds 3

Write-Host '=== [2] สตาร์ต node ==='
Start-Process -FilePath "$B\NvNode\NVIDIA Web Helper.exe" -WorkingDirectory "$B\NvNode"
$ok = $false
foreach ($i in 1..40) { Start-Sleep 1; try { if ((Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare' -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { $ok = $true; break } } catch {} }
Write-Host ("[2] node :59001 → " + $(if ($ok) {'OK (200)'} else {'FAILED'}))

Write-Host '=== [3] รอ boot-time enable ทำงาน + Share เกิด ==='
$shareUp = $false
foreach ($i in 1..30) {
  Start-Sleep 2
  if (Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue) { $shareUp = $true; break }
}
Write-Host ("[3] Share: " + $(if ($shareUp) {'SPAWNED แล้ว!'} else {'ยังไม่เกิด'}))

Write-Host '=== [4] GET /Hotkey/monitor ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 4; Write-Host ("monitor → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("monitor → " + $_.Exception.Message) }

Write-Host '=== [5] สตาร์ต helper ถ้ายังไม่มี ==='
if (-not (Get-Process nvsphelper64 -ErrorAction SilentlyContinue)) {
  Start-Process -FilePath "$B\ShadowPlay\nvsphelper64.exe" -WorkingDirectory "$B\ShadowPlay"
  Start-Sleep -Seconds 6
}
Write-Host ("[5] helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count + " · Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count)
Stop-Transcript
