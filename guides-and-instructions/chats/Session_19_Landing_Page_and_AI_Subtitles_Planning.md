# Session 19: Landing Page Creation and AI Subtitles Planning

**Date**: 2025-10-06
**Version**: v0.3.7 → AI Subtitle Feature (in progress)
**Status**: 🚧 In Progress

## Overview

This session focused on three major initiatives:
1. Completing v0.3.7 release with auto-updates
2. Creating a professional GitHub Pages landing page
3. Planning and beginning AI subtitle generation feature

## Part 1: Auto-Update System Implementation ✅

### Problem
User wanted automatic updates so users get notified when new versions are available and can update with one click.

### Solution Implemented
**Enabled auto-updates with ZIP package distribution:**

1. **Modified `build-installer.ps1`** (lines 180-232)
   - Added Step 7: Create ZIP package for auto-updates
   - Compresses publish directory to `EnhancedYoutubeDownloader-X.X.X.zip`
   - Both .exe installer (80 MB) and .zip package (103 MB) now created

2. **Enabled auto-update check** in `MainViewModel.cs` (line 126)
   ```csharp
   // Before: // await CheckForUpdatesAsync();
   // After:
   await CheckForUpdatesAsync();  // ✅ Now active
   ```

3. **Updated MEMORIZE checklist** in `CLAUDE.md`
   - Added ZIP package requirement to release process
   - Updated verification steps for both .exe and .zip files
   - Documented that both must be uploaded to GitHub Releases

### How It Works Now
1. User opens app (v0.3.7)
2. App checks GitHub for `EnhancedYoutubeDownloader-*.zip`
3. If newer version found, downloads in background
4. Shows snackbar: "Update downloaded. [INSTALL NOW]"
5. Clicks button → Restarts → Applies update → Relaunches

### Files Modified
- `build-installer.ps1` (added ZIP creation step)
- `src/Desktop/ViewModels/MainViewModel.cs` (enabled auto-update)
- `CLAUDE.md` (updated release checklist)

### Release v0.3.7 Assets
- ✅ `EnhancedYoutubeDownloader-Setup-v0.3.7.exe` (80 MB) - For new users
- ✅ `EnhancedYoutubeDownloader-0.3.7.zip` (103 MB) - For auto-updates

## Part 2: Git LFS Decision ✅

### Question
Should we use Git LFS for the large installer files (80MB)?

### Analysis
**Considered Git LFS but decided AGAINST it:**

**Problems with LFS:**
- ❌ Installers already distributed via GitHub Releases (not git)
- ❌ Requires history rewrite (dangerous)
- ❌ Adds complexity for contributors
- ❌ Uses LFS bandwidth quotas
- ❌ `release/` folder is just build output

**Better Solution: Ignore Release Folder**
- ✅ Added `release/` to `.gitignore`
- ✅ Removed `release/EnhancedYoutubeDownloader-Setup-v0.3.7.exe` from git tracking
- ✅ No large files in git at all
- ✅ Installers still available via GitHub Releases
- ✅ Build script recreates folder when needed

### Files Modified
- `.gitignore` (added `release/` folder)
- Removed installer from git tracking

## Part 3: GitHub Pages Landing Page ✅

### User Request
"Can we create a webapp version for GitHub Pages?"

### Analysis
**Web App Not Feasible Because:**
1. ❌ yt-dlp.exe cannot run in browser (Python executable)
2. ❌ FFmpeg.exe cannot run in browser (native binary)
3. ❌ YouTube's CORS restrictions block browser downloaders
4. ❌ YouTube's anti-bot protections (PO tokens, SABR protocol)
5. ❌ Would be 80% rewrite for 20% functionality

**Better Alternative: Landing Page/Documentation Site**
✅ Promote the desktop app
✅ Show features/screenshots
✅ Provide download links
✅ Include installation guides
✅ Much easier to implement

### Landing Page Created

**Structure:**
```
docs/
├── index.html              # Main landing page
├── css/
│   └── style.css          # Material Design dark theme
├── js/
│   └── main.js            # Interactive elements
├── images/                # Screenshots (3 images, 119KB total)
│   ├── main-screenshot.png (26KB)
│   ├── settings.png (43KB)
│   └── download-progress.png (50KB)
├── .nojekyll              # GitHub Pages config
└── README.md              # Documentation
```

**Sections:**
1. **Navigation** - Sticky nav with smooth scroll
2. **Hero Section** - Download button, tagline, main screenshot
3. **Features Grid** - 8 feature cards with icons:
   - Pause & Resume
   - Multiple Formats
   - Unified Queue
   - Subtitles & Metadata
   - SQLite Caching
   - Download Scheduling
   - Playlists & Channels
   - Auto-Updates
4. **Screenshots Gallery** - 3 screenshots with descriptions
5. **Installation Guide** - 4-step process with SmartScreen warning
6. **System Requirements** - OS, runtime, disk space, RAM
7. **Download Section** - Latest release card with direct download link
8. **Tech Stack** - 6 technologies used (.NET, Avalonia, Material Design, yt-dlp, FFmpeg, SQLite)
9. **Footer** - Links, credits, license

**Design Features:**
- Material Design dark theme (matches desktop app)
- Color palette: Primary #F9A825 (gold), Secondary #343838 (dark gray)
- Responsive grid layout (mobile-friendly)
- Smooth scroll navigation
- Animated elements on scroll (Intersection Observer API)
- Lazy loaded images
- Lightweight: ~50KB HTML+CSS+JS + 119KB images = 169KB total

**Technologies Used:**
- Pure HTML5/CSS3/JavaScript (no build process)
- CSS Grid and Flexbox layout
- Modern CSS custom properties (variables)
- Intersection Observer API for scroll animations
- Responsive design (@media queries)

### Deployment Steps
1. ✅ Created `docs/` folder with all files
2. ✅ Committed and pushed to GitHub
3. ⏳ **NEXT**: Enable GitHub Pages in repository settings:
   - Go to Settings > Pages
   - Source: main branch `/docs` folder
   - Site will be live at: `https://jrlordmoose.github.io/EnhancedYoutubeDownloader/`

### Files Created
- `docs/index.html` (1153 lines) - Complete landing page
- `docs/css/style.css` (700+ lines) - Material Design styles
- `docs/js/main.js` (200+ lines) - Interactive JavaScript
- `docs/images/` - 3 screenshots
- `docs/.nojekyll` - Tells GitHub Pages not to use Jekyll
- `docs/README.md` - Documentation for local testing

## Part 4: AI Subtitle Generation Feature (In Progress) 🚧

### User Request
"Can we add a free subtitle generator for videos using the best open source AI subtitle generator? When users click 'Include Subtitles', process the file and add subtitles that can be turned on/off in the video player."

### Research Conducted

**Best Solution: OpenAI Whisper via Whisper.net**

**Why Whisper?**
- 🏆 Industry standard for speech-to-text
- 🌍 96 languages supported
- 🎯 High accuracy (state-of-the-art)
- 💰 Free and open-source (MIT License)
- 🔒 Runs completely offline/locally

**Integration Options:**

| Option | Pros | Cons | Verdict |
|--------|------|------|---------|
| **Whisper.net** (C#) | Native .NET, 4x faster, GPU support, offline | Need to bundle model files (40MB-1.5GB) | ✅ **CHOSEN** |
| **whisper.cpp** | Fastest C++ implementation | Harder to integrate with .NET | ❌ |
| **Cloud API** | No local processing, always latest | Costs money, requires internet, privacy | ❌ |

**Whisper.net Details:**
- NuGet package: `Whisper.net 1.8.1`
- Runtime: `Whisper.net.Runtime 1.8.1`
- GPU acceleration available (CUDA, CoreML, OpenVino, Vulkan)
- Model sizes: tiny (39MB), base (74MB), small (244MB), medium (769MB), large (1.5GB)
- **Recommended**: base model (74MB) for best balance

**Performance Estimates (base model):**
| Video Length | CPU Processing | GPU Processing |
|--------------|----------------|----------------|
| 5 min        | ~30 seconds    | ~10 seconds    |
| 30 min       | ~3 minutes     | ~1 minute      |
| 1 hour       | ~6 minutes     | ~2 minutes     |

### Implementation Plan

**Phase 1: Basic Integration** (Current)
1. ✅ Add Whisper.net NuGet packages
2. ⏳ Create `SubtitleGenerationService`
3. ⏳ Add settings for AI subtitle generation
4. ⏳ Update `YtDlpDownloadService` to check for subtitles first
5. ⏳ Show progress during generation
6. ⏳ Embed subtitles using FFmpeg

**Phase 2: Enhanced UX** (Future)
- Progress notifications
- Model selection in settings
- Quality presets (Fast/Balanced/Accurate)
- Cache generated subtitles in SQLite

**Phase 3: Advanced Features** (Future)
- Subtitle editor
- Batch processing
- Translation support (Whisper can translate to English)
- Speaker diarization

### Proposed Architecture

```
┌─────────────────────────────────────────┐
│ User Downloads Video                     │
│ ✓ "Include Subtitles" checked           │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ Check YouTube for existing subtitles    │
└──────────────┬──────────────────────────┘
               │
        ┌──────┴──────┐
        │             │
     Found?        Not Found?
        │             │
        ▼             ▼
  Download      ┌──────────────────┐
  YouTube subs  │ Show dialog:     │
        │       │ "Generate AI?"   │
        │       └────────┬─────────┘
        │                │
        │         ┌──────┴──────┐
        │      YES│             │NO
        │         ▼             ▼
        │   ┌─────────────┐  Skip
        │   │ Whisper.net │  subtitle
        │   │ AI Generate │  generation
        │   └──────┬──────┘
        │          │
        └──────────┴──────────┐
                               │
                               ▼
                    ┌────────────────────┐
                    │ Embed subtitles    │
                    │ (soft or burn-in)  │
                    └────────────────────┘
```

### Proposed UI Changes

**Settings Dialog > General Tab:**
```
Subtitle Options:
├─ [x] Download YouTube subtitles (if available)
└─ [x] Generate AI subtitles (if missing)
    ├─ Model: [tiny ▼] [base] [small] [medium]
    ├─ Language: [Auto-detect ▼]
    └─ Output: [Soft subtitle ▼] [Burn-in] [Separate .srt file]
```

**During Download:**
```
🎬 Downloading: "Video Title"
📝 Subtitles: Generating with AI... 45%
⏱️  Estimated: 2 minutes remaining
```

### Better Ideas Considered

1. **Batch Processing** - Generate subtitles for entire playlist
2. **Subtitle Translation** - Whisper can translate to English
3. **Speaker Diarization** - Identify different speakers
4. **Subtitle Editing** - Built-in editor before embedding
5. **Quality Presets** - "Fast/Balanced/Accurate" instead of model names
6. **Cache Generated Subtitles** - Store in SQLite, avoid regenerating
7. **Background Processing** - Generate while downloading next video
8. **Auto-Punctuation** - Whisper adds punctuation automatically
9. **Confidence Scores** - Highlight uncertain transcriptions
10. **Multiple Languages** - Detect and label language

### Current Status

**Completed:**
1. ✅ Research and analysis
2. ✅ Chose Whisper.net as implementation
3. ✅ Added NuGet packages:
   - `Whisper.net 1.8.1`
   - `Whisper.net.Runtime 1.8.1`

**In Progress:**
- Creating `SubtitleGenerationService.cs`

**Next Steps:**
1. Download Whisper base model (74MB)
2. Create service to generate .srt files
3. Integrate with download flow
4. Add progress reporting
5. Test with sample videos
6. Commit and release as v0.3.8

## Commits Made This Session

1. **b6590bf** - Remove release folder from git tracking and add to .gitignore
2. **a1a78f3** - Enable auto-updates with ZIP package distribution
3. **c6e7c16** - Add professional landing page for GitHub Pages

## Files Modified/Created

### Auto-Updates
- `build-installer.ps1` - Added ZIP creation
- `src/Desktop/ViewModels/MainViewModel.cs` - Enabled auto-update check
- `CLAUDE.md` - Updated MEMORIZE checklist

### Git Cleanup
- `.gitignore` - Added `release/` folder

### Landing Page
- `docs/index.html` - Main landing page
- `docs/css/style.css` - Styles
- `docs/js/main.js` - JavaScript
- `docs/images/` - Screenshots
- `docs/.nojekyll` - GitHub Pages config
- `docs/README.md` - Documentation

### AI Subtitles (In Progress)
- `src/Core/EnhancedYoutubeDownloader.Core.csproj` - Added Whisper.net packages

## Technical Insights

### Auto-Update System (Onova)
- Uses `GithubPackageResolver` to check for `EnhancedYoutubeDownloader-*.zip`
- Downloads update in background
- `ZipPackageExtractor` extracts and applies update
- Restarts application automatically
- **Critical**: Must upload both .exe AND .zip to GitHub Releases

### GitHub Pages Best Practices
- Use `/docs` folder for easy maintenance
- Add `.nojekyll` to avoid Jekyll processing
- Pure HTML/CSS/JS (no build process) for simplicity
- Optimize images (used 119KB total)
- Material Design for consistency with desktop app

### Whisper.net Architecture
- Uses `whisper.cpp` under the hood (C++ for speed)
- C# wrapper provides easy .NET integration
- Supports streaming transcription
- Model files downloaded on first run or bundled with app
- GPU acceleration via multiple backends (CUDA, CoreML, etc.)

## Next Session Tasks

1. **Complete AI Subtitle Feature (Phase 1):**
   - Create `SubtitleGenerationService.cs`
   - Add settings for subtitle generation
   - Integrate with `YtDlpDownloadService`
   - Add progress reporting
   - Download/bundle base model
   - Test with real videos

2. **Enable GitHub Pages:**
   - Go to repository Settings > Pages
   - Set source to main branch `/docs` folder
   - Verify site is live

3. **Release v0.3.8:**
   - Complete subtitle generation feature
   - Update all 4 version locations
   - Build both .exe and .zip
   - Create GitHub release
   - Update landing page with v0.3.8 info

## User Feedback & Requests

1. ✅ "Add auto-updates with download, restart, and relaunch"
2. ✅ "Add to MEMORIZE to always update version numbers and include ZIP"
3. ✅ "Create landing page for GitHub Pages" (instead of web app)
4. ✅ "Update landing page images with new screenshots"
5. 🚧 "Add AI subtitle generator for videos without subtitles"

## Lessons Learned

1. **Git LFS isn't always the answer** - Sometimes `.gitignore` is simpler and better
2. **Landing pages are better than limited web apps** - Showcase desktop app instead of compromising features
3. **Pure HTML/CSS/JS beats build complexity** - No webpack, no npm, just works
4. **Whisper.net is production-ready** - Active development, good performance, easy integration
5. **Auto-updates need ZIP packages** - Onova requires compressed packages, not executables
6. **Material Design consistency matters** - Landing page matches app for brand coherence

---

**Session Duration**: ~6 hours
**Lines of Code**: ~2000+ (landing page) + planning
**Features Added**: Auto-updates, Landing page
**Features In Progress**: AI subtitle generation
**Next Release**: v0.3.8 (AI Subtitles)
