from pathlib import Path
p = Path(r'C:\My Project\NVIDIA-Shadowplay\CaptureEngine.FFmpegBackend\AudioTap.vb')
s = p.read_text(encoding='utf-8-sig')
old = """        Public Sub FinalizeToNow()\n            If _originTicks = 0 Then"""
new = """        Public Sub FinalizeToNow()\n            FinalizeToTicks(Stopwatch.GetTimestamp())\n        End Sub\n\n        ''' <summary>\n        ''' Close the timeline at an immutable session-stop QPC tick.\n        ''' </summary>\n        Public Sub FinalizeToTicks(targetTicks As Long)\n            If targetTicks <= 0 Then targetTicks = Stopwatch.GetTimestamp()\n            If _originTicks = 0 Then"""
assert old in s
s = s.replace(old, new, 1)
s = s.replace('(Stopwatch.GetTimestamp() - _startRequestedTicks)', '(targetTicks - _startRequestedTicks)', 1)
s = s.replace('(Stopwatch.GetTimestamp() - _lastTicks)', '(targetTicks - _lastTicks)', 1)
p.write_text(s, encoding='utf-8')
