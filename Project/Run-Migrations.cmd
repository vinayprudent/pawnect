@echo off
REM PawNect - Run EF Core migrations (apply pending)
REM For other options use PowerShell: .\Run-Migrations.ps1 -List  or  -Add "Name"  or  -Script

cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Run-Migrations.ps1" %*
exit /b %ERRORLEVEL%
