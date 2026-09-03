$c = New-Object Net.Sockets.TcpClient('127.0.0.1',5000)
$w = New-Object IO.StreamWriter($c.GetStream())
$w.AutoFlush = $true
$p = 'C:\Users\ScotcsDuluka\Videos\Shadowplay\Gallery\AudioFlagFix_2026-09-03.mp4'
$w.WriteLine('[Send] Test|engine_record_start:' + $p)
Start-Sleep -Seconds 2
Start-Process -FilePath 'C:\My Project\NVIDIA-Shadowplay\Overlay\bin\Release\net10.0-windows10.0.26100.0\FFmpeg\ffplay.exe' -ArgumentList '-nodisp','-autoexit','-loglevel','quiet','-f','lavfi','sine=frequency=880:duration=5'
Start-Sleep -Seconds 6
$w.WriteLine('[Send] Test|engine_record_stop')
Start-Sleep -Seconds 8
$w.Dispose()
$c.Dispose()
Write-Output 'AUDIO_FLAG_FIX_TEST_DONE'