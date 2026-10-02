# fix-oscpath64.ps1 — สร้าง GFExperience\FullPath ใน 64-bit view + trigger enable ด้วย payload ที่ถูก
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\fix-oscpath64.log' -Force

Write-Host '=== [1] FullPath ใน 64-bit view (ที่ container 64-bit อ่าน) ==='
reg add "HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience" /v FullPath /t REG_SZ /d "C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Share\NVIDIA Share.exe" /f
reg query "HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience"

Write-Host '=== [2] POST /Launch {"launch":true} ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 10; Write-Host ("Launch → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 8

Write-Host '=== [3] GET /Hotkey/monitor ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 4; Write-Host ("monitor → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("monitor → " + $_.Exception.Message) }

Write-Host '=== [4] process ==='
Get-Process 'NVIDIA Share','NVIDIA Web Helper','nvsphelper64' -ErrorAction SilentlyContinue | Select-Object Id,ProcessName | Format-Table -AutoSize | Out-String
Stop-Transcript
