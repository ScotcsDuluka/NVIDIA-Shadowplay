@echo off
setlocal

if not defined CEF_ROOT if exist "C:\My Project\cef-sdk\cef73\Release\libcef.lib" set "CEF_ROOT=C:\My Project\cef-sdk\cef73"

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\Scripts\build-dev.ps1" %*
exit /b %ERRORLEVEL%
