@echo off
setlocal enabledelayedexpansion

echo =============================================
echo Phase 4B - OFF3 Recording (10ms delay OFF)
echo =============================================
echo.

set OUTPUT_FILE=phase4b-off3.mp4
set LOG_FILE=phase4b-off3.log
set EVIDENCE_FILE=phase4b-off3-evidence.md

echo Starting OFF3 recording...
echo Output file: %OUTPUT_FILE%
echo Log file: %LOG_FILE%
echo Evidence file: %EVIDENCE_FILE%
echo.

dotnet run --project CaptureEngine.Recording.ConsoleDriver -- --duration 3 --output "%OUTPUT_FILE%" --log "%LOG_FILE%" --evidence "%EVIDENCE_FILE%" --no-delay

echo.
echo OFF3 recording completed.
echo =============================================
echo.