# MediPro Frontend Startup Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Starting MediPro Web (React + Vite)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get the script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

Write-Host "Frontend URL: http://localhost:5173" -ForegroundColor Yellow
Write-Host "API URL: http://localhost:5020" -ForegroundColor Yellow
Write-Host ""

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies..." -ForegroundColor Green
    npm install
    Write-Host ""
}

Write-Host "Starting development server..." -ForegroundColor Green
Write-Host "Press Ctrl+C to stop" -ForegroundColor Gray
Write-Host ""

# Start the development server
npm run dev
