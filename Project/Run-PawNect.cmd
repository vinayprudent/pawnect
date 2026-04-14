@echo off
REM PawNect - Double-click or run from cmd to start API + Pet Parent + Admin
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -NoProfile -File "%~dp0Run-PawNect.ps1"
pause
