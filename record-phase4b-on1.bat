@echo off
setlocal enabledelayedexpansion

echo =============================================
echo Phase 4B - ON1 Recording (10ms delay ON - Baseline)
echo =============================================
echo.

set OUTPUT_FILE=phase4b-on1.mp4
set LOG_FILE=phase4b-on1.log
set EVIDENCE_FILE=phase4b-on1-evidence.md

echo Starting ON1 recording...
echo Output file: %OUTPUT_FILE%
echo Log file: %LOG_FILE%
echo Evidence file: %EVIDENCE_FILE%
echo.

dotnet run --project CaptureEngine.Recording.ConsoleDriver -- --duration 3 --output "%OUTPUT_FILE%" --log "%LOG_FILE%" --evidence "%EVIDENCE_FILE%"

echo.
echo ON1 recording completed.
echo =============================================
echo.