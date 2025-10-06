# Enhanced YouTube Downloader

An enhanced, production-ready version of YouTube Downloader that surpasses the original in functionality, user experience, and code quality. This application provides a robust and intuitive interface for downloading videos, playlists, and entire channels from YouTube with advanced features like pause/resume, format selection, and metadata tagging.

![App Screenshot](https://user-images.githubusercontent.com/1 Tyrrrz/YoutubeDownloader/main/screenshot.png) <!-- Placeholder Screenshot -->

## ✨ Features

### Core Functionality
- **Cross-Platform:** Runs on Windows, Linux, and macOS.
- **Versatile Downloads:** Download single videos, entire playlists, or channel uploads.
- **Format Selection:** Choose from various video and audio formats (MP4, WebM, MP3).
- **Quality Options:** Select your desired video quality, from 360p up to 4K.
- **Subtitle Support:** Automatically download and embed subtitles.
- **Metadata Tagging:** Embeds video metadata and thumbnails into the downloaded files.
- **Authentication:** Supports downloading age-restricted and private content by using cookies.

### Enhanced Features
- 🚀 **Unified Download Queue:** Manage all your downloads from a single, clear interface.
- ⏯️ **Pause/Resume Functionality:** Pause and resume downloads at any time. The application state is saved, so you can even resume after a restart.
- 🛡️ **Advanced Error Handling:** Get user-friendly error messages with actionable suggestions for common issues (network, permissions, etc.).
- ⚡ **Metadata Caching:** A fast SQLite-based cache for video metadata reduces redundant API calls and speeds up query resolution.
- ⚙️ **Format Profiles:** Create and save presets for your favorite download settings (e.g., "Audio-Only MP3", "1080p Video").
- 🔔 **Rich User Feedback:** Get real-time feedback through toast notifications and loading indicators.
- 🏎️ **Optimized Performance:** Enjoy parallel downloads, efficient chunk-based downloading, and automatic retry logic.

## 🚀 Getting Started

Follow these instructions to build and run the application from the source code.

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Windows:** PowerShell
- **Linux/macOS:** A shell environment (like Bash)

### Installation Steps

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/EnhancedYoutubeDownloader/EnhancedYoutubeDownloader.git
    cd EnhancedYoutubeDownloader
    ```

2.  **Download FFmpeg:**
    The application uses FFmpeg for media processing. A helper script is included to download it automatically.

    On **Windows** (using PowerShell):
    ```powershell
    ./src/Desktop/Download-FFmpeg.ps1
    ```

    *Note: You may need to adjust your script execution policy: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process`*

    On **Linux/macOS**:
    You will need to install FFmpeg using your system's package manager (e.g., `sudo apt-get install ffmpeg` on Debian/Ubuntu, `brew install ffmpeg` on macOS).

3.  **Restore .NET dependencies:**
    ```bash
    dotnet restore
    ```

4.  **Build and run the application:**
    ```bash
    dotnet run --project src/Desktop/EnhancedYoutubeDownloader.csproj
    ```

## 💻 How to Use

1.  **Enter a URL or Search Query:** Paste a YouTube video, playlist, or channel URL into the input box, or type a search query.
2.  **Process Query:** Click the "Process" button or press Enter. The application will resolve the query.
3.  **Configure Download:**
    - For a **single video**, a dialog will appear allowing you to select the quality, format, and save location.
    - For a **playlist or channel**, a dialog will let you select which videos to download and configure the settings for all of them.
4.  **Start Downloading:** Downloads will be added to the queue. They will start automatically if enabled in settings, or you can start them manually.
5.  **Manage Downloads:** Use the controls for each item in the queue to pause, resume, cancel, or restart a download.

## 🏗️ Architecture

The application follows a clean architecture pattern to ensure a clear separation of concerns, making it maintainable and scalable.

```
src/
├── Core/           # Core business logic, services, and domain models.
│   ├── Services/   # (e.g., DownloadService, CacheService)
│   └── Utils/      # Utility classes.
├── Desktop/        # Avalonia UI application project.
│   ├── Controls/   # Custom UI controls.
│   ├── Framework/  # MVVM framework components (e.g., ViewModelBase, DialogManager).
│   ├── Services/   # Desktop-specific services (e.g., NotificationService, UpdateService).
│   ├── ViewModels/ # ViewModels for the UI.
│   └── Views/      # Views and dialogs (AXAML).
├── Shared/         # Shared models and interfaces used by multiple projects.
│   ├── Interfaces/ # Abstractions for services.
│   └── Models/     # Data models shared across layers.
└── Tests/          # xUnit tests for the application.
```

### Key Components

-   **DownloadService**: Manages the entire download lifecycle, including queuing, pause/resume, and progress reporting.
-   **CacheService**: Implements SQLite-based caching for video metadata to improve performance and reduce API usage.
-   **QueryResolver**: Parses user input (URLs or search terms) and fetches video metadata from YouTube.
-   **SettingsService**: Manages user settings and persists them to disk.
-   **MVVM Framework**: A set of base classes and managers (`DialogManager`, `ViewModelManager`, etc.) to support the Model-View-ViewModel pattern.

## 🛠️ Technology Stack

-   **Framework**: .NET 9.0
-   **UI**: Avalonia UI 11.x with Material.Avalonia for styles.
-   **YouTube API**: YoutubeExplode for fetching metadata and stream information.
-   **Downloader CLI**: yt-dlp for robust video downloading.
-   **Media Processing**: FFmpeg for muxing video and audio streams.
-   **Database**: Microsoft.Data.Sqlite for local caching.
-   **Testing**: xUnit.
-   **CI/CD**: GitHub Actions.

## 🤝 Contributing

Contributions are welcome! If you have a suggestion or want to fix a bug, please follow these steps:

1.  Fork the repository.
2.  Create a new feature branch (`git checkout -b feature/your-feature-name`).
3.  Make your changes.
4.  Add tests for any new functionality.
5.  Ensure all tests pass.
6.  Submit a pull request with a clear description of your changes.

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

-   The original [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) by Tyrrrz, which served as the inspiration for this project.
-   [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) for providing a clean C# interface for the YouTube API.
-   The [Avalonia UI](https://avaloniaui.net/) team for their excellent cross-platform UI framework.
-   The developers of [yt-dlp](https://github.com/yt-dlp/yt-dlp) and [FFmpeg](https://ffmpeg.org/).