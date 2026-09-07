@echo off
rem ================================================================
rem  build-and-run.bat - build + run Duluka.Server in ONE shot.
rem  Rebuilds (Release) every time, then starts the server.
rem  Double-click or run from any working directory (spaces OK).
rem
rem  API    : http://127.0.0.1:5115
rem  Admin  : http://127.0.0.1:5115/admin
rem  Stop   : Ctrl+C
rem ================================================================
setlocal
title Duluka.Server - build and run
cd /d "%~dp0"

echo.
echo   ================================================
echo    Duluka.Server : Build + Run (one shot)
echo   ================================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo   [ERROR] "dotnet" not found in PATH.
    echo           Install the .NET SDK: https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

rem ---- Guard: port 5115 already taken = old instance still running ----
rem (starting a second copy would die with "address already in use")
set "PORTBUSY="
for /f "tokens=5" %%p in ('netstat -aon ^| findstr /r /c:":5115 .*LISTENING"') do set "PORTBUSY=%%p"
if not defined PORTBUSY goto portok
echo   [WARN] Port 5115 is already in use by PID %PORTBUSY%.
echo          An old Duluka.Server instance is probably still running.
echo          Keep it and the new one cannot bind the port.
echo.
choice /c YN /m "   Kill PID %PORTBUSY% and continue"
if errorlevel 2 exit /b 1
taskkill /F /PID %PORTBUSY% >nul 2>&1
timeout /t 1 /nobreak >nul

:portok
echo   [1/2] Building (Release)...
dotnet build "%~dp0Duluka.Server.csproj" -c Release --nologo -v m
if errorlevel 1 (
    echo.
    echo   [ERROR] Build failed - see errors above.
    echo.
    pause
    exit /b 1
)

rem ---- Optional: GitHub OAuth client secret (only needed for GitHub
rem      sign-in; email/password sign-in works without it) ----
set "DULUKA_GitHub__ClientSecret="
if exist "%~dp0..\.server-secret" for /f "usebackq delims=" %%s in ("%~dp0..\.server-secret") do set "DULUKA_GitHub__ClientSecret=%%s"
if defined DULUKA_GitHub__ClientSecret (
    echo   GitHub client secret loaded from Duluka\.server-secret
) else (
    echo   Note: no GitHub client secret - GitHub sign-in is disabled.
    echo         Email/password sign-in works fine. To enable GitHub sign-in,
    echo         run Duluka\run-server.cmd once and paste the secret.
)

set "EXE=%~dp0bin\Release\net10.0\Duluka.Server.exe"
if not exist "%EXE%" (
    echo   [ERROR] Build output not found: %EXE%
    echo           Try: dotnet build "%~dp0Duluka.Server.csproj" -c Release
    pause
    exit /b 1
)

echo.
echo   [2/2] Starting server (Ctrl+C to stop)...
echo         API     : http://127.0.0.1:5115
echo         Admin   : http://127.0.0.1:5115/admin
echo   ---------------------------------------------------------------
echo.
"%EXE%" %*

echo.
echo   Server stopped.
pause
