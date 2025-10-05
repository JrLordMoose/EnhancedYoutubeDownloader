# Analyze and execute YouTube downloader instructions
_Exported on 10/2/2025 at 08:29:49 EDT from Cursor (1.7.28)_

---

**User**

@AI Agent Instruction Prompt_ Clone and Enhance YoutubeDownloader.md @Comprehensive Analysis of YoutubeDownloader Application.md 

thoroughly analyze the Analysis and instructions and then start and run the instruction prompt

---

**Cursor**

I'll thoroughly analyze the provided documents and begin executing the instruction prompt to clone and enhance the YoutubeDownloader application.


## Analysis Summary

Based on my thorough analysis of both documents, I understand the comprehensive requirements:

**Original Application Strengths:**
- Clean MVVM architecture with .NET 9.0 and Avalonia UI
- Cross-platform support (Windows, Linux, macOS)
- No API key requirements (uses YoutubeExplode)
- Material Design aesthetic
- Separation of Core/UI concerns

**Critical Improvements to Implement:**
1. Unified queue management system
2. Pause/resume functionality with state persistence
3. Enhanced error handling with user-friendly messages
4. Metadata caching with SQLite/LiteDB
5. Drag-and-drop support
6. Batch operations and multi-select
7. Download scheduling
8. Format profiles and presets
9. Better visual feedback (toasts, notifications)
10. Performance optimizations (parallel downloads, chunking, retry logic)

Now I'll begin **PHASE 1: UNDERSTAND** by cloning and analyzing the original repository.


Now I have a comprehensive understanding of the original YoutubeDownloader application. Let me complete the **PHASE 1: UNDERSTAND** analysis and move to **PHASE 2: ANALYZE**.


## PHASE 1: UNDERSTAND - Complete Analysis

**Architecture Understanding:**
- **Technology Stack**: .NET 9.0, Avalonia UI 11.3.0, Material.Avalonia 3.9.2, CommunityToolkit.Mvvm 8.4.0
- **Core Libraries**: YoutubeExplode 6.5.4, FFmpeg 7.1.1, TagLibSharp 2.3.0, Gress 2.1.1
- **Architecture**: Clean MVVM with dependency injection, two-project structure (UI + Core)
- **Key Components**: QueryResolver, VideoDownloader, DashboardViewModel, DownloadViewModel

**Data Flow Analysis:**
1. **URL Input** → QueryResolver validates and extracts metadata
2. **Metadata Extraction** → YoutubeExplode gets video/playlist/channel info
3. **Quality Selection** → User chooses format/quality in dialogs
4. **Download Queue** → DashboardViewModel manages concurrent downloads
5. **FFmpeg Processing** → VideoDownloader handles conversion
6. **File Output** → Tag injection and completion

**User Workflows Identified:**
- Single video download with quality selection
- Playlist/channel batch downloads with filtering
- Search-based downloads
- Authentication for private content
- Settings management and updates

## PHASE 2: ANALYZE - Component Breakdown

Based on the analysis, here's the prioritized enhancement breakdown:

### P0 (Critical) - Core Infrastructure
1. **Technology Stack Setup**
   - .NET 9.0 with latest packages
   - Enhanced project structure with proper separation
   - Build pipeline with CI/CD
   - Dependency management

2. **Core Download Functionality**
   - URL parsing and validation (enhanced)
   - YouTube metadata extraction (cached)
   - Quality/format selection (profiles)
   - Download engine (pause/resume)
   - Progress reporting (enhanced)

3. **Basic UI Framework**
   - Unified queue management
   - Enhanced error handling
   - Settings persistence

### P1 (High Priority) - Enhanced Features
1. **Pause/Resume System**
   - Chunked download implementation
   - State persistence and recovery
   - UI controls for pause/resume

2. **Unified Queue Management**
   - Single pane for all download states
   - Drag-and-drop reordering
   - Multi-select operations
   - Status filtering

3. **Enhanced Feedback System**
   - Toast notifications
   - Rich error dialogs
   - Loading states
   - Completion notifications

4. **Metadata Caching**
   - SQLite/LiteDB database
   - Cache invalidation strategy
   - Performance optimization

### P2 (Medium Priority) - Advanced Features
1. **Format Profiles**
   - Quick presets for common scenarios
   - Per-channel preferences
   - Import/export functionality

2. **Advanced Filtering**
   - Date range filters
   - Duration filters
   - Quality presets
   - Regex pattern matching

3. **Download Scheduling**
   - Time-based queue execution
   - Bandwidth limiting
   - Auto-retry policies

### P3 (Nice-to-have) - Future Enhancements
1. **Browser Extension Integration**
2. **Cloud Integration**
3. **Advanced Analytics**

Now I'll begin **PHASE 3: SOLVE** by setting up the enhanced project structure.


## PHASE 3: SOLVE - Step 3.1: Technology Stack Selection

Based on the analysis, I'll use the following **free/open-source technology stack**:

**Core Framework:**
- .NET 9.0 (free, cross-platform)
- Avalonia UI 11.x (MIT license, free)
- Material.Avalonia (free)

**Key Libraries:**
- YoutubeExplode 6.5.4+ (free, LGPL)
- FFmpeg 7.x (free, GPL)
- SQLite (free, public domain)
- CommunityToolkit.Mvvm (free)

**Testing & Quality:**
- xUnit (free)
- GitHub Actions (free for public repos)

Now I'll create the enhanced project structure:


```plaintext
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.7.33920.267
MinimumVisualStudioVersion = 10.0.40219.1
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "EnhancedYoutubeDownloader", "src\Desktop\EnhancedYoutubeDownloader.csproj", "{AF6D645E-DDDD-4034-B644-D5328CC893C1}"
EndProject
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "EnhancedYoutubeDownloader.Core", "src\Core\EnhancedYoutubeDownloader.Core.csproj", "{5122A9DE-232C-4DA8-AD76-8B72AA377D5E}"
EndProject
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "EnhancedYoutubeDownloader.Shared", "src\Shared\EnhancedYoutubeDownloader.Shared.csproj", "{7122A9DE-232C-4DA8-AD76-8B72AA377D5E}"
EndProject
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "EnhancedYoutubeDownloader.Tests", "src\Tests\EnhancedYoutubeDownloader.Tests.csproj", "{8122A9DE-232C-4DA8-AD76-8B72AA377D5E}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Misc", "Misc", "{131C2561-E5A1-43E8-BF38-40E2E23DB0A4}"
	ProjectSection(SolutionItems) = preProject
		Directory.Build.props = Directory.Build.props
		README.md = README.md
		LICENSE = LICENSE
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{AF6D645E-DDDD-4034-B644-D5328CC893C1}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{AF6D645E-DDDD-4034-B644-D5328CC893C1}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{AF6D645E-DDDD-4034-B644-D5328CC893C1}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{AF6D645E-DDDD-4034-B644-D5328CC893C1}.Release|Any CPU.Build.0 = Release|Any CPU
		{5122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{5122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{5122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{5122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Release|Any CPU.Build.0 = Release|Any CPU
		{7122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{7122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{7122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{7122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Release|Any CPU.Build.0 = Release|Any CPU
		{8122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{8122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{8122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{8122A9DE-232C-4DA8-AD76-8B72AA377D5E}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {1455235F-4357-4DB9-BCC1-41A5A8B10AC5}
	EndGlobalSection
EndGlobal
```

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Version>1.0.0</Version>
    <Company>Enhanced YoutubeDownloader</Company>
    <Copyright>Copyright (C) Enhanced YoutubeDownloader Contributors</Copyright>
    <LangVersion>preview</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ILLinkTreatWarningsAsErrors>false</ILLinkTreatWarningsAsErrors>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>false</UseWPF>
    <UseWindowsForms>false</UseWindowsForms>
  </PropertyGroup>
</Project>
```

Now I'll create the project files for each component:


```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
  </ItemGroup>
</Project>
```

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Gress" Version="2.1.1" />
    <PackageReference Include="JsonExtensions" Version="1.2.0" />
    <PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
    <PackageReference Include="TagLibSharp" Version="2.3.0" />
    <PackageReference Include="YoutubeExplode" Version="6.5.4" />
    <PackageReference Include="YoutubeExplode.Converter" Version="6.5.4" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Shared\EnhancedYoutubeDownloader.Shared.csproj" />
  </ItemGroup>
</Project>
```

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <ApplicationIcon>..\..\favicon.ico</ApplicationIcon>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <PublishTrimmed>false</PublishTrimmed>
    <CopyOutputSymbolsToPublishDirectory>false</CopyOutputSymbolsToPublishDirectory>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>
  <PropertyGroup>
    <DownloadFFmpeg>true</DownloadFFmpeg>
  </PropertyGroup>
  <ItemGroup>
    <AvaloniaResource Include="..\..\favicon.ico" Link="favicon.ico" />
  </ItemGroup>
  <ItemGroup>
    <None
      Include="ffmpeg.exe"
      CopyToOutputDirectory="PreserveNewest"
      Condition="Exists('ffmpeg.exe')"
    />
    <None Include="ffmpeg" CopyToOutputDirectory="PreserveNewest" Condition="Exists('ffmpeg')" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="AsyncImageLoader.Avalonia" Version="3.3.0" />
    <PackageReference Include="Avalonia" Version="11.3.0" />
    <PackageReference Include="Avalonia.Controls.DataGrid" Version="11.3.0" />
    <PackageReference Include="Avalonia.Desktop" Version="11.3.0" />
    <PackageReference
      Include="Avalonia.Diagnostics"
      Version="11.3.0"
      Condition="'$(Configuration)' == 'Debug'"
    />
    <PackageReference Include="Cogwheel" Version="2.1.0" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include="CSharpier.MsBuild" Version="1.0.2" PrivateAssets="all" />
    <PackageReference Include="Deorcify" Version="1.1.0" PrivateAssets="all" />
    <PackageReference Include="DialogHost.Avalonia" Version="0.9.3" />
    <PackageReference Include="Gress" Version="2.1.1" />
    <PackageReference Include="Material.Avalonia" Version="3.9.2" />
    <PackageReference Include="Material.Avalonia.DataGrid" Version="3.9.2" />
    <PackageReference Include="Material.Icons.Avalonia" Version="2.2.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.3" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
    <PackageReference Include="Onova" Version="2.6.13" />
    <PackageReference Include="WebView.Avalonia" Version="11.0.0.1" />
    <PackageReference Include="WebView.Avalonia.Desktop" Version="11.0.0.1" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Core\EnhancedYoutubeDownloader.Core.csproj" />
    <ProjectReference Include="..\Shared\EnhancedYoutubeDownloader.Shared.csproj" />
  </ItemGroup>
  <!-- Avalonia.WebView is completely incompatible with trimming -->
  <ItemGroup>
    <TrimmerRootAssembly Include="Avalonia.WebView" />
    <TrimmerRootAssembly Include="Avalonia.WebView.Desktop" />
    <TrimmerRootAssembly Include="Avalonia.WebView.Linux" />
    <TrimmerRootAssembly Include="Avalonia.WebView.MacCatalyst" />
    <TrimmerRootAssembly Include="Avalonia.WebView.Windows" />
    <TrimmerRootAssembly Include="AvaloniaWebView.Shared" />
    <TrimmerRootAssembly Include="Linux.WebView.Core" />
    <TrimmerRootAssembly Include="Microsoft.Web.WebView2.Core" />
    <TrimmerRootAssembly Include="WebView.Avalonia" />
    <TrimmerRootAssembly Include="WebView.Core" />
  </ItemGroup>
  <Target
    Name="DownloadFFmpeg"
    BeforeTargets="Restore;PreBuildEvent"
    Condition="$(DownloadFFmpeg) AND !Exists('ffmpeg.exe') AND !Exists('ffmpeg')"
  >
    <Exec
      Command="pwsh -ExecutionPolicy Bypass -File $(ProjectDir)/Download-FFmpeg.ps1 -Platform $(RuntimeIdentifier) -OutputPath $(ProjectDir)"
      LogStandardErrorAsError="true"
      Condition="'$(RuntimeIdentifier)' != ''"
    />
    <Exec
      Command="pwsh -ExecutionPolicy Bypass -File $(ProjectDir)/Download-FFmpeg.ps1 -OutputPath $(ProjectDir)"
      LogStandardErrorAsError="true"
      Condition="'$(RuntimeIdentifier)' == ''"
    />
  </Target>
</Project>
```

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Core\EnhancedYoutubeDownloader.Core.csproj" />
    <ProjectReference Include="..\Shared\EnhancedYoutubeDownloader.Shared.csproj" />
  </ItemGroup>
</Project>
```

Now I'll create the core shared models and interfaces:


```csharp
namespace EnhancedYoutubeDownloader.Shared.Models;

public enum DownloadStatus
{
    Queued,
    Started,
    Paused,
    Completed,
    Failed,
    Canceled
}
```

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Models;

public partial class DownloadItem : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private IVideo? _video;

    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private DownloadStatus _status = DownloadStatus.Queued;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    [ObservableProperty]
    private DateTime? _startedAt;

    [ObservableProperty]
    private DateTime? _completedAt;

    [ObservableProperty]
    private bool _canPause;

    [ObservableProperty]
    private bool _canResume;

    [ObservableProperty]
    private bool _canCancel;

    [ObservableProperty]
    private bool _canRestart;

    public string Title => Video?.Title ?? "Unknown";
    public string Author => Video?.Author?.ChannelTitle ?? "Unknown";
    public TimeSpan? Duration => Video?.Duration;
    public string? ThumbnailUrl => Video?.Thumbnails?.GetWithHighestResolution()?.Url;
}
```

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace EnhancedYoutubeDownloader.Shared.Models;

public partial class FormatProfile : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _container = "mp4";

    [ObservableProperty]
    private string _quality = "best";

    [ObservableProperty]
    private bool _includeSubtitles = true;

    [ObservableProperty]
    private bool _includeTags = true;

    [ObservableProperty]
    private bool _isDefault;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    [ObservableProperty]
    private DateTime _updatedAt = DateTime.UtcNow;
}
```

```csharp
using EnhancedYoutubeDownloader.Shared.Models;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Interfaces;

public interface IDownloadService
{
    Task<DownloadItem> CreateDownloadAsync(IVideo video, string filePath, FormatProfile? profile = null);
    Task StartDownloadAsync(DownloadItem downloadItem);
    Task PauseDownloadAsync(DownloadItem downloadItem);
    Task ResumeDownloadAsync(DownloadItem downloadItem);
    Task CancelDownloadAsync(DownloadItem downloadItem);
    Task RestartDownloadAsync(DownloadItem downloadItem);
    Task DeleteDownloadAsync(DownloadItem downloadItem);
    
    IObservable<DownloadItem> DownloadStatusChanged { get; }
    IObservable<double> OverallProgress { get; }
}
```

```csharp
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Shared.Interfaces;

public interface ICacheService
{
    Task<IVideo?> GetVideoMetadataAsync(string videoId);
    Task SetVideoMetadataAsync(string videoId, IVideo video, TimeSpan? expiration = null);
    Task<bool> IsVideoMetadataCachedAsync(string videoId);
    Task ClearCacheAsync();
    Task ClearExpiredCacheAsync();
}
```

```csharp
namespace EnhancedYoutubeDownloader.Shared.Interfaces;

public interface INotificationService
{
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInfo(string message);
    void ShowProgress(string message, double progress);
}
```

Now I'll create the core implementation files:


```csharp
using System.Text.Json;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using Microsoft.Data.Sqlite;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.Core.Services;

public class CacheService : ICacheService, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly JsonSerializerOptions _jsonOptions;

    public CacheService()
    {
        var cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EnhancedYoutubeDownloader");
        Directory.CreateDirectory(cacheDir);
        
        _connectionString = $"Data Source={Path.Combine(cacheDir, "cache.db")}";
        _connection = new SqliteConnection(_connectionString);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _connection.Open();
        
        var createTableCommand = _connection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS VideoMetadata (
                VideoId TEXT PRIMARY KEY,
                JsonData TEXT NOT NULL,
                CreatedAt DATETIME NOT NULL,
                ExpiresAt DATETIME
            )";
        createTableCommand.ExecuteNonQuery();
    }

    public async Task<IVideo?> GetVideoMetadataAsync(string videoId)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT JsonData FROM VideoMetadata 
                WHERE VideoId = @videoId 
                AND (ExpiresAt IS NULL OR ExpiresAt > @now)";
            
            command.Parameters.AddWithValue("@videoId", videoId);
            command.Parameters.AddWithValue("@now", DateTime.UtcNow);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var jsonData = reader.GetString("JsonData");
                // Note: This is a simplified implementation
                // In a real implementation, you'd need to deserialize to the actual IVideo type
                return null; // Placeholder
            }
        }
        catch
        {
            // Log error
        }

        return null;
    }

    public async Task SetVideoMetadataAsync(string videoId, IVideo video, TimeSpan? expiration = null)
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(video, _jsonOptions);
            var expiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : (DateTime?)null;

            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO VideoMetadata (VideoId, JsonData, CreatedAt, ExpiresAt)
                VALUES (@videoId, @jsonData, @createdAt, @expiresAt)";
            
            command.Parameters.AddWithValue("@videoId", videoId);
            command.Parameters.AddWithValue("@jsonData", jsonData);
            command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@expiresAt", expiresAt);

            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Log error
        }
    }

    public async Task<bool> IsVideoMetadataCachedAsync(string videoId)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) FROM VideoMetadata 
                WHERE VideoId = @videoId 
                AND (ExpiresAt IS NULL OR ExpiresAt > @now)";
            
            command.Parameters.AddWithValue("@videoId", videoId);
            command.Parameters.AddWithValue("@now", DateTime.UtcNow);

            var count = await command.ExecuteScalarAsync();
            return Convert.ToInt32(count) > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task ClearCacheAsync()
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM VideoMetadata";
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Log error
        }
    }

    public async Task ClearExpiredCacheAsync()
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM VideoMetadata WHERE ExpiresAt IS NOT NULL AND ExpiresAt <= @now";
            command.Parameters.AddWithValue("@now", DateTime.UtcNow);
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Log error
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
```

```csharp
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using Gress;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

namespace EnhancedYoutubeDownloader.Core.Services;

public class DownloadService : IDownloadService, IDisposable
{
    private readonly ConcurrentDictionary<string, DownloadItem> _downloads = new();
    private readonly Subject<DownloadItem> _downloadStatusChanged = new();
    private readonly Subject<double> _overallProgress = new();
    private readonly YoutubeClient _youtubeClient;
    private readonly SemaphoreSlim _downloadSemaphore;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public DownloadService(int maxConcurrentDownloads = 3)
    {
        _youtubeClient = new YoutubeClient();
        _downloadSemaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);
    }

    public IObservable<DownloadItem> DownloadStatusChanged => _downloadStatusChanged;
    public IObservable<double> OverallProgress => _overallProgress;

    public async Task<DownloadItem> CreateDownloadAsync(IVideo video, string filePath, FormatProfile? profile = null)
    {
        var downloadItem = new DownloadItem
        {
            Video = video,
            FilePath = filePath,
            Status = DownloadStatus.Queued
        };

        _downloads[downloadItem.Id] = downloadItem;
        _downloadStatusChanged.OnNext(downloadItem);

        return downloadItem;
    }

    public async Task StartDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Queued && downloadItem.Status != DownloadStatus.Paused)
            return;

        downloadItem.Status = DownloadStatus.Started;
        downloadItem.StartedAt = DateTime.UtcNow;
        downloadItem.CanPause = true;
        downloadItem.CanCancel = true;
        downloadItem.CanResume = false;
        downloadItem.CanRestart = false;
        _downloadStatusChanged.OnNext(downloadItem);

        _ = Task.Run(async () => await ProcessDownloadAsync(downloadItem));
    }

    public async Task PauseDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Started)
            return;

        downloadItem.Status = DownloadStatus.Paused;
        downloadItem.CanPause = false;
        downloadItem.CanResume = true;
        downloadItem.CanCancel = true;
        downloadItem.CanRestart = false;
        _downloadStatusChanged.OnNext(downloadItem);
    }

    public async Task ResumeDownloadAsync(DownloadItem downloadItem)
    {
        if (downloadItem.Status != DownloadStatus.Paused)
            return;

        downloadItem.Status = DownloadStatus.Started;
        downloadItem.CanPause = true;
        downloadItem.CanResume = false;
        downloadItem.CanCancel = true;
        downloadItem.CanRestart = false;
        _downloadStatusChanged.OnNext(downloadItem);

        _ = Task.Run(async () => await ProcessDownloadAsync(downloadItem));
    }

    public async Task CancelDownloadAsync(DownloadItem downloadItem)
    {
        downloadItem.Status = DownloadStatus.Canceled;
        downloadItem.CanPause = false;
        downloadItem.CanResume = false;
        downloadItem.CanCancel = false;
        downloadItem.CanRestart = true;
        _downloadStatusChanged.OnNext(downloadItem);
    }

    public async Task RestartDownloadAsync(DownloadItem downloadItem)
    {
        downloadItem.Status = DownloadStatus.Queued;
        downloadItem.Progress = 0;
        downloadItem.ErrorMessage = null;
        downloadItem.StartedAt = null;
        downloadItem.CompletedAt = null;
        downloadItem.CanPause = false;
        downloadItem.CanResume = false;
        downloadItem.CanCancel = false;
        downloadItem.CanRestart = false;
        _downloadStatusChanged.OnNext(downloadItem);

        await StartDownloadAsync(downloadItem);
    }

    public async Task DeleteDownloadAsync(DownloadItem downloadItem)
    {
        _downloads.TryRemove(downloadItem.Id, out _);
        downloadItem.Status = DownloadStatus.Canceled;
        _downloadStatusChanged.OnNext(downloadItem);
    }

    private async Task ProcessDownloadAsync(DownloadItem downloadItem)
    {
        await _downloadSemaphore.WaitAsync(_cancellationTokenSource.Token);

        try
        {
            if (downloadItem.Status == DownloadStatus.Canceled)
                return;

            // Get download options
            var manifest = await _youtubeClient.Videos.Streams.GetManifestAsync(downloadItem.Video!.Id);
            var streamInfo = manifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
            var audioStreamInfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if (streamInfo == null || audioStreamInfo == null)
            {
                downloadItem.Status = DownloadStatus.Failed;
                downloadItem.ErrorMessage = "No suitable streams found";
                _downloadStatusChanged.OnNext(downloadItem);
                return;
            }

            // Download video
            var progress = new Progress<double>(p =>
            {
                downloadItem.Progress = p * 100;
                _downloadStatusChanged.OnNext(downloadItem);
            });

            await _youtubeClient.Videos.DownloadAsync(
                [streamInfo, audioStreamInfo],
                downloadItem.FilePath!,
                progress,
                _cancellationTokenSource.Token
            );

            downloadItem.Status = DownloadStatus.Completed;
            downloadItem.CompletedAt = DateTime.UtcNow;
            downloadItem.CanPause = false;
            downloadItem.CanResume = false;
            downloadItem.CanCancel = false;
            downloadItem.CanRestart = true;
            _downloadStatusChanged.OnNext(downloadItem);
        }
        catch (Exception ex)
        {
            downloadItem.Status = DownloadStatus.Failed;
            downloadItem.ErrorMessage = ex.Message;
            downloadItem.CanPause = false;
            downloadItem.CanResume = false;
            downloadItem.CanCancel = false;
            downloadItem.CanRestart = true;
            _downloadStatusChanged.OnNext(downloadItem);
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _downloadSemaphore?.Dispose();
        _downloadStatusChanged?.Dispose();
        _overallProgress?.Dispose();
    }
}
```

Now I'll create the main application files:


```csharp
using System;
using System.Reflection;
using Avalonia;
using Avalonia.WebView.Desktop;
using EnhancedYoutubeDownloader.Utils;

namespace EnhancedYoutubeDownloader;

public static class Program
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    public static string Name { get; } = Assembly.GetName().Name ?? "EnhancedYoutubeDownloader";

    public static Version Version { get; } = Assembly.GetName().Version ?? new Version(1, 0, 0);

    public static string VersionString { get; } = Version.ToString(3);

    public static bool IsDevelopmentBuild { get; } = Version.Major is <= 0 or >= 999;

    public static string ProjectUrl { get; } = "https://github.com/EnhancedYoutubeDownloader/EnhancedYoutubeDownloader";

    public static string ProjectReleasesUrl { get; } = $"{ProjectUrl}/releases";

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseDesktopWebView();

    [STAThread]
    public static int Main(string[] args)
    {
        // Build and run the app
        var builder = BuildAvaloniaApp();

        try
        {
            return builder.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            if (OperatingSystem.IsWindows())
                _ = NativeMethods.Windows.MessageBox(0, ex.ToString(), "Fatal Error", 0x10);

            throw;
        }
        finally
        {
            // Clean up after application shutdown
            if (builder.Instance is IDisposable disposableApp)
                disposableApp.Dispose();
        }
    }
}
```

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="EnhancedYoutubeDownloader.App"
             RequestedThemeVariant="Default">
    <Application.Styles>
        <StyleInclude Source="avares://Avalonia.Themes.Fluent/FluentTheme.xaml"/>
        <StyleInclude Source="avares://Material.Avalonia/Material.xaml"/>
        <StyleInclude Source="avares://Material.Avalonia/Material.Avalonia.xaml"/>
    </Application.Styles>
</Application>
```

```csharp
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaWebView;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EnhancedYoutubeDownloader.Core.Services;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;
using EnhancedYoutubeDownloader.Utils;
using EnhancedYoutubeDownloader.Utils.Extensions;
using EnhancedYoutubeDownloader.ViewModels;
using EnhancedYoutubeDownloader.ViewModels.Components;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;
using EnhancedYoutubeDownloader.Views;

namespace EnhancedYoutubeDownloader;

public class App : Application, IDisposable
{
    private readonly DisposableCollector _eventRoot = new();
    private readonly IHost _host;
    private readonly SettingsService _settingsService;
    private readonly MainViewModel _mainViewModel;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        _settingsService = _host.Services.GetRequiredService<SettingsService>();
        _mainViewModel = _host.Services.GetRequiredService<MainViewModel>();

        // Re-initialize the theme when the user changes it
        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.Theme,
                () =>
                {
                    RequestedThemeVariant = _settingsService.Theme switch
                    {
                        ThemeVariant.Light => Avalonia.Styling.ThemeVariant.Light,
                        ThemeVariant.Dark => Avalonia.Styling.ThemeVariant.Dark,
                        _ => Avalonia.Styling.ThemeVariant.Default,
                    };

                    InitializeTheme();
                }
            )
        );
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Framework
        services.AddSingleton<DialogManager>();
        services.AddSingleton<SnackbarManager>();
        services.AddSingleton<ViewManager>();
        services.AddSingleton<ViewModelManager>();

        // Core Services
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IDownloadService, DownloadService>();
        services.AddSingleton<INotificationService, NotificationService>();

        // Application Services
        services.AddSingleton<SettingsService>();
        services.AddSingleton<UpdateService>();

        // View models
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DownloadViewModel>();
        services.AddTransient<AuthSetupViewModel>();
        services.AddTransient<DownloadMultipleSetupViewModel>();
        services.AddTransient<DownloadSingleSetupViewModel>();
        services.AddTransient<MessageBoxViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    public override void Initialize()
    {
        base.Initialize();

        AvaloniaXamlLoader.Load(this);
    }

    public override void RegisterServices()
    {
        base.RegisterServices();

        AvaloniaWebViewBuilder.Initialize(config => config.IsInPrivateModeEnabled = true);
    }

    private void InitializeTheme()
    {
        var actualTheme = RequestedThemeVariant?.Key switch
        {
            "Light" => PlatformThemeVariant.Light,
            "Dark" => PlatformThemeVariant.Dark,
            _ => PlatformSettings?.GetColorValues().ThemeVariant ?? PlatformThemeVariant.Light,
        };

        this.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme =
            actualTheme == PlatformThemeVariant.Light
                ? Theme.Create(Theme.Light, Color.Parse("#343838"), Color.Parse("#F9A825"))
                : Theme.Create(Theme.Dark, Color.Parse("#E8E8E8"), Color.Parse("#F9A825"));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainView { DataContext = _mainViewModel };

        base.OnFrameworkInitializationCompleted();

        // Set up custom theme colors
        InitializeTheme();

        // Load settings
        _settingsService.Load();
    }

    private void Application_OnActualThemeVariantChanged(object? sender, EventArgs args) =>
        // Re-initialize the theme when the system theme changes
        InitializeTheme();

    public void Dispose()
    {
        _eventRoot.Dispose();
        _host?.Dispose();
    }
}
```

Now I need to create the missing utility classes and services. Let me create them:


```csharp
using System;
using System.Runtime.InteropServices;

namespace EnhancedYoutubeDownloader.Utils;

public static class NativeMethods
{
    public static class Windows
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(int hWnd, string text, string caption, uint type);
    }
}
```

```csharp
using System;
using System.Collections.Generic;

namespace EnhancedYoutubeDownloader.Utils;

public class DisposableCollector : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    public void Add(IDisposable disposable)
    {
        if (_disposed)
        {
            disposable?.Dispose();
            return;
        }

        _disposables.Add(disposable);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var disposable in _disposables)
        {
            try
            {
                disposable?.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        _disposables.Clear();
    }
}
```

```csharp
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnhancedYoutubeDownloader.Utils.Extensions;

public static class NotifyPropertyChangedExtensions
{
    public static IDisposable WatchProperty<T>(
        this INotifyPropertyChanged source,
        Func<T> propertySelector,
        Action callback,
        bool callCallbackImmediately = false
    )
    {
        if (callCallbackImmediately)
            callback();

        return new PropertyWatcher<T>(source, propertySelector, callback);
    }

    private class PropertyWatcher<T> : IDisposable
    {
        private readonly INotifyPropertyChanged _source;
        private readonly Func<T> _propertySelector;
        private readonly Action _callback;
        private T? _lastValue;

        public PropertyWatcher(INotifyPropertyChanged source, Func<T> propertySelector, Action callback)
        {
            _source = source;
            _propertySelector = propertySelector;
            _callback = callback;
            _lastValue = propertySelector();

            _source.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var newValue = _propertySelector();
            if (!Equals(_lastValue, newValue))
            {
                _lastValue = newValue;
                _callback();
            }
        }

        public void Dispose()
        {
            _source.PropertyChanged -= OnPropertyChanged;
        }
    }
}
```

```csharp
using System;

namespace EnhancedYoutubeDownloader.Utils.Extensions;

public static class DisposableExtensions
{
    public static T AddTo<T>(this T disposable, DisposableCollector collector) where T : IDisposable
    {
        collector.Add(disposable);
        return disposable;
    }
}
```

```csharp
using Avalonia;
using Avalonia.Controls;
using Material.Styles.Themes;

namespace EnhancedYoutubeDownloader.Utils.Extensions;

public static class AvaloniaExtensions
{
    public static T? LocateMaterialTheme<T>(this Application app) where T : class
    {
        return app.Styles.TryGetResource("MaterialTheme", out var theme) ? theme as T : null;
    }
}
```

Now I need to create the framework classes:


```csharp
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EnhancedYoutubeDownloader.Framework;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
        }

        _disposed = true;
    }
}
```

```csharp
namespace EnhancedYoutubeDownloader.Framework;

public enum ThemeVariant
{
    Light,
    Dark,
    Auto
}
```

```csharp
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EnhancedYoutubeDownloader.Framework;

public class DialogManager
{
    public async Task<T?> ShowDialogAsync<T>(ObservableObject viewModel) where T : class
    {
        // This is a simplified implementation
        // In a real implementation, you'd create the appropriate dialog window
        // and show it modally
        return null;
    }

    public async Task<bool?> ShowDialogAsync(ObservableObject viewModel)
    {
        // This is a simplified implementation
        // In a real implementation, you'd create the appropriate dialog window
        // and show it modally
        return false;
    }

    public async Task<string?> ShowOpenFileDialogAsync(string title, string[] filters)
    {
        // This is a simplified implementation
        // In a real implementation, you'd use Avalonia's file dialogs
        return null;
    }

    public async Task<string?> ShowSaveFileDialogAsync(string title, string[] filters, string defaultName)
    {
        // This is a simplified implementation
        // In a real implementation, you'd use Avalonia's file dialogs
        return null;
    }

    public async Task<string?> ShowFolderDialogAsync(string title)
    {
        // This is a simplified implementation
        // In a real implementation, you'd use Avalonia's folder dialogs
        return null;
    }
}
```

```csharp
using System;

namespace EnhancedYoutubeDownloader.Framework;

public class SnackbarManager
{
    public void Notify(string message, string? actionText = null, Action? action = null)
    {
        // This is a simplified implementation
        // In a real implementation, you'd show a snackbar notification
        Console.WriteLine($"Notification: {message}");
        if (!string.IsNullOrEmpty(actionText) && action != null)
        {
            Console.WriteLine($"Action: {actionText}");
        }
    }
}
```

```csharp
using System;
using Avalonia.Controls;

namespace EnhancedYoutubeDownloader.Framework;

public class ViewManager
{
    public T CreateView<T>() where T : Control, new()
    {
        return new T();
    }
}
```

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using EnhancedYoutubeDownloader.ViewModels;
using EnhancedYoutubeDownloader.ViewModels.Components;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;

namespace EnhancedYoutubeDownloader.Framework;

public class ViewModelManager
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public MainViewModel CreateMainViewModel() => _serviceProvider.GetRequiredService<MainViewModel>();
    public DashboardViewModel CreateDashboardViewModel() => _serviceProvider.GetRequiredService<DashboardViewModel>();
    public DownloadViewModel CreateDownloadViewModel() => _serviceProvider.GetRequiredService<DownloadViewModel>();
    public AuthSetupViewModel CreateAuthSetupViewModel() => _serviceProvider.GetRequiredService<AuthSetupViewModel>();
    public DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel() => _serviceProvider.GetRequiredService<DownloadMultipleSetupViewModel>();
    public DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel() => _serviceProvider.GetRequiredService<DownloadSingleSetupViewModel>();
    public MessageBoxViewModel CreateMessageBoxViewModel() => _serviceProvider.GetRequiredService<MessageBoxViewModel>();
    public SettingsViewModel CreateSettingsViewModel() => _serviceProvider.GetRequiredService<SettingsViewModel>();
}
```

Now I need to create the services and view models. Let me create the basic structure:


```csharp
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cogwheel;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.Services;

public partial class SettingsService : SettingsBase
{
    [DefaultValue(ThemeVariant.Auto)]
    public ThemeVariant Theme { get; set; } = ThemeVariant.Auto;

    [DefaultValue(3)]
    public int ParallelLimit { get; set; } = 3;

    [DefaultValue(true)]
    public bool ShouldInjectSubtitles { get; set; } = true;

    [DefaultValue(true)]
    public bool ShouldInjectTags { get; set; } = true;

    [DefaultValue(true)]
    public bool ShouldInjectLanguageSpecificAudioStreams { get; set; } = true;

    [DefaultValue("")]
    public string LastAuthCookies { get; set; } = string.Empty;

    [DefaultValue(true)]
    public bool IsUkraineSupportMessageEnabled { get; set; } = true;

    public SettingsService() : base(Program.Name)
    {
    }
}
```

```csharp
using System;
using System.Threading.Tasks;
using Onova;
using Onova.Services;

namespace EnhancedYoutubeDownloader.Services;

public class UpdateService
{
    private readonly UpdateManager _updateManager;

    public UpdateService()
    {
        _updateManager = new UpdateManager(
            new GithubPackageResolver("EnhancedYoutubeDownloader", "EnhancedYoutubeDownloader", "EnhancedYoutubeDownloader-*.zip"),
            new ZipPackageExtractor()
        );
    }

    public async Task<Version?> CheckForUpdatesAsync()
    {
        try
        {
            return await _updateManager.CheckForUpdatesAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task PrepareUpdateAsync(Version version)
    {
        await _updateManager.PrepareUpdateAsync(version);
    }

    public void FinalizeUpdate(bool restart)
    {
        _updateManager.LaunchUpdater(restart);
    }
}
```

```csharp
using System;
using EnhancedYoutubeDownloader.Shared.Interfaces;

namespace EnhancedYoutubeDownloader.Services;

public class NotificationService : INotificationService
{
    public void ShowSuccess(string message)
    {
        Console.WriteLine($"Success: {message}");
    }

    public void ShowError(string message)
    {
        Console.WriteLine($"Error: {message}");
    }

    public void ShowWarning(string message)
    {
        Console.WriteLine($"Warning: {message}");
    }

    public void ShowInfo(string message)
    {
        Console.WriteLine($"Info: {message}");
    }

    public void ShowProgress(string message, double progress)
    {
        Console.WriteLine($"Progress: {message} ({progress:P})");
    }
}
```

Now I need to create the view models. Let me create the basic structure:


```csharp
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;
using EnhancedYoutubeDownloader.Utils;
using EnhancedYoutubeDownloader.Utils.Extensions;
using EnhancedYoutubeDownloader.ViewModels.Components;

namespace EnhancedYoutubeDownloader.ViewModels;

public partial class MainViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SnackbarManager snackbarManager,
    SettingsService settingsService,
    UpdateService updateService
) : ViewModelBase
{
    public string Title { get; } = $"{Program.Name} v{Program.VersionString}";

    public DashboardViewModel Dashboard { get; } = viewModelManager.CreateDashboardViewModel();

    private async Task ShowUkraineSupportMessageAsync()
    {
        if (!settingsService.IsUkraineSupportMessageEnabled)
            return;

        var dialog = viewModelManager.CreateMessageBoxViewModel();
        // Set dialog properties here

        // Disable this message in the future
        settingsService.IsUkraineSupportMessageEnabled = false;
        settingsService.Save();
    }

    private async Task ShowDevelopmentBuildMessageAsync()
    {
        if (!Program.IsDevelopmentBuild)
            return;

        // If debugging, the user is likely a developer
        if (Debugger.IsAttached)
            return;

        var dialog = viewModelManager.CreateMessageBoxViewModel();
        // Set dialog properties here
    }

    private async Task ShowFFmpegMessageAsync()
    {
        // Check if FFmpeg is available
        var dialog = viewModelManager.CreateMessageBoxViewModel();
        // Set dialog properties here
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateVersion = await updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            snackbarManager.Notify($"Downloading update to {Program.Name} v{updateVersion}...");
            await updateService.PrepareUpdateAsync(updateVersion);

            snackbarManager.Notify(
                "Update has been downloaded and will be installed when you exit",
                "INSTALL NOW",
                () =>
                {
                    updateService.FinalizeUpdate(true);

                    if (Application.Current?.ApplicationLifetime?.TryShutdown(2) != true)
                        Environment.Exit(2);
                }
            );
        }
        catch
        {
            // Failure to update shouldn't crash the application
            snackbarManager.Notify("Failed to perform application update");
        }
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await ShowUkraineSupportMessageAsync();
        await ShowDevelopmentBuildMessageAsync();
        await ShowFFmpegMessageAsync();
        await CheckForUpdatesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Save settings
            settingsService.Save();

            // Finalize pending updates
            updateService.FinalizeUpdate(false);
        }

        base.Dispose(disposing);
    }
}
```

```csharp
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;
using EnhancedYoutubeDownloader.Shared.Interfaces;
using EnhancedYoutubeDownloader.Shared.Models;
using EnhancedYoutubeDownloader.Utils;
using EnhancedYoutubeDownloader.Utils.Extensions;

namespace EnhancedYoutubeDownloader.ViewModels.Components;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly SnackbarManager _snackbarManager;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;
    private readonly IDownloadService _downloadService;
    private readonly DisposableCollector _eventRoot = new();

    public DashboardViewModel(
        ViewModelManager viewModelManager,
        SnackbarManager snackbarManager,
        DialogManager dialogManager,
        SettingsService settingsService,
        IDownloadService downloadService
    )
    {
        _viewModelManager = viewModelManager;
        _snackbarManager = snackbarManager;
        _dialogManager = dialogManager;
        _settingsService = settingsService;
        _downloadService = downloadService;

        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.ParallelLimit,
                () => OnPropertyChanged(nameof(IsProgressIndeterminate)),
                true
            )
        );
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProgressIndeterminate))]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowAuthSetupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowSettingsCommand))]
    public partial bool IsBusy { get; set; }

    public bool IsProgressIndeterminate => IsBusy && Downloads.Any(d => d.Status == DownloadStatus.Started);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    public partial string? Query { get; set; }

    public ObservableCollection<DownloadItem> Downloads { get; } = [];

    private bool CanShowAuthSetup() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowAuthSetup))]
    private async Task ShowAuthSetupAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateAuthSetupViewModel());

    private bool CanShowSettings() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowSettings))]
    private async Task ShowSettingsAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateSettingsViewModel());

    private bool CanProcessQuery() => !IsBusy && !string.IsNullOrWhiteSpace(Query);

    [RelayCommand(CanExecute = nameof(CanProcessQuery))]
    private async Task ProcessQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
            return;

        IsBusy = true;

        try
        {
            // This is a simplified implementation
            // In a real implementation, you'd resolve the query and create downloads
            _snackbarManager.Notify($"Processing query: {Query}");
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.CreateMessageBoxViewModel()
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void RemoveSuccessfulDownloads()
    {
        var completedDownloads = Downloads.Where(d => d.Status == DownloadStatus.Completed).ToList();
        foreach (var download in completedDownloads)
        {
            Downloads.Remove(download);
        }
    }

    [RelayCommand]
    private void RemoveInactiveDownloads()
    {
        var inactiveDownloads = Downloads.Where(d => 
            d.Status == DownloadStatus.Completed || 
            d.Status == DownloadStatus.Failed || 
            d.Status == DownloadStatus.Canceled).ToList();
        
        foreach (var download in inactiveDownloads)
        {
            Downloads.Remove(download);
        }
    }

    [RelayCommand]
    private async Task RestartDownloadAsync(DownloadItem download)
    {
        await _downloadService.RestartDownloadAsync(download);
    }

    [RelayCommand]
    private async Task PauseDownloadAsync(DownloadItem download)
    {
        await _downloadService.PauseDownloadAsync(download);
    }

    [RelayCommand]
    private async Task ResumeDownloadAsync(DownloadItem download)
    {
        await _downloadService.ResumeDownloadAsync(download);
    }

    [RelayCommand]
    private async Task CancelDownloadAsync(DownloadItem download)
    {
        await _downloadService.CancelDownloadAsync(download);
    }

    [RelayCommand]
    private async Task DeleteDownloadAsync(DownloadItem download)
    {
        await _downloadService.DeleteDownloadAsync(download);
        Downloads.Remove(download);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventRoot.Dispose();
        }

        base.Dispose(disposing);
    }
}
```

Now I need to create the remaining view models. Let me create placeholder implementations:


```csharp
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Shared.Models;

namespace EnhancedYoutubeDownloader.ViewModels.Components;

public partial class DownloadViewModel : ViewModelBase
{
    [ObservableProperty]
    private DownloadItem _downloadItem = new();

    public DownloadViewModel()
    {
    }
}
```

```csharp
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class AuthSetupViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _status = "Ready";

    public AuthSetupViewModel()
    {
    }
}
```

```csharp
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadMultipleSetupViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private List<IVideo> _videos = new();

    [ObservableProperty]
    private bool _selectAll = true;

    public DownloadMultipleSetupViewModel()
    {
    }
}
```

```csharp
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;
using YoutubeExplode.Videos;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadSingleSetupViewModel : ViewModelBase
{
    [ObservableProperty]
    private IVideo? _video;

    [ObservableProperty]
    private List<object> _downloadOptions = new();

    [ObservableProperty]
    private string _filePath = string.Empty;

    public DownloadSingleSetupViewModel()
    {
    }
}
```

```csharp
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class MessageBoxViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string _primaryButtonText = "OK";

    [ObservableProperty]
    private string _secondaryButtonText = string.Empty;

    public MessageBoxViewModel()
    {
    }
}
```

```csharp
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnhancedYoutubeDownloader.Framework;
using EnhancedYoutubeDownloader.Services;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private SettingsService _settings;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settings = settingsService;
    }
}
```

Now I need to create the main view:


```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:vm="using:EnhancedYoutubeDownloader.ViewModels"
        xmlns:components="using:EnhancedYoutubeDownloader.ViewModels.Components"
        mc:Ignorable="d" d:DesignWidth="1200" d:DesignHeight="800"
        x:Class="EnhancedYoutubeDownloader.Views.MainView"
        x:DataType="vm:MainViewModel"
        Title="{Binding Title}"
        Icon="/favicon.ico"
        MinWidth="800" MinHeight="600">

    <Design.DataContext>
        <vm:MainViewModel />
    </Design.DataContext>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <Border Grid.Row="0" Background="{DynamicResource MaterialDesignPaper}" 
                BorderBrush="{DynamicResource MaterialDesignDivider}" 
                BorderThickness="0,0,0,1">
            <Grid Margin="16,12">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <TextBlock Grid.Column="0" 
                          Text="Enhanced YouTube Downloader" 
                          Style="{DynamicResource MaterialDesignHeadline5TextBlock}"
                          VerticalAlignment="Center"/>

                <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8">
                    <Button Content="Settings" 
                            Style="{DynamicResource MaterialDesignOutlinedButton}"
                            Command="{Binding Dashboard.ShowSettingsCommand}"/>
                    <Button Content="Auth" 
                            Style="{DynamicResource MaterialDesignOutlinedButton}"
                            Command="{Binding Dashboard.ShowAuthSetupCommand}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Main Content -->
        <Grid Grid.Row="1">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>

            <!-- Query Input -->
            <Border Grid.Row="0" 
                    Background="{DynamicResource MaterialDesignPaper}" 
                    BorderBrush="{DynamicResource MaterialDesignDivider}" 
                    BorderThickness="0,0,0,1"
                    Padding="16,12">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>

                    <TextBox Grid.Column="0" 
                             Text="{Binding Dashboard.Query, UpdateSourceTrigger=PropertyChanged}"
                             Watermark="Enter YouTube URL, playlist, channel, or search query..."
                             Style="{DynamicResource MaterialDesignOutlinedTextBox}"
                             Margin="0,0,8,0"/>

                    <Button Grid.Column="1" 
                            Content="Download" 
                            Style="{DynamicResource MaterialDesignRaisedButton}"
                            Command="{Binding Dashboard.ProcessQueryCommand}"
                            IsEnabled="{Binding Dashboard.ProcessQueryCommand.CanExecute}"/>
                </Grid>
            </Border>

            <!-- Downloads List -->
            <ScrollViewer Grid.Row="1" Padding="16">
                <ItemsControl ItemsSource="{Binding Dashboard.Downloads}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Background="{DynamicResource MaterialDesignCardBackground}"
                                    BorderBrush="{DynamicResource MaterialDesignDivider}"
                                    BorderThickness="1"
                                    CornerRadius="4"
                                    Margin="0,0,0,8"
                                    Padding="16">
                                <Grid>
                                    <Grid.RowDefinitions>
                                        <RowDefinition Height="Auto"/>
                                        <RowDefinition Height="Auto"/>
                                        <RowDefinition Height="Auto"/>
                                    </Grid.RowDefinitions>

                                    <!-- Title and Status -->
                                    <Grid Grid.Row="0">
                                        <Grid.ColumnDefinitions>
                                            <ColumnDefinition Width="*"/>
                                            <ColumnDefinition Width="Auto"/>
                                        </Grid.ColumnDefinitions>

                                        <StackPanel Grid.Column="0">
                                            <TextBlock Text="{Binding Title}" 
                                                      Style="{DynamicResource MaterialDesignSubtitle1TextBlock}"
                                                      TextWrapping="Wrap"/>
                                            <TextBlock Text="{Binding Author}" 
                                                      Style="{DynamicResource MaterialDesignBody2TextBlock}"
                                                      Foreground="{DynamicResource MaterialDesignBodyLight}"/>
                                        </StackPanel>

                                        <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="4">
                                            <Button Content="Pause" 
                                                    Style="{DynamicResource MaterialDesignOutlinedButton}"
                                                    Command="{Binding $parent[ItemsControl].DataContext.Dashboard.PauseDownloadCommand}"
                                                    CommandParameter="{Binding}"/>
                                            <Button Content="Cancel" 
                                                    Style="{DynamicResource MaterialDesignOutlinedButton}"
                                                    Command="{Binding $parent[ItemsControl].DataContext.Dashboard.CancelDownloadCommand}"
                                                    CommandParameter="{Binding}"/>
                                            <Button Content="Delete" 
                                                    Style="{DynamicResource MaterialDesignOutlinedButton}"
                                                    Command="{Binding $parent[ItemsControl].DataContext.Dashboard.DeleteDownloadCommand}"
                                                    CommandParameter="{Binding}"/>
                                        </StackPanel>
                                    </Grid>

                                    <!-- Progress -->
                                    <ProgressBar Grid.Row="1" 
                                                Value="{Binding Progress}" 
                                                Maximum="100"
                                                Margin="0,8,0,0"
                                                Style="{DynamicResource MaterialDesignLinearProgressBar}"/>

                                    <!-- Status -->
                                    <TextBlock Grid.Row="2" 
                                              Text="{Binding Status}" 
                                              Style="{DynamicResource MaterialDesignCaptionTextBlock}"
                                              Margin="0,4,0,0"/>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </ScrollViewer>
        </Grid>
    </Grid>
</Window>
```

```csharp
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EnhancedYoutubeDownloader.ViewModels;

namespace EnhancedYoutubeDownloader.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
```

Now I need to create the FFmpeg download script:


```powershell
param(
    [string]$Platform = "",
    [string]$OutputPath = "."
)

# FFmpeg download script for Enhanced YouTube Downloader
# This script downloads the appropriate FFmpeg binary for the current platform

$ErrorActionPreference = "Stop"

function Get-PlatformInfo {
    if ($Platform) {
        return $Platform
    }
    
    $os = if ($IsWindows -or $env:OS -eq "Windows_NT") { "win" }
          elseif ($IsLinux) { "linux" }
          elseif ($IsMacOS) { "osx" }
          else { "unknown" }
    
    $arch = switch ([System.Environment]::OSArchitecture) {
        "X64" { "x64" }
        "X86" { "x86" }
        "Arm64" { "arm64" }
        "Arm" { "arm" }
        default { "x64" }
    }
    
    return "$os-$arch"
}

function Download-FFmpeg {
    $platformInfo = Get-PlatformInfo
    $downloadUrl = "https://github.com/FFmpegBin/FFmpegBin/releases/download/7.1.1/ffmpeg-$platformInfo.zip"
    $outputFile = Join-Path $OutputPath "ffmpeg.zip"
    
    Write-Host "Downloading FFmpeg for platform: $platformInfo"
    Write-Host "URL: $downloadUrl"
    
    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $outputFile -UseBasicParsing
        Write-Host "Download completed successfully"
        
        # Extract the archive
        Expand-Archive -Path $outputFile -DestinationPath $OutputPath -Force
        Remove-Item $outputFile -Force
        
        # Make executable on Unix systems
        if ($IsLinux -or $IsMacOS) {
            $ffmpegPath = Join-Path $OutputPath "ffmpeg"
            if (Test-Path $ffmpegPath) {
                chmod +x $ffmpegPath
            }
        }
        
        Write-Host "FFmpeg extracted and configured successfully"
    }
    catch {
        Write-Error "Failed to download or extract FFmpeg: $_"
        throw
    }
}

# Main execution
try {
    Download-FFmpeg
    Write-Host "FFmpeg setup completed successfully"
}
catch {
    Write-Error "FFmpeg setup failed: $_"
    exit 1
}
```

Now I need to create the app manifest:


```plaintext
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity version="1.0.0.0" name="EnhancedYoutubeDownloader"/>
  <trustInfo xmlns="urn:schemas-microsoft-com:asm.v2">
    <security>
      <requestedPrivileges xmlns="urn:schemas-microsoft-com:asm.v3">
        <requestedExecutionLevel level="asInvoker" uiAccess="false" />
      </requestedPrivileges>
    </security>
  </trustInfo>
  <compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
    <application>
      <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
      <supportedOS Id="{1f676c76-80e1-4239-95bb-83d0f6d0da78}" />
      <supportedOS Id="{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}" />
      <supportedOS Id="{35138b9a-5d96-4fbd-8e2d-a2440225f93a}" />
      <supportedOS Id="{e2011457-1546-43c5-a5fe-008f5f5a0000}" />
    </application>
  </compatibility>
</assembly>
```

Now let me create a README file and some basic documentation:


```markdown
# Enhanced YouTube Downloader

An enhanced, production-ready version of YouTube Downloader that surpasses the original in functionality, user experience, and code quality.

## Features

### Core Features
- ✅ Cross-platform support (Windows, Linux, macOS)
- ✅ Download single videos, playlists, and channels
- ✅ Multiple quality and format options
- ✅ Subtitle support
- ✅ Metadata tagging
- ✅ Authentication for private content

### Enhanced Features
- 🚀 **Unified Queue Management** - Single pane for all download states
- 🚀 **Pause/Resume Functionality** - State persistence and recovery
- 🚀 **Enhanced Error Handling** - User-friendly messages with actions
- 🚀 **Metadata Caching** - SQLite-based cache with invalidation
- 🚀 **Drag-and-Drop Support** - URLs anywhere in UI
- 🚀 **Batch Operations** - Multi-select and apply settings
- 🚀 **Download Scheduling** - Time-based queue execution
- 🚀 **Format Profiles** - Quick presets for common scenarios
- 🚀 **Better Feedback** - Toast notifications and loading states
- 🚀 **Performance Optimization** - Parallel downloads, chunking, retry logic

## Technology Stack

- **Framework**: .NET 9.0
- **UI**: Avalonia UI 11.x with Material Design
- **YouTube API**: YoutubeExplode 6.5.4+
- **Media Processing**: FFmpeg 7.x
- **Database**: SQLite for caching
- **Testing**: xUnit
- **CI/CD**: GitHub Actions

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- PowerShell (for FFmpeg download)

### Building from Source

1. Clone the repository:
```bash
git clone https://github.com/EnhancedYoutubeDownloader/EnhancedYoutubeDownloader.git
cd EnhancedYoutubeDownloader
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run --project src/Desktop/EnhancedYoutubeDownloader.csproj
```

### Building for Distribution

```bash
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
```

## Architecture

The application follows a clean architecture pattern with clear separation of concerns:

```
src/
├── Core/           # Business logic and services
├── Desktop/        # Avalonia UI application
├── Shared/         # Shared models and interfaces
└── Tests/          # Unit and integration tests
```

### Key Components

- **DownloadService**: Manages download operations with pause/resume
- **CacheService**: Handles metadata caching with SQLite
- **NotificationService**: Provides user feedback
- **DashboardViewModel**: Main UI logic and queue management

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Original [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) by Tyrrrz
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) for YouTube API access
- [Avalonia UI](https://avaloniaui.net/) for cross-platform UI
- [Material Design](https://material.io/) for design system
```

```plaintext
MIT License

Copyright (c) 2025 Enhanced YouTube Downloader Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

Now let me create a favicon and test the build:


Now let me test the build:


---

**User**

continue

