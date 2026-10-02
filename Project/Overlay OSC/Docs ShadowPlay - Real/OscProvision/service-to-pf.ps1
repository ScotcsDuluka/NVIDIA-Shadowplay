# service-to-pf.ps1 — service ImagePath → PF (whitelist รู้จัก) + รีสตาร์ต + node + Launch
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\service-to-pf.log' -Force
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'
$PF = 'C:\Program Files\NVIDIA Corporation\NvContainer'

Write-Host '=== [1] FailureActions ล้าง ==='
reg add "HKLM\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem" /v FailureActions /t REG_BINARY /d 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 /f

Write-Host '=== [2] ImagePath → PF ==='
$bin = '"' + $PF + '\nvcontainer.exe" -s NvContainerLocalSystem -a -f "C:\ProgramData\NVIDIA\NvContainerLocalSystem.log" -l 3 -d "' + $PF + '\plugins\LocalSystem" -r -p 30000 -st "' + $PF + '\NvContainerTelemetryApi.dll" -ert'
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem' -Name ImagePath -Value $bin -Type ExpandString
(Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem').ImagePath

Write-Host '=== [3] kill ที่เหลือ ==='
Get-Process 'NVIDIA Web Helper','nvsphelper64','NVIDIA Share' -ErrorAction SilentlyContinue | ForEach-Object { Write-Host ("[KILL] " + $_.ProcessName + " PID " + $_.Id); Stop-Process -Id $_.Id -Force }
sc.exe stop NvContainerLocalSystem 2>&1 | Out-Null
Start-Sleep -Seconds 6
Get-Process nvcontainer,NvContainer -ErrorAction SilentlyContinue | ForEach-Object { Write-Host ("[KILL] container ตกค้าง PID " + $_.Id); Stop-Process -Id $_.Id -Force }

Write-Host '=== [4] start service จาก PF ==='
sc.exe start NvContainerLocalSystem 2>&1 | Out-Null
Start-Sleep -Seconds 14
if ((Get-Service NvContainerLocalSystem).Status -ne 'Running') { sc.exe start NvContainerLocalSystem 2>&1 | Out-Null; Start-Sleep -Seconds 14 }
sc.exe query NvContainerLocalSystem | Select-String STATE
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | ForEach-Object { Write-Host ("container " + $_.ProcessId + " → " + $_.ExecutablePath) }

Write-Host '=== [5] node ใหม่ ==='
Start-Process -FilePath "$B\NvNode\NVIDIA Web Helper.exe" -WorkingDirectory "$B\NvNode"
$ok = $false
foreach ($i in 1..40) { Start-Sleep 1; try { if ((Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare' -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { $ok = $true; break } } catch {} }
Write-Host ("[5] node :59001 → " + $(if ($ok) {'OK (200)'} else {'FAILED'}))

Write-Host '=== [6] POST /Launch ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 15; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 15

Write-Host '=== [7] ผล ==='
Write-Host ("[7] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " · helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)
Stop-Transcript
