# PawNect - EF Core migration script
# Applies pending migrations to the database, or adds a new migration.
#
# Usage:
#   .\Run-Migrations.ps1                    # Apply all pending migrations
#   .\Run-Migrations.ps1 -Add "MigrationName" # Create a new migration
#   .\Run-Migrations.ps1 -List              # List migrations
#   .\Run-Migrations.ps1 -Script             # Generate SQL script (no apply)

param(
    [string]$Add = "",
    [switch]$List,
    [switch]$Script,
    [string]$ScriptOutput = ""
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
if (-not $root) { $root = Get-Location | Select-Object -ExpandProperty Path }

$infraPath = Join-Path $root "Backend\PawNect.Infrastructure"
$apiPath   = Join-Path $root "Backend\PawNect.API"

if (-not (Test-Path $infraPath)) {
    Write-Error "Infrastructure project not found at: $infraPath"
}
if (-not (Test-Path $apiPath)) {
    Write-Error "API startup project not found at: $apiPath"
}

# dotnet ef must run from a directory that contains a project or we pass --project/--startup-project.
# Run from solution root with explicit project paths (relative to root).
function Invoke-Ef {
    param([string[]]$Arguments)
    Set-Location -LiteralPath $root
    $allArgs = $Arguments + $efArgs
    & dotnet ef @allArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet ef failed with exit code $LASTEXITCODE" }
}

$efArgs = @(
    "--project", (Join-Path $root "Backend\PawNect.Infrastructure"),
    "--startup-project", (Join-Path $root "Backend\PawNect.API")
)

if ($List) {
    Write-Host "Listing migrations..." -ForegroundColor Cyan
    Invoke-Ef @("migrations", "list")
    exit 0
}

if ($Script) {
    $out = $ScriptOutput
    if (-not $out) { $out = Join-Path $root "Scripts\Migrations.sql" }
    $dir = Split-Path $out -Parent
    if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    Write-Host "Generating SQL script to: $out" -ForegroundColor Cyan
    Invoke-Ef @("migrations", "script", "-o", $out)
    Write-Host "Done. Review and run the script against your database if needed." -ForegroundColor Green
    exit 0
}

if ($Add) {
    $name = $Add.Trim()
    if (-not $name) {
        Write-Error "Migration name is required. Example: .\Run-Migrations.ps1 -Add 'AddLaboratoryRole'"
    }
    Write-Host "Adding migration: $name" -ForegroundColor Cyan
    Invoke-Ef @("migrations", "add", $name)
    Write-Host "Migration '$name' created. Run .\Run-Migrations.ps1 to apply it." -ForegroundColor Green
    exit 0
}

# Default: apply pending migrations
Write-Host "Applying pending migrations to database..." -ForegroundColor Cyan
Invoke-Ef @("database", "update")
Write-Host "Database is up to date." -ForegroundColor Green
