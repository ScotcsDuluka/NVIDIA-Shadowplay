$p = 'C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording\RecordingEngine.vb'
$s = Get-Content -LiteralPath $p -Raw
$pattern = '(?m)^                config\.RequestedHeight = echo\.RequestedHeight\r?\n                config\.EncodeWidth = _encoder\.EncodeWidthOutput'
$replacement = @"
                config.RequestedHeight = echo.RequestedHeight

                Dim encoderFps As Integer = If(echo.Fps > 0, echo.Fps, 60)
                If config.TargetFps > 0 AndAlso config.TargetFps <> encoderFps Then
                    _logger.Warning($"[RecordingEngine] session FPS {config.TargetFps} != persistent NVENC FPS {encoderFps}; using {encoderFps} to preserve real-time playback. Engine rebuild required to apply a new FPS.")
                End If
                config.TargetFps = encoderFps

                config.EncodeWidth = _encoder.EncodeWidthOutput
"@
if (-not [regex]::IsMatch($s,$pattern)) { throw 'session fps anchor not found' }
$s = [regex]::Replace($s,$pattern,$replacement,1)
Set-Content -LiteralPath $p -Value $s -Encoding UTF8
Write-Output 'SESSION_FPS_PATCHED'