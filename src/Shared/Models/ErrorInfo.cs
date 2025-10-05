using System.Collections.Generic;

namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents detailed error information for user feedback
/// </summary>
public class ErrorInfo
{
    public required string Message { get; init; }
    public string? Details { get; init; }
    public ErrorCategory Category { get; init; }
    public List<ErrorAction> SuggestedActions { get; init; } = new();
}

/// <summary>
/// Error category for better user guidance
/// </summary>
public enum ErrorCategory
{
    Unknown,
    Network,
    Permission,
    InvalidUrl,
    FileSystem,
    YouTube,
    VideoNotAvailable,
    FormatNotAvailable
}

/// <summary>
/// Suggested action for error resolution
/// </summary>
public class ErrorAction
{
    public required string Text { get; init; }
    public required string ActionKey { get; init; }
    public string? Description { get; init; }
}
