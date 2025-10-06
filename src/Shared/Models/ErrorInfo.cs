using System.Collections.Generic;

namespace EnhancedYoutubeDownloader.Shared.Models;

/// <summary>
/// Represents detailed error information for user feedback.
/// </summary>
public class ErrorInfo
{
    /// <summary>
    /// Gets or sets the main error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets or sets the detailed error information, such as a stack trace.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Gets or sets the category of the error.
    /// </summary>
    public ErrorCategory Category { get; init; }

    /// <summary>
    /// Gets or sets a list of suggested actions to resolve the error.
    /// </summary>
    public List<ErrorAction> SuggestedActions { get; init; } = new();
}

/// <summary>
/// Defines categories for errors to provide better user guidance.
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// An unknown or uncategorized error.
    /// </summary>
    Unknown,

    /// <summary>
    /// A network-related error.
    /// </summary>
    Network,

    /// <summary>
    /// A permission-related error.
    /// </summary>
    Permission,

    /// <summary>
    /// An invalid URL error.
    /// </summary>
    InvalidUrl,

    /// <summary>
    /// A file system-related error.
    /// </summary>
    FileSystem,

    /// <summary>
    /// A YouTube-specific error.
    /// </summary>
    YouTube,

    /// <summary>
    /// The requested video is not available.
    /// </summary>
    VideoNotAvailable,

    /// <summary>
    /// The requested format is not available.
    /// </summary>
    FormatNotAvailable
}

/// <summary>
/// Represents a suggested action for error resolution.
/// </summary>
public class ErrorAction
{
    /// <summary>
    /// Gets or sets the display text for the action.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets or sets a key to identify the action programmatically.
    /// </summary>
    public required string ActionKey { get; init; }

    /// <summary>
    /// Gets or sets a description of what the action does.
    /// </summary>
    public string? Description { get; init; }
}