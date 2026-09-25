# deploy-cef-owner.ps1 - stage the CEF lane owner slot:
#   dist\nvoverlay-layout\NVIDIA ShadowPlay\NvOverlay\CEF\
# Content per the owner drawing (Logical Architecture\Layout.txt v2 lines
# 21-28): NVIDIA Share.exe + NVIDIA Share.dll + CEF runtime *.dll +
# Resources\ (with the osc web UI bundle) + locales\ + cef.pak + NVIDIA
# Share.json.
#
# Usage:
#   powershell -File Overlay.Cef\deploy-cef-owner.ps1            # stage
#   powershell -File Overlay.Cef\deploy-cef-owner.ps1 -Dest <path>
param(
    [string]$Dest = "",
    # -Build: build the lane vcxprojs first (Release x64, VS MSBuild via
    # vswhere) so the one-shot pipeline can stage from fresh outputs.
    [switch]$Build
)
$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$bin  = Join-Path $PSScriptRoot 'bin\x64\Release'
$cefSdk = 'C:\My Project\cef-sdk\cef73'
$oscSrc = Join-Path $repo '..\..\Docs\reference\osc\osc'
if (-not (Test-Path (Join-Path $oscSrc 'app.js'))) {
    $oscSrc = Join-Path $repo '..\..\IDK This is\dist\NVIDIA ShadowPlay\NvOverlay\CEF\Resources\osc'
}
if ($Dest -eq "") { $Dest = Join-Path $repo 'dist\nvoverlay-layout\NVIDIA ShadowPlay\NvOverlay\CEF' }
$Dest = [System.IO.Path]::GetFullPath($Dest)

if ($Build) {
    $pf86 = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
    if ([string]::IsNullOrWhiteSpace($pf86)) { $pf86 = $env:ProgramFiles }
    $vswhere = Join-Path $pf86 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (-not $msbuild) { $msbuild = 'C:\Visual Studio\MSBuild\Current\Bin\MSBuild.exe' }
    if (-not (Test-Path -LiteralPath $msbuild)) { throw "CEF host build: MSBuild.exe not found (vswhere + fallback failed)" }
    Write-Output "CEF host build via $msbuild"
    foreach ($proj in @('NVIDIA Share.vcxproj', 'NVIDIA Share Bootstrap.vcxproj')) {
        & $msbuild (Join-Path $PSScriptRoot $proj) -p:Configuration=Release -p:Platform=x64 -m -v:q -nologo
        if ($LASTEXITCODE -ne 0) { throw "CEF host build FAILED: $proj (exit $LASTEXITCODE)" }
    }
}

function CopyFile([string]$src, [string]$name) {
    $p = Join-Path $src $name
    if (-not (Test-Path -LiteralPath $p)) {
        Write-Output "  [MISSING] $name (not found in $src)"
        return $false
    }
    Copy-Item -LiteralPath $p -Destination (Join-Path $Dest $name) -Force
    return $true
}
function CopyDir([string]$src, [string]$relDst) {
    if (-not (Test-Path -LiteralPath $src)) {
        Write-Output "  [MISSING] $relDst\ (source missing: $src)"
        return
    }
    $target = Join-Path $Dest $relDst
    if (-not (Test-Path -LiteralPath $target)) { New-Item -ItemType Directory -Force -Path $target | Out-Null }
    robocopy $src $target /E /R:1 /W:1 /NJH /NP /NDL /NFL /NS /NC | Out-Null
    Write-Output "  [ok] $relDst\"
}

Write-Output "CEF lane owner staging: $Dest"
if (-not (Test-Path -LiteralPath $Dest)) { New-Item -ItemType Directory -Force -Path $Dest | Out-Null }

# 1. lane builds
Write-Output "== lane builds =="
$okExe = CopyFile $bin 'NVIDIA Share.exe'
$okDll = CopyFile $bin 'NVIDIA Share.dll'
if (-not ($okExe -and $okDll)) { throw "lane builds missing - build the vcxprojs first" }

# 2. NVIDIA Share.json (evidence-shaped config, consumed by the host)
CopyFile $PSScriptRoot 'NVIDIA Share.json'

# 3. CEF runtime (Release) - the "*.dll" + blob files of the owner drawing
Write-Output "== CEF runtime (Release) =="
foreach ($f in 'libcef.dll','chrome_elf.dll','d3dcompiler_47.dll','d3dcompiler_43.dll',
               'libEGL.dll','libGLESv2.dll','natives_blob.bin','snapshot_blob.bin',
               'v8_context_snapshot.bin') {
    CopyFile (Join-Path $cefSdk 'Release') $f | Out-Null
}

# 4. CEF resources (Resources\) + locales\ + cef.pak
Write-Output "== CEF resources =="
foreach ($f in 'cef.pak','cef_100_percent.pak','cef_200_percent.pak',
               'cef_extensions.pak','devtools_resources.pak','icudtl.dat') {
    CopyFile (Join-Path $cefSdk 'Resources') $f | Out-Null
}
CopyDir (Join-Path $cefSdk 'Resources\locales') 'locales'
# 4b. SwiftShader software-GL fallback (Release\swiftshader: libEGL/libGLESv2)
CopyDir (Join-Path $cefSdk 'Release\swiftshader') 'swiftshader'

# 5. the real OSC web UI bundle -> Resources\osc\
Write-Output "== osc bundle =="
CopyDir $oscSrc 'Resources\osc'

# 6. runtime manifest (evidence: name/size/sha256 of everything staged)
$manifest = Join-Path $Dest 'CEF-RUNTIME-MANIFEST.txt'
$rev = git -C $repo rev-parse --short HEAD 2>$null
if (-not $rev) { $rev = 'unknown' }
$lines = @(
    "NvOverlay\CEF owner stage - gfe-rebuild $rev - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
    "CEF runtime: cef_binary_73.1.12+gee4b49f+chromium-73.0.3683.75 (windows64 minimal)",
    "SDK sha1: 9166371d513cd3a86c052778a6086f9a39e8e5c0 (cef-builds.spotifycdn.com)",
    "Reference runtime: GFE 3.28.0.412 GFExperience (libcef 73.0.0-HEAD.1933+gee4b49f+chromium-73.0.3683.75)",
    ""
)
Get-ChildItem -LiteralPath $Dest -Recurse -File | ForEach-Object {
    $rel = $_.FullName.Substring($Dest.Length + 1)
    $sha = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLower()
    $lines += "{0}`t{1}`t{2}" -f $rel, $_.Length, $sha
}
$lines | Set-Content -LiteralPath $manifest
Write-Output "  [ok] CEF-RUNTIME-MANIFEST.txt ($((Get-ChildItem -LiteralPath $Dest -Recurse -File).Count) files)"
Write-Output "STAGED OK"
