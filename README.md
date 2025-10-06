# Enhanced YouTube Downloader

An enhanced, production-ready version of YouTube Downloader that surpasses the original in functionality, user experience, and code quality.

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
- ✅ Authentication for private content

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
1. **[Download EnhancedYoutubeDownloader-Setup-v0.3.7.exe](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/download/v0.3.7/EnhancedYoutubeDownloader-Setup-v0.3.7.exe)** (79.72 MB)
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

#### Other Platforms
Download the appropriate package for your platform from the [Releases](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases) page.

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

