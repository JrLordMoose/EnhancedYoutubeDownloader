# Enhanced YouTube Downloader

An enhanced, production-ready version of YouTube Downloader that surpasses the original in functionality, user experience, and code quality.

## 🌐 [View Landing Page](https://jrlordmoose.github.io/EnhancedYoutubeDownloader/)

Experience our professional landing page featuring:
- Modern glassmorphism design with animated gradients
- Interactive 3D mockups and smooth animations
- Comprehensive feature showcase
- Easy installation guide

<br>
<img width="1126" height="754" alt="enhanced_ytdownloader_screenshot" src="https://github.com/user-attachments/assets/12869c5f-3c74-4778-8658-85c0d2370ba5" />
<br>

## Features

### Core Features
- ✅ Cross-platform support (Windows, Linux, macOS)
- ✅ Download single videos, playlists, and channels
- ✅ Multiple quality and format options
- ✅ Subtitle support
- ✅ Metadata tagging
- ⚠️ Authentication for private content (not fully implemented - do not modify auth settings)

### Enhanced Features
- 🚀 **Unified Queue Management** - Single pane for all download states
- 🚀 **Pause/Resume Functionality** - State persistence and recovery
- 🚀 **Enhanced Error Handling** - User-friendly messages with actions
- 🚀 **Metadata Caching** - SQLite-based cache with invalidation
- 🚀 **Drag-and-Drop Support** - URLs anywhere in UI
- 🚀 **Batch Operations** - Multi-select and apply settings
- 🚀 **Download Scheduling** - Time-based queue execution
- 🚀 **Format Profiles** - Quick presets for common scenarios
- 🚀 **Better Feedback** - Toast notifications and loading states
- 🚀 **Performance Optimization** - Parallel downloads, chunking, retry logic

## Technology Stack

- **Framework**: .NET 9.0
- **UI**: Avalonia UI 11.x with Material Design
- **YouTube API**: YoutubeExplode 6.5.4+
- **Media Processing**: FFmpeg 7.x
- **Database**: SQLite for caching
- **Testing**: xUnit
- **CI/CD**: GitHub Actions

## Getting Started

### Installation (End Users)

#### Windows
Download and run the installer:
1. **[Download EnhancedYoutubeDownloader-Setup-v0.3.9.exe](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/latest)** (~80 MB)
2. Run the installer and follow the setup wizard

**⚠️ Windows SmartScreen Warning**

You may see a red warning screen saying "Windows protected your PC".

<br>
<p>&nbsp;</p>

<img width="530" height="494" alt="ytscreenshot23" src="https://github.com/user-attachments/assets/3ad1fcbf-4f86-4520-afa6-2285c0baaa95" />

<br>
<p>&nbsp;</p>

**This is normal and safe!** The warning appears because the installer isn't digitally signed (signing costs $200-500/year).
**To proceed:**
1. Click **"More info"** on the warning screen
2. Click **"Run anyway"** button that appears
3. The installer is safe - all code is open-source and can be reviewed on GitHub

3. Choose installation options:
   - ✅ Create desktop shortcut (default: checked)
   - ✅ Launch after installation (default: checked)
4. Click Install and you're ready to download!

**Features:**
- Professional Windows installer with desktop shortcut
- Start Menu integration with uninstall shortcut
- Add/Remove Programs integration
- Custom uninstaller (uninstall.exe) in install folder
- Smart uninstaller with optional data cleanup
- FFmpeg + yt-dlp bundled for video download and conversion (80 MB total)
- Dependency validation on startup with download links

#### Mac & macOS
Currently, Mac support is experimental. To run Enhanced YouTube Downloader on Mac:

1. **Install .NET 9.0 Runtime**:
   - Download from [dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
   - Choose "macOS" → "x64" or "ARM64" (for M1/M2 Macs)
   - Run the installer package

2. **Download and Run**:
   ```bash
   # Clone the repository
   git clone https://github.com/JrLordMoose/EnhancedYoutubeDownloader.git
   cd EnhancedYoutubeDownloader

   # Restore dependencies
   dotnet restore

   # Run the application
   dotnet run --project src/Desktop/EnhancedYoutubeDownloader.csproj
   ```

**Note:** Full Mac installer coming soon!

#### Linux
To run on Linux:

1. **Install .NET 9.0 Runtime**:
   ```bash
   # Ubuntu/Debian
   wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt-get update && sudo apt-get install -y dotnet-runtime-9.0

   # Fedora/RHEL
   sudo dnf install dotnet-runtime-9.0

   # Arch Linux
   sudo pacman -S dotnet-runtime
   ```

2. **Download and Run**:
   ```bash
   # Clone the repository
   git clone https://github.com/JrLordMoose/EnhancedYoutubeDownloader.git
   cd EnhancedYoutubeDownloader

   # Restore dependencies
   dotnet restore

   # Run the application
   dotnet run --project src/Desktop/EnhancedYoutubeDownloader.csproj
   ```

**Note:** FFmpeg will be automatically downloaded on first run.

### Building from Source

#### Prerequisites
- .NET 9.0 SDK
- PowerShell (for FFmpeg download)
- Inno Setup 6+ (optional, for building Windows installer)

#### Build Steps

1. Clone the repository:
```bash
git clone https://github.com/JrLordMoose/EnhancedYoutubeDownloader.git
cd EnhancedYoutubeDownloader
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run --project src/Desktop/EnhancedYoutubeDownloader.csproj
```

### Building for Distribution

#### Standard Distribution
```bash
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
```

#### Windows Installer
Build a professional Windows installer with desktop shortcut and launch options:

**Automated Build (PowerShell):**
```powershell
.\build-installer.ps1
```

**Manual Build:**
```bash
# 1. Publish self-contained application
dotnet publish src/Desktop/EnhancedYoutubeDownloader.csproj -c Release -r win-x64 --self-contained

# 2. Build installer with Inno Setup
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup.iss

# Installer output: release/EnhancedYoutubeDownloader-Setup-v0.3.7.exe
```

**Installer Features:**
- Desktop shortcut creation (default: enabled)
- Launch application after installation (default: enabled)
- Start Menu shortcuts (app + uninstaller)
- Add/Remove Programs integration
- Smart data cleanup on uninstall with user confirmation

See [CLAUDE.md](CLAUDE.md) for detailed build instructions.

## Architecture

The application follows a clean architecture pattern with clear separation of concerns:

```
src/
├── Core/           # Business logic and services
├── Desktop/        # Avalonia UI application
├── Shared/         # Shared models and interfaces
└── Tests/          # Unit and integration tests
```

### Key Components

- **DownloadService**: Manages download operations with pause/resume
- **CacheService**: Handles metadata caching with SQLite
- **NotificationService**: Provides user feedback
- **DashboardViewModel**: Main UI logic and queue management

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Original [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) by Tyrrrz
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) for YouTube API access
- [Avalonia UI](https://avaloniaui.net/) for cross-platform UI

- [Material Design](https://material.io/) for design system

