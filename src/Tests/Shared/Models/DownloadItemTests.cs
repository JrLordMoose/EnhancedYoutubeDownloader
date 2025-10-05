using EnhancedYoutubeDownloader.Shared.Models;
using FluentAssertions;
using Xunit;

namespace EnhancedYoutubeDownloader.Tests.Shared.Models;

public class DownloadItemTests
{
    [Fact]
    public void DownloadItem_ShouldInitializeWithDefaultValues()
    {
        // Act
        var downloadItem = new DownloadItem();

        // Assert
        downloadItem.Id.Should().NotBeNullOrEmpty();
        downloadItem.Status.Should().Be(DownloadStatus.Queued);
        downloadItem.Progress.Should().Be(0);
        downloadItem.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DownloadItem_ShouldUpdateStatus()
    {
        // Arrange
        var downloadItem = new DownloadItem();

        // Act
        downloadItem.Status = DownloadStatus.Started;

        // Assert
        downloadItem.Status.Should().Be(DownloadStatus.Started);
    }

    [Fact]
    public void DownloadItem_ShouldTrackProgress()
    {
        // Arrange
        var downloadItem = new DownloadItem();

        // Act
        downloadItem.Progress = 50.5;

        // Assert
        downloadItem.Progress.Should().Be(50.5);
    }

    [Fact]
    public void DownloadItem_ShouldHandleErrorMessages()
    {
        // Arrange
        var downloadItem = new DownloadItem();
        var errorMessage = "Download failed due to network error";

        // Act
        downloadItem.ErrorMessage = errorMessage;
        downloadItem.Status = DownloadStatus.Failed;

        // Assert
        downloadItem.ErrorMessage.Should().Be(errorMessage);
        downloadItem.Status.Should().Be(DownloadStatus.Failed);
    }

    [Fact]
    public void DownloadItem_ShouldManageActionFlags()
    {
        // Arrange
        var downloadItem = new DownloadItem();

        // Act
        downloadItem.CanPause = true;
        downloadItem.CanCancel = true;
        downloadItem.CanResume = false;
        downloadItem.CanRestart = false;

        // Assert
        downloadItem.CanPause.Should().BeTrue();
        downloadItem.CanCancel.Should().BeTrue();
        downloadItem.CanResume.Should().BeFalse();
        downloadItem.CanRestart.Should().BeFalse();
    }
}
