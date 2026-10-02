# phaseB-restore.ps1 — คืนสถานะ Phase B (ทุก container จาก build path) เป๊ะตาม flip-svc-build + flip-build-final
# บทเรียน §9 ที่ใส่แล้ว: ล้าง FailureActions ก่อน stop · รอบแรก start ตายให้ start รอบสอง · Share ตายตาม container ให้บูตใหม่
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\phaseB-restore.log' -Force
$B  = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvContainer'
$wd = 'HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog'

Write-Host '=== [A] FailureActions ล้าง (ก่อน stop — กัน self-heal) ==='
reg add "HKLM\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem" /v FailureActions /t REG_BINARY /d 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 /f

Write-Host '=== [B] ImagePath → build (ค่าเดียวกับ flip-svc-build.log) ==='
$bin = '"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvContainer\nvcontainer.exe" -s NvContainerLocalSystem -a -f "C:\ProgramData\NVIDIA\NvContainerLocalSystem.log" -l 3 -d "C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvContainer\plugins\LocalSystem" -r -p 30000 -st "C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvContainer\NvContainerTelemetryApi.dll" -ert'
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem' -Name ImagePath -Value $bin -Type ExpandString
Write-Host 'ImagePath ตอนนี้:'
(Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem').ImagePath

Write-Host '=== [C] watchdog SPUserX64/UserX64 → build (ค่าเดียวกับ flip-build-final.log) ==='
reg add "$wd\SPUserX64" /v Folder /t REG_SZ /d "$B\plugins\SPUser" /f
reg add "$wd\SPUserX64" /v Container /t REG_SZ /d "$B\nvcontainer.exe" /f
reg add "$wd\SPUserX64" /v Parameters /t REG_SZ /d "-f \"C:\ProgramData\NVIDIA\NvContainerUser%dSPUser.log\" -d \"$B\plugins\SPUser\" -r -l 3 -p 30000" /f
reg add "$wd\UserX64" /v Folder /t REG_SZ /d "$B\plugins\User" /f
reg add "$wd\UserX64" /v Container /t REG_SZ /d "$B\nvcontainer.exe" /f
reg add "$wd\UserX64" /v Parameters /t REG_SZ /d "-f \"C:\ProgramData\NVIDIA\NvContainerUser%d.log\" -d \"$B\plugins\User\" -r -l 3 -p 30000" /f
Write-Host 'watchdog flipped to build'

Write-Host '=== [D] stop service + kill (ประกาศรายตัว) ==='
sc.exe stop NvContainerLocalSystem 2>&1 | Out-Null
Start-Sleep -Seconds 4
foreach ($n in 'nvsphelper64','nvcontainer','NvContainer','NVIDIA Web Helper','NVIDIA Share') {
  Get-Process $n -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host ("[KILL] " + $_.ProcessName + " PID " + $_.Id + " (flip กลับ Phase B)")
    Stop-Process -Id $_.Id -Force
  }
}

Write-Host '=== [E] start service (ตายรอบแรก = start รอบสอง ตาม pattern ที่รู้) ==='
sc.exe start NvContainerLocalSystem 2>&1 | Out-Null
Start-Sleep -Seconds 14
if ((Get-Service NvContainerLocalSystem).Status -ne 'Running') {
  Write-Host 'รอบแรกไม่ขึ้น — start รอบสอง'
  sc.exe start NvContainerLocalSystem 2>&1 | Out-Null
  Start-Sleep -Seconds 14
}
sc.exe query NvContainerLocalSystem | Select-String STATE
Write-Host '=== containers ที่เพิ่งบูต (ต้องมาจาก build ทั้งหมด) ==='
Get-Process nvcontainer,NvContainer -ErrorAction SilentlyContinue | Select-Object Id,Path | Format-Table -AutoSize | Out-String -Width 145
Stop-Transcript

Write-Host '=== [F] บูต user layer ต่อด้วย start-osc.ps1 (elevated อยู่แล้ว — ไม่มี UAC เพิ่ม) ==='
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\start-osc.ps1'
