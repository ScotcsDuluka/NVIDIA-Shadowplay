# final-enable.ps1 — รีเกิด SPUser container (COscProcMgr ใหม่) + รีบูต node + enable + ดู Share spawn
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\final-enable.log' -Force
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'

Write-Host '=== [1] รีเกิด SPUser container ==='
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object {
  Write-Host ("[KILL] nvcontainer SPUser PID " + $_.ProcessId + " (COscProcMgr thread ใหม่)")
  Stop-Process -Id $_.ProcessId -Force
}
Start-Sleep -Seconds 16

Write-Host '=== [2] รีบูต node ==='
Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue | ForEach-Object {
  Write-Host ("[KILL] NVIDIA Web Helper PID " + $_.Id)
  Stop-Process -Id $_.Id -Force
}
Start-Sleep -Seconds 3
Start-Process -FilePath "$B\NvNode\NVIDIA Web Helper.exe" -WorkingDirectory "$B\NvNode"
$ok = $false
foreach ($i in 1..40) { Start-Sleep 1; try { if ((Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare' -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { $ok = $true; break } } catch {} }
Write-Host ("[2] node :59001 → " + $(if ($ok) {'OK (200)'} else {'FAILED'}))

Write-Host '=== [3] POST /Launch (fire; ไม่รอเกิน 15 วิ) ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 15; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }

Write-Host '=== [4] รอ Share spawn (90 วิ) ==='
$shareUp = $false
foreach ($i in 1..45) { Start-Sleep 2; if (Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue) { $shareUp = $true; break } }
Write-Host ("[4] Share: " + $(if ($shareUp) {'SPAWNED!'} else {'ไม่เกิด'}))
Start-Sleep -Seconds 10
Write-Host ("[4] Share ยังอยู่: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " ตัว")

Write-Host '=== [5] GET /Hotkey/monitor ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 5; Write-Host ("monitor → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("monitor → " + $_.Exception.Message) }

Write-Host '=== [6] helper ==='
if (-not (Get-Process nvsphelper64 -ErrorAction SilentlyContinue)) {
  Start-Process -FilePath "$B\ShadowPlay\nvsphelper64.exe" -WorkingDirectory "$B\ShadowPlay"
  Start-Sleep -Seconds 6
}
Write-Host ("[6] helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count + " · Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count)
Stop-Transcript
