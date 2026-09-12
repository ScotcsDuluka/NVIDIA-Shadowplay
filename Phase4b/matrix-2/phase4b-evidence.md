# Phase 4B — Raw Run Evidence

- date: 2026-09-11 23:03:08
- machine: MATEBOOK-HUAWEI · Microsoft Windows NT 10.0.26300.0
- stopwatch frequency: 10000000 Hz
- args: arm=both runs=3 duration=3s fps=60 warmupMs=28 frameMs=16.667 t0Mode=both backend=synthetic
- ffmpeg: `C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe`
- production HEAD: cca2b2d (Engine-Rebuild-Stabilization) — production sources untouched

### run arm-on-1
- pass=True framesEncoded=181 (captured=197, dup=0) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.016727s packets=181 size=118183B
- log: phase4b-arm-on1.log

### run arm-on10-1
- pass=True framesEncoded=180 (captured=191, dup=1) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-arm-on101.log

### run arm-off-1
- pass=True framesEncoded=180 (captured=190, dup=2) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-arm-off1.log

### run arm-on-2
- pass=True framesEncoded=181 (captured=196, dup=0) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.016727s packets=181 size=118183B
- log: phase4b-arm-on2.log

### run arm-on10-2
- pass=True framesEncoded=180 (captured=191, dup=1) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-arm-on102.log

### run arm-off-2
- pass=True framesEncoded=180 (captured=190, dup=2) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-arm-off2.log

### run arm-on-3
- pass=True framesEncoded=181 (captured=197, dup=0) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.016727s packets=181 size=118183B
- log: phase4b-arm-on3.log

### run arm-on10-3
- pass=True framesEncoded=180 (captured=191, dup=1) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-arm-on103.log

### run arm-off-3
- pass=True framesEncoded=180 (captured=192, dup=2) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-arm-off3.log

### run loop-on-1
- pass=True framesEncoded=181 (captured=196, dup=0) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.016727s packets=181 size=118183B
- log: phase4b-loop-on1.log

### run loop-on10-1
- pass=True framesEncoded=180 (captured=193, dup=1) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-loop-on101.log

### run loop-off-1
- pass=True framesEncoded=180 (captured=191, dup=1) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-loop-off1.log

### run loop-on-2
- pass=True framesEncoded=181 (captured=199, dup=0) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.016727s packets=181 size=118183B
- log: phase4b-loop-on2.log

### run loop-on10-2
- pass=True framesEncoded=180 (captured=192, dup=1) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-loop-on102.log

### run loop-off-2
- pass=True framesEncoded=180 (captured=191, dup=1) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-loop-off2.log

### run loop-on-3
- pass=True framesEncoded=181 (captured=198, dup=0) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.016727s packets=181 size=118183B
- log: phase4b-loop-on3.log

### run loop-on10-3
- pass=True framesEncoded=180 (captured=189, dup=5) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-loop-on103.log

### run loop-off-3
- pass=True framesEncoded=180 (captured=190, dup=1) nvencErrors=0
- mp4: first_video_pts=0.000000s start_time=0.000000s duration=3.000060s packets=180 size=117541B
- log: phase4b-loop-off3.log

