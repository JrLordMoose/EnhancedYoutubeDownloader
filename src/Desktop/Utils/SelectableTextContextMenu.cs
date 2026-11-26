using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace EnhancedYoutubeDownloader.Utils;

/// <summary>
/// Gives every <see cref="SelectableTextBlock"/> a right-click menu with Copy and Select All.
/// Material.Avalonia doesn't style one, so without this users could select text but only
/// copy it with Ctrl+C. Copy with nothing selected copies the whole text.
/// </summary>
public static class SelectableTextContextMenu
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
            return;

        _registered = true;
        Control.ContextRequestedEvent.AddClassHandler<SelectableTextBlock>(OnContextRequested);
    }

    private static void OnContextRequested(
        SelectableTextBlock textBlock,
        ContextRequestedEventArgs e
    )
    {
        if (e.Handled)
            return;

        var copy = new MenuItem
        {
            Header = "Copy",
            InputGesture = new KeyGesture(Key.C, KeyModifiers.Control),
        };
        copy.Click += async (_, _) => await CopyAsync(textBlock);

        var selectAll = new MenuItem { Header = "Select All" };
        selectAll.Click += (_, _) => textBlock.SelectAll();

        var menu = new ContextMenu { ItemsSource = new[] { copy, selectAll } };
        menu.Open(textBlock);
        e.Handled = true;
    }

    private static async System.Threading.Tasks.Task CopyAsync(SelectableTextBlock textBlock)
    {
        if (!string.IsNullOrEmpty(textBlock.SelectedText))
        {
            textBlock.Copy();
            return;
        }

        var clipboard = TopLevel.GetTopLevel(textBlock)?.Clipboard;
        if (clipboard != null && !string.IsNullOrEmpty(textBlock.Text))
            await clipboard.SetTextAsync(textBlock.Text);
    }
}
