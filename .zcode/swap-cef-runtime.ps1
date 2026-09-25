# Overlay GFE 3.28's NVIDIA-custom CEF runtime onto the cef73 SDK.
$ErrorActionPreference = 'Stop'
$g = 'C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience'
$c = 'C:\My Project\cef-sdk\cef73'

$toRelease = @('libcef.dll','chrome_elf.dll','libEGL.dll','libGLESv2.dll','d3dcompiler_43.dll','d3dcompiler_47.dll','natives_blob.bin','snapshot_blob.bin','v8_context_snapshot.bin')
$toResources = @('icudtl.dat','cef.pak','cef_100_percent.pak','cef_200_percent.pak','cef_extensions.pak')

foreach ($f in $toRelease) {
    Copy-Item -LiteralPath (Join-Path $g $f) -Destination (Join-Path $c ('Release\' + $f)) -Force
}
foreach ($f in $toResources) {
    Copy-Item -LiteralPath (Join-Path $g $f) -Destination (Join-Path $c ('Resources\' + $f)) -Force
}
if (Test-Path (Join-Path $g 'locales')) {
    Copy-Item -LiteralPath (Join-Path $g 'locales') -Destination (Join-Path $c 'Resources') -Recurse -Force
}
foreach ($junk in @('Debug', 'tests')) {
    $p = Join-Path $c $junk
    if (Test-Path $p) { Remove-Item -LiteralPath $p -Recurse -Force }
}

Write-Host ('Release libcef: ' + (Get-Item (Join-Path $c 'Release\libcef.dll')).VersionInfo.FileVersion)
Write-Host ('GFE    libcef: ' + (Get-Item (Join-Path $g 'libcef.dll')).VersionInfo.FileVersion)
Write-Host ('FREE: ' + [Math]::Round((Get-PSDrive C).Free/1GB, 2) + 'GB')
