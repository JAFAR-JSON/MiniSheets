using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Models;

namespace MiniSheets;

internal sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "MiniSheets";
        Width = 960;
        Height = 600;

        var menuBar = new MenuBar();
        var formulaBar = new FormulaBar();

        var spreadsheet = new Spreadsheet(100, 100);
        var gridView = new GridView(spreadsheet);
        var uiHandler = new SpreadSheetUiEventHandler(gridView, formulaBar, menuBar);

        DockPanel.SetDock(menuBar, Dock.Top);
        DockPanel.SetDock(formulaBar, Dock.Top);
        DockPanel.SetDock(gridView, Dock.Top);

        Content = new DockPanel
        {
            LastChildFill = true,
            Children =
            {
                menuBar,
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
                formulaBar,
                gridView,
                new Border()
            }
        };

    }
}
