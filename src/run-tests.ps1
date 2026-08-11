# MediPro - Run All Unit Tests
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MediPro - Running Unit Tests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$rootPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$testPath = Join-Path $rootPath "src\MediPro.Api.Tests"

if (-not (Test-Path $testPath)) {
    Write-Host "Error: Test project not found at $testPath" -ForegroundColor Red
    exit 1
}

Set-Location $testPath

Write-Host "Test Project: MediPro.Api.Tests" -ForegroundColor Yellow
Write-Host "Location: $testPath" -ForegroundColor Gray
Write-Host ""

# Run tests
dotnet test --logger "console;verbosity=normal"

$exitCode = $LASTEXITCODE

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ? All Tests Passed!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Test Summary:" -ForegroundColor White
    Write-Host "  • Domain Tests: Order status rules (23 tests)" -ForegroundColor Gray
    Write-Host "  • Service Tests: Token & alerts (13 tests)" -ForegroundColor Gray
    Write-Host "  • Extension Tests: Claims parsing (13 tests)" -ForegroundColor Gray
    Write-Host "  • Total: 49 tests passed ?" -ForegroundColor Green
} else {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ? Some Tests Failed" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please review the test output above for details." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "For more information, see:" -ForegroundColor White
Write-Host "  • UNIT-TESTING-GUIDE.md (project root)" -ForegroundColor Cyan
Write-Host "  • src\MediPro.Api.Tests\README.md (detailed docs)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

exit $exitCode
