using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EnhancedYoutubeDownloader.ViewModels;

namespace EnhancedYoutubeDownloader.Views;

public partial class MainView : Window
{
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
