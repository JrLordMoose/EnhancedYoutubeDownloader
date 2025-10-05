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

### Prerequisites
- .NET 9.0 SDK
- PowerShell (for FFmpeg download)

### Building from Source

1. Clone the repository:
```bash
git clone https://github.com/EnhancedYoutubeDownloader/EnhancedYoutubeDownloader.git
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

```bash
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
```

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