# node-fresh-arm.ps1 — node ใหม่ (ล้างธงเท็จ+failure count) → re-arm receiver → รอ Alt+Z ทดสอบ
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\node-fresh-arm.log' -Force
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'

Write-Host '=== [1] สถานะ Share ก่อน (ต้องมีชีวิตรอ) ==='
Write-Host ("[1] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " · helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)

Write-Host '=== [2] kill node ==='
Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue | ForEach-Object {
  Write-Host ("[KILL] NVIDIA Web Helper PID " + $_.Id)
  Stop-Process -Id $_.Id -Force
}
Start-Sleep -Seconds 3
Start-Process -FilePath "$B\NvNode\NVIDIA Web Helper.exe" -WorkingDirectory "$B\NvNode"
$ok = $false
foreach ($i in 1..40) { Start-Sleep 1; try { if ((Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare' -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { $ok = $true; break } } catch {} }
Write-Host ("[2] node :59001 → " + $(if ($ok) {'OK (200)'} else {'FAILED'}))

Write-Host '=== [3] POST /Launch (re-arm receiver) ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 12; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 4

Write-Host '=== [4] GET /Launch (ธงจริง) ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -UseBasicParsing -TimeoutSec 5; Write-Host ("state → " + $r.Content) } catch { Write-Host ("state → " + $_.Exception.Message) }

Write-Host '=== [5] สรุป ==='
Write-Host ("[5] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " · helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count + " · node: " + @(Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue).Count)
Stop-Transcript
