# Enhanced YouTube Downloader - Build Installer Script
# This PowerShell script builds the application and creates a Windows installer using Inno Setup

param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Enhanced YouTube Downloader - Build Installer" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Check if Inno Setup is installed
$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $InnoSetupPath)) {
    Write-Host "ERROR: Inno Setup 6 not found at: $InnoSetupPath" -ForegroundColor Red
    Write-Host "Please download and install Inno Setup from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found Inno Setup at: $InnoSetupPath" -ForegroundColor Green
Write-Host ""

# Define paths
$ProjectRoot = $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "src\Desktop\EnhancedYoutubeDownloader.csproj"
$PublishDir = Join-Path $ProjectRoot "src\Desktop\bin\$Configuration\net9.0\win-x64\publish"
$ReleaseDir = Join-Path $ProjectRoot "release"
$InnoSetupScript = Join-Path $ProjectRoot "setup.iss"

# Step 1: Clean previous builds
Write-Host "[1/5] Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
    Write-Host "  - Removed publish directory" -ForegroundColor Gray
}
if (Test-Path $ReleaseDir) {
    Remove-Item $ReleaseDir -Recurse -Force
    Write-Host "  - Removed release directory" -ForegroundColor Gray
}
Write-Host "  Clean completed" -ForegroundColor Green
Write-Host ""

# Step 2: Restore dependencies
Write-Host "[2/5] Restoring dependencies..." -ForegroundColor Yellow
dotnet restore $ProjectFile
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to restore dependencies" -ForegroundColor Red
    exit 1
}
Write-Host "  Dependencies restored" -ForegroundColor Green
Write-Host ""

# Step 3: Publish self-contained application
Write-Host "[3/5] Publishing self-contained application for win-x64..." -ForegroundColor Yellow
dotnet publish $ProjectFile `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $PublishDir `
    /p:PublishSingleFile=false `
    /p:PublishTrimmed=false `
    /p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to publish application" -ForegroundColor Red
    exit 1
}
Write-Host "  Application published to: $PublishDir" -ForegroundColor Green
Write-Host ""

# Step 4: Verify FFmpeg is included
Write-Host "[4/5] Verifying FFmpeg..." -ForegroundColor Yellow
$FFmpegPath = Join-Path $PublishDir "ffmpeg.exe"
if (Test-Path $FFmpegPath) {
    $FFmpegSize = (Get-Item $FFmpegPath).Length / 1MB
    Write-Host "  - FFmpeg found: $([math]::Round($FFmpegSize, 2)) MB" -ForegroundColor Green
} else {
    Write-Host "  WARNING: FFmpeg not found in publish directory" -ForegroundColor Yellow
    Write-Host "  FFmpeg will be downloaded on first run" -ForegroundColor Yellow
}
Write-Host ""

# Step 5: Build installer with Inno Setup
Write-Host "[5/5] Building installer with Inno Setup..." -ForegroundColor Yellow
Write-Host "  Using script: $InnoSetupScript" -ForegroundColor Gray

# Update version in setup.iss if specified
if ($Version -ne "1.0.0") {
    Write-Host "  Updating version to: $Version" -ForegroundColor Gray
    $SetupContent = Get-Content $InnoSetupScript -Raw
    $SetupContent = $SetupContent -replace '#define MyAppVersion ".*"', "#define MyAppVersion `"$Version`""
    Set-Content $InnoSetupScript -Value $SetupContent
}

# Run Inno Setup compiler
& $InnoSetupPath $InnoSetupScript

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to build installer" -ForegroundColor Red
    exit 1
}

Write-Host "  Installer built successfully" -ForegroundColor Green
Write-Host ""

# Display results
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

$InstallerFiles = Get-ChildItem $ReleaseDir -Filter "*.exe" | Sort-Object LastWriteTime -Descending
if ($InstallerFiles.Count -gt 0) {
    $InstallerFile = $InstallerFiles[0]
    $InstallerSize = [math]::Round($InstallerFile.Length / 1MB, 2)
    Write-Host "Installer created:" -ForegroundColor Cyan
    Write-Host "  File: $($InstallerFile.Name)" -ForegroundColor White
    Write-Host "  Size: $InstallerSize MB" -ForegroundColor White
    Write-Host "  Path: $($InstallerFile.FullName)" -ForegroundColor White
} else {
    Write-Host "WARNING: No installer file found in release directory" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "You can now distribute the installer to users!" -ForegroundColor Green
