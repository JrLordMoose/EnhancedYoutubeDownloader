using EnhancedYoutubeDownloader.Core.Services;
using EnhancedYoutubeDownloader.Shared.Models;
using FluentAssertions;
using Xunit;

namespace EnhancedYoutubeDownloader.Tests.Core.Services;

public class CacheServiceTests : IDisposable
{
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _sut = new CacheService();
    }

    [Fact]
    public async Task SetVideoMetadataAsync_ShouldCacheVideo()
    {
        // Arrange
        var video = new CachedVideo
        {
            Id = "test123",
            Title = "Test Video",
            Author = "Test Author",
            AuthorChannelId = "UC123",
            Duration = TimeSpan.FromMinutes(5)
        };

        // Act
        await _sut.SetVideoMetadataAsync(video.Id, video);
        var isCached = await _sut.IsVideoMetadataCachedAsync(video.Id);

        // Assert
        isCached.Should().BeTrue();
    }

    [Fact]
    public async Task GetVideoMetadataAsync_ShouldReturnCachedVideo()
    {
        // Arrange
        var video = new CachedVideo
        {
            Id = "test456",
            Title = "Another Test Video",
            Author = "Another Author",
            AuthorChannelId = "UC456",
            Duration = TimeSpan.FromMinutes(10)
        };

        await _sut.SetVideoMetadataAsync(video.Id, video);

        // Act
        var cachedVideo = await _sut.GetVideoMetadataAsync(video.Id);

        // Assert
        cachedVideo.Should().NotBeNull();
        cachedVideo!.Id.Should().Be(video.Id);
        cachedVideo.Title.Should().Be(video.Title);
        cachedVideo.Author.Should().Be(video.Author);
    }

    [Fact]
    public async Task GetVideoMetadataAsync_NonExistentVideo_ShouldReturnNull()
    {
        // Act
        var cachedVideo = await _sut.GetVideoMetadataAsync("nonexistent");

        // Assert
        cachedVideo.Should().BeNull();
    }

    [Fact]
    public async Task ClearCacheAsync_ShouldRemoveAllEntries()
    {
        // Arrange
        var video = new CachedVideo
        {
            Id = "test789",
            Title = "Video to Clear",
            Author = "Clearable Author",
            AuthorChannelId = "UC789",
            Duration = TimeSpan.FromMinutes(3)
        };

        await _sut.SetVideoMetadataAsync(video.Id, video);

        // Act
        await _sut.ClearCacheAsync();
        var isCached = await _sut.IsVideoMetadataCachedAsync(video.Id);

        // Assert
        isCached.Should().BeFalse();
    }

    public void Dispose()
    {
        _sut?.Dispose();
    }
}
