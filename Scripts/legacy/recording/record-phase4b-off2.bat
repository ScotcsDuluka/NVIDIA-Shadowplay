@echo off
setlocal enabledelayedexpansion

echo =============================================
echo Phase 4B - OFF2 Recording (10ms delay OFF)
echo =============================================
echo.

set OUTPUT_FILE=phase4b-off2.mp4
set LOG_FILE=phase4b-off2.log
set EVIDENCE_FILE=phase4b-off2-evidence.md

echo Starting OFF2 recording...
echo Output file: %OUTPUT_FILE%
echo Log file: %LOG_FILE%
echo Evidence file: %EVIDENCE_FILE%
echo.

dotnet run --project CaptureEngine.Recording.ConsoleDriver -- --duration 3 --output "%OUTPUT_FILE%" --log "%LOG_FILE%" --evidence "%EVIDENCE_FILE%" --no-delay

echo.
echo OFF2 recording completed.
echo =============================================
echo.