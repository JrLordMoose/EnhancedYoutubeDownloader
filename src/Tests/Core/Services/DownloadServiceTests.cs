using EnhancedYoutubeDownloader.Core.Services;
using EnhancedYoutubeDownloader.Shared.Models;
using FluentAssertions;
using Moq;
using YoutubeExplode.Videos;
using Xunit;

namespace EnhancedYoutubeDownloader.Tests.Core.Services;

public class DownloadServiceTests : IDisposable
{
    private readonly DownloadService _downloadService;
    private readonly Mock<IVideo> _mockVideo;
    private readonly string _testFilePath;
    private readonly string _testPartialFilePath;

    public DownloadServiceTests()
    {
        _downloadService = new DownloadService();
        _mockVideo = new Mock<IVideo>();
        _mockVideo.Setup(v => v.Id).Returns(VideoId.Parse("dQw4w9WgXcQ"));
        _mockVideo.Setup(v => v.Title).Returns("Test Video");
        _mockVideo.Setup(v => v.Duration).Returns(TimeSpan.FromMinutes(3));

        // Use temp directory for test files
        var tempDir = Path.Combine(Path.GetTempPath(), "DownloadServiceTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        _testFilePath = Path.Combine(tempDir, "test_video.mp4");
        _testPartialFilePath = _testFilePath + ".part";
    }

    [Fact]
    public async Task CreateDownloadAsync_ShouldCreateDownloadItem()
    {
        // Arrange & Act
        var downloadItem = await _downloadService.CreateDownloadAsync(
            _mockVideo.Object,
            _testFilePath
        );

        // Assert
        downloadItem.Should().NotBeNull();
        downloadItem.Video.Should().Be(_mockVideo.Object);
        downloadItem.FilePath.Should().Be(_testFilePath);
        downloadItem.Status.Should().Be(DownloadStatus.Queued);
        downloadItem.PartialFilePath.Should().Be(_testFilePath + ".part");
    }

    [Fact]
    public async Task PauseDownloadAsync_ShouldUpdateStatus()
    {
        // Arrange
        var downloadItem = await _downloadService.CreateDownloadAsync(
            _mockVideo.Object,
            _testFilePath
        );

        // Set status to Started (simulating a download in progress)
        downloadItem.Status = DownloadStatus.Started;
        downloadItem.BytesDownloaded = 1024;
        downloadItem.TotalBytes = 2048;

        // Act
        await _downloadService.PauseDownloadAsync(downloadItem);

        // Assert
        downloadItem.Status.Should().Be(DownloadStatus.Paused);
        downloadItem.CanPause.Should().BeFalse();
        downloadItem.CanResume.Should().BeTrue();
        downloadItem.CanCancel.Should().BeTrue();
        downloadItem.CanRestart.Should().BeFalse();
    }

    [Fact]
    public async Task PauseDownloadAsync_ShouldNotPauseIfNotStarted()
    {
        // Arrange
        var downloadItem = await _downloadService.CreateDownloadAsync(
            _mockVideo.Object,
            _testFilePath
        );

        // Status is Queued by default

        // Act
        await _downloadService.PauseDownloadAsync(downloadItem);

        // Assert
        downloadItem.Status.Should().Be(DownloadStatus.Queued);
    }

    [Fact]
    public async Task ResumeDownloadAsync_ShouldNotResumeIfNotPaused()
    {
        // Arrange
        var downloadItem = await _downloadService.CreateDownloadAsync(
            _mockVideo.Object,
            _testFilePath
        );

        // Status is Queued by default

        // Act
        await _downloadService.ResumeDownloadAsync(downloadItem);

        // Assert
        downloadItem.Status.Should().Be(DownloadStatus.Queued);
    }

    [Fact]
    public async Task CancelDownloadAsync_ShouldUpdateStatusAndCleanup()
    {
        // Arrange
        var downloadItem = await _downloadService.CreateDownloadAsync(
            _mockVideo.Object,
            _testFilePath
        );

        // Create a partial file to test cleanup
        await File.WriteAllTextAsync(downloadItem.PartialFilePath!, "test data");
        File.Exists(downloadItem.PartialFilePath).Should().BeTrue();

        downloadItem.Status = DownloadStatus.Started;

        // Act
        await _downloadService.CancelDownloadAsync(downloadItem);

        // Assert
        downloadItem.Status.Should().Be(DownloadStatus.Canceled);
        downloadItem.CanPause.Should().BeFalse();
        downloadItem.CanResume.Should().BeFalse();
        downloadItem.CanCancel.Should().BeFalse();
        downloadItem.CanRestart.Should().BeTrue();

        // Partial file should be deleted
        File.Exists(downloadItem.PartialFilePath!).Should().BeFalse();
    }

    [Fact]
    public async Task RestartDownloadAsync_ShouldResetDownloadState()
    {
        // Arrange
        var downloadItem = await _downloadService.CreateDownloadAsync(
            _mockVideo.Object,
            _testFilePath
        );

        downloadItem.Status = DownloadStatus.Failed;
        downloadItem.Progress = 50;
        downloadItem.BytesDownloaded = 1024;
        downloadItem.TotalBytes = 2048;
        downloadItem.ErrorMessage = "Test error";

        // Create a partial file to test cleanup
        await File.WriteAllTextAsync(downloadItem.PartialFilePath!, "test data");

        // Act
        await _downloadService.RestartDownloadAsync(downloadItem);

        // Give it a moment to process
        await Task.Delay(100);

        // Assert
        downloadItem.Progress.Should().Be(0);
        downloadItem.BytesDownloaded.Should().Be(0);
        downloadItem.TotalBytes.Should().Be(0);
        downloadItem.ErrorMessage.Should().BeNull();
        downloadItem.Status.Should().BeOneOf(DownloadStatus.Queued, DownloadStatus.Started);

        // Partial file should be deleted
        File.Exists(downloadItem.PartialFilePath!).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteDownloadAsync_ShouldRemoveDownloadAndCleanup()
    {
        // Arrange
        var downloadItem = await _downloadService.CreateDownloadAsync(
            _mockVideo.Object,
            _testFilePath
        );

        // Create a partial file to test cleanup
        await File.WriteAllTextAsync(downloadItem.PartialFilePath!, "test data");

        // Act
        await _downloadService.DeleteDownloadAsync(downloadItem);

        // Assert
        downloadItem.Status.Should().Be(DownloadStatus.Canceled);
        File.Exists(downloadItem.PartialFilePath!).Should().BeFalse();
    }

    [Fact]
    public void BytesProgress_ShouldCalculateCorrectly()
    {
        // Arrange
        var downloadItem = new DownloadItem
        {
            BytesDownloaded = 1024,
            TotalBytes = 2048
        };

        // Act
        var progress = downloadItem.BytesProgress;

        // Assert
        progress.Should().BeApproximately(50.0, 0.01);
    }

    [Fact]
    public void BytesProgress_ShouldReturnZeroWhenTotalIsZero()
    {
        // Arrange
        var downloadItem = new DownloadItem
        {
            BytesDownloaded = 1024,
            TotalBytes = 0
        };

        // Act
        var progress = downloadItem.BytesProgress;

        // Assert
        progress.Should().Be(0);
    }

    public void Dispose()
    {
        _downloadService?.Dispose();

        // Clean up test files and directory
        try
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);

            if (File.Exists(_testPartialFilePath))
                File.Delete(_testPartialFilePath);

            var testDir = Path.GetDirectoryName(_testFilePath);
            if (!string.IsNullOrEmpty(testDir) && Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
