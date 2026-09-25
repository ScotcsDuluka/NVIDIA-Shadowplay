# build-dev.ps1
# Canonical Dev Build:
#   Project\  ->  Build\NVIDIA ShadowPlay\
# Layout authority:
#   Build\Build-Config\dev-layout.json
# Protected reference trees are never read for staging:
#   IDK This is\dist\
#   IDK This is\deploy\

param(
    [switch]$Clean,
    [switch]$Strict
)

$ErrorActionPreference = 'Stop'

$Repo        = Split-Path -Parent $PSScriptRoot
$ProjectRoot = Join-Path $Repo 'Project'
$Solution    = Join-Path $ProjectRoot 'NVIDIA ShadowPlay.sln'
$BuildRoot   = Join-Path $Repo 'Build\NVIDIA ShadowPlay'
$ConfigRoot  = Join-Path $Repo 'Build\Build-Config'
$LayoutFile  = Join-Path $ConfigRoot 'dev-layout.json'

if (-not (Test-Path -LiteralPath $Solution)) { throw "Canonical solution missing: $Solution" }
if (-not (Test-Path -LiteralPath $LayoutFile)) { throw "Layout config missing: $LayoutFile" }

# CEF C++ lane needs BOTH the CEF SDK and the VS C++ workload. Until both are
# present, build the no-CEF solution filter instead of failing on the three
# vcxproj — the CEF owner stages as PENDING either way.
$SolutionToBuild = $Solution
$NoCefFilter = Join-Path $ProjectRoot 'NVIDIA ShadowPlay Dev (no CEF).slnf'
$cefSdkReady = Test-Path -LiteralPath 'C:\My Project\cef-sdk\cef73\Release\libcef.lib'
$vcToolsReady = $false
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
if (Test-Path -LiteralPath $vswhere) {
    $vcInst = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
    $vcToolsReady = [bool]$vcInst
}
if (-not ($cefSdkReady -and $vcToolsReady)) {
    if (Test-Path -LiteralPath $NoCefFilter) { $SolutionToBuild = $NoCefFilter }
    elseif ($cefSdkReady) { throw 'CEF SDK present but the VS C++ workload is missing — install "Desktop development with C++" (or move the SDK away).' }
    else { throw 'CEF SDK missing and no-CEF solution filter missing: ' + $NoCefFilter }
}

$Layout = Get-Content -LiteralPath $LayoutFile -Raw | ConvertFrom-Json

$msbuildCandidates = @(
    'C:\Visual Studio\MSBuild\Current\Bin\MSBuild.exe',
    'C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe',
    'C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe',
    'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe',
    'C:\Program Files\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe',
    'C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe',
    'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe'
)
$MSBuild = $msbuildCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $MSBuild) { throw 'MSBuild.exe not found' }

$Configuration = 'Release'
$TargetFramework = 'net10.0-windows10.0.26100.0'
$pending = New-Object System.Collections.Generic.List[string]

function Ensure-Dir([string]$path) {
    New-Item -ItemType Directory -Force -Path $path | Out-Null
}

function Copy-Tree([string]$src, [string]$dst) {
    if (-not (Test-Path -LiteralPath $src)) {
        Write-Host "PENDING tree: $src" -ForegroundColor Yellow
        $pending.Add($src)
        return
    }
    Ensure-Dir $dst
    robocopy $src $dst /E /R:1 /W:1 /NJH /NJS /NP /NDL /NFL /NS /NC | Out-Null
    if ($LASTEXITCODE -gt 7) { throw "robocopy failed: $src -> $dst (exit $LASTEXITCODE)" }
}

function Copy-RuntimeFiles([string]$srcBin, [string]$dst, [string]$ownerName) {
    if (-not (Test-Path -LiteralPath $srcBin)) {
        Write-Host "PENDING binary output: $ownerName -> $srcBin" -ForegroundColor Yellow
        $pending.Add("${ownerName}: $srcBin")
        return
    }
    Ensure-Dir $dst
    Get-ChildItem -LiteralPath $srcBin -Recurse -File -Force |
        Where-Object {
            $_.Extension -in '.exe','.dll','.json' -and
            $_.Extension -notin '.pdb','.xml' -and
            $_.FullName -notmatch '\\obj\\'
        } |
        ForEach-Object {
            $rel = $_.FullName.Substring($srcBin.Length).TrimStart('\')
            $to = Join-Path $dst $rel
            Ensure-Dir (Split-Path -Parent $to)
            Copy-Item -LiteralPath $_.FullName -Destination $to -Force
        }
}

function Copy-FlatDlls([string]$srcBin, [string]$dst, [string[]]$names) {
    Ensure-Dir $dst
    foreach ($name in $names) {
        $src = Join-Path $srcBin $name
        if (Test-Path -LiteralPath $src) {
            Copy-Item -LiteralPath $src -Destination (Join-Path $dst $name) -Force
        } else {
            Write-Host "PENDING dependency: $name" -ForegroundColor Yellow
            $pending.Add("$src")
        }
    }
}

if ($Clean -and (Test-Path -LiteralPath $BuildRoot)) {
    Remove-Item -LiteralPath $BuildRoot -Recurse -Force
}

Ensure-Dir $BuildRoot
Ensure-Dir $ConfigRoot

Write-Host '=== NVIDIA SHADOWPLAY DEV BUILD ===' -ForegroundColor Cyan
Write-Host "Solution : $SolutionToBuild"
if ($SolutionToBuild -ne $Solution) { Write-Host '(no-CEF filter: CEF lane deferred — SDK or VC tools missing)' -ForegroundColor Yellow }
Write-Host "Output   : $BuildRoot"
Write-Host "Layout   : $LayoutFile"

Write-Host '== Restore ==' -ForegroundColor Cyan
& $MSBuild $SolutionToBuild /t:Restore /p:Configuration=$Configuration /m:1 /nr:false /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "RESTORE FAILED (exit $LASTEXITCODE)" }

Write-Host '== Build ==' -ForegroundColor Cyan
& $MSBuild $SolutionToBuild /t:Build /p:Configuration=$Configuration /m:1 /nr:false /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "DEV BUILD FAILED (exit $LASTEXITCODE)" }

# Root layout directories are created from the layout specification.
foreach ($rel in $Layout.rootDirectories) {
    Ensure-Dir (Join-Path $BuildRoot ($rel -replace '/', '\'))
}

# Runtime/static payloads. These are Project-owned sources, never IDK This is\dist/deploy.
Copy-Tree (Join-Path $ProjectRoot '.NET Deployment') (Join-Path $BuildRoot '.NET Deployment')
Copy-Tree (Join-Path $ProjectRoot 'Data')          (Join-Path $BuildRoot 'Data')
# Localized UI strings live at the ROOT owner (AppLayout.P("Languages", ...)).
Copy-Tree (Join-Path $ProjectRoot 'Resources\Languages') (Join-Path $BuildRoot 'Languages')
Copy-Tree (Join-Path $ProjectRoot 'Flags')         (Join-Path $BuildRoot 'Flags')
Copy-Tree (Join-Path $ProjectRoot 'Resources')     (Join-Path $BuildRoot 'Resources')
Ensure-Dir (Join-Path $BuildRoot '.vs')
Ensure-Dir (Join-Path $BuildRoot 'Logs')
Ensure-Dir (Join-Path $BuildRoot 'NvAnsel')

# .NET runtime bootstrap lives in .NET Deployment in the historical layout.
$runtimeBootstrap = Join-Path $ProjectRoot 'Runtime\64bit.runtime.exe'
if (Test-Path -LiteralPath $runtimeBootstrap) {
    Copy-Item -LiteralPath $runtimeBootstrap -Destination (Join-Path $BuildRoot '.NET Deployment\64bit.runtime.exe') -Force
}

# Common runtime dependencies and family dependencies.
$overlayBin = Join-Path $ProjectRoot ("NvOverlay\WinForm\NvShadowPlay.exe\bin\Release\{0}" -f $TargetFramework)
Ensure-Dir (Join-Path $BuildRoot 'Runtime')
if (Test-Path -LiteralPath $overlayBin) {
    Copy-FlatDlls $overlayBin (Join-Path $BuildRoot 'Runtime') @(
        'Microsoft.Windows.SDK.NET.dll',
        'Newtonsoft.Json.dll',
        'WinRT.Runtime.dll',
        'SharpGen.Runtime.dll',
        'SharpGen.Runtime.COM.dll',
        'System.Management.dll'
    )
}

# Launcher -> root.
$launcherBin = Join-Path $ProjectRoot ("Launcher.exe\bin\Release\{0}" -f $TargetFramework)
Copy-RuntimeFiles $launcherBin $BuildRoot 'Launcher'

# Launcher CEF lane (new native launcher) -> root + NvOverlay\Cef.
# Guarded on built artifacts: present = the CEF launcher replaces the
# WinForm root entry (deploy-launcher.ps1 contract). Absent (no MSVC/CEF
# SDK on the build machine) = the WinForm launcher stages as before.
$cefLauncherBin = Join-Path $ProjectRoot 'Launcher.Cef\bin\x64\Release'
$cefLauncherExe = Join-Path $cefLauncherBin 'Launcher.exe'
$cefLauncherDll = Join-Path $cefLauncherBin 'Launcher.dll'
if ((Test-Path -LiteralPath $cefLauncherExe) -and (Test-Path -LiteralPath $cefLauncherDll)) {
    Copy-Item -LiteralPath $cefLauncherExe -Destination (Join-Path $BuildRoot 'Launcher.exe') -Force
    $cefSlot = Join-Path $BuildRoot 'NvOverlay\Cef'
    Ensure-Dir $cefSlot
    Copy-Item -LiteralPath $cefLauncherDll -Destination (Join-Path $cefSlot 'Launcher.dll') -Force
    $cefUiSrc = Join-Path $ProjectRoot 'Launcher.Cef\ui'
    if (Test-Path -LiteralPath (Join-Path $cefUiSrc 'index.html')) {
        Copy-Tree $cefUiSrc (Join-Path $cefSlot 'Resources\launcher')
    }
    Write-Host '[launcher] CEF lane staged (root Launcher.exe + NvOverlay\Cef\Launcher.dll)'
} else {
    Write-Host '[launcher] CEF lane artifacts missing - WinForm launcher stays'
}

# NvBackend -> owner folder + Node backend source (Web Helper hosts the node backend).
$backendProjectBin = Join-Path $ProjectRoot ("NvBackend\NVIDIA Web Helper.exe\bin\Release\{0}" -f $TargetFramework)
$backendOut = Join-Path $BuildRoot 'NvBackend'
Copy-RuntimeFiles $backendProjectBin $backendOut 'NVIDIA Web Helper'
$backendSrc = Join-Path $ProjectRoot 'NvBackend\NVIDIA Web Helper.exe\Backend'
Copy-Tree $backendSrc $backendOut

# NvConfig runtime seed: default the overlay stack ON (owner: WinForm family
# is the default). The user's toggles rewrite this file at runtime; a -Clean
# must not silently flip the overlay stack back off.
$nvConfigDir = Join-Path $BuildRoot 'NvConfig'
Ensure-Dir $nvConfigDir
$seedConfigPath = Join-Path $nvConfigDir 'config.json'
if (-not (Test-Path -LiteralPath $seedConfigPath)) {
    '{ "Overlay": { "UseOverlayEnabled": true } }' | Set-Content -LiteralPath $seedConfigPath -Encoding utf8
}

# Install Node production dependencies into the staged backend, without mutating Project.
if ((Test-Path (Join-Path $backendOut 'package.json')) -and (Get-Command npm.cmd -ErrorAction SilentlyContinue)) {
    Push-Location $backendOut
    try {
        & npm.cmd ci --omit=dev --ignore-scripts --no-audit --no-fund
        if ($LASTEXITCODE -ne 0) {
            Write-Host "PENDING Node dependency install: npm ci failed (exit $LASTEXITCODE)" -ForegroundColor Yellow
            $pending.Add('NvBackend\node_modules')
        }
    }
    finally {
        Pop-Location
    }
} elseif (-not (Test-Path (Join-Path $backendOut 'package.json'))) {
    Write-Host 'PENDING Node dependency install: package.json missing' -ForegroundColor Yellow
    $pending.Add('NvBackend\package.json')
} else {
    Write-Host 'PENDING Node dependency install: npm.cmd not found' -ForegroundColor Yellow
    $pending.Add('npm.cmd')
}

# NvContainer -> owner folder. The worker contract follows the requested Dev layout.
$containerBin = Join-Path $ProjectRoot ("NvContainer\NvContainer.exe\bin\Release\{0}" -f $TargetFramework)
$containerOut = Join-Path $BuildRoot 'NvContainer'
Copy-RuntimeFiles $containerBin $containerOut 'NVIDIA Container'
$containerConfig = [ordered]@{
    port = 5050
    securityCookie = 'eb6eb0702ec25f9aeb0b0f8f79d06d5b'
    workers = @(
        [ordered]@{
            name = 'nvsphelper64'
            exe = '..\ShadowPlay\nvsphelper64.exe'
            args = ''
            workingDirectory = '..\ShadowPlay'
            enabled = $true
            adoptExisting = $true
            maxRestarts = 10
            restartBackoffSeconds = 5
        }
    )
}
$containerConfigDir = Join-Path $containerOut 'Config'
Ensure-Dir $containerConfigDir
# The container reads NvContainer\Config\NvContainer.json (NOT the exe dir)
# and self-seeds a 0-worker default there when the file is missing.
$containerConfig | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $containerConfigDir 'NvContainer.json') -Encoding utf8

# NvBackend API TCP hub (WinForm family contract: API TCP + Notifier +
# NVIDIA ShadowPlay.exe + NVIDIA ShadowPlay Helper.exe). ExePath("NvBackend.exe")
# resolves here; the overlay also links NvBackend.dll in-process (kept in
# NvOverlay\WinForm by the dedupe keep-rule).
$hubBin = Join-Path $ProjectRoot ("NvBackend\NvBackend.exe\bin\Release\{0}" -f $TargetFramework)
Copy-RuntimeFiles $hubBin (Join-Path $BuildRoot 'NvBackend') 'NvBackend'

# Gallery.Video
$galleryBin = Join-Path $ProjectRoot 'NvGallery\WinForm\Gallery.Video.dll\bin\Release\net10.0'
$galleryOut = Join-Path $BuildRoot 'NvGallery\WinForm'
Copy-RuntimeFiles $galleryBin $galleryOut 'Gallery.Video'

# Overlay WinForm owners. (NVIDIA API was moved to NvBackend\NVIDIA Backend —
# its types ship in-process with the overlay via ProjectReference.)
$controlsBin = Join-Path $ProjectRoot ("NvOverlay\WinForm\NvControls\NvControls.dll\bin\Release\{0}" -f $TargetFramework)
$notifierBin = Join-Path $ProjectRoot ("NvOverlay\WinForm\NvNotifier.exe\bin\Release\{0}" -f $TargetFramework)
$overlayWinBin = Join-Path $ProjectRoot ("NvOverlay\WinForm\NvShadowPlay.exe\bin\Release\{0}" -f $TargetFramework)

Copy-RuntimeFiles $controlsBin (Join-Path $BuildRoot 'NvOverlay\WinForm') 'NVIDIA Controls'
Copy-RuntimeFiles $notifierBin (Join-Path $BuildRoot 'NvOverlay\WinForm') 'NVIDIA Notifier'
Copy-RuntimeFiles $overlayWinBin (Join-Path $BuildRoot 'NvOverlay\WinForm') 'NVIDIA ShadowPlay'

# Audio / graphics dependency families.
$naudioNames = @('NAudio.dll','NAudio.Asio.dll','NAudio.Core.dll','NAudio.Midi.dll','NAudio.Wasapi.dll','NAudio.WinForms.dll','NAudio.WinMM.dll')
Copy-FlatDlls $overlayWinBin (Join-Path $BuildRoot 'NvAudio') $naudioNames
$graphicsNames = @('Vortice.Direct3D11.dll','Vortice.DirectX.dll','Vortice.DXGI.dll','Vortice.Mathematics.dll')
Copy-FlatDlls $overlayWinBin (Join-Path $BuildRoot 'NvGraphics') $graphicsNames

# Config owner.
$notifierConfig = Join-Path $ProjectRoot 'NvOverlay\WinForm\NvNotifier.exe\notifier_obs.json'
if (Test-Path $notifierConfig) {
    Copy-Item $notifierConfig (Join-Path $BuildRoot 'NvConfig\notifier_obs.json') -Force
}

# Capture engine family. Tests are intentionally excluded.
$captureProjects = @(
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.dll';                  Tfm='net10.0' },
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.Audio.dll';            Tfm='net10.0' },
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.Audio.Wasapi.dll';    Tfm='net10.0' },
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.Encoder.dll';         Tfm='net10.0' },
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.Encoder.Nvenc.dll';   Tfm='net10.0-windows' },
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.FFmpegBackend.dll';   Tfm='net10.0' },
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.Recording.dll';       Tfm='net10.0-windows' },
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.Recording.ConsoleDriver.exe'; Tfm='net10.0-windows' },
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.Video.dll';            Tfm='net10.0' },
    @{ Rel='ShadowPlay\NvCapture\CaptureEngine.Video.Ddagrab.dll';   Tfm='net10.0-windows' },
    @{ Rel='ShadowPlay\WgcCapture.dll';                              Tfm=$TargetFramework }
)
$captureOut = Join-Path $BuildRoot 'ShadowPlay\NvCapture'
foreach ($c in $captureProjects) {
    $src = Join-Path $ProjectRoot ($c.Rel + "\bin\Release\" + $c.Tfm)
    Copy-RuntimeFiles $src $captureOut ($c.Rel)
}

# FFmpeg is Project-owned runtime payload, staged at the ROOT owner
# (AppLayout v2 contract — every FFmpegLocator candidate probes
# <root>\FFmpeg\ffmpeg.exe; the old ShadowPlay\FFmpeg spot is not probed).
Copy-Tree (Join-Path $ProjectRoot 'ShadowPlay\FFmpeg') (Join-Path $BuildRoot 'FFmpeg')

# Helper root runtime (WinForm + CEF shared engine lane; alias:
# NVIDIA ShadowPlay Helper — exe identity stays nvsphelper64 per owner).
$helperBin = Join-Path $ProjectRoot ("ShadowPlay\nvsphelper64.exe\bin\Release\{0}" -f $TargetFramework)
$shadowOut = Join-Path $BuildRoot 'ShadowPlay'
Copy-RuntimeFiles $helperBin $shadowOut 'nvsphelper64'
foreach ($name in @('nvsphelper64.exe','nvsphelper64.dll','nvsphelper64.runtimeconfig.json')) {
    $p = Join-Path $helperBin $name
    if (Test-Path $p) { Copy-Item $p (Join-Path $shadowOut $name) -Force }
}

# Native hook: use a real Project-owned nvspcap*.dll if one exists.
$hookDll = Get-ChildItem $ProjectRoot -Recurse -File -Force -Filter 'nvspcap*.dll' -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\Debug\\' } |
    Select-Object -First 1
if ($hookDll) {
    Copy-Item $hookDll.FullName -Destination (Join-Path $shadowOut $hookDll.Name) -Force
} else {
    Write-Host 'PENDING runtime artifact: ShadowPlay\nvspcap.dll (current hook project is still a placeholder and does not produce nvspcap.dll)' -ForegroundColor Yellow
    $pending.Add('ShadowPlay\nvspcap.dll')
}

# Root product icon.
$icon = Join-Path $ProjectRoot 'NVIDIA ShadowPlay.ico'
if (Test-Path $icon) { Copy-Item $icon (Join-Path $BuildRoot 'NVIDIA ShadowPlay.ico') -Force }

# CEF owner: built host + pinned CEF runtime + OSC bundle.
$cefOut = Join-Path $BuildRoot 'NvOverlay\Cef'
Ensure-Dir $cefOut
$cefBin = Join-Path $ProjectRoot 'NvOverlay\Cef\bin\x64\Release'
Copy-RuntimeFiles $cefBin $cefOut 'NVIDIA Share CEF host'
$shareJson = Join-Path $ProjectRoot 'NvOverlay\Cef\NVIDIA Share.json'
if (Test-Path $shareJson) { Copy-Item $shareJson (Join-Path $cefOut 'NVIDIA Share.json') -Force }

$cefSdk = 'C:\My Project\cef-sdk\cef73'
if (Test-Path $cefSdk) {
    foreach ($f in @('libcef.dll','chrome_elf.dll','d3dcompiler_47.dll','d3dcompiler_43.dll','libEGL.dll','libGLESv2.dll','natives_blob.bin','snapshot_blob.bin','v8_context_snapshot.bin')) {
        $src = Join-Path $cefSdk "Release\$f"
        if (Test-Path $src) { Copy-Item $src (Join-Path $cefOut $f) -Force }
        else { $pending.Add("CEF:$f") }
    }
    foreach ($f in @('cef.pak','cef_100_percent.pak','cef_200_percent.pak','cef_extensions.pak','devtools_resources.pak','icudtl.dat')) {
        $src = Join-Path $cefSdk "Resources\$f"
        if (Test-Path $src) { Copy-Item $src (Join-Path $cefOut $f) -Force }
        else { $pending.Add("CEF Resources:$f") }
    }
    Copy-Tree (Join-Path $cefSdk 'Resources\locales') (Join-Path $cefOut 'locales')
    Copy-Tree (Join-Path $cefSdk 'Release\swiftshader') (Join-Path $cefOut 'swiftshader')
} else {
    $pending.Add('CEF SDK: C:\My Project\cef-sdk\cef73')
}

# OSC frontend bundle: Project-owned copy of the GFE 3.28 WebView\osc
# bundle (index.html + vendor/common/app.js + config.js + assets). Docs\osc
# is the osc DOCUMENTATION set, not the bundle — staging it here produced a
# "not found" page (no index.html to serve).
$oscSrc = Join-Path $ProjectRoot 'NvOverlay\Cef\osc'
$oscDst = Join-Path $cefOut 'Resources\osc'
if (Test-Path $oscSrc) {
    if (Test-Path $oscDst) { Remove-Item -LiteralPath $oscDst -Recurse -Force }
    Copy-Tree $oscSrc $oscDst
} else {
    Write-Host "PENDING OSC bundle: $oscSrc" -ForegroundColor Yellow
    $pending.Add('NvOverlay\Cef\osc (OSC frontend bundle)')
}

# Central deployment metadata: keep deps.json discoverable without removing them
# from the runnable folders.
$deploymentRoot = Join-Path $BuildRoot '.NET Deployment'
Ensure-Dir $deploymentRoot
Get-ChildItem -LiteralPath $BuildRoot -Recurse -File -Filter '*.deps.json' -Force |
    Where-Object { $_.FullName -notlike ($deploymentRoot + '\*') } |
    ForEach-Object {
        $rel = $_.FullName.Substring($BuildRoot.Length + 1)
        $safe = $rel -replace '\\','__'
        Copy-Item $_.FullName (Join-Path $deploymentRoot $safe) -Force
    }

# ── Owner dedupe (owner-tree rule: ONE canonical folder per assembly) ────
# Copy-RuntimeFiles stages each project bin WHOLE, so referenced-project
# outputs (CaptureEngine/NAudio/Vortice/Launcher/...) leak into every
# referencing owner folder. AppLayout.vb's Resolving handler supplies
# cross-folder assemblies (NvCapture/ShadowPlay/ShadowPlay\NvCapture/
# NvAudio/NvGraphics/Runtime/NvGallery probes), so those leaked copies are
# dead weight — prune them back to the canonical owner. Patterns are
# filename PREFIXES; a folder's own primary outputs are never listed as
# foreign. WinForm keeps NvBackend.dll (in-process library of the overlay)
# but loses NvBackend.exe (legacy apphost, superseded by Web Helper).
$ownerForeign = @(
    @{ Folder = '.'                    ; Foreign = @('NVIDIA Controls.', 'Microsoft.Windows.SDK.NET.', 'WinRT.Runtime.', 'Newtonsoft.Json.', 'System.Management.', 'CaptureEngine.', 'NAudio.', 'Vortice.') },
    @{ Folder = 'NvOverlay\WinForm'    ; Foreign = @('CaptureEngine.', 'WgcCapture.', 'NAudio.', 'Vortice.', 'SharpGen.', 'Gallery.Video.', 'Launcher.', 'nvsphelper64.', 'NvBackend.exe', 'NVIDIA Backend.exe', 'Microsoft.Windows.SDK.NET.', 'WinRT.Runtime.', 'Newtonsoft.Json.', 'System.Management.') },
    @{ Folder = 'ShadowPlay'           ; Foreign = @('CaptureEngine.', 'WgcCapture.', 'NAudio.', 'Vortice.', 'SharpGen.', 'Gallery.Video.', 'Launcher.', 'Microsoft.Windows.SDK.NET.', 'WinRT.Runtime.', 'Newtonsoft.Json.', 'System.Management.') },
    @{ Folder = 'ShadowPlay\NvCapture' ; Foreign = @('NAudio.', 'Vortice.', 'SharpGen.', 'Microsoft.Windows.SDK.NET.', 'WinRT.Runtime.', 'Newtonsoft.Json.', 'System.Management.') },
    @{ Folder = 'NvGallery\WinForm'    ; Foreign = @('NAudio.', 'Vortice.', 'SharpGen.', 'Microsoft.Windows.SDK.NET.', 'WinRT.Runtime.', 'Newtonsoft.Json.', 'System.Management.') },
    @{ Folder = 'NvBackend'            ; Foreign = @('NAudio.', 'Vortice.', 'SharpGen.', 'Microsoft.Windows.SDK.NET.', 'WinRT.Runtime.', 'Newtonsoft.Json.', 'System.Management.') },
    @{ Folder = 'NvContainer'          ; Foreign = @('NAudio.', 'Vortice.', 'SharpGen.', 'Microsoft.Windows.SDK.NET.', 'WinRT.Runtime.', 'Newtonsoft.Json.', 'System.Management.', 'CaptureEngine.', 'Launcher.') }
)
$pruned = 0
foreach ($rule in $ownerForeign) {
    $dir = Join-Path $BuildRoot $rule.Folder
    if (-not (Test-Path -LiteralPath $dir)) { continue }
    Get-ChildItem -LiteralPath $dir -File -Force | ForEach-Object {
        $f = $_
        if ($f.Extension -notin '.dll', '.exe') { return }
        $hit = $rule.Foreign | Where-Object { $f.Name -like ($_ + '*') }
        if ($hit) {
            Remove-Item -LiteralPath $f.FullName -Force
            $script:pruned++
        }
    }
}
Write-Host "OWNER DEDUPE: pruned $pruned duplicate binary file(s)." -ForegroundColor Cyan

$manifest = [ordered]@{
    buildUtc = [DateTime]::UtcNow.ToString('o')
    configuration = $Configuration
    solution = if ($SolutionToBuild -ne $Solution) { 'Project/NVIDIA ShadowPlay Dev (no CEF).slnf' } else { 'Project/NVIDIA ShadowPlay.sln' }
    output = 'Build/NVIDIA ShadowPlay'
    layout = 'Build/Build-Config/dev-layout.json'
    protectedReferences = @('IDK This is/dist','IDK This is/deploy')
    testsExcluded = $true
    buildResult = 'MSBuild exit 0'
    pendingArtifacts = @($pending)
}
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $ConfigRoot 'dev-build.json') -Encoding utf8

if ($pending.Count -gt 0) {
    Write-Host "DEV BUILD STAGED WITH PENDING ARTIFACTS: $($pending.Count)" -ForegroundColor Yellow
    $pending | ForEach-Object { Write-Host "  PENDING: $_" -ForegroundColor Yellow }
    if ($Strict) { exit 2 }
} else {
    Write-Host 'DEV BUILD STAGED COMPLETE.' -ForegroundColor Green
}
exit 0
