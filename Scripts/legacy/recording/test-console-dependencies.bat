@echo off
echo Testing console driver dependencies...
echo.

cd "C:\My Project\NVIDIA-Shadowplay\CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows"

echo Checking .NET runtime...
dotnet --version

echo.
echo Checking if executable exists...
if exist "CaptureEngine.Recording.ConsoleDriver.exe" (
    echo Executable exists
    echo.
    echo Testing minimal command...
    echo.
    echo Command: .\CaptureEngine.Recording.ConsoleDriver.exe --help
    echo.
    .\CaptureEngine.Recording.ConsoleDriver.exe --help
    echo.
    echo Exit code: %ERRORLEVEL%
) else (
    echo Executable not found
)

echo.
echo Listing directory contents...
dir *.exe *.dll *.config