# MediPro - Test & Verify Running Status
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MediPro - Status Check" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

function Test-ServiceRunning {
    param(
        [string]$ServiceName,
        [int]$Port,
        [string]$Url
    )
    
    Write-Host "Checking $ServiceName..." -ForegroundColor Yellow
    
    # Check if port is listening
    $portOpen = $false
    try {
        $connection = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
        $portOpen = $connection -ne $null
    } catch {
        $portOpen = $false
    }
    
    if ($portOpen) {
        Write-Host "  ? Port $Port is LISTENING" -ForegroundColor Green
        
        # Try HTTP request
        try {
            $response = Invoke-WebRequest -Uri $Url -TimeoutSec 5 -ErrorAction Stop
            Write-Host "  ? HTTP request successful (Status: $($response.StatusCode))" -ForegroundColor Green
            Write-Host "  ? $ServiceName is RUNNING" -ForegroundColor Green
            return $true
        } catch {
            Write-Host "  ? Port is open but HTTP request failed: $($_.Exception.Message)" -ForegroundColor Yellow
            return $false
        }
    } else {
        Write-Host "  ? Port $Port is NOT listening" -ForegroundColor Red
        Write-Host "  ? $ServiceName is NOT RUNNING" -ForegroundColor Red
        return $false
    }
}

# Check Backend API
Write-Host ""
$apiRunning = Test-ServiceRunning -ServiceName "Backend API" -Port 5020 -Url "http://localhost:5020/api/health"

Write-Host ""
Write-Host "---" -ForegroundColor Gray
Write-Host ""

# Check Frontend Web
$webRunning = Test-ServiceRunning -ServiceName "Frontend Web" -Port 5173 -Url "http://localhost:5173"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($apiRunning -and $webRunning) {
    Write-Host "? Both services are RUNNING!" -ForegroundColor Green
    Write-Host ""
    Write-Host "You can access:" -ForegroundColor White
    Write-Host "  Backend API:  http://localhost:5020" -ForegroundColor Yellow
    Write-Host "  Frontend Web: http://localhost:5173" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Login credentials:" -ForegroundColor White
    Write-Host "  Email:    admin@medipro.local" -ForegroundColor Cyan
    Write-Host "  Password: ChangeMe!12345" -ForegroundColor Cyan
} elseif ($apiRunning -or $webRunning) {
    Write-Host "? Only some services are running:" -ForegroundColor Yellow
    if ($apiRunning) {
        Write-Host "  ? Backend API is running" -ForegroundColor Green
    } else {
        Write-Host "  ? Backend API is NOT running" -ForegroundColor Red
    }
    if ($webRunning) {
        Write-Host "  ? Frontend Web is running" -ForegroundColor Green
    } else {
        Write-Host "  ? Frontend Web is NOT running" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "To start missing services:" -ForegroundColor White
    if (-not $apiRunning) {
        Write-Host "  Backend:  cd B:\MediPro\src\MediPro.Api; .\start-api.ps1" -ForegroundColor Gray
    }
    if (-not $webRunning) {
        Write-Host "  Frontend: cd B:\MediPro\src\MediPro.Web; .\start-web.ps1" -ForegroundColor Gray
    }
} else {
    Write-Host "? NO services are running" -ForegroundColor Red
    Write-Host ""
    Write-Host "To start both services, run ONE of these:" -ForegroundColor White
    Write-Host "  1. Double-click: start-medipro.bat" -ForegroundColor Yellow
    Write-Host "  2. Run: .\start-medipro.ps1" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Or start them manually in separate terminals:" -ForegroundColor White
    Write-Host "  Backend:  cd B:\MediPro\src\MediPro.Api; .\start-api.ps1" -ForegroundColor Gray
    Write-Host "  Frontend: cd B:\MediPro\src\MediPro.Web; .\start-web.ps1" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
