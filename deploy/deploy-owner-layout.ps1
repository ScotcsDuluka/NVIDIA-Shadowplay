# deploy-owner-layout.ps1 - stage the OWNER target layout (OBT3-style root-exe tree)
#
# Owner target (2026-09-23):
#   NVIDIA ShadowPlay\
#     Launcher.exe  NvContainer.exe  NVIDIA Web Helper.exe
#     nvsphelper64.exe  NVIDIA ShadowPlay.exe  NVIDIA Share.exe  NVIDIA Notifier.exe
#     Application\ Overlay\ WebView\ Hook\ Services\ Engine\
#     Core\ Audio\ Graphics\ FFmpeg\ Config\ Data\ Languages\ Resources\
#
# Sources (the PROVEN tree):
#   TFM   = Overlay\bin\Release\net10.0-windows10.0.26100.0   (product tree)
#   NVC   = NvContainer\bin\Release\net10.0-windows10.0.26100.0
#
# Implemented now (unambiguous):
#   - root: Launcher.{exe,dll,rtcfg}                <- TFM root
#   - root: NvContainer.exe + NvContainer.deps/rtcfg<- NVC
#   - root: nvsphelper64.exe (apphost)              <- TFM\Application (OBT3 root-exe rule)
#   - Services\: nvsphelper64.{dll,rtcfg}           <- TFM\Services (body split, unchanged)
#   - passthrough: Application\ Overlay\ WebView\ Hook\ Services\ Engine\ Core\
#     Audio\ Graphics\ FFmpeg\ Libraries\ Config\ Data\ Languages\ Resources\
#     Redist\ .NET Deployment\  <- TFM (whole subtree, robocopy)
#
# NOT staged (owner decision pending - printed as TODO):
#   - NVIDIA Web Helper.exe   (WebHelper\ is the JS helper; an exe wrapper project
#                              does not exist yet)
#   - NVIDIA ShadowPlay.exe vs NVIDIA Share.exe at root (4 family instances are
#                              per-role projects today: Coordinator/WinForm/WebView/
#                              Hook; the owner tree names two root exes - mapping
#                              needs a decision: root exe per role? args? subdirs?)
#   - NVIDIA Notifier.exe root slot (notifier apphost lives in Application\ today)
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
if (-not (Test-Path -LiteralPath $tfm)) { throw "product tree missing: $tfm (build the sln first)" }
if (-not (Test-Path -LiteralPath $nvc)) { throw "NvContainer bin missing: $nvc" }

if (-not (Test-Path -LiteralPath (Join-Path $tfm 'Overlay\NVIDIA Share.dll'))) {
    throw "TFM tree looks stale (no Overlay\NVIDIA Share.dll) - rebuild"
}

if ($Dest -eq "") { $Dest = Join-Path $repo 'dist\owner-layout\NVIDIA ShadowPlay' }
$Dest = [System.IO.Path]::GetFullPath($Dest)

Write-Output "SOURCE TFM: $tfm"
Write-Output "SOURCE NVC: $nvc"
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
Write-Output '== OWNER DECISIONS PENDING (not staged) =='
Write-Output '  1. NVIDIA Web Helper.exe  - WebHelper\ is node JS today; needs an exe wrapper project (owner: future NvContainer sibling, overlay/WebView side only)'
Write-Output '  2. NVIDIA ShadowPlay.exe + NVIDIA Share.exe root slots - the live family is four per-role projects (Coordinator\WinForm\WebView\Hook = separate NVIDIA Share.dll builds); root-exe-per-role needs a mapping decision (args? role dirs? single dll?)'
Write-Output '  3. NVIDIA Notifier.exe root slot - notifier apphost is in Application\ today (Services\NVIDIA Notifier.dll body)'
Write-Output '  4. NvContainer worker config must point at the NEW root nvsphelper64.exe when this layout goes live (Config\NvContainer.json)'

if ($Apply) {
    Write-Output ''
    Write-Output ("STAGED: " + $Dest)
    $manifest = Join-Path $Dest 'build-info.txt'
    ("owner-layout staged " + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss') +
     " from gfe-rebuild " + (git -C $repo rev-parse --short HEAD)) | Set-Content -LiteralPath $manifest
    Write-Output ("build-info.txt written (source commit stamped)")
} else {
    Write-Output ''
    Write-Output 'DRY RUN complete - re-run with -Apply to stage.'
}
