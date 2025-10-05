# Enhanced YouTube Downloader

An enhanced, production-ready version of YouTube Downloader that surpasses the original in functionality, user experience, and code quality.

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
1. Download `EnhancedYoutubeDownloader-Setup-v1.0.0.exe` from the [Releases](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases) page
2. Run the installer and follow the setup wizard
3. Choose installation options:
   - ✅ Create desktop shortcut (default: checked)
   - ✅ Launch after installation (default: checked)
4. Click Install and you're ready to download!

**Features:**
- Professional Windows installer with desktop shortcut
- Start Menu integration with uninstall shortcut
- Add/Remove Programs integration
- Smart uninstaller with optional data cleanup

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

# Installer output: release/EnhancedYoutubeDownloader-Setup-v1.0.0.exe
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