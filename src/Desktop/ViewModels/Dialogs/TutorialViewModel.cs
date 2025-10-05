using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnhancedYoutubeDownloader.Framework;

namespace EnhancedYoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// ViewModel for the Tutorial/Help dialog
/// </summary>
public partial class TutorialViewModel : DialogViewModelBase
{
    public string Title => "How to Use Enhanced YouTube Downloader";

    public string WelcomeText => "Welcome to Enhanced YouTube Downloader!";

    public string InstructionsText =>
        @"Quick Start Guide:

1. **Enter a URL or Search Query**
   - Paste any YouTube video, playlist, or channel URL
   - Or enter a search query to find videos
   - Press Enter or click Download

2. **Configure Download Settings**
   - Choose quality (Best Quality, 1080p, 720p, etc.)
   - Select format (MP4, WebM, MP3)
   - Enable/disable subtitles and metadata injection

3. **Manage Downloads**
   - Pause/Resume downloads as needed
   - Cancel unwanted downloads
   - Open completed files from the dashboard

4. **Settings**
   - Configure default download location
   - Set parallel download limit
   - Customize default quality and format
   - Choose your preferred theme

5. **Authentication** (Optional)
   - Sign in with Google for private videos
   - Access age-restricted content

Need help or found a bug? Use the buttons below!";

    public TutorialViewModel()
    {
        // Design-time constructor
    }

    [RelayCommand]
    private void OpenGitHub()
    {
        try
        {
            Console.WriteLine("[TUTORIAL] Opening GitHub page...");
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "https://github.com/JrLordMoose/EnhancedYoutubeDownloader",
                    UseShellExecute = true,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TUTORIAL] Failed to open GitHub: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenBugReport()
    {
        try
        {
            Console.WriteLine("[TUTORIAL] Opening bug report form...");
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "https://forms.gle/PiFJk212eFwrFB8Z6",
                    UseShellExecute = true,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TUTORIAL] Failed to open bug report: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CloseDialog()
    {
        Close(true);
    }
}
