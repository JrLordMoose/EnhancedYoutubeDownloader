using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EnhancedYoutubeDownloader.ViewModels.Dialogs;

namespace EnhancedYoutubeDownloader.Views.Dialogs;

public partial class SettingsDialog : UserControl
{
    public SettingsDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void UpdateLinkPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsViewModel viewModel)
        {
            viewModel.GoToUpdateCheckCommand.Execute(null);
        }
    }
}
