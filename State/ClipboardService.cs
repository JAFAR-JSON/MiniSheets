using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Input.Platform;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.Input;

namespace MiniSheets.State;

public static class Clipboard
{
    public static IClipboard? Get()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }) {
            return window.Clipboard;

        }
        else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime { MainView: { } mainView }) {
            if (TopLevel.GetTopLevel(mainView) is TopLevel topLevel) {
                return topLevel.Clipboard;
            }
        }

        return null;
    }

    public static async Task CopyAsync(string text)
    {
        var clipboard = Get();

        if (clipboard == null)
            return;

        var data = new DataTransfer();
        data.Add(DataTransferItem.CreateText(text));

        await clipboard.SetDataAsync(data);

        await clipboard.FlushAsync();
    }

    public static async Task<string> PasteAsync()
    {
        var clipboard = Get();
        return clipboard == null
            ? string.Empty
            : await clipboard.TryGetTextAsync() ?? string.Empty;
    }
}