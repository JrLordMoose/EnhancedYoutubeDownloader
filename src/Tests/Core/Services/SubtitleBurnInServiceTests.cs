using System.Reflection;
using System.Runtime.InteropServices;
using EnhancedYoutubeDownloader.Core.Services;
using FluentAssertions;
using Xunit;

namespace EnhancedYoutubeDownloader.Tests.Core.Services;

/// <summary>
/// Tests for SubtitleBurnInService, specifically the path escaping logic
/// </summary>
public class SubtitleBurnInServiceTests
{
    /// <summary>
    /// Helper method to access the private EscapeSubtitlePath method via reflection
    /// </summary>
    private static string CallEscapeSubtitlePath(string path)
    {
        var method = typeof(SubtitleBurnInService).GetMethod(
            "EscapeSubtitlePath",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        if (method == null)
            throw new InvalidOperationException("EscapeSubtitlePath method not found");

        var result = method.Invoke(null, new object[] { path });
        return result?.ToString() ?? string.Empty;
    }

    [Fact]
    public void EscapeSubtitlePath_WithParentheses_ShouldEscapeCorrectly()
    {
        // Arrange - The actual failing path from the issue
        var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\Users\leore\Videos\Is Codex CLI Worth The Switch (from Claude Code)_.mp4.part.en.srt"
            : "/home/user/Videos/Is Codex CLI Worth The Switch (from Claude Code)_.mp4.part.en.srt";

        // Act
        var escaped = CallEscapeSubtitlePath(path);

        // Assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: backslashes should be quadruple-escaped, colons double-escaped, parentheses single-escaped
            escaped.Should().Contain("\\(", "opening parenthesis should be escaped");
            escaped.Should().Contain("\\)", "closing parenthesis should be escaped");
            escaped.Should().Contain("\\\\\\\\", "backslashes should be quadruple-escaped");
            escaped.Should().Contain("\\\\:", "colons should be double-escaped");
        }
        else
        {
            // Unix: parentheses should be escaped with single backslash
            escaped.Should().Contain("\\(", "opening parenthesis should be escaped");
            escaped.Should().Contain("\\)", "closing parenthesis should be escaped");
        }
    }

    [Fact]
    public void EscapeSubtitlePath_WithSquareBrackets_ShouldEscapeCorrectly()
    {
        // Arrange
        var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\Videos\Video [1080p].srt"
            : "/home/Videos/Video [1080p].srt";

        // Act
        var escaped = CallEscapeSubtitlePath(path);

        // Assert
        escaped.Should().Contain("\\[", "opening bracket should be escaped");
        escaped.Should().Contain("\\]", "closing bracket should be escaped");
    }

    [Fact]
    public void EscapeSubtitlePath_WithSingleQuote_ShouldEscapeCorrectly()
    {
        // Arrange
        var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\Videos\Someone's Video.srt"
            : "/home/Videos/Someone's Video.srt";

        // Act
        var escaped = CallEscapeSubtitlePath(path);

        // Assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            escaped.Should().Contain("\\\\'", "single quote should be escaped with double backslash on Windows");
        }
        else
        {
            escaped.Should().Contain("\\'", "single quote should be escaped with single backslash on Unix");
        }
    }

    [Theory]
    [InlineData("Simple.srt")]
    [InlineData("Video Title.srt")]
    [InlineData("Video_With_Underscores.srt")]
    [InlineData("Video-With-Dashes.srt")]
    public void EscapeSubtitlePath_WithSimpleFilenames_ShouldNotBreak(string filename)
    {
        // Arrange
        var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $@"C:\Videos\{filename}"
            : $"/home/Videos/{filename}";

        // Act
        var escaped = CallEscapeSubtitlePath(path);

        // Assert
        escaped.Should().NotBeNullOrEmpty();
        escaped.Should().Contain(filename.Replace("\\", "\\\\\\\\"), "filename should be preserved (with proper escaping)");
    }

    [Fact]
    public void EscapeSubtitlePath_WithMultipleSpecialCharacters_ShouldEscapeAll()
    {
        // Arrange - Kitchen sink test with multiple special characters
        var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\Videos\It's [Great] (Really).srt"
            : "/home/Videos/It's [Great] (Really).srt";

        // Act
        var escaped = CallEscapeSubtitlePath(path);

        // Assert
        escaped.Should().Contain("\\(", "parentheses should be escaped");
        escaped.Should().Contain("\\)", "parentheses should be escaped");
        escaped.Should().Contain("\\[", "brackets should be escaped");
        escaped.Should().Contain("\\]", "brackets should be escaped");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            escaped.Should().Contain("\\\\'", "single quote should be escaped");
            escaped.Should().Contain("\\\\:", "colon should be escaped");
        }
        else
        {
            escaped.Should().Contain("\\'", "single quote should be escaped");
            escaped.Should().Contain("\\:", "colon should be escaped");
        }
    }

    [Fact]
    public void EscapeSubtitlePath_WindowsDriveLetter_ShouldEscapeColon()
    {
        // This test only runs on Windows
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        // Arrange
        var path = @"C:\Videos\Test.srt";

        // Act
        var escaped = CallEscapeSubtitlePath(path);

        // Assert
        // The drive letter colon should be escaped: C: → C\\:
        escaped.Should().StartWith("C\\\\\\\\\\\\:", "drive letter colon should be escaped after quadruple backslash escaping");
    }

    [Fact]
    public void EscapeSubtitlePath_UnixPathWithColon_ShouldEscapeColon()
    {
        // This test only runs on Unix-like systems
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        // Arrange
        var path = "/home/Videos/Time:Stamp.srt";

        // Act
        var escaped = CallEscapeSubtitlePath(path);

        // Assert
        escaped.Should().Contain("\\:", "colon in filename should be escaped");
    }

    [Fact]
    public void EscapeSubtitlePath_EmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var path = string.Empty;

        // Act
        var escaped = CallEscapeSubtitlePath(path);

        // Assert
        escaped.Should().BeEmpty();
    }

    [Fact]
    public void EscapeSubtitlePath_PathWithBackslashInFilename_ShouldEscapeCorrectly()
    {
        // This is rare but possible on Unix systems (backslash in filename, not as separator)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        // Arrange
        var path = "/home/Videos/Test\\Backslash.srt";

        // Act
        var escaped = CallEscapeSubtitlePath(path);

        // Assert
        escaped.Should().Contain("\\\\", "backslash in filename should be double-escaped");
    }
}
