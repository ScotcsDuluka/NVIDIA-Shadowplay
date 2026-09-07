@echo off
rem run-server.cmd — double-click launcher for Duluka.Server (works from any CWD).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-server.ps1" %*
pause
