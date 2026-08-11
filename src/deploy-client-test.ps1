# Deploy MediPro to Fly.io for client UAT.
# Usage: .\deploy-client-test.ps1 -AppName medipro-client-test
# Prerequisites: flyctl, Docker (optional), fly auth login, volume created once.

param(
    [Parameter(Mandatory = $true)]
    [string] $AppName,

    [switch] $SetSecrets,
    [string] $AdminPassword = '',
    [string] $StorePassword = '',
    [string] $JwtSigningKey = ''
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
Set-Location $root

if (-not (Get-Command fly -ErrorAction SilentlyContinue)) {
    Write-Error "flyctl not found. Install: https://fly.io/docs/hands-on/install-flyctl/"
}

# Sync app name in fly.toml
$flyToml = Join-Path $root 'fly.toml'
$content = Get-Content $flyToml -Raw
$content = $content -replace "app = '[^']*'", "app = '$AppName'"
Set-Content -Path $flyToml -Value $content -NoNewline

Write-Host "App: $AppName" -ForegroundColor Cyan

if ($SetSecrets) {
    if ([string]::IsNullOrWhiteSpace($JwtSigningKey)) {
        $chars = (48..57) + (65..90) + (97..122)
        $JwtSigningKey = -join (1..48 | ForEach-Object { [char]($chars | Get-Random) })
        Write-Host "Generated Jwt__SigningKey (save it securely)." -ForegroundColor Yellow
    }
    if ([string]::IsNullOrWhiteSpace($AdminPassword)) {
        $AdminPassword = Read-Host "DevSeed admin password (min 8 chars)"
    }
    if ([string]::IsNullOrWhiteSpace($StorePassword)) {
        $StorePassword = Read-Host "Demo store password (min 8 chars)"
    }
    fly secrets set `
        "Jwt__SigningKey=$JwtSigningKey" `
        "DevSeed__AdminPassword=$AdminPassword" `
        "DevSeed__DemoStorePassword=$StorePassword" `
        --app $AppName
}

Write-Host "Deploying..." -ForegroundColor Cyan
fly deploy --app $AppName

Write-Host ""
Write-Host "Done. Open: https://$AppName.fly.dev" -ForegroundColor Green
Write-Host "Health: https://$AppName.fly.dev/api/health"
Write-Host "Admin: admin@medipro.local (password from DevSeed__AdminPassword secret)"
