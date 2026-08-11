# MediPro - Master Startup Script
# This script starts both Backend API and Frontend Web in separate windows

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MediPro - Starting Full Stack" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiPath = Join-Path $scriptPath "src\MediPro.Api"
$webPath = Join-Path $scriptPath "src\MediPro.Web"

# Check if paths exist
if (-not (Test-Path $apiPath)) {
    Write-Host "Error: API path not found: $apiPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $webPath)) {
    Write-Host "Error: Web path not found: $webPath" -ForegroundColor Red
    exit 1
}

Write-Host "Starting Backend API..." -ForegroundColor Green
Write-Host "  Location: $apiPath" -ForegroundColor Gray
Write-Host "  URL: http://localhost:5020" -ForegroundColor Yellow
Write-Host ""

# Start Backend API in a new PowerShell window
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "& { Set-Location '$apiPath'; .\start-api.ps1 }"
)

# Wait a moment before starting frontend
Start-Sleep -Seconds 2

Write-Host "Starting Frontend Web..." -ForegroundColor Green
Write-Host "  Location: $webPath" -ForegroundColor Gray
Write-Host "  URL: http://localhost:5173" -ForegroundColor Yellow
Write-Host ""

# Start Frontend Web in a new PowerShell window
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "& { Set-Location '$webPath'; .\start-web.ps1 }"
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Both services are starting!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Backend API:     http://localhost:5020" -ForegroundColor Yellow
Write-Host "Frontend Web:    http://localhost:5173" -ForegroundColor Yellow
Write-Host ""
Write-Host "Login Credentials:" -ForegroundColor Cyan
Write-Host "  Email:    admin@medipro.local" -ForegroundColor White
Write-Host "  Password: ChangeMe!12345" -ForegroundColor White
Write-Host ""
Write-Host "Two PowerShell windows have been opened." -ForegroundColor Gray
Write-Host "Close those windows or press Ctrl+C to stop the services." -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit this window..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
