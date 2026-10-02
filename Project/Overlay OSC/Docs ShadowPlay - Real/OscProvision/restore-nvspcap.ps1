# restore-nvspcap.ps1 — คืน nvspcap shims เข้า System32/SysWOW64 (ประกาศแล้ว) + รีเกิด SPUser container
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\restore-nvspcap.log' -Force
$P = 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Payload\System32Shims'

Write-Host '=== [1] restore nvspcap64.dll → System32 ==='
Copy-Item "$P\nvspcap64.dll" 'C:\Windows\System32\nvspcap64.dll' -Force
Get-Item 'C:\Windows\System32\nvspcap64.dll' | Select-Object Length,LastWriteTime | Format-List | Out-String

Write-Host '=== [2] restore nvspcap.dll → SysWOW64 ==='
Copy-Item "$P\SysWOW64\nvspcap.dll" 'C:\Windows\SysWOW64\nvspcap.dll' -Force
Get-Item 'C:\Windows\SysWOW64\nvspcap.dll' | Select-Object Length,LastWriteTime | Format-List | Out-String

Write-Host '=== [3] kill SPUser container (IpcProxy init ใหม่) ==='
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object {
  Write-Host ("[KILL] nvcontainer SPUser PID " + $_.ProcessId)
  Stop-Process -Id $_.ProcessId -Force
}
Start-Sleep -Seconds 30
Write-Host '=== [4] สถานะ ==='
Write-Host ("[4] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " · helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)
Stop-Transcript
