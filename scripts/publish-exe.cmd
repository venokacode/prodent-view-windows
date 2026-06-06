@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish-exe.ps1" %*
exit /b %ERRORLEVEL%
