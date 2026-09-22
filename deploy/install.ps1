# ============================================================
#  install.ps1 — NVIDIA ShadowPlay Portable, GFE product tree
#  (Phase 4 of the 4-phase GFE rebuild)
#
#  Builds the GFE layout from repo build outputs and wires the
#  autostart. Run from an ADMIN PowerShell.
#
#  Product tree created:
#    C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\
#    ├── osc\          (frontend static — served by the backend)
#    ├── Backend\      (Node.js backend, port 59001, Web Helper replacement)
#    ├── Coordinator\  (Share.exe #1 — HKCU Run entry points here)
#    ├── Desktop\      (NVIDIA Share.exe #2 — Overlay.Engine build)
#    └── Hook\         (Share.exe #3 — placeholder)
#
#  Usage:  .\install.ps1 [-BackendSrc path] [-OscSrc path] [-CoordinatorSrc path]
#                        [-DesktopSrc path] [-HookSrc path] [-NoStart]
# ============================================================
param(
  [string]$BackendSrc = '',
  [string]$OscSrc = '',
  [string]$CoordinatorSrc = '',
  [string]$DesktopSrc = '',
  [string]$HookSrc = '',
  [switch]$NoStart
)

$ErrorActionPreference = 'Continue'
$repo = $PSScriptRoot
$gfe  = 'C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience'
$tfm  = 'net10.0-windows10.0.26100.0'

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) { Write-Host 'Run from an ADMIN PowerShell.'; exit 1 }

function Resolve-Src([string]$kind, [string]$explicit, [string[]]$candidates) {
  if ($explicit -and (Test-Path $explicit)) { return $explicit }
  foreach ($c in $candidates) { if (Test-Path $c) { return $c } }
  Write-Host "  WARNING: $kind source not found (looked: $($candidates -join '; '))"
  return $null
}

Write-Host '[1/9] resolving sources...'
$backendSrc = Resolve-Src 'Backend' $BackendSrc @(
  (Join-Path $repo 'Backend'),
  (Join-Path $repo 'nvidia-shadowplay\Backend'))
$oscSrc = Resolve-Src 'osc frontend' $OscSrc @(
  (Join-Path $repo 'osc'),
  (Join-Path $repo 'Overlay.Engine\osc'),
  (Join-Path $repo 'nvidia-shadowplay\Overlay.Engine\osc'))
$coordSrc = Resolve-Src 'Coordinator build' $CoordinatorSrc @(
  (Join-Path (Join-Path $repo 'Product\Coordinator\bin\Release') $tfm),
  (Join-Path (Join-Path $repo 'nvidia-shadowplay\Product\Coordinator\bin\Release') $tfm))
$desktopSrc = Resolve-Src 'Desktop (Overlay.Engine) build' $DesktopSrc @(
  (Join-Path (Join-Path $repo 'Overlay.Engine\bin\Release') $tfm),
  (Join-Path (Join-Path $repo 'nvidia-shadowplay\Overlay.Engine\bin\Release') $tfm))
$hookSrc = Resolve-Src 'Hook build' $HookSrc @(
  (Join-Path (Join-Path $repo 'Product\Hook\bin\Release') $tfm),
  (Join-Path (Join-Path $repo 'nvidia-shadowplay\Product\Hook\bin\Release') $tfm))

# ── stop whatever is running from a previous install ────────────────
Write-Host '[2/9] stopping previous processes...'
foreach ($n in 'Share', 'NVIDIA Share', 'NVIDIA Capture', 'node') {
  Get-Process $n -ErrorAction SilentlyContinue | ForEach-Object {
    try {
      $p = $_.Path
      if ($p -like "$gfe*" -or $_.ProcessName -eq 'node') { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
    } catch { }
  }
}
Get-CimInstance Win32_Process -Filter "Name='node.exe'" -ErrorAction SilentlyContinue |
  Where-Object { $_.CommandLine -like '*NVIDIA GeForce Experience\Backend*' } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
Start-Sleep 1

# ── copy the tree ───────────────────────────────────────────────────
Write-Host '[3/9] copying product tree...'
foreach ($slot in @(
  @('osc',        $oscSrc),
  @('Backend',    $backendSrc),
  @('Coordinator', $coordSrc),
  @('Desktop',    $desktopSrc),
  @('Hook',       $hookSrc))) {
  $name = $slot[0]; $src = $slot[1]
  $dst = Join-Path $gfe $name
  if ($src) {
    & robocopy $src $dst /E /NFL /NDL /NJH /NJS /NP | Out-Null
    Write-Host ('  ' + $name + ' <- ' + $src + ' (robocopy=' + $LASTEXITCODE + ')')
  } else {
    Write-Host ('  SKIPPED ' + $name + ' (no source)')
  }
}

# Backend must not ship its dev-time data/ state or node_modules junk
$junk = Join-Path $gfe 'Backend\data'
if (Test-Path $junk) { Remove-Item $junk -Recurse -Force -ErrorAction SilentlyContinue }

# ── node runtime ────────────────────────────────────────────────────
Write-Host '[4/9] locating node.exe...'
$nodeExe = Get-Command node.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue
if (-not $nodeExe) {
  foreach ($cand in @("$env:ProgramFiles\nodejs\node.exe", "$env:LOCALAPPDATA\Programs\nodejs\node.exe")) {
    if (Test-Path $cand) { $nodeExe = $cand; break }
  }
}
if ($nodeExe) {
  Write-Host ('  node: ' + $nodeExe)
} else {
  Write-Host '  WARNING: node.exe not found — the Backend needs Node.js (>=12). Install it or copy a portable node.exe into Backend\.'
}

# ── registry: NvNode port + security off (both views, what the real ──
#    Web Helper reads; osc tooling may probe it too)
Write-Host '[5/9] registry: Global\NvNode port=59001 disableSecurity=1 (both views)...'
foreach ($hive in 'HKLM:\SOFTWARE\NVIDIA Corporation\Global\NvNode', 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\NvNode') {
  New-Item -Path $hive -Force | Out-Null
  Set-ItemProperty -Path $hive -Name 'port' -Value 59001 -Type DWord
  Set-ItemProperty -Path $hive -Name 'disableSecurity' -Value 1 -Type DWord
}

# ── firewall: backend is loopback-only — nothing to open. The Duluka ─
#    Server (:5115) keeps its own rule if present.

# ── autostart: HKCU Run -> Share.exe #1 (Coordinator) ───────────────
Write-Host '[6/9] autostart: HKCU Run "NvPortableShare" -> Coordinator\Share.exe'
$coordExe = Join-Path $gfe 'Coordinator\Share.exe'
if (Test-Path $coordExe) {
  New-Item -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Force | Out-Null
  Set-ItemProperty -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Name 'NvPortableShare' -Value ('"' + $coordExe + '"') -Type String
} else {
  Write-Host '  WARNING: Coordinator\Share.exe missing — autostart NOT wired'
}

# Alt+Z wire-through: the Desktop overlay (#2) registers NO global hotkeys
# (OscHotkeys ownership stays off), so on driverless machines the listener
# keeps owning Alt+Z and POSTs the backend's debounced Toggle route.
Write-Host '[6b/9] Alt+Z wire-through listener -> Coordinator\hotkey-listener.ps1'
$listenerSrc = Join-Path $repo 'deploy\hotkey-listener.ps1'
if (-not (Test-Path $listenerSrc)) { $listenerSrc = Join-Path $repo 'hotkey-listener.ps1' }
if (Test-Path $listenerSrc) {
  $listenerDst = Join-Path $gfe 'Coordinator\hotkey-listener.ps1'
  Copy-Item $listenerSrc $listenerDst -Force
  $runVal = 'powershell.exe -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File "' + $listenerDst + '"'
  Set-ItemProperty -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Name 'NvPortableHotkey' -Value $runVal -Type String
  # kill any stale listener from a previous install (RegisterHotKey is exclusive)
  Get-CimInstance Win32_Process -Filter "Name='powershell.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -like '*hotkey-listener.ps1*' } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
  Write-Host '  HKCU Run NvPortableHotkey wired (POSTs :59001 Hotkey/Toggle)'
} else {
  Write-Host '  WARNING: hotkey-listener.ps1 not found — Alt+Z stays with whatever owns it'
}

# ── backend autostart (separate HKCU entry, node.exe + index.js) ────
Write-Host '[7/9] backend autostart: HKCU Run "NvPortableBackend"'
if ($nodeExe -and (Test-Path (Join-Path $gfe 'Backend\index.js'))) {
  $cmd = '"' + $nodeExe + '" "' + (Join-Path $gfe 'Backend\index.js') + '"'
  New-Item -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Force | Out-Null
  Set-ItemProperty -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Name 'NvPortableBackend' -Value $cmd -Type String
} else {
  Write-Host '  WARNING: backend/node missing — backend autostart NOT wired'
}

# ── uninstaller ─────────────────────────────────────────────────────
Write-Host '[8/9] one-click uninstaller...'
$unDir = 'C:\ProgramData\NVIDIA-Shadowplay-Portable'
New-Item -ItemType Directory -Force -Path $unDir | Out-Null
@'
# uninstall-host.ps1 — removes the ShadowPlay Portable product tree
$gfe = "C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience"
foreach ($n in "Share", "NVIDIA Share", "node") {
  Get-Process $n -ErrorAction SilentlyContinue | ForEach-Object {
    try { if ($_.Path -like "$gfe*") { Stop-Process -Id $_.Id -Force } } catch {}
  }
}
Remove-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name "NvPortableShare" -ErrorAction SilentlyContinue
Remove-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name "NvPortableBackend" -ErrorAction SilentlyContinue
foreach ($hive in "HKLM:\SOFTWARE\NVIDIA Corporation\Global\NvNode", "HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\NvNode") {
  Remove-Item -Path $hive -Recurse -Force -ErrorAction SilentlyContinue
}
if (Test-Path $gfe) { Remove-Item $gfe -Recurse -Force }
Write-Host "ShadowPlay Portable removed (NVIDIA driver components untouched)."
'@ | Out-File (Join-Path $unDir 'uninstall-host.ps1') -Encoding utf8
@'
@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0uninstall-host.ps1"
pause
'@ | Out-File (Join-Path $unDir 'UNINSTALL.cmd') -Encoding ascii
Write-Host ('  removal: double-click ' + (Join-Path $unDir 'UNINSTALL.cmd'))

# ── launch ──────────────────────────────────────────────────────────
if (-not $NoStart) {
  Write-Host '[9/9] launching...'
  if ($nodeExe -and (Test-Path (Join-Path $gfe 'Backend\index.js'))) {
    Start-Process -FilePath $nodeExe -ArgumentList ('"' + (Join-Path $gfe 'Backend\index.js') + '"') -WindowStyle Hidden
  }
  if (Test-Path $coordExe) {
    Start-Process -FilePath $coordExe
  }
  Start-Sleep 3
}

Write-Host ''
Write-Host 'DONE. Product tree:'
Write-Host ('  ' + $gfe)
Write-Host '  Backend   : http://127.0.0.1:59001 (health: /Backend/v.1.0/health)'
Write-Host '  Overlay   : Alt+Z (hotkey-listener) or Coordinator-spawned Desktop'
Write-Host '  Autostart : HKCU Run -> NvPortableShare + NvPortableBackend'
