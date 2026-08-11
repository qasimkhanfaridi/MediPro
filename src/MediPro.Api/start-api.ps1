# MediPro API Startup Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Starting MediPro API (.NET 9)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables to avoid CET/shadow stack issues
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_EnableWriteXorExecute = "0"
$env:COMPlus_EnableWriteXorExecute = "0"

# Get the script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

Write-Host "Environment: $env:ASPNETCORE_ENVIRONMENT" -ForegroundColor Yellow
Write-Host "API URL: http://localhost:5020" -ForegroundColor Yellow
Write-Host ""
Write-Host "Building project..." -ForegroundColor Green

# Build the project
dotnet build --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Starting API server..." -ForegroundColor Green
    Write-Host "Press Ctrl+C to stop" -ForegroundColor Gray
    Write-Host ""
    
    # Run using the DLL directly (avoids apphost exe issues)
    dotnet "bin\Debug\net9.0\MediPro.Api.dll" --urls "http://localhost:5020"
} else {
    Write-Host "Build failed! Please check the errors above." -ForegroundColor Red
    Read-Host "Press Enter to exit"
}
