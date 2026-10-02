# attach-osc.ps1 — ลำดับแบบยุค Light ที่พิสูจน์แล้ว: Share ยืนรอ → container เกิดใหม่ → แนบกับ Share จริง
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\attach-osc.log' -Force
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'

Write-Host '=== [1] kill SPUser container (ให้ init ใหม่หลัง Share ยืนรอ) ==='
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object {
  Write-Host ("[KILL] nvcontainer SPUser PID " + $_.ProcessId)
  Stop-Process -Id $_.ProcessId -Force
}

Write-Host '=== [2] สตาร์ต Share (build) ให้ยืนรอ container init ==='
Start-Process -FilePath "$B\Share\NVIDIA Share.exe" -WorkingDirectory "$B\Share"
Start-Sleep -Seconds 10
Write-Host ("[2] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " ตัว (ยืนรอ)")

Write-Host '=== [3] รอ SPUser respawn + attach (35 วิ) ==='
Start-Sleep -Seconds 35

Write-Host '=== [4] สถานะ Share หลัง attach ==='
Write-Host ("[4] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " ตัว")

Write-Host '=== [5] POST /Launch {"launch":true} ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 15; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 6

Write-Host '=== [6] GET /Hotkey/monitor ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 5; Write-Host ("monitor → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("monitor → " + $_.Exception.Message) }

Write-Host '=== [7] สรุป process ==='
Write-Host ("[7] helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count + " · Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " · node: " + @(Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue).Count)
Stop-Transcript
