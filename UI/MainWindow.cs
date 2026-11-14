using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace MiniSheets;

internal sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "MiniSheets";
        Width = 960;
        Height = 600;
        Content = new DockPanel
        {
            LastChildFill = true,
            Children =
            {
                new MenuBar()
                {
                    [DockPanel.DockProperty] = Dock.Top
                },
                new Separator
                {
                    [DockPanel.DockProperty] = Dock.Top,
                    Height = 1,
                    Background = new SolidColorBrush(Color.Parse("#aca3a38e")),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                new ToolBar()
                {
                    [DockPanel.DockProperty] = Dock.Top
                },
                new Separator
                {
                    [DockPanel.DockProperty] = Dock.Top,
                    Height = 1,
                    Background = new SolidColorBrush(Color.Parse("#aca3a38e")),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                new FormulaBar()
                {
                    [DockPanel.DockProperty] = Dock.Top
                },
                new GridView()
                {
                    [DockPanel.DockProperty] = Dock.Top
                },
                new Border()
            }
        };
    }
}
