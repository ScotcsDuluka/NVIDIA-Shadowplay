@echo off
setlocal enabledelayedexpansion

echo =============================================
echo Phase 4B - OFF1 Recording (10ms delay OFF)
echo =============================================
echo.

set OUTPUT_FILE=phase4b-off1.mp4
set LOG_FILE=phase4b-off1.log
set EVIDENCE_FILE=phase4b-off1-evidence.md

echo Starting OFF1 recording...
echo Output file: %OUTPUT_FILE%
echo Log file: %LOG_FILE%
echo Evidence file: %EVIDENCE_FILE%
echo.

dotnet run --project CaptureEngine.Recording.ConsoleDriver -- --duration 3 --output "%OUTPUT_FILE%" --log "%LOG_FILE%" --evidence "%EVIDENCE_FILE%" --no-delay

echo.
echo OFF1 recording completed.
echo =============================================
echo.