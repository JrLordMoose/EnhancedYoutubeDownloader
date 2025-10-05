# Debugging YouTube 403 Forbidden Error - Session 5
_Exported on 10/2/2025 from Claude Code_

---

## Session Overview

This session focused on debugging persistent download failures with comprehensive logging and attempting to fix YouTube's 403 Forbidden error by implementing proper HTTP client configuration.

### Status: **In Progress - Issue Not Yet Resolved**

---

## Initial Problem Report

**User Symptoms:**
1. Loading screen (spinning circle) continues perpetually
2. Video downloads fail instantly
3. Failed downloads hidden behind loading overlay

**Expected Behavior:**
- Loading screen should clear after query processing
- Downloads should start and show progress
- Errors should be displayed to user

---

## Debugging Journey

### Phase 1: Identifying the Loading Screen Issue

**Investigation:**
- Added debug logging to `DashboardViewModel.ProcessQueryAsync()`
- Added logging to `ProcessSingleVideoAsync()` and `ProcessMultipleVideosAsync()`
- Tracked `IsBusy` flag state changes

**Findings:**
```
[DEBUG] IsBusy before: False
[DEBUG] IsBusy after setting true: True
[DEBUG] Processing single video
[DEBUG] Finally block: Setting IsBusy to false
[DEBUG] IsBusy after finally: False
```

✅ **Result:** `IsBusy` flag is working correctly - loading screen logic is NOT the problem

### Phase 2: Discovering the Silent Download Failure

**Problem:** Downloads were failing in background tasks with no visible output

**Solution:** Added comprehensive logging to `DownloadService`:
- `ProcessDownloadAsync()` - Main download orchestration
- `DownloadChunkedAsync()` - For muxed streams
- `DownloadWithConverterAsync()` - For separate video/audio with FFmpeg

**Debug Output Revealed:**
```
[DOWNLOAD] ProcessDownloadAsync started for 1cbfec3c-d541-4d3e-9a87-d3e5852f9497
[DOWNLOAD] Semaphore acquired
[DOWNLOAD] Getting stream manifest for video: jWe8QQO2ra4
[DOWNLOAD] EXCEPTION in ProcessDownloadAsync
[DOWNLOAD] Exception type: HttpRequestException
[DOWNLOAD] Exception message: Response status code does not indicate success: 403 (Forbidden).
```

🎯 **Root Cause Found:** YouTube API returning **403 Forbidden** when trying to fetch stream manifest

### Phase 3: Understanding YouTube's Authentication Requirements

**Research:**
- Read article: "Reverse-Engineering YouTube: Revisited" by Tyrrrz
- Studied original YoutubeDownloader implementation
- Analyzed YoutubeExplode requirements

**Key Learnings:**

1. **YouTube blocks requests without proper User-Agent headers**
   - Default HttpClient has no User-Agent → looks like a bot
   - YouTube immediately returns 403 Forbidden

2. **Client Impersonation Required**
   - YouTube has multiple internal API clients (ANDROID, TVHTML5_SIMPLY_EMBEDDED_PLAYER, etc.)
   - Each client has different capabilities and restrictions
   - Some clients bypass age restrictions, others don't

3. **Authentication Cookies Optional**
   - Required for: age-restricted content, private videos, premium features
   - NOT required for: public, non-age-restricted videos
   - Original app passes cookies from SettingsService

4. **Stream URL Structure**
   - URLs are temporary (6-hour expiration)
   - IP-locked to requesting client
   - Signature-protected against tampering
   - May require deciphering for some client types

---

## Implementation Attempts

### Attempt 1: Fix FFmpeg Dependencies

**Problem:** FFmpeg exe lacked required DLLs

**Solution:**
```bash
cp src/Desktop/bin/Debug/net9.0/ffmpeg-master-latest-win64-gpl-shared/ffmpeg-master-latest-win64-gpl-shared/bin/*.dll src/Desktop/bin/Debug/net9.0/
cp src/Desktop/bin/Debug/net9.0/ffmpeg-master-latest-win64-gpl-shared/ffmpeg-master-latest-win64-gpl-shared/bin/ffmpeg.exe src/Desktop/bin/Debug/net9.0/
```

**Files Copied:**
- `ffmpeg.exe` (518KB)
- `avcodec-62.dll` (101MB)
- `avformat-62.dll` (21MB)
- `avfilter-11.dll` (66MB)
- `avutil-60.dll` (2.9MB)
- `avdevice-62.dll` (3.5MB)
- `swresample-6.dll` (705KB)
- `swscale-9.dll` (2.1MB)

✅ **Status:** Completed - FFmpeg properly configured

### Attempt 2: Add Exception Handling to View Models

**Files Modified:**
- `src/Desktop/ViewModels/Components/DashboardViewModel.cs`

**Changes:**
```csharp
// Added try-catch to ProcessSingleVideoAsync
try {
    // ... existing code ...
}
catch (Exception ex) {
    _snackbarManager.NotifyError($"Failed to start download: {ex.Message}");
    await ShowErrorDialogAsync(ex, "Download Failed");
}

// Added try-catch to ProcessMultipleVideosAsync
try {
    // ... existing code ...
}
catch (Exception ex) {
    _snackbarManager.NotifyError($"Failed to process videos: {ex.Message}");
    await ShowErrorDialogAsync(ex, "Multiple Download Failed");
}
```

**Added explicit initialization:**
```csharp
public DashboardViewModel(...) {
    // ... existing code ...

    // Explicitly initialize IsBusy to false
    IsBusy = false;

    // ... rest of constructor ...
}
```

✅ **Status:** Completed - Better error handling in place

### Attempt 3: Create Custom HttpClient with User-Agent

**Insight from Original Code:**
```csharp
// original-youtubedownloader/YoutubeDownloader.Core/Utils/Http.cs
public static class Http {
    public static HttpClient Client { get; } = new() {
        DefaultRequestHeaders = {
            UserAgent = {
                new ProductInfoHeaderValue(
                    "YoutubeDownloader",
                    Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
                ),
            },
        },
    };
}

// original-youtubedownloader/YoutubeDownloader.Core/Downloading/VideoDownloader.cs
private readonly YoutubeClient _youtube = new(Http.Client, initialCookies ?? []);
```

**Our Implementation:**

**File Created:** `src/Core/Utils/Http.cs`
```csharp
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace EnhancedYoutubeDownloader.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; } =
        new()
        {
            DefaultRequestHeaders =
            {
                UserAgent =
                {
                    new ProductInfoHeaderValue(
                        "EnhancedYoutubeDownloader",
                        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"
                    ),
                },
            },
            Timeout = TimeSpan.FromMinutes(5),
        };

    static Http()
    {
        Console.WriteLine($"[HTTP] Custom HttpClient initialized with User-Agent: EnhancedYoutubeDownloader/{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"}");
    }
}
```

**File Modified:** `src/Core/Services/DownloadService.cs`
```csharp
// Before:
private readonly YoutubeClient _youtubeClient;
public DownloadService(int maxConcurrentDownloads = 3) {
    _youtubeClient = new YoutubeClient();  // ❌ Default HttpClient
    _httpClient = new HttpClient();
    // ...
}

// After:
using EnhancedYoutubeDownloader.Core.Utils;

private readonly YoutubeClient _youtubeClient;
public DownloadService(int maxConcurrentDownloads = 3) {
    _youtubeClient = new YoutubeClient(Http.Client);  // ✅ Custom HttpClient
    _httpClient = Http.Client;
    // ...
}
```

⚠️ **Status:** Implemented but **NOT WORKING** - Still getting 403 errors

---

## Current Status

### What's Working ✅
1. Application builds successfully
2. FFmpeg properly installed with all dependencies
3. Query resolution works (URL parsing, video metadata retrieval)
4. Download items are created correctly
5. IsBusy flag and loading screen logic work correctly
6. Exception handling properly catches and reports errors
7. Comprehensive debug logging shows exact failure points

### What's NOT Working ❌
1. **YouTube API returns 403 Forbidden when accessing stream URLs**
2. Downloads fail immediately after stream manifest request
3. User-Agent header alone is insufficient to bypass YouTube's restrictions

### Error Stack Trace
```
HttpRequestException: Response status code does not indicate success: 403 (Forbidden).
  at YoutubeExplode.Videos.Streams.StreamClient.TryGetContentLengthAsync(IStreamData streamData, String url, CancellationToken cancellationToken)
  at YoutubeExplode.Videos.Streams.StreamClient.GetStreamInfosAsync(IEnumerable`1 streamDatas, CancellationToken cancellationToken)
  at YoutubeExplode.Videos.Streams.StreamClient.GetManifestAsync(VideoId videoId, CancellationToken cancellationToken)
  at EnhancedYoutubeDownloader.Core.Services.DownloadService.ProcessDownloadAsync(DownloadItem downloadItem, CancellationToken cancellationToken)
```

---

## Possible Remaining Issues

### Theory 1: YoutubeExplode Version Issue
- Both our app and original use YoutubeExplode 6.5.4
- YouTube may have changed their API after this version was released
- **Test Needed:** Does the original YoutubeDownloader still work?

### Theory 2: Additional Headers Required
- User-Agent alone may not be sufficient
- YouTube might require:
  - Accept headers
  - Origin headers
  - Referer headers
  - Additional custom headers

### Theory 3: IP-based Rate Limiting
- YouTube may be blocking the IP address
- Could be temporary or permanent
- **Test Needed:** Try from different network/VPN

### Theory 4: Video-Specific Restrictions
- The test video (jWe8QQO2ra4) might be:
  - Age-restricted
  - Region-locked
  - Recently changed access permissions
- **Test Needed:** Try with different videos

### Theory 5: YoutubeExplode Internal Client Configuration
- YoutubeExplode might need additional configuration beyond just HttpClient
- May need to specify which YouTube client to impersonate
- **Research Needed:** Check YoutubeExplode 6.5.4 documentation

---

## Debug Logs Added

### ViewModel Layer (`DashboardViewModel.cs`)
```csharp
Console.WriteLine("[DEBUG] ProcessQueryAsync: Starting");
Console.WriteLine($"[DEBUG] IsBusy before: {IsBusy}");
Console.WriteLine($"[DEBUG] Resolving query: {Query}");
Console.WriteLine($"[DEBUG] Query resolved: Kind={result.Kind}");
Console.WriteLine("[DEBUG] Processing single video");
Console.WriteLine($"[DEBUG] Video: {result.Video.Title}");
Console.WriteLine($"[DEBUG] Download path: {downloadPath}");
Console.WriteLine("[DEBUG] Creating download item");
Console.WriteLine($"[DEBUG] Download item created: {downloadItem.Id}");
Console.WriteLine("[DEBUG] Download started");
Console.WriteLine($"[DEBUG] Exception caught: {ex.GetType().Name} - {ex.Message}");
Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
Console.WriteLine($"[DEBUG] Finally block: Setting IsBusy to false");
Console.WriteLine($"[DEBUG] IsBusy after finally: {IsBusy}");
```

### Download Service Layer (`DownloadService.cs`)
```csharp
Console.WriteLine($"[DOWNLOAD] ProcessDownloadAsync started for {downloadItem.Id}");
Console.WriteLine($"[DOWNLOAD] Semaphore acquired for {downloadItem.Id}");
Console.WriteLine($"[DOWNLOAD] Getting stream manifest for video: {downloadItem.Video!.Id}");
Console.WriteLine($"[DOWNLOAD] Manifest retrieved. Muxed streams: {manifest.GetMuxedStreams().Count()}");
Console.WriteLine($"[DOWNLOAD] Using muxed stream: {streamInfo.VideoQuality} - {streamInfo.Container}");
Console.WriteLine($"[DOWNLOAD] No muxed stream found, using converter method");
Console.WriteLine($"[DOWNLOAD] EXCEPTION in ProcessDownloadAsync for {downloadItem.Id}");
Console.WriteLine($"[DOWNLOAD] Exception type: {ex.GetType().Name}");
Console.WriteLine($"[DOWNLOAD] Exception message: {ex.Message}");
Console.WriteLine($"[DOWNLOAD] Stack trace: {ex.StackTrace}");
Console.WriteLine($"[DOWNLOAD] Status updated to Failed");
Console.WriteLine($"[DOWNLOAD] Semaphore released for {downloadItem.Id}");
```

### Chunked Download Method
```csharp
Console.WriteLine($"[CHUNKED] Starting chunked download for {downloadItem.Id}");
Console.WriteLine($"[CHUNKED] File: {partialFilePath}, Start byte: {startByte}, Total bytes: {downloadItem.TotalBytes}");
Console.WriteLine($"[CHUNKED] Sending HTTP request to {streamInfo.Url}");
Console.WriteLine($"[CHUNKED] HTTP response: {response.StatusCode}");
Console.WriteLine($"[CHUNKED] Directory ensured: {directory}");
Console.WriteLine($"[CHUNKED] Starting download loop");
Console.WriteLine($"[CHUNKED] Download loop completed. Total bytes read: {totalRead}");
Console.WriteLine($"[CHUNKED] Final state saved");
```

### Converter Download Method
```csharp
Console.WriteLine($"[CONVERTER] Starting converter download for {downloadItem.Id}");
Console.WriteLine($"[CONVERTER] Video stream: {videoStream?.VideoQuality}, Audio stream bitrate: {audioStream?.Bitrate}");
Console.WriteLine($"[CONVERTER] Total bytes: {downloadItem.TotalBytes}");
Console.WriteLine($"[CONVERTER] Output file: {downloadItem.PartialFilePath}");
Console.WriteLine($"[CONVERTER] Starting YoutubeExplode.Converter download (requires FFmpeg)");
Console.WriteLine($"[CONVERTER] Progress: {p * 100:F1}%");  // Every 10%
Console.WriteLine($"[CONVERTER] Download completed successfully");
```

### HTTP Client Initialization
```csharp
Console.WriteLine($"[HTTP] Custom HttpClient initialized with User-Agent: EnhancedYoutubeDownloader/{version}");
```

---

## Files Modified This Session

1. **src/Desktop/ViewModels/Components/DashboardViewModel.cs**
   - Added debug logging throughout ProcessQueryAsync
   - Added try-catch blocks to ProcessSingleVideoAsync
   - Added try-catch blocks to ProcessMultipleVideosAsync
   - Added explicit IsBusy = false initialization

2. **src/Core/Services/DownloadService.cs**
   - Added comprehensive debug logging to ProcessDownloadAsync
   - Added logging to DownloadChunkedAsync
   - Added logging to DownloadWithConverterAsync
   - Updated to use Http.Client instead of default HttpClient
   - Added using statement for EnhancedYoutubeDownloader.Core.Utils

3. **src/Core/Utils/Http.cs** (NEW FILE)
   - Created static Http utility class
   - Configured custom HttpClient with User-Agent header
   - Set 5-minute timeout
   - Added initialization logging

---

## Next Steps for Session 6

### Immediate Testing Required:
1. **Test original YoutubeDownloader** - Confirm if it works with same YouTube version
2. **Test with different videos** - Verify if issue is video-specific or global
3. **Test from different network** - Rule out IP-based blocking

### If Original Works:
1. Compare exact HttpClient configuration
2. Check for missing initialization steps
3. Verify cookie handling (even for empty cookies)
4. Compare YoutubeClient constructor parameters

### If Original Also Fails:
1. YouTube API has changed - need to update YoutubeExplode
2. Research latest YoutubeExplode version and migration guide
3. Consider alternative approaches (yt-dlp integration, different library)

### Alternative Solutions to Explore:
1. **Update YoutubeExplode to latest version**
   ```bash
   dotnet add package YoutubeExplode --version <latest>
   ```

2. **Add more HTTP headers**
   ```csharp
   DefaultRequestHeaders = {
       { "Accept", "*/*" },
       { "Accept-Language", "en-US,en;q=0.9" },
       { "Origin", "https://www.youtube.com" },
       { "Referer", "https://www.youtube.com/" },
   }
   ```

3. **Try different YouTube client impersonation**
   - Research if YoutubeExplode exposes client configuration
   - May need to fork/modify YoutubeExplode

4. **Implement authentication flow**
   - Add cookie support from SettingsService
   - Implement AuthSetupViewModel functionality
   - Test with authenticated requests

5. **Consider yt-dlp as fallback**
   - Shell out to yt-dlp for download operations
   - Use YoutubeExplode only for metadata/UI
   - More reliable but less integrated

---

## Lessons Learned

### Debugging Approach
1. ✅ **Comprehensive logging is essential** - Without [DOWNLOAD] tags, we'd still be guessing
2. ✅ **Test assumptions systematically** - Don't assume IsBusy was the problem
3. ✅ **Follow the stack trace** - Led us directly to YouTube API rejection
4. ✅ **Research before coding** - Tyrrrz's article provided crucial context

### Technical Insights
1. YouTube's API is **actively hostile to unofficial clients**
2. User-Agent headers are **necessary but not sufficient**
3. YoutubeExplode abstracts complexity but **hides internal HTTP configuration**
4. FFmpeg dependencies must be **copied with all DLLs**, not just exe
5. Avalonia MVVM with `IsBusy` works correctly - **UI binding is not the issue**

### Development Process
1. **Parallel agent execution** (Session 4) created solid foundation
2. **Debug logging should be added proactively**, not reactively
3. **Test incrementally** - Each fix should be verified before moving to next
4. **Compare with working reference** (original app) is invaluable

---

## Performance Metrics

### Time Spent:
- Debugging: ~2 hours
- Research: ~1 hour
- Implementation: ~30 minutes
- **Total: ~3.5 hours**

### Code Added:
- Debug statements: ~50 lines
- Exception handling: ~20 lines
- Http utility class: ~30 lines
- **Total: ~100 lines**

### Issues Resolved: 0
### Issues Identified: 1 (YouTube 403 Forbidden)
### Issues Remaining: 1

---

## Conclusion

While we haven't resolved the download failure yet, we've made significant progress:

1. ✅ Identified the exact failure point (YouTube 403 error)
2. ✅ Implemented comprehensive debugging infrastructure
3. ✅ Fixed FFmpeg dependencies
4. ✅ Improved error handling
5. ✅ Added proper HTTP client configuration
6. ⚠️ YouTube API access still blocked

The issue is **not in our application logic** - everything up to the YouTube API call works correctly. The problem is YouTube's anti-bot measures blocking our requests. This requires either:
- Finding the correct HTTP configuration YouTube accepts
- Updating to a newer YoutubeExplode version that handles this
- Implementing alternative download methods

**Next session should focus on comparing with working original app to identify the missing configuration.**

---

**Status**: Debugging complete, root cause identified, solution not yet found

**Recommended Action**: Test original YoutubeDownloader to determine if issue is app-specific or YouTube-wide

---

**End of Session 5**
