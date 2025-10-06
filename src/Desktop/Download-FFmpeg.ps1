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

    # Use proper FFmpeg builds from gyan.dev (Windows) or official sources
    if ($platformInfo -like "win-*") {
        # Download from gyan.dev - reliable Windows FFmpeg builds
        $downloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
        $outputFile = Join-Path $OutputPath "ffmpeg.zip"
        $extractPath = Join-Path $OutputPath "ffmpeg-temp"

        Write-Host "Downloading FFmpeg for platform: $platformInfo"
        Write-Host "URL: $downloadUrl"
        Write-Host "This may take a few minutes (60-80 MB download)..."

        try {
            # Download FFmpeg
            Invoke-WebRequest -Uri $downloadUrl -OutFile $outputFile -UseBasicParsing
            Write-Host "Download completed successfully"

            # Extract the archive
            Write-Host "Extracting FFmpeg..."
            Expand-Archive -Path $outputFile -DestinationPath $extractPath -Force

            # Find ffmpeg.exe and ffprobe.exe in the extracted folder (usually in bin subdirectory)
            $ffmpegExe = Get-ChildItem -Path $extractPath -Filter "ffmpeg.exe" -Recurse | Select-Object -First 1
            $ffprobeExe = Get-ChildItem -Path $extractPath -Filter "ffprobe.exe" -Recurse | Select-Object -First 1

            if ($ffmpegExe) {
                # Copy ffmpeg.exe to output directory
                Copy-Item $ffmpegExe.FullName -Destination (Join-Path $OutputPath "ffmpeg.exe") -Force
                Write-Host "FFmpeg copied to: $OutputPath"

                # Verify the file size
                $fileSize = (Get-Item (Join-Path $OutputPath "ffmpeg.exe")).Length / 1MB
                Write-Host "FFmpeg size: $([math]::Round($fileSize, 2)) MB"

                if ($fileSize -lt 50) {
                    throw "FFmpeg file is too small ($([math]::Round($fileSize, 2)) MB). Expected at least 50 MB."
                }
            } else {
                throw "Could not find ffmpeg.exe in the downloaded archive"
            }

            if ($ffprobeExe) {
                # Copy ffprobe.exe to output directory
                Copy-Item $ffprobeExe.FullName -Destination (Join-Path $OutputPath "ffprobe.exe") -Force
                Write-Host "FFprobe copied to: $OutputPath"

                # Verify the file size
                $probeSize = (Get-Item (Join-Path $OutputPath "ffprobe.exe")).Length / 1MB
                Write-Host "FFprobe size: $([math]::Round($probeSize, 2)) MB"
            } else {
                Write-Warning "Could not find ffprobe.exe in the downloaded archive. Subtitle burn-in progress reporting may not work."
            }

            # Cleanup
            Remove-Item $outputFile -Force
            Remove-Item $extractPath -Recurse -Force

            Write-Host "FFmpeg extracted and configured successfully"
        }
        catch {
            Write-Error "Failed to download or extract FFmpeg: $_"
            throw
        }
    }
    else {
        # For non-Windows platforms, try the FFmpegBin repo
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
            $ffmpegPath = Join-Path $OutputPath "ffmpeg"
            if (Test-Path $ffmpegPath) {
                chmod +x $ffmpegPath
            }

            Write-Host "FFmpeg extracted and configured successfully"
        }
        catch {
            Write-Error "Failed to download or extract FFmpeg: $_"
            throw
        }
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