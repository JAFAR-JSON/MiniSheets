using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace MiniSheets;

internal sealed class MenuBar : Border
{
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
                CreateMenuButton("Edit", "Undo", "Redo", "Cut", "Copy", "Paste"),
                CreateMenuButton("Format", "Fill", "Text colour", "Increase decimals", "Decrease decimals"),
                CreateMenuButton("Sheet", "Add Sheet", "Rename Sheet", "Delete Sheet", "Insert row above", "Delete row", "Insert column left", "Delete column"),
            }
        };
    }

    private Button CreateMenuButton(string title, params string[] items)
    {
        var menu = new MenuFlyout();

        foreach (var item in items)
        {
            menu.Items.Add(new MenuItem
            {
                Header = item
            });
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
}