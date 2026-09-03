$path = 'C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording\CaptureSession.vb'
$text = Get-Content -LiteralPath $path -Raw
$old1 = 'Dim actualSessionSeconds As Double = Math.Min(duration.TotalSeconds, sw.Elapsed.TotalSeconds)'
if ($text.Contains($old1)) { $text = $text.Replace($old1, 'Dim actualSessionSeconds As Double = stopElapsedSeconds') }
$old2 = 'WasapiPositionCapture.QpcTicksTo100ns(Stopwatch.GetTimestamp()))'
if ($text.Contains($old2)) { $text = $text.Replace($old2, 'WasapiPositionCapture.QpcTicksTo100ns(stopQpcTicks))') }
$old3 = 'result.ActualDurationSec = sw.Elapsed.TotalSeconds'
if ($text.Contains($old3)) { $text = $text.Replace($old3, 'result.ActualDurationSec = stopElapsedSeconds') }
$tmp = $path + '.patchtmp'
Set-Content -LiteralPath $tmp -Value $text -Encoding UTF8
Move-Item -LiteralPath $tmp -Destination $path -Force
Write-Output 'PATCH2_OK'
