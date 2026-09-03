$p = 'C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Encoder.Nvenc\NvencEncoderBackend.vb'
$s = Get-Content -LiteralPath $p -Raw
$old = "        Public ReadOnly Property EncodeHeightOutput As Integer`r`n            Get`r`n                Return CInt(_encodeHeight)`r`n            End Get`r`n        End Property"
$new = $old + "`r`n`r`n        Public ReadOnly Property FrameRateFps As Integer`r`n            Get`r`n                If _frameRateDen = 0 Then Return 60`r`n                Return Math.Max(1, CInt(_frameRateNum \ _frameRateDen))`r`n            End Get`r`n        End Property"
if (-not $s.Contains($old)) { throw 'encoder property anchor not found' }
$s = $s.Replace($old,$new)
Set-Content -LiteralPath $p -Value $s -Encoding UTF8
Write-Output 'ENCODER_PROPERTY_PATCHED'