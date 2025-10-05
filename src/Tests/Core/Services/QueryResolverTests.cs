using EnhancedYoutubeDownloader.Core.Services;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnhancedYoutubeDownloader.Tests.Core.Services;

public class QueryResolverTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly QueryResolver _sut;

    public QueryResolverTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _sut = new QueryResolver(_cacheServiceMock.Object);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=abc123DEF45", "abc123DEF45")]
    public void ExtractVideoId_ValidUrls_ShouldReturnVideoId(string url, string expectedId)
    {
        // Act
        var videoId = _sut.ExtractVideoId(url);

        // Assert
        videoId.Should().Be(expectedId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("https://www.google.com")]
    [InlineData("not a url")]
    public void ExtractVideoId_InvalidUrls_ShouldReturnNull(string? url)
    {
        // Act
        var videoId = _sut.ExtractVideoId(url!);

        // Assert
        videoId.Should().BeNull();
    }

    [Theory]
    [InlineData("https://www.youtube.com/playlist?list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf", "PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf")]
    [InlineData("https://www.youtube.com/watch?v=abc&list=PLtest123", "PLtest123")]
    public void ExtractPlaylistId_ValidUrls_ShouldReturnPlaylistId(string url, string expectedId)
    {
        // Act
        var playlistId = _sut.ExtractPlaylistId(url);

        // Assert
        playlistId.Should().Be(expectedId);
    }

    [Theory]
    [InlineData("https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw", "UCuAXFkgsw1L7xaCfnd5JJOw")]
    public void ExtractChannelId_ValidUrls_ShouldReturnChannelId(string url, string expectedId)
    {
        // Act
        var channelId = _sut.ExtractChannelId(url);

        // Assert
        channelId.Should().Be(expectedId);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=test")]
    [InlineData("https://youtu.be/test")]
    [InlineData("https://www.youtube.com/channel/UCtest")]
    public void IsYouTubeUrl_ValidYouTubeUrls_ShouldReturnTrue(string url)
    {
        // Act
        var result = _sut.IsYouTubeUrl(url);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("https://www.vimeo.com/123")]
    [InlineData("")]
    [InlineData(null)]
    public void IsYouTubeUrl_NonYouTubeUrls_ShouldReturnFalse(string? url)
    {
        // Act
        var result = _sut.IsYouTubeUrl(url!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveAsync_EmptyQuery_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _sut.ResolveAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Query cannot be empty*");
    }

    [Fact]
    public async Task ResolveAsync_NullQuery_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _sut.ResolveAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // Note: Integration tests with actual YouTube API calls would be in a separate test suite
    // These tests focus on URL parsing and validation logic
}
