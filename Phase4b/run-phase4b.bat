@echo off
REM ============================================================================
REM run-phase4b.bat - deterministic Phase 4B A/B evidence runner (M2-W2)
REM
REM Runs ON1-3 (10ms timeline delay ON, production build) and OFF1-3
REM (timeline delay OFF, shadow no-delay build), collecting per run:
REM MP4 + runtime trace + config + manifest into evidence\phase4b\{ON,OFF}\runN
REM
REM Requirements: NVIDIA GPU machine (ddagrab pins vendor 0x10DE + native NVENC),
REM .NET SDK (dotnet on PATH). Duration/FPS fixed inside phase4b-harness.ps1
REM (defaults 3s / 60fps, same for every run).
REM
REM Never commits. Production sources are never modified (OFF arm builds a
REM shadow copy under %TEMP%; production file hash is verified unchanged).
REM ============================================================================
setlocal
set REPO=%~dp0..
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0phase4b-harness.ps1" -Mode ALL -RunsPerMode 3 %*
echo.
echo Exit code: %ERRORLEVEL%
exit /b %ERRORLEVEL%
