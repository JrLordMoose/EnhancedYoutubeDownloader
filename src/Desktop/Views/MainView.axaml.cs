using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EnhancedYoutubeDownloader.ViewModels;

namespace EnhancedYoutubeDownloader.Views;

/// <summary>
/// The main window of the application.
/// </summary>
public partial class MainView : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        // Call InitializeAsync when the window is opened
        this.Opened += async (sender, args) =>
        {
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.InitializeCommand.ExecuteAsync(null);
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}