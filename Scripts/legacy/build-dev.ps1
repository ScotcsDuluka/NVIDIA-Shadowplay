# build-dev.ps1 — canonical Project -> Build dev tree
# Does NOT touch dist/ or deploy/.

param(
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
$Repo = Split-Path -Parent $PSScriptRoot
$Solution = Join-Path $Repo 'Project\NVIDIA ShadowPlay.sln'
$BuildRoot = Join-Path $Repo 'Build Dev'
$ConfigRoot = Join-Path $Repo 'Build\Build-Config'

if (-not (Test-Path -LiteralPath $Solution)) {
    throw "Canonical solution missing: $Solution"
}

New-Item -ItemType Directory -Force -Path $BuildRoot,$ConfigRoot | Out-Null

if ($Clean) {
    Get-ChildItem -LiteralPath $BuildRoot -Force -ErrorAction SilentlyContinue |
        Remove-Item -Recurse -Force
}

Write-Host '=== NVIDIA SHADOWPLAY DEV BUILD ===' -ForegroundColor Cyan
Write-Host "Solution: $Solution"
Write-Host "Output:   $BuildRoot"

# Sequential build avoids the shared-tree MSBuild file-lock race previously observed.
dotnet build $Solution -c Release -m:1 --nologo -v:minimal
if ($LASTEXITCODE -ne 0) {
    throw 'DEV BUILD FAILED'
}

$map = @(
    @{ Name='Launcher'; Src='Project\Launcher\bin\Release'; Dst='' },
    @{ Name='NvContainer'; Src='Project\NvContainer\NVIDIA Container\bin\Release'; Dst='NvContainer' },
    @{ Name='NVIDIA Web Helper'; Src='Project\NvBackend\NVIDIA Backend\bin\Release'; Dst='NvBackend' },
    @{ Name='NVIDIA ShadowPlay'; Src='Project\NvOverlay\WinForm\NVIDIA Overlay\bin\Release'; Dst='NvOverlay\WinForm' },
    @{ Name='NVIDIA Notifier'; Src='Project\NvOverlay\WinForm\NVIDIA Notifier\bin\Release'; Dst='NvOverlay\WinForm' },
    @{ Name='nvsphelper64'; Src='Project\NvShadowPlayHelper\NVIDIA ShadowPlay Helper\bin\Release'; Dst='ShadowPlay' },
    @{ Name='Gallery.Video'; Src='Project\NvGallery\WinForm\Gallery.Video\bin\Release'; Dst='NvGallery\WinForm' },
    @{ Name='NVIDIA Controls'; Src='Project\NvOverlay\WinForm\NVIDIA Controls\bin\Release'; Dst='NvOverlay\WinForm' },
    @{ Name='NVIDIA API'; Src='Project\NvOverlay\WinForm\NVIDIA API\bin\Release'; Dst='NvOverlay\WinForm' }
)

foreach ($m in $map) {
    $src = Join-Path $Repo $m.Src
    if (-not (Test-Path -LiteralPath $src)) {
        Write-Host "SKIP $($m.Name): output not found yet" -ForegroundColor Yellow
        continue
    }
    $dst = Join-Path $BuildRoot $m.Dst
    New-Item -ItemType Directory -Force -Path $dst | Out-Null
    Get-ChildItem -LiteralPath $src -Recurse -File |
        Where-Object { $_.Extension -in '.exe','.dll','.json' -and $_.FullName -notmatch '\\obj\\' } |
        ForEach-Object {
            $name = $_.Name
            if ($name.EndsWith('.deps.json')) {
                $metaOwner = if ($m.Name -eq 'Launcher') { 'NvLauncher' }
                    elseif ($m.Name -eq 'NVIDIA Web Helper') { 'NvBackend' }
                    elseif ($m.Name -eq 'NvContainer') { 'NvContainer' }
                    elseif ($m.Name -eq 'nvsphelper64') { 'NvCapture' }
                    elseif ($m.Name -in @('NVIDIA ShadowPlay','NVIDIA Notifier')) { 'NvOverlay' }
                    else { $null }
                if ($metaOwner) {
                    $meta = Join-Path $BuildRoot ".NET Deployment\$metaOwner"
                    New-Item -ItemType Directory -Force -Path $meta | Out-Null
                    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $meta $name) -Force
                }
            } else {
                Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $dst $name) -Force
            }
        }
    Write-Host "STAGED $($m.Name) -> $($m.Dst)" -ForegroundColor Green
}

# Native CEF lane keeps its x64 build output together.
$cef = Join-Path $Repo 'Project\NvOverlay\Cef\bin\x64\Release'
if (Test-Path -LiteralPath $cef) {
    $dst = Join-Path $BuildRoot 'NvOverlay\Cef'
    New-Item -ItemType Directory -Force -Path $dst | Out-Null
    Copy-Item -LiteralPath (Join-Path $cef '*') -Destination $dst -Recurse -Force
    Write-Host 'STAGED CEF -> NvOverlay\Cef' -ForegroundColor Green
}

# Normalize deployment metadata after copying referenced-project outputs.
# MSBuild may place referenced app metadata in more than one producer bin;
# the runtime tree has one authoritative location per executable.
$metadataOwners = @{
    'Launcher.deps.json' = 'NvLauncher'
    'NVIDIA API.deps.json' = 'NvOverlay'
    'NVIDIA Notifier.deps.json' = 'NvOverlay'
    'NVIDIA Share.deps.json' = 'NvOverlay'
    'NVIDIA Web Helper.deps.json' = 'NvBackend'
    'NvContainer.deps.json' = 'NvContainer'
    'nvsphelper64.deps.json' = 'NvCapture'
}
$metadataRoot = Join-Path $BuildRoot '.NET Deployment'
New-Item -ItemType Directory -Force -Path $metadataRoot | Out-Null
foreach ($entry in $metadataOwners.GetEnumerator()) {
    $sources = @(Get-ChildItem -LiteralPath $BuildRoot -Recurse -File -Filter $entry.Key)
    if ($sources.Count -gt 0) {
        $ownerDir = Join-Path $metadataRoot $entry.Value
        New-Item -ItemType Directory -Force -Path $ownerDir | Out-Null
        $target = Join-Path $ownerDir $entry.Key
        $source = $sources | Where-Object FullName -ne $target | Select-Object -First 1
        if ($source) {
            Copy-Item -LiteralPath $source.FullName -Destination $target -Force
        }
        foreach ($source in $sources) {
            if ($source.FullName -ne $target) {
                Remove-Item -LiteralPath $source.FullName -Force
            }
        }
    }
}

# Referenced projects can also copy their apphosts into a consumer's bin.
# Keep each executable in its owner directory only.
$cefDir = Join-Path $BuildRoot 'NvOverlay\Cef'
$overlayDir = Join-Path $BuildRoot 'NvOverlay\WinForm'
New-Item -ItemType Directory -Force -Path $cefDir | Out-Null
$shareFromOverlay = Join-Path $overlayDir 'NVIDIA Share.exe'
if (Test-Path $shareFromOverlay) {
    Copy-Item $shareFromOverlay (Join-Path $cefDir 'NVIDIA Share.exe') -Force
}
$shareDllFromOverlay = Join-Path $overlayDir 'NVIDIA Share.dll'
if (Test-Path $shareDllFromOverlay) {
    Copy-Item $shareDllFromOverlay (Join-Path $cefDir 'NVIDIA Share.dll') -Force
}
foreach ($duplicate in @(
    (Join-Path $overlayDir 'Launcher.exe'),
    (Join-Path $overlayDir 'NVIDIA Share.exe'),
    (Join-Path $overlayDir 'NVIDIA Share.dll'),
    (Join-Path $overlayDir 'nvsphelper64.exe'),
    (Join-Path $overlayDir 'nvsphelper64.dll')
)) {
    if (Test-Path $duplicate) { Remove-Item $duplicate -Force }
}

foreach ($dir in @(
    '.NET Deployment', '.vs', 'Data', 'Flags', 'Logs', 'NvAnsel', 'NvAudio',
    'NvBackend', 'NvConfig', 'NvContainer', 'NvGallery\WinForm', 'NvGraphics',
    'NvOverlay\Cef', 'NvOverlay\WinForm', 'Resources', 'Runtime',
    'ShadowPlay', 'ShadowPlay\FFmpeg', 'ShadowPlay\NvCapture'
)) {
    New-Item -ItemType Directory -Force -Path (Join-Path $BuildRoot $dir) | Out-Null
}

$manifest = [ordered]@{
    buildUtc        = [DateTime]::UtcNow.ToString('o')
    configuration   = 'Release'
    solution        = 'Project/NVIDIA ShadowPlay.sln'
    output          = 'Build/NVIDIA ShadowPlay'
    buildNumberFile = 'Build/Build-Config/build.txt'
    note            = 'Dev build only; deps.json is centralized under .NET Deployment.'
}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $ConfigRoot 'dev-build.json')

Write-Host 'DEV BUILD COMPLETE.' -ForegroundColor Green
