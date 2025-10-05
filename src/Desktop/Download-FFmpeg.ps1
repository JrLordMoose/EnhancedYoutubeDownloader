param(
    [string]$Platform = "",
    [string]$OutputPath = "."
)

# FFmpeg download script for Enhanced YouTube Downloader
# This script downloads the appropriate FFmpeg binary for the current platform

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

function Download-FFmpeg {
    $platformInfo = Get-PlatformInfo
    $downloadUrl = "https://github.com/FFmpegBin/FFmpegBin/releases/download/7.1.1/ffmpeg-$platformInfo.zip"
    $outputFile = Join-Path $OutputPath "ffmpeg.zip"
    
    Write-Host "Downloading FFmpeg for platform: $platformInfo"
    Write-Host "URL: $downloadUrl"
    
    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $outputFile -UseBasicParsing
        Write-Host "Download completed successfully"
        
        # Extract the archive
        Expand-Archive -Path $outputFile -DestinationPath $OutputPath -Force
        Remove-Item $outputFile -Force
        
        # Make executable on Unix systems
        if ($IsLinux -or $IsMacOS) {
            $ffmpegPath = Join-Path $OutputPath "ffmpeg"
            if (Test-Path $ffmpegPath) {
                chmod +x $ffmpegPath
            }
        }
        
        Write-Host "FFmpeg extracted and configured successfully"
    }
    catch {
        Write-Error "Failed to download or extract FFmpeg: $_"
        throw
    }
}

# Main execution
try {
    Download-FFmpeg
    Write-Host "FFmpeg setup completed successfully"
}
catch {
    Write-Error "FFmpeg setup failed: $_"
    exit 1
}