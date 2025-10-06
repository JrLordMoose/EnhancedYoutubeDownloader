using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    private void OnQueryTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is MainViewModel viewModel)
        {
            // Check if the command can execute
            if (viewModel.Dashboard.ProcessQueryCommand.CanExecute(null))
            {
                // Execute the command
                viewModel.Dashboard.ProcessQueryCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
