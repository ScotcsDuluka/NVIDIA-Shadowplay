# attach-final.ps1 — สูตรที่นายจำได้: Share(build) ยืนรอ → container(PF) เกิดใหม่แล้วแนบ
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\attach-final.log' -Force
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'

Write-Host '=== [1] kill SPUser container (ตัวเดียว) ==='
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object {
  Write-Host ("[KILL] SPUser PID " + $_.ProcessId)
  Stop-Process -Id $_.ProcessId -Force
}

Write-Host '=== [2] สตาร์ต Share จาก BUILD (nv-osc=true) ให้ยืนรอ ==='
Start-Process -FilePath "$B\Share\NVIDIA Share.exe" -WorkingDirectory "$B\Share"
Start-Sleep -Seconds 10
Write-Host ("[2] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " ตัว (ยืนรอ attach)")

Write-Host '=== [3] รอ container respawn + attach (40 วิ) ==='
Start-Sleep -Seconds 40

Write-Host '=== [4] POST /Launch ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 15; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 10

Write-Host '=== [5] สถานะ ==='
Write-Host ("[5] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " · helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)
if (-not (Get-Process nvsphelper64 -ErrorAction SilentlyContinue)) { Start-Process -FilePath "$B\ShadowPlay\nvsphelper64.exe" -WorkingDirectory "$B\ShadowPlay"; Start-Sleep -Seconds 5; Write-Host ("[5b] helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count) }
Stop-Transcript
