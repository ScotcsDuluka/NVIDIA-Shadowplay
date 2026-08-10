========================================
  ShadowPlay CPU Encoder Test Script
========================================

WHAT YOU NEED:
1. FFmpeg (any build with libx264/libx265/svt_av1)
   - Standard build from gyan.dev or BtbQ works fine
   - No special GPU build needed

2. Any CPU (no GPU required!)

3. PowerShell (any version - included in Windows)


HOW TO RUN:
-----------
Option A - FFmpeg in same folder as script:
  powershell -ExecutionPolicy Bypass -File ".\test_cpu.ps1"

Option B - Specify FFmpeg path:
  powershell -ExecutionPolicy Bypass -File ".\test_cpu.ps1" -FFmpegPath "C:\path\to\ffmpeg.exe"

Option C - Quick test (fewer tests, ~5 min):
  powershell -ExecutionPolicy Bypass -File ".\test_cpu.ps1" -Quick

Option D - Shorter duration (5 sec per test):
  powershell -ExecutionPolicy Bypass -File ".\test_cpu.ps1" -Duration 5

Combine: Quick + short duration:
  powershell -ExecutionPolicy Bypass -File ".\test_cpu.ps1" -Quick -Duration 5


WHAT IT TESTS (20 groups):
--------------------------
  Group 1:  libx264 capture methods (gdigrab, ddagrab, gfxcapture)
  Group 2:  libx264 CRF quality levels (18-30)
  Group 3:  libx264 presets (ultrafast to veryslow, 9 levels)
  Group 4:  libx264 CBR mode (5M-50M bitrate)
  Group 5:  libx264 tune options (film, animation, game, zerolatency...)
  Group 6:  libx264 pixel formats (yuv420p, nv12, yuvj420p, 10-bit)
  Group 7:  libx264 FPS and scale
  Group 8:  libx265 capture methods
  Group 9:  libx265 CRF quality levels
  Group 10: libx265 presets
  Group 11: libx265 CBR mode
  Group 12: libx265 10-bit encoding
  Group 13: libx265 tune options
  Group 14: svt_av1 capture methods
  Group 15: svt_av1 CRF quality levels
  Group 16: svt_av1 speed presets (0-13)
  Group 17: svt_av1 CBR mode
  Group 18: svt_av1 10-bit encoding
  Group 19: libx264 game capture profiles (zerolatency)
  Group 20: Best config stress tests


OUTPUT FILES:
-------------
  CPU_TestResults.csv  - All test results in CSV format
  Logs\                - FFmpeg log files for each test
  CPU_Tests\           - Temporary video files (auto-deleted)


WHAT TO SEND BACK:
------------------
  1. Screenshot of the summary at the end (PASS/FAIL counts)
  2. The file: CPU_TestResults.csv
  3. If any tests FAIL: screenshot of the FAIL lines


IMPORTANT NOTES:
-----------------
  - CPU encoding is MUCH slower than GPU encoding
  - Each test takes ~8 seconds by default
  - Full test suite (no Quick): ~60+ tests = ~8-15 minutes
  - Quick mode: ~25 tests = ~3-5 minutes
  - libx265 tests are slower than libx264
  - svt_av1 is faster than libaom-av1 (auto-selected)
  - 10-bit tests on libx264 may FAIL (expected - x264 is 8-bit only)


TROUBLESHOOTING:
-----------------
  "libx264 NOT FOUND" -> Your FFmpeg build lacks libx264 (unlikely)
  "ffmpeg.exe not found" -> Use -FFmpegPath with full path
  "Access denied" -> Run as Administrator
