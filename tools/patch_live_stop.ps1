$path = 'C:\My Project\NVIDIA-Shadowplay\CaptureEngine.FFmpegBackend\LiveMuxSession.vb'
$text = [IO.File]::ReadAllText($path)
$pattern = '(?s)_video\.RequestStopAndDrain\(timeoutMs\).*?res\.FFmpegExitCode = _proc\.ExitCode'
$replacement = @'
                ' One global stop budget. Never spend timeoutMs separately on
                ' every pipe — three 30s joins could otherwise turn one Stop
                ' click into a 90s+ hang before FFmpeg even receives EOF.
                Dim stopClock As Stopwatch = Stopwatch.StartNew()
                Dim pipeBudgetMs As Integer = Math.Min(3000, Math.Max(1000, timeoutMs \ 3))

                Log($"[live-mux] stop requested: timeout={timeoutMs}ms, pipeBudget={pipeBudgetMs}ms")
                _video.RequestStopAndDrain(pipeBudgetMs)
                _audio.RequestStopAndDrain(pipeBudgetMs)
                _mic?.RequestStopAndDrain(pipeBudgetMs)

                Dim remainingMs As Integer = Math.Max(1000, timeoutMs - CInt(stopClock.ElapsedMilliseconds))
                Log($"[live-mux] pipes closed; waiting for ffmpeg EOF/finalize ({remainingMs}ms remaining)")
                If Not _proc.WaitForExit(remainingMs) Then
                    Try : _proc.Kill() : Catch : End Try
                    res.ErrorMessage = "ffmpeg finalize timeout"
                End If
                res.FFmpegExitCode = _proc.ExitCode
'@
$new = [regex]::Replace($text, $pattern, $replacement, 1)
if ($new -eq $text) { throw 'Stop block regex not found' }
[IO.File]::WriteAllText($path, $new, [Text.Encoding]::UTF8)
