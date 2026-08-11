@echo off
echo ========================================
echo   MediPro - Starting Full Stack
echo ========================================
echo.

REM Get the directory where this batch file is located
set "BASE_DIR=%~dp0"

echo Starting Backend API...
echo   URL: http://localhost:5020
start "MediPro API" powershell -NoExit -ExecutionPolicy Bypass -File "%BASE_DIR%src\MediPro.Api\start-api.ps1"

timeout /t 3 /nobreak >nul

echo.
echo Starting Frontend Web...
echo   URL: http://localhost:5173
start "MediPro Web" powershell -NoExit -ExecutionPolicy Bypass -File "%BASE_DIR%src\MediPro.Web\start-web.ps1"

echo.
echo ========================================
echo   Both services are starting!
echo ========================================
echo.
echo Backend API:     http://localhost:5020
echo Frontend Web:    http://localhost:5173
echo.
echo Login Credentials:
echo   Email:    admin@medipro.local
echo   Password: ChangeMe!12345
echo.
echo Two PowerShell windows have been opened.
echo Close those windows to stop the services.
echo.
pause
