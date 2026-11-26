# Enhanced YouTube Downloader

> **🎉 Automatic updates are fixed (v0.4.10 and newer).** Versions **v0.4.8 and older** can't update themselves because of bugs. Please download the [latest installer](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/latest) once; after that the app updates itself.
>
> 📬 **Get notified of new releases:** [Sign up for update emails](https://docs.google.com/forms/d/e/1FAIpQLSdkOHoyhYYekLvWVZDa5oYiTyP32EwcaqBqki8dIvO77KEKew/viewform). We'll email you directly from **Thiink Media Graphics** whenever a new version is out.
>
> 🐞 **Found a bug or have an idea?** Tell us on the [Bug Report & Suggestion Form](https://docs.google.com/forms/d/e/1FAIpQLSfgvSuqvusqpGBs6UUbUImzUB10YzROqomsvGFZhnp41VTeRg/viewform). Your reports shape the next release.

An enhanced, production-ready version of YouTube Downloader that surpasses the original in functionality, user experience, and code quality.

## 🌐 [View Landing Page](https://jrlordmoose.github.io/EnhancedYoutubeDownloader/)

<br>
<img width="1332" height="836" alt="app-preview-main-hero-image2" src="https://github.com/user-attachments/assets/95d56841-c67c-4926-afe3-a795ba48d032" />
</br>
<p>&nbsp;</p>

Experience our professional landing page featuring:
- Multi-platform support (YouTube, TikTok, Instagram, Twitter, 1,800+ sites)
- Modern glassmorphism design with animated gradients
- Netflix-style burned-in subtitles
- Queue management with pause and restart

---

## Features

<br>
<img width="1193" height="665" alt="ytdownloader-app-branded-preview-image" src="https://github.com/user-attachments/assets/e5adfbf5-1044-4468-bb14-43ce6ada46e9" />
<br>

### Core Features
- ✅ Windows 10/11 (installer with automatic updates)
- ✅ Download single videos, playlists, and channels
- ✅ MP4, WebM and MP3 (with selectable MP3 bitrate)
- ✅ Subtitles in your language: embedded, burned-in, or a separate .srt file
- ✅ Optional AI-generated subtitles (Whisper) when a video has none
- ✅ Metadata tagging and thumbnails
- ✅ **Sign-in support (cookies)** - Choose a `cookies.txt` file (or use Firefox's cookies) in Settings → Advanced → Authentication. Needed for private/age-restricted videos, Instagram, Reddit, and YouTube's "Sign in to confirm you're not a bot" check. On Windows, Chrome/Edge/Brave cookies can't be read directly, so export a `cookies.txt` instead. **[Step-by-step cookies guide](COOKIES_GUIDE.md)** (what it is, when you need it, setup, and fixes).

### Enhanced Features
- 🚀 **Unified Queue Management** - Single pane for all download states
- 🚀 **Pause and Restart** - Pause a download and continue it later (it restarts from the beginning)
- 🚀 **Enhanced Error Handling** - User-friendly messages with actions, plus an automatic sign-in prompt
- 🚀 **Multi-Platform Downloads** - YouTube, TikTok, Instagram, X/Twitter, Facebook, Vimeo, Dailymotion, SoundCloud and 1,800+ sites via yt-dlp, with real titles and thumbnails
- 🚀 **Automatic Updates** - Prompted on startup, or check any time from Settings (Windows)
- 🚀 **Light and Dark Themes** - Follows Windows automatically or pick one
- 🚀 **Better Feedback** - Notifications when downloads finish, selectable/copyable text
- 🚀 **Performance Optimization** - Parallel downloads and retry logic

## 📘 How to Use

1. **Paste a link (or search).** Paste a video, Short, playlist or channel link from any supported site, or type words to search YouTube, then press Enter or **Download**. The app fetches the real title and thumbnail first.
2. **Pick quality, format and where to save.** MP4 is video that plays everywhere, WebM is smaller video, and MP3 is audio only. Choose a quality (Best, 1080p, 720p, 480p, 360p or Audio Only), subtitles and tags. For playlists, tick the videos you want.
3. **Manage downloads.** Each one shows progress, speed and time left. You can Pause, Resume (restarts that download), Cancel, Restart, Delete, or **Open** the finished file. Select any title or error and right-click › **Copy**.
4. **If a site asks you to sign in,** the app asks automatically. Choose a `cookies.txt` file. See the **[🍪 Cookies Guide](COOKIES_GUIDE.md)**.

### Supported Platforms

| | Platforms |
|---|---|
| ✅ **Works out of the box** | YouTube (videos, Shorts, playlists, channels, youtu.be and m.youtube.com links), TikTok, X / Twitter, Facebook, Dailymotion, Twitch clips, SoundCloud (audio), direct video links (.mp4) |
| 🔐 **Needs sign-in (cookies.txt)** | Instagram, Reddit, Vimeo, and YouTube videos that are private, age-restricted or blocked by "Sign in to confirm you're not a bot" |
| ➕ **Also** | 1,800+ other sites supported by yt-dlp. Paste the link and try it. DRM-protected videos (paid streaming) can't be downloaded. |

### Settings Explained

| Tab | Setting | What it does |
|---|---|---|
| **General** | Theme | Light, Dark, or Auto (follows Windows) |
| | Automatic Updates | Checks for a new version at startup and asks before installing. **Check for updates now** checks any time. |
| **Downloads** | Parallel Downloads | How many download at once (the rest wait in line) |
| | Default Quality / Format / MP3 Quality | What the download window starts with; MP3 Quality sets the audio bitrate (Best VBR, 320, 192, 128 kbps) |
| | Subtitles / Subtitle Language / Style | Download subtitles in your language: **Embedded** (toggle in your player), **Burned-In** (always visible) or a separate **.srt** file |
| | AI Subtitles | Creates subtitles with Whisper when a video has none (slower; downloads a ~74 MB model once) |
| | Tags | Adds title, artist and cover art to the file |
| | Auto-start / Open Folder / Notify When Done | Start downloads right away, open the folder when one finishes, show a notification |
| | Storage | Default download folder, and where the app keeps its cache (restart after changing) |
| **Advanced** | Authentication | A **cookies.txt** file for sites that need sign-in (recommended), or browser cookies (only Firefox works on Windows). See the [Cookies Guide](COOKIES_GUIDE.md). |
| | Cache Management | Shows the size of saved video info and clears it |
| | Installation | Where the app is installed (updates go here), with **Open in Explorer** and **Uninstall** |
| | Reset / About | Restore every setting to default, see your version, check for updates |

### Updating

The app checks for updates at startup and asks **Update Now / Later**. Update Now shows the download progress and the install folder, then the app closes, installs and reopens by itself (about 10–20 seconds). Later installs it the next time you close the app.

> **On v0.4.8 or older?** Those versions can't finish an automatic update. Download and run the latest installer from [Releases](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/latest). It updates in place and keeps your downloads. On **v0.4.9**, choose **Update Now** (not Later) when prompted.

---

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
1. **[Download EnhancedYoutubeDownloader-Setup-v0.4.11.exe](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/download/v0.4.11/EnhancedYoutubeDownloader-Setup-v0.4.11.exe)** (~84 MB)
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

> ⚠️ **Note:** Only Windows is currently supported and tested. The download pipeline expects Windows builds of yt-dlp/FFmpeg, so the macOS and Linux steps below are experimental and may not work.

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

# Installer output: release/EnhancedYoutubeDownloader-Setup-v0.4.11.exe
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

<br>
<img width="1421" height="738" alt="realtimedownloads-image" src="https://github.com/user-attachments/assets/68295c6f-f0c5-4e47-80ec-b4f2d774c765" />
<br>
<p>&nbsp;</p>

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

## Contributors

Special thanks to the following contributors who have helped make this project better:

- **[ThiinkMG](https://github.com/ThiinkMG)** - Graphics and design contributions

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Original [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) by Tyrrrz
- [Thiink Media Graphics](https://www.thiinkmediagraphics.com/) for design and landing page
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) for YouTube API access
- [Avalonia UI](https://avaloniaui.net/) for cross-platform UI
- [Material Design](https://material.io/) for design system







