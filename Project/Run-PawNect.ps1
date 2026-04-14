# PawNect - Build and run API + Pet Parent + Admin in one window with 3 tabs (Windows Terminal)
# Usage: .\Run-PawNect.ps1   or   powershell -ExecutionPolicy Bypass -File .\Run-PawNect.ps1

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
if (-not $root) { $root = Get-Location | Select-Object -ExpandProperty Path }

Write-Host "PawNect - Building solution..." -ForegroundColor Cyan
Push-Location $root
try {
    dotnet build PawNect.sln -c Debug -nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "Build failed." }
}
finally {
    Pop-Location
}

$apiDir    = Join-Path $root "Backend\PawNect.API"
$parentDir = Join-Path $root "Frontend\PawNect.PetParent.Web"
$adminDir  = Join-Path $root "Frontend\PawNect.AdminPortal.Web"

# Prefer Windows Terminal (one window, 3 tabs); fall back to 3 separate windows
$wt = Get-Command wt -ErrorAction SilentlyContinue
if ($wt) {
    Write-Host "Starting applications in one window with 3 tabs (Windows Terminal)..." -ForegroundColor Cyan
    # No semicolons inside -Command values, or wt treats them as tab separators and creates 6 tabs
    $apiCmd    = "dotnet run --no-build --urls 'http://localhost:5000'"
    $parentCmd = "dotnet run --no-build --urls 'http://localhost:5100'"
    $adminCmd  = "dotnet run --no-build --urls 'http://localhost:5200'"
    $wtCmd = "-d `"$apiDir`" --title `"PawNect API`" powershell -NoExit -Command `"$apiCmd`" ; new-tab -d `"$parentDir`" --title `"Pet Parent`" powershell -NoExit -Command `"$parentCmd`" ; new-tab -d `"$adminDir`" --title `"Admin Portal`" powershell -NoExit -Command `"$adminCmd`""
    Start-Process -FilePath "wt" -ArgumentList $wtCmd
} else {
    Write-Host "Windows Terminal (wt) not found. Starting 3 separate windows..." -ForegroundColor Yellow
    Write-Host "Install Windows Terminal from Microsoft Store for a single window with tabs." -ForegroundColor Gray
    Start-Process powershell -ArgumentList @(
        "-NoExit", "-Command",
        "Set-Location '$apiDir'; Write-Host 'PawNect API - http://localhost:5000' -ForegroundColor Green; dotnet run --no-build --urls 'http://localhost:5000'"
    ) -WindowStyle Normal
    Start-Sleep -Seconds 1
    Start-Process powershell -ArgumentList @(
        "-NoExit", "-Command",
        "Set-Location '$parentDir'; Write-Host 'Pet Parent - http://localhost:5100' -ForegroundColor Green; dotnet run --no-build --urls 'http://localhost:5100'"
    ) -WindowStyle Normal
    Start-Sleep -Seconds 1
    Start-Process powershell -ArgumentList @(
        "-NoExit", "-Command",
        "Set-Location '$adminDir'; Write-Host 'Admin Portal - http://localhost:5200' -ForegroundColor Green; dotnet run --no-build --urls 'http://localhost:5200'"
    ) -WindowStyle Normal
}

Write-Host ""
Write-Host "PawNect is starting. Open in browser:" -ForegroundColor Green
Write-Host "  API (Swagger):  http://localhost:5000/swagger"
Write-Host "  Pet Parent:     http://localhost:5100"
Write-Host "  Admin Portal:   http://localhost:5200"
Write-Host ""
Write-Host "Close a tab (or window) to stop that application." -ForegroundColor Gray
