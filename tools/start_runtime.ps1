$base = 'C:\My Project\NVIDIA-Shadowplay\Overlay\bin\Release\net10.0-windows10.0.26100.0'
Start-Process -FilePath (Join-Path $base 'Application\NVIDIA Capture.exe')
Start-Process -FilePath (Join-Path $base 'NVIDIA ShadowPlay.exe')
Start-Sleep -Seconds 5
Get-Process | Where-Object { $_.ProcessName -in @('NVIDIA Capture','NVIDIA ShadowPlay','NVIDIA API','NVIDIA Notifier','NVIDIA Experience') } | Select-Object Id,ProcessName