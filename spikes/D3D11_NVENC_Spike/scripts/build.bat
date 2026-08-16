@echo off
REM
REM build.bat — Build the D3D11/NVENC spike on Windows.
REM
REM Prerequisites:
REM   - .NET 8 SDK (x64): https://dotnet.microsoft.com/download/dotnet/8.0
REM   - Windows 10 1903+ (DXGI Desktop Duplication API requires DXGI 1.5+)
REM   - NVIDIA GPU with NVENC support
REM
REM Usage:
REM   scripts\build.bat            # Debug build
REM   scripts\build.bat Release    # Release build
REM

setlocal

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Debug

echo.
echo ============================================
echo  Building CaptureEngine.Video.Spike.D3D11
echo  Configuration: %CONFIG%
echo ============================================
echo.

REM Restore + build
dotnet restore "%~dp0\..\CaptureEngine.Video.Spike.D3D11.csproj"
if errorlevel 1 (
    echo.
    echo FAIL: dotnet restore failed.
    exit /b 1
)

dotnet build "%~dp0\..\CaptureEngine.Video.Spike.D3D11.csproj" -c %CONFIG% -p:Platform=x64
if errorlevel 1 (
    echo.
    echo FAIL: dotnet build failed.
    exit /b 1
)

echo.
echo ============================================
echo  Build SUCCESS
echo ============================================
echo  Output: %~dp0\..\bin\x64\%CONFIG%\net8.0-windows\
echo.
echo  Next steps:
echo    1. Copy nvEncodeAPI.dll from NVIDIA Video Codec SDK's Lib\x64\ folder
echo       to the output directory above (next to CaptureEngine.Video.Spike.D3D11.exe)
echo    2. Run scripts\run-all.bat
echo ============================================
echo.

endlocal
exit /b 0
