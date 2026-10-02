# node-restart.ps1 — รีบูต node + helper เพื่อให้ enable ใหม่กับ container 25132 (FullPath = build แล้ว)
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\node-restart.log' -Force
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'

Write-Host '=== [1] kill node (ประกาศตามกติกา) ==='
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

Write-Host '=== [3] POST /Launch (กระตุ้น enable — container ตัวใหม่ path build) ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 8; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 5

Write-Host '=== [4] GET /Hotkey/monitor ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 4; Write-Host ("monitor → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("monitor → " + $_.Exception.Message) }

Write-Host '=== [5] สตาร์ต helper (node พร้อมแล้ว) ==='
if (-not (Get-Process nvsphelper64 -ErrorAction SilentlyContinue)) {
  Start-Process -FilePath "$B\ShadowPlay\nvsphelper64.exe" -WorkingDirectory "$B\ShadowPlay"
  Start-Sleep -Seconds 8
}
Write-Host ("[5] helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count + " ตัว")

Write-Host '=== [6] สรุป process ==='
Get-Process 'NVIDIA Web Helper','NVIDIA Share','nvsphelper64' -ErrorAction SilentlyContinue | Select-Object Id,ProcessName | Format-Table -AutoSize | Out-String
Stop-Transcript
