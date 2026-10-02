# spuser-to-pf.ps1 — watchdog profile → PF (whitelist pass) + ยิง enable
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\spuser-to-pf.log' -Force
$PF = 'C:\Program Files\NVIDIA Corporation\NvContainer'
$wd = 'HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog'

Write-Host '=== [1] watchdog SPUserX64 → PF (NVIDIA Plugin ตามแผน) ==='
reg add "$wd\SPUserX64" /v Folder /t REG_SZ /d "$PF\plugins\SPUser" /f
reg add "$wd\SPUserX64" /v Container /t REG_SZ /d "$PF\nvcontainer.exe" /f
reg add "$wd\SPUserX64" /v Parameters /t REG_SZ /d "-f \"C:\ProgramData\NVIDIA\NvContainerUser%dSPUser.log\" -d \"$PF\plugins\SPUser\" -r -l 3 -p 30000" /f
reg query "$wd\SPUserX64" /v Container

Write-Host '=== [2] kill SPUser ทั้งหมด (watchdog เกิดใหม่จาก PF) ==='
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object {
  Write-Host ("[KILL] SPUser PID " + $_.ProcessId)
  Stop-Process -Id $_.ProcessId -Force
}
Start-Sleep -Seconds 25

Write-Host '=== [3] POST /Launch (ValidatePID ควรผ่าน) ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 15; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 12

Write-Host '=== [4] สถานะ ==='
Write-Host ("[4] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " · helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object { Write-Host ("SPUser: " + $_.ProcessId + " → " + $_.ExecutablePath) }
Stop-Transcript
