Set-Location 'C:\My Project\NVIDIA-Shadowplay'
$ErrorActionPreference='Stop'
& dotnet build '.\Engine\NVIDIA Capture.vbproj' -c Release --nologo
$code=$LASTEXITCODE
Remove-Item '.\_tmp_build.ps1' -Force
exit $code
