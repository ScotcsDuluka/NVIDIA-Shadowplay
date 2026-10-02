# arm-and-fire.ps1 — node ใหม่ (IPC สด ชี้ container PF ปัจจุบัน) → Launch → ดู spawn
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\arm-and-fire.log' -Force
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'

Write-Host '=== [1] kill node ==='
Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue | ForEach-Object { Write-Host ("[KILL] NVIDIA Web Helper PID " + $_.Id); Stop-Process -Id $_.Id -Force }
Start-Sleep -Seconds 3

Write-Host '=== [2] node ใหม่ ==='
Start-Process -FilePath "$B\NvNode\NVIDIA Web Helper.exe" -WorkingDirectory "$B\NvNode"
$ok = $false
foreach ($i in 1..40) { Start-Sleep 1; try { if ((Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare' -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { $ok = $true; break } } catch {} }
Write-Host ("[2] node :59001 → " + $(if ($ok) {'OK (200)'} else {'FAILED'}))

Write-Host '=== [3] POST /Launch ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 20; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 18

Write-Host '=== [4] ผล ==='
Write-Host ("[4] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " · helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)
if (-not (Get-Process nvsphelper64 -ErrorAction SilentlyContinue)) { Start-Process -FilePath "$B\ShadowPlay\nvsphelper64.exe" -WorkingDirectory "$B\ShadowPlay"; Start-Sleep -Seconds 6; Write-Host ("[4b] helper เริ่มเอง: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count) }
Stop-Transcript
