# grand-boot.ps1 — บูตใหญ่ครบวงจร: ทุก fix พร้อมกัน (shims+FullPath+watchdog PF+nv-osc=false)
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\grand-boot.log' -Force
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'

Write-Host '=== [1] ล้าง FailureActions (ก่อน stop — บทเรียน §9) ==='
reg add "HKLM\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem" /v FailureActions /t REG_BINARY /d 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 /f

Write-Host '=== [2] kill SPUser + node (ประกาศ) ==='
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object { Write-Host ("[KILL] SPUser PID " + $_.ProcessId); Stop-Process -Id $_.ProcessId -Force }
Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue | ForEach-Object { Write-Host ("[KILL] NVIDIA Web Helper PID " + $_.Id); Stop-Process -Id $_.Id -Force }
Get-Process nvsphelper64,'NVIDIA Share' -ErrorAction SilentlyContinue | ForEach-Object { Write-Host ("[KILL] " + $_.ProcessName + " PID " + $_.Id); Stop-Process -Id $_.Id -Force }

Write-Host '=== [3] รีสตาร์ต service (watchdog โหลด profile PF ใหม่) ==='
sc.exe stop NvContainerLocalSystem 2>&1 | Out-Null
Start-Sleep -Seconds 6
sc.exe start NvContainerLocalSystem 2>&1 | Out-Null
Start-Sleep -Seconds 14
if ((Get-Service NvContainerLocalSystem).Status -ne 'Running') { Write-Host 'start รอบสอง (pattern เดิม)'; sc.exe start NvContainerLocalSystem 2>&1 | Out-Null; Start-Sleep -Seconds 14 }
sc.exe query NvContainerLocalSystem | Select-String STATE

Write-Host '=== [4] containers ที่เกิด (SPUser ต้องมาจาก PF!) ==='
Start-Sleep -Seconds 6
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | ForEach-Object { Write-Host ("container " + $_.ProcessId + " → " + $_.ExecutablePath) }

Write-Host '=== [5] สตาร์ต node ==='
Start-Process -FilePath "$B\NvNode\NVIDIA Web Helper.exe" -WorkingDirectory "$B\NvNode"
$ok = $false
foreach ($i in 1..40) { Start-Sleep 1; try { if ((Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare' -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { $ok = $true; break } } catch {} }
Write-Host ("[5] node :59001 → " + $(if ($ok) {'OK (200)'} else {'FAILED'}))

Write-Host '=== [6] POST /Launch (enable — ValidatePID ควรผ่านครั้งแรกในประวัติ!) ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 15; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 15

Write-Host '=== [7] ผลลัพธ์ ==='
Write-Host ("[7] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " · helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)
Stop-Transcript
