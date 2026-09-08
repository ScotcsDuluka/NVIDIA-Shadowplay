@echo off
rem ================================================================
rem  build-and-run.bat - build + run Duluka.Server in ONE shot.
rem  Rebuilds (Release) every time, then starts the server.
rem  Double-click or run from any working directory (spaces OK).
rem  Run with "public" to open the server to OTHER machines (LAN);
rem  see Duluka\README.md for the internet/TLS notes.
rem
rem  API    : http://127.0.0.1:5115   (default: this machine only)
rem  Admin  : http://127.0.0.1:5115/admin
rem  Stop   : Ctrl+C
rem ================================================================
setlocal
title Duluka.Server - build and run
cd /d "%~dp0"

rem ---- Optional: "public" opens the bind to other machines (LAN) ----
if /i not "%~1"=="public" goto bind_default
set "DULUKA_Urls=http://0.0.0.0:5115"
echo   PUBLIC MODE: other machines can connect (bind 0.0.0.0:5115).
echo   v0 is PLAIN HTTP - for internet use put a TLS proxy/tunnel in front.
echo   If clients still cannot connect, allow the port once (as admin):
echo     netsh advfirewall firewall add rule name="Duluka.Server 5115" dir=in action=allow protocol=TCP localport=5115
echo.
:bind_default

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

rem ---- Stop ALL leftover Duluka.Server instances before building ----
rem (a running instance holds port 5115 AND locks Duluka.Server.exe —
rem  the build cannot overwrite a running EXE, MSB3027. Killing only the
rem  port owner is not enough: a second instance that failed to bind can
rem  still hold the file lock, so kill by IMAGE name, all of them.)
set "KILLED="
taskkill /F /IM Duluka.Server.exe >nul 2>&1 && set "KILLED=1"
if defined KILLED (
    echo   [INFO] Stopped a running Duluka.Server instance before rebuild.
    timeout /t 2 /nobreak >nul
)

rem ---- Guard: port 5115 still taken = some OTHER program owns it ----
set "PORTBUSY="
for /f "tokens=5" %%p in ('netstat -aon ^| findstr /r /c:":5115 .*LISTENING"') do set "PORTBUSY=%%p"
if not defined PORTBUSY goto portok
echo   [ERROR] Port 5115 is in use by PID %PORTBUSY% (NOT Duluka.Server).
echo          Another program owns the port - close it or free the port,
echo          then run this script again.
echo.
pause
exit /b 1

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
rem      Sources, in order: Duluka\.server-secret (git-ignored) >
rem      one-time inline paste (saved to that file) > GitHub off.
set "DULUKA_GitHub__ClientSecret="
if not exist "%~dp0..\.server-secret" goto secret_ask
for /f "usebackq delims=" %%s in ("%~dp0..\.server-secret") do set "DULUKA_GitHub__ClientSecret=%%s"
if defined DULUKA_GitHub__ClientSecret (
    echo   GitHub client secret loaded from Duluka\.server-secret
    goto secret_done
)
:secret_ask
echo   Note: no GitHub client secret - GitHub sign-in is disabled.
echo         Email/password sign-in works fine.
echo         Get it: GitHub ^> Settings ^> Developer settings ^>
echo         GitHub Apps ^> "Duluka Shadow" ^> Generate a client secret
choice /c YN /m "   Paste the client secret now (saved locally, never committed)"
if errorlevel 2 goto secret_done
set "SECRET_INPUT="
set /p "SECRET_INPUT=   Paste the client secret and press Enter: "
if not defined SECRET_INPUT (
    echo   No secret entered - continuing WITHOUT GitHub sign-in.
    goto secret_done
)
> "%~dp0..\.server-secret" <nul set /p "=%SECRET_INPUT%"
set "DULUKA_GitHub__ClientSecret=%SECRET_INPUT%"
set "SECRET_INPUT="
echo   Saved to Duluka\.server-secret (git-ignored - it will not be committed).
:secret_done

set "EXE=%~dp0bin\Release\net10.0\Duluka.Server.exe"
if not exist "%EXE%" (
    echo   [ERROR] Build output not found: %EXE%
    echo           Try: dotnet build "%~dp0Duluka.Server.csproj" -c Release
    pause
    exit /b 1
)

echo.
echo   [2/2] Starting server (Ctrl+C to stop)...
echo         Bind    : %DULUKA_Urls%
echo         API     : http://127.0.0.1:5115
echo         Admin   : http://127.0.0.1:5115/admin
echo   ---------------------------------------------------------------
echo.
"%EXE%" %*

echo.
echo   Server stopped.
pause
