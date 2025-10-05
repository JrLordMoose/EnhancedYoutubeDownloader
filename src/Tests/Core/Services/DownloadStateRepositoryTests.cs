using EnhancedYoutubeDownloader.Core.Services;
using EnhancedYoutubeDownloader.Shared.Models;
using FluentAssertions;
using Moq;
using YoutubeExplode.Videos;
using Xunit;

namespace EnhancedYoutubeDownloader.Tests.Core.Services;

public class DownloadStateRepositoryTests : IDisposable
{
    private readonly DownloadStateRepository _repository;
    private readonly Mock<IVideo> _mockVideo;

    public DownloadStateRepositoryTests()
    {
        _repository = new DownloadStateRepository();
        _mockVideo = new Mock<IVideo>();
        _mockVideo.Setup(v => v.Id).Returns(VideoId.Parse("dQw4w9WgXcQ"));
        _mockVideo.Setup(v => v.Title).Returns("Test Video");
    }

    [Fact]
    public async Task SaveStateAsync_ShouldPersistDownloadState()
    {
        // Arrange
        var downloadItem = new DownloadItem
        {
            Id = Guid.NewGuid().ToString(),
            Video = _mockVideo.Object,
            FilePath = "/path/to/video.mp4",
            PartialFilePath = "/path/to/video.mp4.part",
            BytesDownloaded = 1024,
            TotalBytes = 2048,
            Status = DownloadStatus.Paused
        };

        // Act
        await _repository.SaveStateAsync(downloadItem);

        // Assert
        var loadedState = await _repository.LoadStateAsync(downloadItem.Id);
        loadedState.Should().NotBeNull();
        loadedState!.DownloadId.Should().Be(downloadItem.Id);
        loadedState.VideoId.Should().Be(downloadItem.Video.Id.ToString());
        loadedState.FilePath.Should().Be(downloadItem.FilePath);
        loadedState.PartialFilePath.Should().Be(downloadItem.PartialFilePath);
        loadedState.BytesDownloaded.Should().Be(1024);
        loadedState.TotalBytes.Should().Be(2048);
        loadedState.Status.Should().Be(DownloadStatus.Paused);
    }

    [Fact]
    public async Task LoadStateAsync_ShouldReturnNullForNonExistentDownload()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var state = await _repository.LoadStateAsync(nonExistentId);

        // Assert
        state.Should().BeNull();
    }

    [Fact]
    public async Task SaveStateAsync_ShouldUpdateExistingState()
    {
        // Arrange
        var downloadItem = new DownloadItem
        {
            Id = Guid.NewGuid().ToString(),
            Video = _mockVideo.Object,
            FilePath = "/path/to/video.mp4",
            PartialFilePath = "/path/to/video.mp4.part",
            BytesDownloaded = 1024,
            TotalBytes = 2048,
            Status = DownloadStatus.Paused
        };

        // Act - Save initial state
        await _repository.SaveStateAsync(downloadItem);

        // Update the download item
        downloadItem.BytesDownloaded = 1536;
        downloadItem.Status = DownloadStatus.Started;

        // Save updated state
        await _repository.SaveStateAsync(downloadItem);

        // Assert
        var loadedState = await _repository.LoadStateAsync(downloadItem.Id);
        loadedState.Should().NotBeNull();
        loadedState!.BytesDownloaded.Should().Be(1536);
        loadedState.Status.Should().Be(DownloadStatus.Started);
    }

    [Fact]
    public async Task DeleteStateAsync_ShouldRemoveDownloadState()
    {
        // Arrange
        var downloadItem = new DownloadItem
        {
            Id = Guid.NewGuid().ToString(),
            Video = _mockVideo.Object,
            FilePath = "/path/to/video.mp4",
            PartialFilePath = "/path/to/video.mp4.part",
            BytesDownloaded = 1024,
            TotalBytes = 2048,
            Status = DownloadStatus.Paused
        };

        await _repository.SaveStateAsync(downloadItem);

        // Act
        await _repository.DeleteStateAsync(downloadItem.Id);

        // Assert
        var loadedState = await _repository.LoadStateAsync(downloadItem.Id);
        loadedState.Should().BeNull();
    }

    [Fact]
    public async Task GetAllStatesAsync_ShouldReturnAllDownloadStates()
    {
        // Arrange
        var downloadItem1 = new DownloadItem
        {
            Id = Guid.NewGuid().ToString(),
            Video = _mockVideo.Object,
            FilePath = "/path/to/video1.mp4",
            PartialFilePath = "/path/to/video1.mp4.part",
            BytesDownloaded = 1024,
            TotalBytes = 2048,
            Status = DownloadStatus.Paused
        };

        var downloadItem2 = new DownloadItem
        {
            Id = Guid.NewGuid().ToString(),
            Video = _mockVideo.Object,
            FilePath = "/path/to/video2.mp4",
            PartialFilePath = "/path/to/video2.mp4.part",
            BytesDownloaded = 512,
            TotalBytes = 1024,
            Status = DownloadStatus.Started
        };

        await _repository.SaveStateAsync(downloadItem1);
        await _repository.SaveStateAsync(downloadItem2);

        // Act
        var allStates = await _repository.GetAllStatesAsync();

        // Assert
        allStates.Should().HaveCountGreaterOrEqualTo(2);
        allStates.Should().Contain(s => s.DownloadId == downloadItem1.Id);
        allStates.Should().Contain(s => s.DownloadId == downloadItem2.Id);
    }

    [Fact]
    public async Task SaveStateAsync_ShouldHandleNullPartialFilePath()
    {
        // Arrange
        var downloadItem = new DownloadItem
        {
            Id = Guid.NewGuid().ToString(),
            Video = _mockVideo.Object,
            FilePath = "/path/to/video.mp4",
            PartialFilePath = null,
            BytesDownloaded = 0,
            TotalBytes = 2048,
            Status = DownloadStatus.Queued
        };

        // Act
        await _repository.SaveStateAsync(downloadItem);

        // Assert
        var loadedState = await _repository.LoadStateAsync(downloadItem.Id);
        loadedState.Should().NotBeNull();
        loadedState!.PartialFilePath.Should().BeNull();
    }

    public void Dispose()
    {
        _repository?.Dispose();
    }
}
