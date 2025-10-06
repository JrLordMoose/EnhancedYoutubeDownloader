param(
    [string]$Platform = "",
    [string]$OutputPath = "."
)

# yt-dlp download script for Enhanced YouTube Downloader
# This script downloads the appropriate yt-dlp binary for the current platform

$ErrorActionPreference = "Stop"

function Get-PlatformInfo {
    if ($Platform) {
        return $Platform
    }

    $os = if ($IsWindows -or $env:OS -eq "Windows_NT") { "win" }
          elseif ($IsLinux) { "linux" }
          elseif ($IsMacOS) { "osx" }
          else { "unknown" }

    $arch = switch ([System.Environment]::OSArchitecture) {
        "X64" { "x64" }
        "X86" { "x86" }
        "Arm64" { "arm64" }
        "Arm" { "arm" }
        default { "x64" }
    }

    return "$os-$arch"
}

function Download-YtDlp {
    $platformInfo = Get-PlatformInfo

    # Download from official yt-dlp GitHub releases
    if ($platformInfo -like "win-*") {
        # Windows: Download yt-dlp.exe
        $downloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"
        $outputFile = Join-Path $OutputPath "yt-dlp.exe"

        Write-Host "Downloading yt-dlp for platform: $platformInfo"
        Write-Host "URL: $downloadUrl"
        Write-Host "This may take a few moments (10-15 MB download)..."

        try {
            # Download yt-dlp.exe
            Invoke-WebRequest -Uri $downloadUrl -OutFile $outputFile -UseBasicParsing
            Write-Host "Download completed successfully"

            # Verify the file size
            $fileSize = (Get-Item $outputFile).Length / 1MB
            Write-Host "yt-dlp size: $([math]::Round($fileSize, 2)) MB"

            if ($fileSize -lt 5) {
                throw "yt-dlp file is too small ($([math]::Round($fileSize, 2)) MB). Expected at least 5 MB."
            }

            Write-Host "yt-dlp downloaded and configured successfully"
        }
        catch {
            Write-Error "Failed to download yt-dlp: $_"
            throw
        }
    }
    elseif ($platformInfo -like "linux-*") {
        # Linux: Download yt-dlp (no .exe extension)
        $downloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp"
        $outputFile = Join-Path $OutputPath "yt-dlp"

        Write-Host "Downloading yt-dlp for platform: $platformInfo"
        Write-Host "URL: $downloadUrl"

        try {
            Invoke-WebRequest -Uri $downloadUrl -OutFile $outputFile -UseBasicParsing
            Write-Host "Download completed successfully"

            # Make executable on Unix systems
            if (Test-Path $outputFile) {
                chmod +x $outputFile
            }

            Write-Host "yt-dlp downloaded and configured successfully"
        }
        catch {
            Write-Error "Failed to download yt-dlp: $_"
            throw
        }
    }
    elseif ($platformInfo -like "osx-*") {
        # macOS: Download yt-dlp_macos
        $downloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_macos"
        $outputFile = Join-Path $OutputPath "yt-dlp"

        Write-Host "Downloading yt-dlp for platform: $platformInfo"
        Write-Host "URL: $downloadUrl"

        try {
            Invoke-WebRequest -Uri $downloadUrl -OutFile $outputFile -UseBasicParsing
            Write-Host "Download completed successfully"

            # Make executable on Unix systems
            if (Test-Path $outputFile) {
                chmod +x $outputFile
            }

            Write-Host "yt-dlp downloaded and configured successfully"
        }
        catch {
            Write-Error "Failed to download yt-dlp: $_"
            throw
        }
    }
    else {
        Write-Error "Unsupported platform: $platformInfo"
        throw "Unsupported platform: $platformInfo"
    }
}

# Main execution
try {
    Download-YtDlp
    Write-Host "yt-dlp setup completed successfully"
}
catch {
    Write-Error "yt-dlp setup failed: $_"
    exit 1
}
