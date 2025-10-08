# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 🔴 MEMORIZE: Critical Version Update Checklist

**ALWAYS update ALL FOUR version locations when releasing a new version!**

Failure to update all locations will cause critical bugs like the v0.3.2-v0.3.5 incident where the application version remained at 0.3.1 despite installer showing correct version, causing all bug fixes to not be compiled into the binary.

## ⚠️ CRITICAL: Auto-Update Requires ZIP File!

**NEVER forget to upload the ZIP file to GitHub releases!**

The auto-update system (Onova) REQUIRES a ZIP package to function. Without it:
- ❌ Users won't receive auto-update notifications
- ❌ The "Update Available" dialog won't appear
- ❌ Users stay on old versions with bugs
- ❌ Update system completely breaks

**ALWAYS upload BOTH files to every release:**
1. `EnhancedYoutubeDownloader-Setup-vX.X.X.exe` - For new installations
2. `EnhancedYoutubeDownloader-X.X.X.zip` - **FOR AUTO-UPDATES** (CRITICAL!)

The build-installer.ps1 script creates BOTH files automatically. YOU MUST UPLOAD BOTH!

### Required Updates for Every Release:

1. **`Directory.Build.props`** (line 4) - **THE SOURCE OF TRUTH**
   ```xml
   <Version>X.X.X</Version>
   ```
   - This is THE actual application binary version
   - Read by MSBuild during compilation
   - Sets `Assembly.GetName().Version`
   - Displayed in window title bar
   - **MUST BE UPDATED FIRST!**

2. **`setup.iss`** (line 5) - Installer package version
   ```inno
   #define MyAppVersion "X.X.X"
   ```
   - Controls installer filename and metadata
   - Shown in Windows Add/Remove Programs

3. **`build-installer.ps1`** (line 6) - Build script default version
   ```powershell
   [string]$Version = "X.X.X"
   ```
   - Default version parameter for build script

4. **`src/Desktop/Views/Dialogs/SettingsDialog.axaml`** (line 406) - Settings UI version
   ```xml
   <TextBlock Text="Version X.X.X"
   ```
   - Displayed in Settings > About section
   - User-facing version display

5. **`README.md`** - Update download links (lines 47, 140)
   ```markdown
   [Download EnhancedYoutubeDownloader-Setup-vX.X.X.exe](https://github.com/.../vX.X.X/EnhancedYoutubeDownloader-Setup-vX.X.X.exe)
   ```

6. **`docs/index.html`** - **CRITICAL: Landing Page Download Buttons**
   - **Hero section** (line ~39): Update primary download button href
   - **Download section** (line ~301): Update installer download button href
   ```html
   <a href="https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/download/vX.X.X/EnhancedYoutubeDownloader-Setup-vX.X.X.exe" class="btn btn-primary btn-large">
   ```
   - ⚠️ **MUST update BOTH buttons** or users will download old version!
   - Landing page is the main entry point for new users
   - Direct download links provide better UX than redirecting to releases page

7. **GitHub Release** - Include direct download link in release notes AND upload ZIP package
   ```markdown
   **[Download EnhancedYoutubeDownloader-Setup-vX.X.X.exe](https://github.com/.../vX.X.X/EnhancedYoutubeDownloader-Setup-vX.X.X.exe)** (XX MB)
   ```

### Version Update Order (CRITICAL):
1. `Directory.Build.props` (source of truth)
2. `setup.iss` (installer version)
3. `build-installer.ps1` (build script)
4. `SettingsDialog.axaml` (UI display)
5. `README.md` (documentation)
6. **`docs/index.html`** (landing page download buttons - BOTH locations)
7. Build installer with `build-installer.ps1` (creates both .exe and .zip)
8. Create GitHub release with **BOTH files**:
   - `EnhancedYoutubeDownloader-Setup-vX.X.X.exe` (for new users)
   - `EnhancedYoutubeDownloader-X.X.X.zip` (for auto-updates)
9. Include direct download link in release notes

### 📦 Release File Organization System

**CRITICAL: GitHub Release MUST contain exactly TWO files:**

#### File 1: Windows Installer (for new users)
```
EnhancedYoutubeDownloader-Setup-vX.X.X.exe
```
- **Purpose**: Fresh installations for new users
- **Created by**: `build-installer.ps1` script
- **Location**: `release/EnhancedYoutubeDownloader-Setup-vX.X.X.exe`
- **Size**: ~83 MB (self-contained with .NET runtime, FFmpeg, yt-dlp)
- **Upload command**: `gh release upload vX.X.X release/EnhancedYoutubeDownloader-Setup-vX.X.X.exe`

#### File 2: ZIP Package (for auto-updates) ⚠️ CRITICAL
```
EnhancedYoutubeDownloader-X.X.X.zip
```
- **Purpose**: **AUTO-UPDATE SYSTEM** (Onova requires this!)
- **Created by**: `build-installer.ps1` script (automatic)
- **Location**: `release/EnhancedYoutubeDownloader-X.X.X.zip`
- **Size**: ~108 MB (published binaries)
- **Upload command**: `gh release upload vX.X.X release/EnhancedYoutubeDownloader-X.X.X.zip`
- **⚠️ WITHOUT THIS FILE**: Users won't receive update notifications!

### Verification Checklist (MUST complete ALL):
- [ ] Build completes successfully (`build-installer.ps1 -Version "X.X.X"`)
- [ ] **BOTH files created** in `release/` folder
  - [ ] `EnhancedYoutubeDownloader-Setup-vX.X.X.exe` exists (~83 MB)
  - [ ] `EnhancedYoutubeDownloader-X.X.X.zip` exists (~108 MB)
- [ ] Filenames match version exactly (no typos!)
- [ ] **Landing page buttons updated** (`docs/index.html`):
  - [ ] Hero section download button (line ~39) points to new version
  - [ ] Download section button (line ~301) points to new version
- [ ] Create GitHub release: `gh release create vX.X.X`
- [ ] **Upload BOTH files to release**:
  - [ ] `gh release upload vX.X.X release/EnhancedYoutubeDownloader-Setup-vX.X.X.exe`
  - [ ] `gh release upload vX.X.X release/EnhancedYoutubeDownloader-X.X.X.zip` ⚠️
- [ ] Verify release has 2 assets: `gh release view vX.X.X`
- [ ] Update release notes with direct download links (both files)
- [ ] Test: Install EXE and verify version in title bar
- [ ] Test: Check Settings > About shows correct version
- [ ] Test: Verify auto-update check works (check for updates in app)
- [ ] Test: Landing page download buttons work (direct download starts)

**NEVER skip these steps! Missing ZIP = broken auto-updates for ALL users!**

---

## Project Overview

Enhanced YouTube Downloader is a production-ready cross-platform desktop application built with .NET 9.0 and Avalonia UI. It provides video downloading from YouTube with advanced features like pause/resume, queue management, caching, and scheduling.

Always analyze and keep in mind all files within the "guides-and-instructions" folder. This folder contains instruction and context documents, as well as previous chats located "@guides-andinstructions/chats". The most important files to focus on are:

@guides-andinstructions/AI Agent Instruction Prompt_ Clone and Enhance YoutubeDownloader.md
@guides-andinstructions/Comprehensive Analysis of YoutubeDownloader Application.md

### Create Subagents
When Task becomes too long always breakdown the tasks and then create and deploy subagents that will talk to one another to limit bugs, and stay on task and then report back to you with their updates and then you summarize and share with me the changes made or updates made   

1. Task Decomposition

Break complex projects into manageable subtasks
Identify dependencies between tasks
Establish clear priorities and sequencing

2. Subagent Coordination

Deploy specialized agents for specific task types
Manage parallel workflows
Integrate outputs from multiple agents

3. Progress Tracking

Monitor completion status
Identify bottlenecks
Adjust plans dynamically

4. Quality Assurance

Verify subtask completion
Ensure consistency across outputs
Validate final deliverables

Analyze the complexity and requirements
Break it into logical subtasks
Identify which specialized agents to deploy
Create an execution plan
Coordinate the work and integrate results

## Chats & Sessions
The latest exported chat file located within the folder "@guides-andinstructions/chats", identified by the highest number at the end of its filename, indicating it's the most recent. These documents provide essential guidance and context for your tasks.

## Building and Running

### Prerequisites
- .NET 9.0 SDK
- PowerShell (for automatic FFmpeg download)

### Common Commands

```bash
# Restore dependencies (downloads FFmpeg automatically via PowerShell script)
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run --project src/Desktop/EnhancedYoutubeDownloader.csproj

# Run tests
dotnet test src/Tests/EnhancedYoutubeDownloader.Tests.csproj

# Run specific test
dotnet test src/Tests/EnhancedYoutubeDownloader.Tests.csproj --filter "FullyQualifiedName~TestName"

# Build for distribution (self-contained)
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
```

### FFmpeg Setup

FFmpeg is automatically downloaded during restore via the `Download-FFmpeg.ps1` script. If manual download is needed, the script is located in `src/Desktop/Download-FFmpeg.ps1`.

### Building Windows Installer

The project includes a Windows installer for easy distribution to end users:

#### Prerequisites
- .NET 9.0 SDK
- [Inno Setup 6](https://jrsoftware.org/isdl.php) - Free Windows installer creator

#### Build Installer

```powershell
# Build installer with default version (1.0.0)
.\build-installer.ps1

# Build installer with custom version
.\build-installer.ps1 -Version "1.2.3"

# Build in Debug configuration (default is Release)
.\build-installer.ps1 -Configuration Debug
```

The script performs these steps:
1. Cleans previous builds
2. Restores dependencies
3. Publishes self-contained win-x64 application
4. Verifies FFmpeg is included
5. Compiles Inno Setup script to create installer EXE

Output: `release/EnhancedYoutubeDownloader-Setup-v{version}.exe`

#### Installer Features

The Windows installer (`setup.iss`) provides:
- **Self-contained deployment** - Bundles .NET 9.0 runtime, no prerequisites needed
- **Desktop shortcut** - Optional (checked by default), creates shortcut on user's desktop
- **Launch after install** - Optional (checked by default), runs app immediately after installation
- **Start Menu integration** - Adds program shortcuts and uninstaller
- **Add/Remove Programs** - Professional uninstall experience
- **Modern UI** - Clean, professional installation wizard
- **No admin required** - Installs to user's Program Files folder (per-user installation)

#### Manual Installer Build

If you prefer to build manually:

```bash
# 1. Publish self-contained application
dotnet publish src/Desktop/EnhancedYoutubeDownloader.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output src/Desktop/bin/Release/net9.0/win-x64/publish

# 2. Compile Inno Setup script
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup.iss
```

#### Installer Configuration

Edit `setup.iss` to customize:
- `#define MyAppVersion` - Version number (line 5)
- `AppId` - Unique GUID for the application (line 13)
- `DefaultDirName` - Installation directory (line 18)
- `LicenseFile` - Path to LICENSE file (line 21)
- `SetupIconFile` - Application icon for installer (line 23)

For advanced customization, see [Inno Setup documentation](https://jrsoftware.org/ishelp/).

### Release Process

When creating a new release, follow these steps to ensure version consistency:

#### Version Update Checklist
**CRITICAL**: There are THREE version locations that must ALL be updated:

1. **Directory.Build.props** (line 4) - ✅ **UPDATE THIS FIRST!**
   ```xml
   <Version>X.Y.Z</Version>
   ```
   - This is the **source of truth** for the .NET application version
   - Read by MSBuild and displayed in title bar
   - Forgetting this causes version mismatch bugs!

2. **setup.iss** (line 5)
   ```inno
   #define MyAppVersion "X.Y.Z"
   ```
   - Installer package metadata

3. **build-installer.ps1** (line 6)
   ```powershell
   [string]$Version = "X.Y.Z"
   ```
   - Build script default version

4. **README.md** (lines 47, 140)
   - Update download links to new version

#### GitHub Release Guidelines

When creating releases with `gh release create`, **always include**:

1. **Direct download link** to the installer EXE in the release notes:
   ```markdown
   **[Download EnhancedYoutubeDownloader-Setup-vX.Y.Z.exe](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/download/vX.Y.Z/EnhancedYoutubeDownloader-Setup-vX.Y.Z.exe)** (79.72 MB)
   ```

2. **Installation instructions** with SmartScreen warning

3. **What's New section** highlighting key features/fixes

4. **Links section** with:
   - Installation Guide
   - Issue reporting
   - Full changelog

**Example Release Command**:
```bash
gh release create vX.Y.Z release/EnhancedYoutubeDownloader-Setup-vX.Y.Z.exe \
  --title "vX.Y.Z - Feature Name" \
  --notes "$(cat <<'EOF'
## What's New
- Feature 1
- Fix 1

## Installation
**[Download EnhancedYoutubeDownloader-Setup-vX.Y.Z.exe](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/download/vX.Y.Z/EnhancedYoutubeDownloader-Setup-vX.Y.Z.exe)** (79.72 MB)

You may see a Windows SmartScreen warning - click "More info" and "Run anyway".

## Links
- [Installation Guide](...)
- [Report Issues](...)

**Full Changelog**: https://github.com/JrLordMoose/EnhancedYoutubeDownloader/compare/vX.Y.Z-1...vX.Y.Z
EOF
)"
```

## Architecture

The solution follows a clean architecture with clear separation:

```
src/
├── Shared/    - Interfaces and shared models (IDownloadService, DownloadItem, DownloadStatus)
├── Core/      - Business logic and core services (DownloadService, CacheService)
├── Desktop/   - Avalonia UI application (ViewModels, Views, Framework)
└── Tests/     - xUnit tests with Moq and FluentAssertions
```

### Key Architectural Patterns

**Dependency Injection**: The application uses Microsoft.Extensions.DependencyInjection configured in `src/Desktop/App.axaml.cs:64-90`. All services are registered as singletons, ViewModels as transient.

**MVVM Pattern**: Avalonia UI with CommunityToolkit.Mvvm for ViewModels. Views are in AXAML (Avalonia XAML), code-behind in `.axaml.cs` files.

**Reactive Programming**: Uses `System.Reactive` with `Subject<T>` for event streams. The DownloadService exposes `IObservable<DownloadItem>` for status changes.

**Service Layer**: Business logic is encapsulated in services implementing interfaces from `Shared/Interfaces/`:
- `IDownloadService` - Download operations, pause/resume, queue management
- `ICacheService` - SQLite-based metadata caching with expiration
- `INotificationService` - User feedback (toast notifications)
- `IQueryResolver` - URL parsing and YouTube query resolution

## Important Components

### Core Services (`src/Core/Services/`)

**DownloadService** (`DownloadService.cs`):
- Manages concurrent downloads with configurable parallelism (default: 3)
- State machine: Queued → Started → [Paused] → Completed/Failed/Canceled
- **Pause/resume with chunked HTTP downloads** - Uses Range headers for resumable downloads
- **Per-download CancellationTokens** - Independent pause/cancel of individual downloads
- **State persistence** - SQLite-based download state with byte-level progress
- Uses YoutubeExplode for YouTube API access
- Periodic state saves (every 1MB) for crash recovery

**DownloadStateRepository** (`DownloadStateRepository.cs`):
- SQLite database at `%AppData%/EnhancedYoutubeDownloader/download_state.db`
- Persists download progress for pause/resume functionality
- Schema: `DownloadState` table with DownloadId, VideoId, FilePath, BytesDownloaded, TotalBytes, Status
- Automatic cleanup on download completion

**CacheService** (`CacheService.cs`):
- SQLite database at `%AppData%/EnhancedYoutubeDownloader/cache.db`
- Caches video metadata with TTL expiration (24 hours default)
- Schema: `VideoMetadata` table with VideoId, JsonData, CreatedAt, ExpiresAt

**QueryResolver** (`QueryResolver.cs`):
- **URL parsing** - Regex-based extraction of video/playlist/channel IDs
- **Multi-format support** - Handles youtube.com/watch, youtu.be, channel URLs, @handles
- **Query resolution** - Single videos, playlists, channels, search queries
- **Cache integration** - Checks CacheService before fetching from YouTube
- Returns QueryResult with QueryResultKind (Video, Playlist, Channel, Search)

### Desktop Layer (`src/Desktop/`)

**Framework** (`Framework/`):
- `ViewModelManager` - Factory for creating ViewModels via DI
- `DialogManager` - Modal dialog management with DialogHost.Avalonia
- `SnackbarManager` - **Thread-safe toast notification queue** with Material Design integration
  - Supports 4 severity levels (Info, Success, Warning, Error)
  - Auto-dismiss with configurable timeouts
  - Action button support for user interactions
  - Progress notifications (persistent, no auto-dismiss)
- `ViewModelBase` - Base class for all ViewModels using `ObservableObject`

**ViewModels** (`ViewModels/`):
- `DashboardViewModel` - Main queue UI, handles URL processing with QueryResolver integration
  - **Error categorization** - Maps exceptions to 8 error categories
  - **Suggested actions** - Context-aware error remediation
  - Batch operations and download management
- `DownloadViewModel` - Individual download item representation
- **Dialog ViewModels** in `ViewModels/Dialogs/`:
  - `SettingsViewModel` - App settings with 3 tabs (General, Downloads, Advanced)
  - `AuthSetupViewModel` - Google authentication for private content
  - `DownloadSingleSetupViewModel` - Single video download configuration
  - `DownloadMultipleSetupViewModel` - Playlist/channel download configuration
  - `MessageBoxViewModel` - Generic message/confirm dialogs
  - `ErrorDialogViewModel` - **Rich error display with suggested actions**

**Services** (`Services/`):
- `SettingsService` - User preferences with JSON persistence (uses Cogwheel)
- `UpdateService` - Auto-updates via Onova package
- `NotificationService` - Toast implementation wrapping SnackbarManager

**Controls** (`Controls/`):
- `SnackbarHost` - Material Design snackbar UI component with queue visualization
- `LoadingIndicator` - Circular progress with configurable message and subtext

**Views** (`Views/Dialogs/`):
- **DownloadSingleSetupDialog** - Video info, quality/format selector, file path picker
- **DownloadMultipleSetupDialog** - Video checklist with virtualization, batch settings
- **SettingsDialog** - TabControl with 3 sections (General, Downloads, Advanced)
- **AuthSetupDialog** - WebView for Google authentication with instructions
- **MessageBoxDialog** - Icon, title, message, primary/secondary buttons
- **ErrorDialog** - Error icon, message, expandable details, category badge, action buttons

### Shared Models (`src/Shared/Models/`)

**DownloadItem** - Observable model representing a download:
- Properties: Video, FilePath, Status, Progress, timestamps
- **Byte-level tracking**: BytesDownloaded, TotalBytes, PartialFilePath
- Computed properties: Title, Author, Duration, ThumbnailUrl, BytesProgress
- Action flags: CanPause, CanResume, CanCancel, CanRestart

**DownloadStatus** enum: Queued, Started, Paused, Completed, Failed, Canceled

**FormatProfile** - Presets for quality/format combinations

**QueryResult** - Result of URL/query resolution:
- Properties: Kind (QueryResultKind), Video (single), Videos (multiple), Title, Description, Author
- Used by QueryResolver for all YouTube query types

**QueryResultKind** enum: Video, Playlist, Channel, Search

**ErrorInfo** - Structured error information:
- Properties: Message, Details, Category (ErrorCategory), SuggestedActions (List<ErrorAction>)
- **ErrorCategory** enum: Unknown, Network, Permission, InvalidUrl, FileSystem, YouTube, VideoNotAvailable, FormatNotAvailable
- **ErrorAction**: Text, ActionKey, Description

**CachedVideo** - Serializable video metadata for caching:
- Properties: Id, Title, Author, AuthorChannelId, Duration, Description, Keywords, Thumbnails, UploadDate
- Used by CacheService for SQLite persistence

## Dependencies

### Core Packages
- **YoutubeExplode 6.5.4+** - YouTube API access and video resolution
- **YoutubeExplode.Converter** - Media conversion with FFmpeg integration
- **Gress** - Progress reporting
- **Microsoft.Data.Sqlite 9.0.0** - Caching database

### UI Packages
- **Avalonia 11.3.0** - Cross-platform UI framework
- **Material.Avalonia 3.9.2** - Material Design components
- **DialogHost.Avalonia** - Modal dialogs
- **CommunityToolkit.Mvvm 8.4.0** - MVVM helpers

### Development
- **xUnit** - Test framework
- **Moq** - Mocking
- **FluentAssertions** - Assertion library
- **CSharpier.MsBuild** - Code formatting

## Configuration

- **Directory.Build.props** - Shared MSBuild properties (targets .NET 9.0, enables nullable, treats warnings as errors)
- **Settings**: Stored in user AppData via SettingsService (Cogwheel-based)
- **Material Theme**: Custom colors defined in `App.axaml.cs:117` (Light: #343838/#F9A825, Dark: #E8E8E8/#F9A825)

## Development Notes

### Event Handling Pattern
The codebase uses `DisposableCollector` to manage event subscriptions. ViewModels subscribe to service events and collect disposables:
```csharp
_eventRoot.Add(_settingsService.WatchProperty(o => o.Theme, OnThemeChanged));
```

### Observable Properties
ViewModels use CommunityToolkit.Mvvm's `[ObservableProperty]` attribute with partial classes:
```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
private string? _query;
```

### FFmpeg Integration
YoutubeExplode.Converter automatically finds FFmpeg in the output directory. The build process copies `ffmpeg.exe` (Windows) or `ffmpeg` (Unix) to the output.

## Testing

Tests are in `src/Tests/` using xUnit. Run all tests with `dotnet test` or specific tests with `--filter`.

The test project references both Core and Shared projects and uses Moq for service mocking.

### Test Coverage
- **DownloadServiceTests** (10 tests) - Pause, resume, cancel, restart, byte progress
- **DownloadStateRepositoryTests** (7 tests) - State persistence and retrieval
- **CacheServiceTests** (5 tests) - Metadata caching and expiration
- **DownloadItemTests** (6 tests) - Model state management
- **QueryResolverTests** (10 tests) - URL parsing and validation

## New Features (Session 4)

### Pause/Resume Functionality ✅
- **Chunked HTTP downloads** with Range header support for resumable downloads
- **Per-download cancellation** - Each download has its own CancellationTokenSource
- **State persistence** - DownloadStateRepository saves progress to SQLite every 1MB
- **Partial file management** - Downloads saved to `.part` files, renamed on completion
- **Resume validation** - Verifies partial file size matches saved state before resuming
- **17 unit tests** with full coverage of pause/resume scenarios

### UI Feedback System ✅
- **SnackbarManager** - Thread-safe Material Design toast notification queue
  - 4 severity levels with distinct colors and icons
  - Auto-dismiss (3-5 seconds) with manual close option
  - Action buttons for user interactions
  - Progress notifications (persistent until dismissed)
- **ErrorDialog** - Rich error display with:
  - 8 error categories (Network, Permission, InvalidUrl, etc.)
  - Expandable details section with full stack trace
  - Suggested actions based on error type
  - Copy to clipboard functionality
- **LoadingIndicator** - Material circular progress with configurable messages
- **Error categorization** - Intelligent exception mapping to user-friendly categories

### Dialog Views ✅
All 5 Material Design dialogs created with complete AXAML markup:
- **DownloadSingleSetupDialog** - Video info, quality/format selectors, file path picker
- **DownloadMultipleSetupDialog** - Video checklist with virtualization, batch configuration
- **SettingsDialog** - 3-tab interface (General, Downloads, Advanced)
- **AuthSetupDialog** - WebView placeholder for Google authentication
- **MessageBoxDialog** - Flexible message/confirmation dialog

### QueryResolver ✅
- **URL parsing** - Regex extraction for all YouTube URL formats
- **Multi-source support** - Videos, playlists, channels, @handles, search queries
- **Cache integration** - Checks CacheService before API calls (24-hour TTL)
- **Automatic fallback** - Search queries when URL parsing fails

## Related Resources

- Original project: [YoutubeDownloader by Tyrrrz](https://github.com/Tyrrrz/YoutubeDownloader)
- YoutubeExplode docs: https://github.com/Tyrrrz/YoutubeExplode
- Avalonia docs: https://docs.avaloniaui.net/
- 🔄 Enhanced Workflow Template (Based on Actual Usage)

  📍 START OF SESSION (3-5 minutes)

  1. Context Loading (2-3 minutes)
  # Open last session's Quick Resume
  # Location: guides-and-instructions/chats/Session_[NN]_*.md
  # Read ONLY the "Quick Resume" section (3-5 bullets)

  2. Check Git Status (1 minute)
  git status
  git log --oneline -5  # See recent work

  3. Review Next Steps (1 minute)
  - Read "Next Session Priorities" from previous session
  - Confirm task with user if unclear

  4. Optional: Check Session Index (30 seconds)
  - If unsure about past work: Ctrl+F in SESSION_INDEX.md
  - Find related sessions by keyword

  Total: 3-5 minutes (vs. 20-30 min without docs)

  ---
  🛠️ DURING SESSION (Variable)

  Real Pattern Observed:

  User Request → Immediate Action Loop:
  1. User provides request ("fix hamburger menu", "organize files")
  2. You analyze and plan (use TodoWrite for multi-step tasks)
  3. You implement immediately (Read → Edit/Write → Test)
  4. User gives feedback ("too small", "works great")
  5. You iterate quickly (adjust and validate)
  6. Repeat until complete

  Key Behaviors:
  - ✅ Use TodoWrite for tasks with 3+ steps (helps track progress)
  - ✅ Read files before editing (required by tools)
  - ✅ Make multiple parallel tool calls when possible (efficiency)
  - ✅ Commit frequently (atomic commits per feature/fix)
  - ✅ Push to GitHub after major work (validate deployment)

  Documentation During Session:
  - ❌ Don't stop to document mid-session
  - ✅ Keep mental notes of decisions made
  - ✅ Use git commit messages to capture "what" (session doc will capture "why")

  ---
  🎬 END OF SESSION (10-12 minutes)

  User Signals:
  - "let's wrap up"
  - "that's good for now"
  - Asks for summary/documentation
  - Natural stopping point reached

  Closing Checklist:

  1. Final Commit & Push (2 minutes)
  git add -A
  git status  # Verify changes
  git commit -m "Descriptive message"
  git push origin main

  2. Invoke Session Documentation Agent (5 minutes)
  User says:
  Create session documentation for today's work
  or
  Document Session [NN]: [brief description]

  Agent generates:
  - Quick Resume (3-5 bullets)
  - Key Accomplishments (detailed)
  - File changes with line numbers
  - Technical decisions with rationale
  - Next session priorities

  3. Review Generated Doc (2 minutes)
  - Check Quick Resume accuracy (most critical)
  - Verify file paths and line numbers
  - Confirm next steps are clear

  4. Update Session Index (1 minute)
  Agent will remind you to add entry to:
  guides-and-instructions/chats/SESSION_INDEX.md

  Quick add:
  - Session number and title
  - Date
  - Keywords
  - Related sessions

  5. Commit Documentation (1 minute)
  git add guides-and-instructions/chats/Session_[NN]_*.md
  git add guides-and-instructions/chats/SESSION_INDEX.md  # if updated
  git commit -m "Add Session [NN] documentation"
  git push origin main

  Total: 10-12 minutes

  ---
  📊 Time Investment Analysis

  Per Session:
  | Activity               | Time      | Value                            |
  |------------------------|-----------|----------------------------------|
  | Start: Context loading | 3-5 min   | Get up to speed fast             |
  | During: Normal work    | Variable  | Focus on coding, not documenting |
  | End: Generate docs     | 5 min     | Automated with agent             |
  | End: Review & commit   | 5-7 min   | Quality check                    |
  | Total overhead         | 13-17 min | vs. 0 min without docs           |

  But Next Session:
  | Activity         | Without Docs | With Docs | Savings   |
  |------------------|--------------|-----------|-----------|
  | Remember context | 20-30 min    | 3-5 min   | 15-25 min |
  | Find past work   | 10-15 min    | 30 sec    | 9-15 min  |
  | Recall decisions | 5-10 min     | 1 min     | 4-9 min   |
  | Total            | 35-55 min    | 5 min     | 30-50 min |

  NET BENEFIT PER SESSION PAIR: +13 to +33 minutes saved

  Over 20 Sessions: Save 4-11 hours of context-loading time

  ---
  🎯 Pattern-Specific Optimizations

  When User Has Multiple Requests

  Observed Pattern (Session 23):
  User: "analyze directory and organize files"
  You: [analyze] → [create plan] → [execute cleanup] → "done"
  User: "also the hamburger menu doesn't work"
  You: [diagnose] → [fix CSS/JS] → "done"
  User: "and the hero image is tilted"
  You: [fix transform] → "done"
  User: "create a session documentation agent"
  You: [design agent] → [write spec] → "done"

  Optimization:
  - ✅ Use TodoWrite at start if multiple requests (track progress)
  - ✅ Complete each task fully before moving to next
  - ✅ Mark todos as completed immediately (don't batch)
  - ✅ Commit after each major accomplishment (atomic commits)

  ---
  When User Requests Documentation

  Triggers:
  - "create session doc"
  - "document this session"
  - "summarize what we did"
  - End of session reached

  Immediate Action:
  1. Run git commands to gather metadata
  2. Analyze conversation for key accomplishments
  3. Generate 15-section doc with Quick Resume
  4. Save to guides-and-instructions/chats/
  5. Remind user to update Session Index

  ---
  When Starting New Session

  If User Says Nothing:
  1. Check if there's a "Next Steps" from previous session
  2. Proactively mention: "Last session we [X]. Should we continue with [Y]?"
  3. Wait for confirmation before proceeding

  If User Provides New Task:
  1. Quickly scan previous session's Quick Resume (30 sec)
  2. Check if related to past work
  3. If yes, mention: "This relates to Session [NN] where we [X]"
  4. Proceed with new task

  ---
  🚨 Anti-Patterns to Avoid

  ❌ DON'T:
  - Stop mid-session to write documentation (breaks flow)
  - Create session docs for trivial changes (typo fixes, single-line edits)
  - Skip git commits thinking "I'll commit at end" (lose atomic history)
  - Generate docs before pushing to GitHub (might forget to push)
  - Read entire previous session doc (just Quick Resume is enough)

  ✅ DO:
  - Commit frequently with clear messages
  - Use TodoWrite for complex multi-step tasks
  - Ask user for clarification if context is unclear
  - Mark todos as completed immediately
  - Generate session docs at END of every significant session
  - Update Session Index right after creating session doc

  ---
  🔧 Tool Usage Patterns

  Based on Session 23 Actual Usage:

  File Operations:
  Read → Edit/Write (required sequence)
  Never edit without reading first (tool requirement)

  Git Operations:
  Status → Add → Commit → Push (standard flow)
  Check status before committing (verify what's staged)

  Multiple Independent Tasks:
  Use parallel tool calls (multiple Read, multiple Bash in one message)
  Maximizes efficiency

  Dependent Tasks:
  Sequential tool calls (wait for result before next)
  E.g., Read file → Edit file → Commit

  ---
  📋 Checklist Template

  Start of Session ✅

  - Read previous session's Quick Resume (2 min)
  - Check git status and recent commits (1 min)
  - Review "Next Steps" or ask user for task (1 min)
  - Create TodoWrite list if multi-step task

  During Session ✅

  - Use TodoWrite to track progress (3+ step tasks)
  - Read files before editing (tool requirement)
  - Commit after each major accomplishment
  - Push to GitHub after significant work
  - Mark todos completed immediately

  End of Session ✅

  - Final commit and push to GitHub
  - Generate session documentation (invoke agent)
  - Review Quick Resume for accuracy
  - Update Session Index with new entry
  - Commit documentation files
  - Push documentation to GitHub

  ---
  🎓 Lessons from Session 23

  What Worked Well:
  - ✅ TodoWrite tracked 9 tasks through directory cleanup
  - ✅ Multiple parallel tool calls for efficiency (ls, grep, find)
  - ✅ Atomic commits (mobile nav separate from file cleanup separate from docs)
  - ✅ Quick Resume section made session doc scannable

  What to Improve:
  - ⚠️ Could have tested mobile nav on real device before pushing (deferred to Session 24)
  - ⚠️ Could have run Lighthouse audit for before/after comparison (deferred)
  - ⚠️ Session Index could have been created during session (done at end instead)

  Takeaways:
  1. Real device testing should be part of mobile feature completion
  2. Performance baselines should be captured before optimizations
  3. Session Index can be updated incrementally (don't wait for end)

  ---
  🚀 Optimal Workflow Summary

  Golden Rule: Document at END, not during. Focus on shipping code first, capturing context second.

  Time Breakdown:
  - Start: 3-5 min (context loading)
  - During: 0 min overhead (just work normally)
  - End: 10-12 min (docs + commits)
  - Total: 13-17 min per session
  - Savings next session: 30-50 min
  - Net benefit: +13 to +33 min per session pair

  For 20 Sessions:
  - Time invested: 4-6 hours
  - Time saved: 10-17 hours
  - Net gain: 4-11 hours

  Plus Intangible Benefits:
  - No more "what did I do last time?"
  - Searchable history of all decisions
  - Easy onboarding for new contributors
  - Audit trail for troubleshooting