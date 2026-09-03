$p = 'C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Audio.Wasapi\WasapiPacket.cs'
Write-Output 'SOURCE_FLAGS:'
Select-String -LiteralPath $p -Pattern 'public const int (Silent|TimestampError|Discontinuity)' | ForEach-Object { Write-Output $_.Line }
$r = 'C:\My Project\NVIDIA-Shadowplay\Overlay\bin\Release\net10.0-windows10.0.26100.0\Engine\CaptureEngine.Audio.Wasapi.dll'
Write-Output ('RUNTIME_DLL=' + (Test-Path -LiteralPath $r))
Get-Item -LiteralPath $r | Select-Object FullName,Length,LastWriteTime