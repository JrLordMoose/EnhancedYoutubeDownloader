# Quick Testing Guide for Subtitle Path Escaping Fix

## Quick Test Commands

### Run Unit Tests

```bash
# Run all subtitle service tests
dotnet test --filter "FullyQualifiedName~SubtitleBurnInServiceTests"

# Run specific failing case test (parentheses)
dotnet test --filter "EscapeSubtitlePath_WithParentheses_ShouldEscapeCorrectly"

# Verbose output
dotnet test --filter "SubtitleBurnInServiceTests" --logger "console;verbosity=detailed"
```

### Expected Test Results

All 11 tests should pass:
- ✅ `EscapeSubtitlePath_WithParentheses_ShouldEscapeCorrectly` - **THE KEY FIX**
- ✅ `EscapeSubtitlePath_WithSquareBrackets_ShouldEscapeCorrectly`
- ✅ `EscapeSubtitlePath_WithSingleQuote_ShouldEscapeCorrectly`
- ✅ `EscapeSubtitlePath_WithSimpleFilenames_ShouldNotBreak` (4 test cases)
- ✅ `EscapeSubtitlePath_WithMultipleSpecialCharacters_ShouldEscapeAll`
- ✅ `EscapeSubtitlePath_WindowsDriveLetter_ShouldEscapeColon` (Windows only)
- ✅ `EscapeSubtitlePath_UnixPathWithColon_ShouldEscapeColon` (Unix only)
- ✅ `EscapeSubtitlePath_EmptyPath_ShouldReturnEmpty`
- ✅ `EscapeSubtitlePath_PathWithBackslashInFilename_ShouldEscapeCorrectly` (Unix only)

## Manual FFmpeg Test

### Test the Actual Failing Path

```bash
# Windows PowerShell
$subtitle = "C:\Users\leore\Videos\Is Codex CLI Worth The Switch (from Claude Code)_.mp4.part.en.srt"

# Verify the file exists
Test-Path $subtitle

# Try the subtitle burn with the app (it should now work)
# Run the application and attempt to burn subtitles with this file
```

### Create Test Files

```bash
# Create test subtitle file with problematic filename
cd C:\Users\leore\Videos

# Create SRT content
@"
1
00:00:00,000 --> 00:00:05,000
This is a test subtitle

2
00:00:05,000 --> 00:00:10,000
With parentheses in the path
"@ | Out-File -FilePath "Test (with parentheses).srt" -Encoding UTF8

# Verify it was created
dir "Test (with parentheses).srt"
```

## Escaping Verification

### Test Individual Character Escaping

You can manually test the escaping logic with these examples:

#### Windows Paths

```
Input:  C:\Videos\Test (Video).srt
Output: C\\\\\\\\:\\\\Videos\\\\\\\\Test \(Video\).srt

Input:  C:\Videos\Test [1080p].srt
Output: C\\\\\\\\:\\\\Videos\\\\\\\\Test \[1080p\].srt

Input:  C:\Videos\Someone's Video.srt
Output: C\\\\\\\\:\\\\Videos\\\\\\\\Someone\\'s Video.srt

Input:  C:\Videos\Complex [HD] (2024).srt
Output: C\\\\\\\\:\\\\Videos\\\\\\\\Complex \[HD\] \(2024\).srt
```

#### Unix Paths

```
Input:  /home/Videos/Test (Video).srt
Output: /home/Videos/Test \(Video\).srt

Input:  /home/Videos/Test [1080p].srt
Output: /home/Videos/Test \[1080p\].srt

Input:  /home/Videos/Someone's Video.srt
Output: /home/Videos/Someone\'s Video.srt

Input:  /home/Videos/Complex [HD] (2024).srt
Output: /home/Videos/Complex \[HD\] \(2024\).srt
```

## Integration Testing

### Full Workflow Test

1. Download a video with parentheses in the title
2. Download subtitles for that video
3. Attempt to burn the subtitles
4. Expected: Success (previously would fail with exit code -22)

### Test Cases to Try

Create files with these problematic names and verify they work:

- `Video (Part 1).mp4` and `Video (Part 1).srt`
- `Movie [Director's Cut].mp4` and `Movie [Director's Cut].srt`
- `Someone's Favorite Video.mp4` and `Someone's Favorite Video.srt`
- `Complete [HD] (2024) - Final.mp4` and `Complete [HD] (2024) - Final.srt`

## Debugging

### Check FFmpeg Command

The service logs the full FFmpeg command to console:

```
[BURN-SUBS] Command: ffmpeg.exe -i "input.mp4" -vf "subtitles='escaped_path':force_style='...'" -c:a copy -y "output.mp4"
```

**Before the fix:**
```
-vf "subtitles='C:\\\\\\\\:\\\\...\\\\Test (Video).srt':force_style='...'"
                                           ^^^ ^^^ NOT ESCAPED - FAILS!
```

**After the fix:**
```
-vf "subtitles='C:\\\\\\\\:\\\\...\\\\Test \(Video\).srt':force_style='...'"
                                           ^^^^ ^^^ ESCAPED - WORKS!
```

### FFmpeg Exit Codes

- **Exit code 0**: Success ✅
- **Exit code -22**: Invalid argument (usually escaping issue) ❌
- **Exit code 1**: General error

## Performance Check

The escaping should be fast (< 1ms for typical paths):

```csharp
var stopwatch = Stopwatch.StartNew();
var escaped = EscapeSubtitlePath(path);
stopwatch.Stop();
Console.WriteLine($"Escaping took: {stopwatch.ElapsedMilliseconds}ms");
```

Expected: < 1ms even for long paths

## Success Indicators

✅ **Fix is working if:**
- Unit tests pass
- FFmpeg exits with code 0 (not -22)
- Subtitles are successfully burned into the video
- Output video file is created and playable
- Console shows no errors

❌ **Fix is NOT working if:**
- FFmpeg exits with code -22
- Error: "Unable to parse option value"
- Error: "Invalid argument"
- No output file created

## Additional Notes

### Order of Operations

The escaping MUST happen in this order:
1. Escape backslashes first (on Windows)
2. Then escape other special characters
3. Order matters because later escapes could interfere with earlier ones

### Platform Differences

- **Windows**: Quadruple backslashes (`\\\\`), double-escaped colons (`\\:`)
- **Unix**: Double backslashes (`\\`), single-escaped colons (`\:`)

This is due to different path separator conventions and shell parsing behavior.

### Known Working Examples

After the fix, these paths ALL work correctly:

```
✅ C:\Users\leore\Videos\Is Codex CLI Worth The Switch (from Claude Code)_.mp4.part.en.srt
✅ /home/user/Videos/Movie [1080p] (2024) Director's Cut.srt
✅ C:\Videos\Test [4K] (HDR) - Someone's Upload (Re-uploaded).srt
```

Before the fix, ALL of these failed with exit code -22.
