@echo off
echo NVIDIA ShadowPlay Forensic Timing Analysis
echo ======================================

set EVIDENCE_DIR=C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows\evidence
set OUTPUT_DIR=C:\My Project\NVIDIA-Shadowplay\test-recordings
set ANALYSIS_FILE=%OUTPUT_DIR%\forensic-analysis.txt

if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo Extracting timing data from evidence files...
echo.

echo Timing Analysis Report > "%ANALYSIS_FILE%"
echo Date: %DATE% %TIME% >> "%ANALYSIS_FILE%"
echo ====================================== >> "%ANALYSIS_FILE%"
echo. >> "%ANALYSIS_FILE%"

for %%f in ("%EVIDENCE_DIR%\phase-12b-validation-*.md") do (
    echo Processing: %%~nxf
    echo Processing: %%~nxf >> "%ANALYSIS_FILE%"
    echo ====================================== >> "%ANALYSIS_FILE%"
    
    REM Extract timing data from the evidence file
    findstr /C:"selectedTs=" "%%f" >> "%ANALYSIS_FILE%"
    findstr /C:"selectedLag=" "%%f" >> "%ANALYSIS_FILE%"
    findstr /C:"selectedTick=" "%%f" >> "%ANALYSIS_FILE%"
    findstr /C:"muxFeedTick=" "%%f" >> "%ANALYSIS_FILE%"
    findstr /C:"cfrTick=" "%%f" >> "%ANALYSIS_FILE%"
    findstr /C:"_timelineStartTicks=" "%%f" >> "%ANALYSIS_FILE%"
    echo. >> "%ANALYSIS_FILE%"
)

echo Analysis complete. Results saved to: %ANALYSIS_FILE%
echo.