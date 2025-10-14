using System.Text.RegularExpressions;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Core.Services;

public class QueryResolver : IQueryResolver
{
    private readonly YoutubeClient _youtubeClient;
    private readonly ICacheService _cacheService;

    private static readonly Regex VideoIdRegex = new(
        @"(?:youtube\.com/watch\?v=|youtube\.com/shorts/|youtu\.be/|youtube\.com/embed/)([a-zA-Z0-9_-]{11})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex PlaylistIdRegex = new(
        @"[?&]list=([a-zA-Z0-9_-]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex ChannelIdRegex = new(
        @"youtube\.com/channel/([a-zA-Z0-9_-]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex ChannelHandleRegex = new(
        @"youtube\.com/@([a-zA-Z0-9_-]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public QueryResolver(ICacheService cacheService)
    {
        _youtubeClient = new YoutubeClient();
        _cacheService = cacheService;
    }

    public async Task<QueryResult> ResolveAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty", nameof(query));

        query = query.Trim();

        // Try to resolve as video URL
        var videoId = ExtractVideoId(query);
        if (videoId != null)
        {
            var video = await GetVideoAsync(videoId);
            return new QueryResult
            {
                Kind = QueryResultKind.Video,
                Video = video,
                Title = video.Title,
                Author = video.Author.ChannelTitle
            };
        }

        // Try to resolve as playlist URL
        var playlistId = ExtractPlaylistId(query);
        if (playlistId != null)
        {
            return await ResolvePlaylistAsync(playlistId);
        }

        // Try to resolve as channel URL
        var channelId = ExtractChannelId(query);
        if (channelId != null)
        {
            return await ResolveChannelAsync(channelId);
        }

        // Try to resolve as channel handle
        var channelHandle = ExtractChannelHandle(query);
        if (channelHandle != null)
        {
            return await ResolveChannelByHandleAsync(channelHandle);
        }

        // Fallback to search
        return await ResolveSearchAsync(query);
    }

    public bool IsYouTubeUrl(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        return query.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
               query.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);
    }

    public string? ExtractVideoId(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var match = VideoIdRegex.Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    public string? ExtractPlaylistId(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var match = PlaylistIdRegex.Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    public string? ExtractChannelId(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var match = ChannelIdRegex.Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    private string? ExtractChannelHandle(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var match = ChannelHandleRegex.Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    private async Task<IVideo> GetVideoAsync(string videoId)
    {
        // Check cache first
        var cached = await _cacheService.GetVideoMetadataAsync(videoId);
        if (cached != null)
        {
            // Note: In a real implementation, you'd convert CachedVideo back to IVideo
            // For now, we'll fetch from YouTube (cache will be updated after)
        }

        // Fetch from YouTube
        var video = await _youtubeClient.Videos.GetAsync(videoId);

        // Cache for future use (24 hours)
        var cachedVideo = new CachedVideo
        {
            Id = video.Id,
            Title = video.Title,
            Author = video.Author.ChannelTitle,
            AuthorChannelId = video.Author.ChannelId,
            Duration = video.Duration ?? TimeSpan.Zero,
            Description = video.Description,
            Keywords = video.Keywords.ToList(),
            Thumbnails = video.Thumbnails.Select(t => new CachedThumbnail
            {
                Url = t.Url,
                Width = t.Resolution.Width,
                Height = t.Resolution.Height
            }).ToList(),
            UploadDate = video.UploadDate
        };

        await _cacheService.SetVideoMetadataAsync(videoId, cachedVideo, TimeSpan.FromHours(24));

        return video;
    }

    private async Task<QueryResult> ResolvePlaylistAsync(string playlistId)
    {
        var playlist = await _youtubeClient.Playlists.GetAsync(playlistId);
        var videos = new List<IVideo>();

        await foreach (var video in _youtubeClient.Playlists.GetVideosAsync(playlistId))
        {
            videos.Add(video);
        }

        return new QueryResult
        {
            Kind = QueryResultKind.Playlist,
            Videos = videos,
            Title = playlist.Title,
            Description = playlist.Description,
            Author = playlist.Author?.ChannelTitle
        };
    }

    private async Task<QueryResult> ResolveChannelAsync(string channelId)
    {
        var channel = await _youtubeClient.Channels.GetAsync(channelId);
        var videos = new List<IVideo>();

        await foreach (var video in _youtubeClient.Channels.GetUploadsAsync(channelId))
        {
            videos.Add(video);
        }

        return new QueryResult
        {
            Kind = QueryResultKind.Channel,
            Videos = videos,
            Title = channel.Title,
            Description = null,
            Author = channel.Title
        };
    }

    private async Task<QueryResult> ResolveChannelByHandleAsync(string handle)
    {
        // YoutubeExplode supports handles via ChannelHandle
        var channelHandle = new ChannelHandle(handle);
        var channel = await _youtubeClient.Channels.GetByHandleAsync(handle);
        var videos = new List<IVideo>();

        await foreach (var video in _youtubeClient.Channels.GetUploadsAsync(channel.Id))
        {
            videos.Add(video);
        }

        return new QueryResult
        {
            Kind = QueryResultKind.Channel,
            Videos = videos,
            Title = channel.Title,
            Description = null,
            Author = channel.Title
        };
    }

    private async Task<QueryResult> ResolveSearchAsync(string searchQuery)
    {
        var videos = new List<IVideo>();
        var count = 0;

        await foreach (var video in _youtubeClient.Search.GetVideosAsync(searchQuery))
        {
            videos.Add(video);
            if (++count >= 20) break;
        }

        return new QueryResult
        {
            Kind = QueryResultKind.Search,
            Videos = videos,
            Title = $"Search results for: {searchQuery}",
            Description = null,
            Author = null
        };
    }
}
