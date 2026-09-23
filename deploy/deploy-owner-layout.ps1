# deploy-owner-layout.ps1 - stage the OWNER target layout (OBT3-style root-exe tree)
#
# Owner target (2026-09-23):
#   NVIDIA ShadowPlay\
#     Launcher.exe  NvContainer.exe  nvsphelper64.exe  NVIDIA Notifier.exe
#     NVIDIA ShadowPlay.exe (WinForm, pending rename round)
#     NVIDIA Share.exe (root shim = NvShim dispatcher, LANDED fe7a1fd5f7)
#     Application\ Overlay\ WebView\ Hook\ Services\ Engine\
#     Core\ Audio\ Graphics\ FFmpeg\ Config\ Data\ Languages\ Resources\
#
# Owner decisions LOCKED (2026-09-23, delegated by owner to lead):
#   1. NVIDIA Web Helper.exe root slot -> DEFERRED (backend :59001 lives in the real
#      NvNode helper; wrapper project comes later, overlay/WebView side only)
#   2. Root exe mapping -> Option A: NVIDIA Share.exe at root = NvShim dispatcher
#      (VB.NET, --role=coordinator|winform|webview|hook -> spawn per-role exe in its
#      subdir then exit 0; launcher ONLY, no supervision; self-seeds Config\NvShim.json
#      + Logs\NvShim.log on first run). LANDED + lead-verified (fe7a1fd5f7). Family
#      role-subdir staging into this layout = NEXT round (default roles expect
#      Coordinator\ WinForm\ WebView\ Hook\ subdirs; missing role exe = clean exit 3).
#      NVIDIA ShadowPlay.exe at root = WinForm apphost move (needs Share->ShadowPlay
#      assembly rename round, surgical patch discipline).
#   3. NVIDIA Notifier.exe root slot -> DONE (apphost staged from Application\)
#   4. NvContainer worker config -> DONE (dest Config\NvContainer.json retargeted to
#      the root nvsphelper64.exe at stage time)
#
# Sources (the PROVEN tree):
#   TFM   = Overlay\bin\Release\net10.0-windows10.0.26100.0   (product tree)
#   NVC   = NvContainer\bin\Release\net10.0-windows10.0.26100.0
#   SHIM  = NvShim\bin\Release\net10.0-windows10.0.26100.0    (root dispatcher)
#
# Implemented now (unambiguous):
#   - root: Launcher.{exe,dll,rtcfg}                <- TFM root
#   - root: NvContainer.exe + deps/rtcfg            <- NVC
#   - root: nvsphelper64.exe (apphost)              <- TFM\Application (OBT3 root-exe rule)
#   - root: NVIDIA Notifier.exe (apphost)           <- TFM\Application (owner decision 3)
#   - root: NVIDIA Share.exe + dll/rtcfg/deps       <- SHIM (owner decision 2, Option A)
#   - Services\: nvsphelper64.{dll,rtcfg}           <- TFM\Services (body split, unchanged)
#   - Config\: NvContainer.json retargeted to root worker (owner decision 4)
#   - passthrough: Application\ Overlay\ WebView\ Hook\ Services\ Engine\
#     Audio\ Graphics\ FFmpeg\ Libraries\ Config\ Data\ Languages\ Resources\
#     Redist\ .NET Deployment\  <- TFM (whole subtree, robocopy)
#
# Usage:
#   powershell -File deploy\deploy-owner-layout.ps1                    # dry-run
#   powershell -File deploy\deploy-owner-layout.ps1 -Apply             # stage to dist\
#   powershell -File deploy\deploy-owner-layout.ps1 -Apply -Dest <path>
param(
    [switch]$Apply,
    [string]$Dest = ""
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$tfm  = Join-Path $repo 'Overlay\bin\Release\net10.0-windows10.0.26100.0'
$nvc  = Join-Path $repo 'NvContainer\bin\Release\net10.0-windows10.0.26100.0'
$shim = Join-Path $repo 'NvShim\bin\Release\net10.0-windows10.0.26100.0'
if (-not (Test-Path -LiteralPath $tfm)) { throw "product tree missing: $tfm (build the sln first)" }
if (-not (Test-Path -LiteralPath $nvc)) { throw "NvContainer bin missing: $nvc" }
if (-not (Test-Path -LiteralPath (Join-Path $shim 'NVIDIA Share.exe'))) { throw "NvShim bin missing: $shim (dotnet build NvShim\NvShim.vbproj -c Release)" }

if (-not (Test-Path -LiteralPath (Join-Path $tfm 'Overlay\NVIDIA Share.dll'))) {
    throw "TFM tree looks stale (no Overlay\NVIDIA Share.dll) - rebuild"
}

if ($Dest -eq "") { $Dest = Join-Path $repo 'dist\owner-layout\NVIDIA ShadowPlay' }
$Dest = [System.IO.Path]::GetFullPath($Dest)

Write-Output "SOURCE TFM: $tfm"
Write-Output "SOURCE NVC: $nvc"
Write-Output "SOURCE SHIM: $shim"
Write-Output "DEST:       $Dest $(if (-not $Apply) { '(DRY RUN - no writes; add -Apply)' })"

function Stage([string]$src, [string]$relDst) {
    $target = Join-Path $Dest $relDst
    if (-not $Apply) {
        Write-Output ("  [dry] {0} -> {1}" -f $src, $relDst)
        return
    }
    $dir = Split-Path -Parent $target
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    Copy-Item -LiteralPath $src -Destination $target -Force
    Write-Output ("  [ok]  {0}" -f $relDst)
}

Write-Output ''
Write-Output '== ROOT EXES =='
Stage (Join-Path $tfm 'Launcher.exe')                    'Launcher.exe'
Stage (Join-Path $tfm 'Launcher.dll')                    'Launcher.dll'
Stage (Join-Path $tfm 'Launcher.runtimeconfig.json')     'Launcher.runtimeconfig.json'
Stage (Join-Path $nvc 'NvContainer.exe')                 'NvContainer.exe'
Stage (Join-Path $nvc 'NvContainer.dll')                 'NvContainer.dll'
Stage (Join-Path $nvc 'NvContainer.runtimeconfig.json')  'NvContainer.runtimeconfig.json'
Stage (Join-Path $nvc 'NvContainer.deps.json')           'NvContainer.deps.json'
# OBT3 root-exe rule: the capture engine apphost lives at the root, its body stays in Services\
Stage (Join-Path $tfm 'Application\nvsphelper64.exe')    'nvsphelper64.exe'
# Owner decision 3: notifier apphost at the root (body stays in Services\)
$notifierHost = Join-Path $tfm 'Application\NVIDIA Notifier.exe'
if (Test-Path -LiteralPath $notifierHost) {
    Stage $notifierHost 'NVIDIA Notifier.exe'
} else {
    Write-Output '  [skip] NVIDIA Notifier.exe (apphost not found in Application\ - check build)'
}

# Owner decision 2 (Option A): root NVIDIA Share.exe = NvShim dispatcher.
# OBT3 root pattern (Launcher.{exe,dll,rtcfg} shape): apphost + body side by side at root.
Write-Output ''
Write-Output '== ROOT SHIM (owner decision 2, Option A) =='
Stage (Join-Path $shim 'NVIDIA Share.exe')                 'NVIDIA Share.exe'
Stage (Join-Path $shim 'NVIDIA Share.dll')                 'NVIDIA Share.dll'
Stage (Join-Path $shim 'NVIDIA Share.runtimeconfig.json')  'NVIDIA Share.runtimeconfig.json'
Stage (Join-Path $shim 'NVIDIA Share.deps.json')           'NVIDIA Share.deps.json'

Write-Output ''
Write-Output '== PASSTHROUGH SUBTREES (robocopy, preserves the proven tree) =='
$subtrees = @('Application','Overlay','WebView','Hook','Services','Engine',
              'Core','Audio','Graphics','FFmpeg','Libraries',
              'Config','Data','Languages','Resources','Redist','.NET Deployment')
foreach ($s in $subtrees) {
    $srcDir = Join-Path $tfm $s
    if (-not (Test-Path -LiteralPath $srcDir)) {
        Write-Output ("  [skip] {0} (not present in TFM)" -f $s)
        continue
    }
    $target = Join-Path $Dest $s
    if ($Apply) {
        $prev = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
        robocopy $srcDir $target /E /R:1 /W:1 /NJH /NP /NDL /NFL /NS /NC | Out-Null
        $ErrorActionPreference = $prev
        Write-Output ("  [ok]  {0}\" -f $s)
    } else {
        Write-Output ("  [dry] {0}\ (whole subtree)" -f $s)
    }
}

Write-Output ''
Write-Output '== NVCONTAINER WORKER CONFIG (owner decision 4) =='
$destCfgDir = Join-Path $Dest 'Config'
$destCfg    = Join-Path $destCfgDir 'NvContainer.json'
$workerCfg = [ordered]@{
    port           = 5050
    securityCookie = 'eb6eb0702ec25f9aeb0b0f8f79d06d5b'
    workers        = @(
        [ordered]@{
            name                  = 'nvsphelper64'
            exe                   = (Join-Path $Dest 'nvsphelper64.exe')
            args                  = ''
            workingDirectory      = $Dest
            enabled               = $true
            adoptExisting         = $true
            maxRestarts           = 10
            restartBackoffSeconds = 5
        }
    )
}
if ($Apply) {
    if (-not (Test-Path -LiteralPath $destCfgDir)) { New-Item -ItemType Directory -Force -Path $destCfgDir | Out-Null }
    [System.IO.File]::WriteAllText($destCfg, ($workerCfg | ConvertTo-Json -Depth 5))
    Write-Output '  [ok]  Config\NvContainer.json -> worker = root nvsphelper64.exe'
} else {
    Write-Output '  [dry] Config\NvContainer.json (retargeted to root nvsphelper64.exe)'
}

Write-Output ''
Write-Output '== OWNER DECISIONS (locked by owner 2026-09-23) =='
Write-Output '  1. NVIDIA Web Helper.exe root slot = DEFERRED (backend :59001 lives in the real NvNode helper; wrapper project later, overlay/WebView side only)'
Write-Output '  2. Root exe mapping = Option A: NVIDIA Share.exe at root = NvShim dispatcher LANDED (fe7a1fd5f7; self-seeds Config\NvShim.json; family role-subdir staging next round); NVIDIA ShadowPlay.exe at root = WinForm apphost move (Share->ShadowPlay rename round pending)'
Write-Output '  3. NVIDIA Notifier.exe root slot = DONE (staged from Application\ apphost)'
Write-Output '  4. NvContainer worker config = DONE (dest Config\NvContainer.json -> root nvsphelper64.exe)'

if ($Apply) {
    Write-Output ''
    Write-Output ("STAGED: " + $Dest)
    $manifest = Join-Path $Dest 'build-info.txt'
    ("owner-layout staged " + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss') +
     " from gfe-rebuild " + (git -C $repo rev-parse --short HEAD)) | Set-Content -LiteralPath $manifest
    Write-Output 'build-info.txt written (source commit stamped)'
} else {
    Write-Output ''
    Write-Output 'DRY RUN complete - re-run with -Apply to stage.'
}
