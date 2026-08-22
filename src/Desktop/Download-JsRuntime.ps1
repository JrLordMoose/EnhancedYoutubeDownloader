param(
    [string]$Platform = "",
    [string]$OutputPath = "."
)

# JavaScript runtime download script for Enhanced YouTube Downloader.
#
# yt-dlp requires a JavaScript runtime to solve YouTube's EJS player challenges.
# Without one, signature/nsig deciphering fails and every download returns
# HTTP 403 Forbidden. We bundle QuickJS (~2 MB) rather than Deno (~97 MB) to
# keep the installer small; yt-dlp still prefers a system Deno/Node/Bun when
# one is present.

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

function Download-JsRuntime {
    $platformInfo = Get-PlatformInfo

    # quickjs-ng publishes one static binary per platform. Windows ARM64 has no
    # native build, so it uses the x86_64 binary under emulation.
    $asset = switch -Wildcard ($platformInfo) {
        "win-x86"     { "qjs-windows-x86.exe" }
        "win-*"       { "qjs-windows-x86_64.exe" }
        "linux-arm64" { "qjs-linux-aarch64" }
        "linux-arm"   { "qjs-linux-armv7" }
        "linux-x86"   { "qjs-linux-x86" }
        "linux-*"     { "qjs-linux-x86_64" }
        "osx-arm64"   { "qjs-darwin-arm64" }
        "osx-*"       { "qjs-darwin-x86_64" }
        default       { throw "Unsupported platform: $platformInfo" }
    }

    $isWin = $platformInfo -like "win-*"
    $exeName = if ($isWin) { "qjs.exe" } else { "qjs" }
    $downloadUrl = "https://github.com/quickjs-ng/quickjs/releases/latest/download/$asset"
    $outputFile = Join-Path $OutputPath $exeName

    Write-Host "Downloading QuickJS JavaScript runtime for platform: $platformInfo"
    Write-Host "URL: $downloadUrl"

    Invoke-WebRequest -Uri $downloadUrl -OutFile $outputFile -UseBasicParsing

    $fileSize = (Get-Item $outputFile).Length / 1MB
    Write-Host "QuickJS size: $([math]::Round($fileSize, 2)) MB"

    if ($fileSize -lt 0.5) {
        Remove-Item $outputFile -Force
        throw "QuickJS file is too small ($([math]::Round($fileSize, 2)) MB). Download likely failed."
    }

    if (-not $isWin) {
        chmod +x $outputFile
    }

    Write-Host "QuickJS copied to: $outputFile"
}

Download-JsRuntime
Write-Host "JavaScript runtime setup completed successfully"
