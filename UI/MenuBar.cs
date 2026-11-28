using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using MiniSheets.UI.Models;

namespace MiniSheets.UI;

internal sealed class MenuBar : Border
{
    public event EventHandler? BordersClicked;

    public event EventHandler? CopyButtonClicked;
    public event EventHandler? PasteButtonClicked;
    public event EventHandler? CutButtonClicked;

    public event EventHandler? ExportCsvClicked;
    public event EventHandler? ImportCsvClicked;

    private readonly Dictionary<string, ClipboardEnum> _clipboardActions = new()
    {
        ["Copy"] = ClipboardEnum.Copy,
        ["Paste"] = ClipboardEnum.Paste,
        ["Cut"] = ClipboardEnum.Cut
    };

    private readonly Dictionary<string, FileActionsEnum> _fileActions = new()
    {
        ["New"] = FileActionsEnum.New,
        ["Open"] = FileActionsEnum.Open,
        ["Save"] = FileActionsEnum.Save,
        ["Save As"] = FileActionsEnum.SaveAs,
        ["Import CSV"] = FileActionsEnum.ImportCSV,
        ["Export CSV"] = FileActionsEnum.ExportCSV,
        ["Settings"] = FileActionsEnum.Settings,
        ["Exit"] = FileActionsEnum.Exit
    };

    public MenuBar()
    {
        Padding = new Thickness(6, 2);

        Child = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                CreateMenuButton("File", "New", "Open", "Save", "Save As", "Import CSV", "Export CSV", "Settings", "Exit"),
                CreateMenuButton("Edit", "Cut", "Copy", "Paste"),
                CreateMenuButton("Format", "Fill", "Text colour", "Increase decimals", "Decrease decimals", "Borders"),
                //CreateMenuButton("Sheet", "Add Sheet", "Rename Sheet", "Delete Sheet", "Insert row above", "Delete row", "Insert column left", "Delete column"),
            }
        };
    }

    private Button CreateMenuButton(string title, params string[] items)
    {
        var menu = new MenuFlyout();

        var index = 0;
        foreach (var item in items)
        {
            var menuItem = new MenuItem
            {
                Header = item
            };

            if (item == "Borders")
            {
                menuItem.Click += (_, _) => BordersClicked?.Invoke(this, EventArgs.Empty);
            }
            else if (_clipboardActions.ContainsKey(item))
            {
                var action = _clipboardActions[item];
                menuItem.Click += (_, _) => HandleClipboardAction(action);
            }
            else if (_fileActions.ContainsKey(item))
            {
                var action = _fileActions[item];
                menuItem.Click += (_, _) => HandleFileActions(action);
            }

            menu.Items.Add(menuItem);
            index++;
        }

        menu.Placement = PlacementMode.BottomEdgeAlignedLeft;

        return new Button
        {
            Content = title,
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Flyout = menu
        };
    }

    private void HandleClipboardAction(ClipboardEnum action)
    {
        switch (action)
        {
            case ClipboardEnum.Copy:
                CopyButtonClicked?.Invoke(this, EventArgs.Empty);
                break;

            case ClipboardEnum.Paste:
                PasteButtonClicked?.Invoke(this, EventArgs.Empty);
                break;

            case ClipboardEnum.Cut:
                CutButtonClicked?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
    
    private void HandleFileActions(FileActionsEnum action)
    {
        switch (action)
        {
            case FileActionsEnum.ExportCSV:
                ExportCsvClicked?.Invoke(this, EventArgs.Empty);
                break;

            case FileActionsEnum.ImportCSV:
                ImportCsvClicked?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
}
