# fix-share-path.ps1 — ชี้ FullPath GFE → build Share แล้วให้ container เกิด Share ตัวเอง
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\fix-share-path.log' -Force

Write-Host '=== [1] FullPath → build Share ==='
reg add "HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience" /v FullPath /t REG_SZ /d "C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Share\NVIDIA Share.exe" /f
reg query "HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience" /v FullPath

Write-Host '=== [2] kill: Share ตัวมือ (2) + SPUser container (watchdog เกิดใหม่เอง) ==='
foreach ($n in 'NVIDIA Share') {
  Get-Process $n -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host ("[KILL] " + $_.ProcessName + " PID " + $_.Id + " (ให้ container สตาร์ต Share เองจาก build)")
    Stop-Process -Id $_.Id -Force
  }
}
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object {
  Write-Host ("[KILL] nvcontainer SPUser PID " + $_.ProcessId + " (watchdog respawn)")
  Stop-Process -Id $_.ProcessId -Force
}
Start-Sleep -Seconds 14

Write-Host '=== [3] สถานะหลัง respawn ==='
Get-Process 'NVIDIA Share','nvcontainer','NvContainer' -ErrorAction SilentlyContinue | Select-Object Id,ProcessName | Format-Table -AutoSize | Out-String
Write-Host '=== [4] trigger enable แบบเดียวกับหน้า (POST /Launch) ==='
try { Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 5 | Select-Object StatusCode | Out-String } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 6
Write-Host '=== [5] GET /Hotkey/monitor (ต้องไม่ E_INVALIDARG แล้ว) ==='
try { (Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 3).Content } catch { Write-Host ("monitor → " + $_.Exception.Message) }
Stop-Transcript
