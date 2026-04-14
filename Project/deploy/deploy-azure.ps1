<#
.SYNOPSIS
  Build and deploy PawNect to Azure App Service (Zip Deploy).
  URL layout: https://pawnect.azurewebsites.net (PetParent), /admin (AdminPortal), /api (API).

.PARAMETER PublishProfile
  Path to your .PublishSettings file (e.g. from Azure Portal download).

.PARAMETER Configuration
  Build configuration (default: Release).

.EXAMPLE
  .\deploy-azure.ps1 -PublishProfile "C:\Users\vinay\Downloads\Data\pawnect.PublishSettings"
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $PublishProfile,
    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$DeployRoot = Join-Path $PSScriptRoot "publish"
$WwwRoot = Join-Path $DeployRoot "wwwroot"
$ZipPath = Join-Path $DeployRoot "deploy.zip"

# Parse Zip Deploy publish profile
if (-not (Test-Path $PublishProfile)) { throw "Publish profile not found: $PublishProfile" }
[xml] $xml = Get-Content $PublishProfile -Raw
$zipProfile = $xml.publishData.publishProfile | Where-Object { $_.publishMethod -eq "ZipDeploy" } | Select-Object -First 1
if (-not $zipProfile) { throw "Zip Deploy profile not found in $PublishProfile" }
$publishUrl = $zipProfile.publishUrl
$userName = $zipProfile.userName
$userPWD = $zipProfile.userPWD
$scmHost = $publishUrl -replace ":443$", ""
Write-Host "Deploying to $scmHost (site: pawnect)" -ForegroundColor Cyan

# Clean and create layout
if (Test-Path $DeployRoot) { Remove-Item $DeployRoot -Recurse -Force }
New-Item -ItemType Directory -Path $WwwRoot -Force | Out-Null

# Publish each app
$apiDir = Join-Path $ProjectRoot "Backend\PawNect.API"
$petDir = Join-Path $ProjectRoot "Frontend\PawNect.PetParent.Web"
$adminDir = Join-Path $ProjectRoot "Frontend\PawNect.AdminPortal.Web"

Write-Host "Publishing PawNect.PetParent.Web (root)..." -ForegroundColor Yellow
dotnet publish (Join-Path $petDir "PawNect.PetParent.Web.csproj") -c $Configuration -o (Join-Path $WwwRoot "petparent") --no-self-contained /p:ExcludeReferencedProjectContent=true
if ($LASTEXITCODE -ne 0) { throw "Publish PetParent failed" }

Write-Host "Publishing PawNect.AdminPortal.Web (/admin)..." -ForegroundColor Yellow
dotnet publish (Join-Path $adminDir "PawNect.AdminPortal.Web.csproj") -c $Configuration -o (Join-Path $WwwRoot "admin") --no-self-contained /p:ExcludeReferencedProjectContent=true
if ($LASTEXITCODE -ne 0) { throw "Publish AdminPortal failed" }

Write-Host "Publishing PawNect.API (/api)..." -ForegroundColor Yellow
dotnet publish (Join-Path $apiDir "PawNect.API.csproj") -c $Configuration -o (Join-Path $WwwRoot "api") --no-self-contained
if ($LASTEXITCODE -ne 0) { throw "Publish API failed" }

# Layout: root = PetParent, admin = AdminPortal, api = API
$rootDest = $WwwRoot
$petSrc = Join-Path $WwwRoot "petparent"
Get-ChildItem $petSrc -Force | Move-Item -Destination $rootDest -Force
Remove-Item $petSrc -Force -ErrorAction SilentlyContinue

# Zip wwwroot contents (zip must contain files at root that go to wwwroot)
Write-Host "Creating deploy.zip..." -ForegroundColor Yellow
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($WwwRoot, $ZipPath)

# Zip Deploy API
$zipDeployUrl = "https://${scmHost}/api/zipdeploy"
$pair = "${userName}:${userPWD}"
$bytes = [System.Text.Encoding]::UTF8.GetBytes($pair)
$base64 = [Convert]::ToBase64String($bytes)
$headers = @{
    "Authorization" = "Basic $base64"
    "Content-Type"  = "application/zip"
}

Write-Host "Uploading to Azure Zip Deploy..." -ForegroundColor Yellow
Invoke-RestMethod -Uri $zipDeployUrl -Method POST -Headers $headers -InFile $ZipPath -TimeoutSec 600

Write-Host "Deploy completed. Cleaning up." -ForegroundColor Green
Remove-Item $ZipPath -Force -ErrorAction SilentlyContinue
# Optionally keep publish folder for inspection: Remove-Item $DeployRoot -Recurse -Force

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. In Azure Portal: App Service 'pawnect' -> Configuration -> Path mappings"
Write-Host "   Add: /admin -> site\wwwroot\admin (Application)"
Write-Host "   Add: /api    -> site\wwwroot\api  (Application)"
Write-Host "2. Set Application setting: ConnectionStrings__DefaultConnection (for API/SQL)"
Write-Host "3. Open https://pawnect.azurewebsites.net and https://pawnect.azurewebsites.net/admin"
Write-Host ""
