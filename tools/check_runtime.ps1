Write-Output 'NVIDIA_PROCESSES'
Get-Process | Where-Object { $_.ProcessName -like 'NVIDIA*' } | Select-Object Id,ProcessName
Write-Output 'PORT5000'
Test-NetConnection 127.0.0.1 -Port 5000 | Select-Object TcpTestSucceeded
Write-Output 'LOG_TAIL'
Get-Content -LiteralPath 'C:\My Project\NVIDIA-Shadowplay\Overlay\bin\Release\net10.0-windows10.0.26100.0\Logs\ui-engine.log' -Tail 30