# portability-test.ps1 — พิสูจน์: ลบ/ย้าย NVIDIA ออกจาก PF ทั้งหมด → กดโหลด → ต้องฟื้น
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\portability-test.log' -Force
$BK = 'C:\My Project\NVIDIA-Plugins\backup'

Write-Host '=== [1] หยุดทั้งชุด (ประกาศทีละตัว) ==='
foreach ($n in 'NVIDIA Share','nvsphelper64','NVIDIA Web Helper') {
  Get-Process $n -ErrorAction SilentlyContinue | ForEach-Object { Write-Host ("[KILL] " + $_.ProcessName + " PID " + $_.Id); Stop-Process -Id $_.Id -Force }
}
sc.exe stop NvContainerLocalSystem 2>&1 | Out-Null
Start-Sleep -Seconds 5
Get-Process nvcontainer -ErrorAction SilentlyContinue | ForEach-Object { Write-Host ("[KILL] nvcontainer PID " + $_.Id); Stop-Process -Id $_.Id -Force }
Start-Sleep -Seconds 2

Write-Host '=== [2] BACKUP-MOVE: ย้ายต้นไม้ NVIDIA ออกจาก PF ==='
New-Item -ItemType Directory -Force -Path $BK | Out-Null
foreach ($pair in @(
  @('C:\Program Files\NVIDIA Corporation\NvContainer', "$BK\NvContainer"),
  @('C:\Program Files\NVIDIA Corporation\ShadowPlay', "$BK\ShadowPlay"),
  @('C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience', "$BK\GFE"),
  @('C:\Program Files (x86)\NVIDIA Corporation\NvNode', "$BK\NvNode")
)) {
  $src = $pair[0]; $dst = $pair[1]
  if (Test-Path $src) {
    if (Test-Path $dst) { Remove-Item $dst -Recurse -Force -ErrorAction SilentlyContinue }
    Move-Item $src $dst -Force
    Write-Host ("[MOVED] " + $src + "  →  " + $dst)
  } else { Write-Host ("[SKIP] ไม่มี: " + $src) }
}
Write-Host '--- หลักฐานว่า PF ว่างแล้ว ---'
foreach ($p in @('C:\Program Files\NVIDIA Corporation\NvContainer','C:\Program Files\NVIDIA Corporation\ShadowPlay','C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience','C:\Program Files (x86)\NVIDIA Corporation\NvNode')) {
  if (Test-Path $p) { Write-Host ("ยังมี: " + $p) } else { Write-Host ("ว่าง: " + $p) }
}

Write-Host '=== [3] start server (hidden) ==='
Start-Process 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvPlugins\NvPlugins.exe' -ArgumentList 'serve' -WindowStyle Hidden
Start-Sleep -Seconds 4

Write-Host '=== [4] DEPLOY จาก localhost:15246 (โหลด → วาง → registry → บูต) ==='
& 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvPlugins\NvPlugins.exe' deploy http://127.0.0.1:15246
$deployExit = $LASTEXITCODE
Write-Host ("deploy exit = " + $deployExit)

Write-Host '=== [5] ตรวจฟื้น ==='
Start-Sleep -Seconds 4
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe' or Name='NVIDIA Web Helper.exe' or Name='NVIDIA Share.exe' or Name='nvsphelper64.exe'" | ForEach-Object { Write-Host ("  live: " + $_.Name + " PID " + $_.ProcessId) }
Stop-Transcript
