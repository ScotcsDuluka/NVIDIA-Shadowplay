$out = 'C:\Users\ScotcsDuluka\Videos\Shadowplay\Gallery\FpsSyncFix_2026-09-03.mp4'
$c = New-Object System.Net.Sockets.TcpClient
$c.Connect('127.0.0.1',5000)
$w = New-Object System.IO.StreamWriter($c.GetStream())
$w.AutoFlush = $true
$w.WriteLine('[Send] FpsTest|engine_record_start:' + $out)
Start-Sleep -Seconds 8
$w.WriteLine('[Send] FpsTest|engine_record_stop:')
Start-Sleep -Seconds 5
$w.Dispose()
$c.Dispose()
Write-Output 'FPS_SYNC_TEST_DONE'
