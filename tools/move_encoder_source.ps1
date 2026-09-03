$p = 'C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Encoder.Nvenc\NvencEncoderBackend.vb'
$q = 'C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Encoder.Nvenc\NvencEncoderBackend.patchsrc.vb'
Move-Item -LiteralPath $p -Destination $q -Force
Move-Item -LiteralPath $q -Destination $p -Force
Write-Output 'MOVE_SOURCE_OK'