# MediPro API — use when `dotnet run` crashes (CET/shadow-stack assert on some Windows 10 builds).
# From PowerShell:  cd B:\MediPro\src   .\run-api.ps1

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\MediPro.Api

$env:ASPNETCORE_URLS = "http://localhost:5020"
if (-not $env:ASPNETCORE_ENVIRONMENT) { $env:ASPNETCORE_ENVIRONMENT = "Development" }

dotnet build -c Debug -v minimal | Out-Null
Write-Host "Starting MediPro.Api via dotnet exec (CET workaround) on $env:ASPNETCORE_URLS ..."
dotnet exec ".\bin\Debug\net9.0\MediPro.Api.dll"
