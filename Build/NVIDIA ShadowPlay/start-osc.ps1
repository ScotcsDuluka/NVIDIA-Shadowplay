# start-osc.ps1 — บูตระบบ OSC ทั้งชุด (ลำดับเดียว ใช้ซ้ำได้เสมอ)
# ลำดับ: service (NVIDIA Plugin) → helper (build) → node (build) → Share (build)
$ErrorActionPreference = 'Continue'
$B = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay'

Write-Host '=== [1] container service (NVIDIA Plugin — ห้ามแตะอื่น) ==='
$svc = Get-Service NvContainerLocalSystem -ErrorAction SilentlyContinue
if (-not $svc) { Write-Host 'SERVICE MISSING — ติดตั้ง NVIDIA Plugins ก่อน'; exit 1 }
if ($svc.Status -ne 'Running') { Start-Service NvContainerLocalSystem; Start-Sleep 8 }
Write-Host ("[1] service: " + (Get-Service NvContainerLocalSystem).Status)

Write-Host '=== [2] helper (build path) ==='
if (-not (Get-Process nvsphelper64 -ErrorAction SilentlyContinue)) {
  Start-Process -FilePath "$B\ShadowPlay\nvsphelper64.exe" -WorkingDirectory "$B\ShadowPlay"
  Start-Sleep 4
}
Write-Host ("[2] helper: " + $(if (Get-Process nvsphelper64 -ErrorAction SilentlyContinue) {'running'} else {'FAILED'}))

Write-Host '=== [3] node (build path) ==='
Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Process -FilePath "$B\NvNode\NVIDIA Web Helper.exe" -WorkingDirectory "$B\NvNode"
$ok = $false
foreach ($i in 1..20) {
  Start-Sleep 1
  try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare' -UseBasicParsing -TimeoutSec 2
        if ($r.StatusCode -eq 200) { $ok = $true; break } } catch {}
}
Write-Host ("[3] node :59001 → " + $(if ($ok) {'OK (Initialization complete)'} else {'FAILED'}))

Write-Host '=== [4] Share (build path) ==='
Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Process -FilePath "$B\Share\NVIDIA Share.exe" -WorkingDirectory "$B\Share"
Start-Sleep 6
Write-Host ("[4] Share: " + (Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + " process(es)")

Write-Host ''
Write-Host '=== จบบูต — กด Alt+Z เพื่อเปิด Overlay ==='
