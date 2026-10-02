# fix-oscjson-gfe.ps1 — nv-osc=false ใน GFE Share.json + รีเกิด SPUser container
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\fix-oscjson-gfe.log' -Force
$J = 'C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\NVIDIA Share.json'

Write-Host '=== [1] nv-osc=false ใน GFE Share.json ==='
$c = Get-Content $J -Raw
$c = $c -replace 'nv-osc=true', 'nv-osc=false'
Set-Content -Path $J -Value $c -Encoding UTF8 -NoNewline
Get-Content $J

Write-Host '=== [2] kill SPUser container (spawn ใหม่อ่าน config ใหม่) ==='
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" | Where-Object { $_.CommandLine -match 'SPUser' } | ForEach-Object {
  Write-Host ("[KILL] nvcontainer SPUser PID " + $_.ProcessId)
  Stop-Process -Id $_.ProcessId -Force
}
Start-Sleep -Seconds 30

Write-Host '=== [3] สถานะ: Share spawn ใหม่อยู่มั้ย ==='
Write-Host ("[3] Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " ตัว")
Stop-Transcript
