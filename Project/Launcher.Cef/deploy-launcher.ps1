# deploy-launcher.ps1 - build + stage the Launcher CEF lane:
#   Launcher.exe  -> <Dest>\                      (root slot, replaces the
#                                                   WinForm launcher entry)
#   Launcher.dll  -> <Dest>\NvOverlay\Cef\        (shared CEF runtime slot —
#                                                   "ลงที่เดียวกับ osc":
#                                                   libcef.dll / cef.pak /
#                                                   locales used once)
#   ui\           -> <Dest>\NvOverlay\Cef\Resources\launcher\
#
# Usage:
#   powershell -File deploy-launcher.ps1            # build + stage
#   powershell -File deploy-launcher.ps1 -NoBuild   # stage current bin only
#   powershell -File deploy-launcher.ps1 -Dest <path>
param(
    [string]$Dest = "",
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$bin  = Join-Path $PSScriptRoot 'bin\x64\Release'
$wrapperLib = Join-Path $repo 'Project\Overlay OSC\NVIDIA OSC\obj\wrapper73\libcef_dll_wrapper73.lib'
if ($Dest -eq "") { $Dest = Join-Path $repo 'build\NVIDIA ShadowPlay' }
$Dest = [System.IO.Path]::GetFullPath($Dest)

if (-not $NoBuild) {
    if (-not (Test-Path -LiteralPath $wrapperLib)) {
        throw "CEF 73 wrapper library missing: $wrapperLib. Build the OSC CEF 73 wrapper first."
    }
    $msbuild = 'C:\Visual Studio\MSBuild\Current\Bin\MSBuild.exe'
    if (-not (Test-Path -LiteralPath $msbuild)) {
        $pf86 = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
        $vswhere = Join-Path $pf86 'Microsoft Visual Studio\Installer\vswhere.exe'
        $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }
    if (-not (Test-Path -LiteralPath $msbuild)) { throw "MSBuild.exe not found" }
    Write-Output "build via $msbuild"
    foreach ($proj in @('Launcher.vcxproj', 'Launcher Bootstrap.vcxproj')) {
        & $msbuild (Join-Path $PSScriptRoot $proj) -p:Configuration=Release -p:Platform=x64 -m -v:q -nologo
        if ($LASTEXITCODE -ne 0) { throw "build FAILED: $proj (exit $LASTEXITCODE)" }
    }
}

function MustExist([string]$p) {
    if (-not (Test-Path -LiteralPath $p)) { throw "missing staged input: $p" }
}

$exe = Join-Path $bin 'Launcher.exe'
$dll = Join-Path $bin 'Launcher.dll'
$ui  = Join-Path $PSScriptRoot 'ui'
MustExist $exe
MustExist $dll
MustExist (Join-Path $ui 'index.html')
$cefSlot = Join-Path $Dest 'NvOverlay\Cef'
MustExist (Join-Path $cefSlot 'libcef.dll')
MustExist (Join-Path $cefSlot 'cef.pak')
MustExist (Join-Path $cefSlot 'locales\en-US.pak')

# Stop only this product's launcher and its CEF subprocesses. Other
# applications may also use the image name Launcher.exe.
$targetExe = [System.IO.Path]::GetFullPath((Join-Path $Dest 'Launcher.exe'))
Get-CimInstance Win32_Process -Filter "Name = 'Launcher.exe'" | ForEach-Object {
    if ($_.ExecutablePath -and
        [string]::Equals([System.IO.Path]::GetFullPath($_.ExecutablePath),
                         $targetExe, [System.StringComparison]::OrdinalIgnoreCase)) {
        Write-Output "  stopping product Launcher (pid $($_.ProcessId))"
        Stop-Process -Id $_.ProcessId -Force -ErrorAction Stop
    }
}
Start-Sleep -Milliseconds 250
if (Get-CimInstance Win32_Process -Filter "Name = 'Launcher.exe'" |
    Where-Object { $_.ExecutablePath -and
        [string]::Equals([System.IO.Path]::GetFullPath($_.ExecutablePath),
                         $targetExe, [System.StringComparison]::OrdinalIgnoreCase) }) {
    throw "The product Launcher is still running; refusing to replace $targetExe"
}

Copy-Item -LiteralPath $exe -Destination (Join-Path $Dest 'Launcher.exe') -Force
Write-Output "  [ok] Launcher.exe -> root"

New-Item -ItemType Directory -Force -Path $cefSlot | Out-Null
Copy-Item -LiteralPath $dll -Destination (Join-Path $cefSlot 'Launcher.dll') -Force
Write-Output "  [ok] Launcher.dll -> NvOverlay\Cef\ (shared CEF runtime slot)"

$uiDest = Join-Path $cefSlot 'Resources\launcher'
New-Item -ItemType Directory -Force -Path $uiDest | Out-Null
robocopy $ui $uiDest /E /R:1 /W:1 /NJH /NP /NDL /NFL /NS /NC | Out-Null
if ($LASTEXITCODE -ge 8) { throw "UI staging failed (robocopy exit $LASTEXITCODE)" }
Write-Output "  [ok] ui -> NvOverlay\Cef\Resources\launcher\"

Write-Output "staged to $Dest"
