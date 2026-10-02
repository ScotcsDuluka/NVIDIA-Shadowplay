# fix-oscpath-restart.ps1 — คีย์ครบ + รีเกิด SPUser container ให้ GetOSCPath อ่านใหม่
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\fix-oscpath-restart.log' -Force

Write-Host '=== [1] คีย์ 64-bit ให้ครบแบบเดียวกับ genuine (Installed/FullPath/Version/Architecture) ==='
reg add "HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience" /v Installed /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience" /v FullPath /t REG_SZ /d "C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Share\NVIDIA Share.exe" /f
reg add "HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience" /v Version /t REG_SZ /d "3.28.0.412" /f
reg add "HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience" /v Architecture /t REG_SZ /d "x64" /f
reg query "HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience"

Write-Host '=== [2] รีเกิด SPUser container (watchdog respawn) ==='
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object {
  Write-Host ("[KILL] nvcontainer SPUser PID " + $_.ProcessId + " (ให้ GetOSCPath อ่าน FullPath ใหม่)")
  Stop-Process -Id $_.ProcessId -Force
}
Start-Sleep -Seconds 16

Write-Host '=== [3] POST /Launch {"launch":true} ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 10; Write-Host ("Launch → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 8

Write-Host '=== [4] GET /Hotkey/monitor ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 4; Write-Host ("monitor → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("monitor → " + $_.Exception.Message) }

Write-Host '=== [5] process ==='
Get-Process 'NVIDIA Share','NVIDIA Web Helper','nvsphelper64','nvcontainer','NvContainer' -ErrorAction SilentlyContinue | Select-Object Id,ProcessName | Format-Table -AutoSize | Out-String
Stop-Transcript
