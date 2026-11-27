using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;
using MiniSheets.State;

namespace MiniSheets.UI;

internal sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "MiniSheets";
        Width = 960;
        Height = 600;

        var menuBar = new MenuBar();
        var formulaBar = new FormulaBar();
        var toolBar = new ToolBar();
        var statusBar = new StatusBar();

        var workbook = new Workbook();
        var sheet = new Sheet("Sheet1");
        workbook.AddSheet(sheet);
        var spreadsheetEngine = new SpreadsheetEngine(workbook);
        var spreadsheetProviderService = new SpreadsheetProviderService(spreadsheetEngine);
        var gridView = new GridView(sheet, spreadsheetProviderService);
        var uiHandler = new SpreadSheetUiEventHandler(gridView, formulaBar, menuBar, toolBar, statusBar);

        

        DockPanel.SetDock(menuBar, Dock.Top);
        DockPanel.SetDock(toolBar, Dock.Top);
        DockPanel.SetDock(formulaBar, Dock.Top);

        var footer = new DockPanel
        {
            [DockPanel.DockProperty] = Dock.Bottom
        };
        footer.Children.Add(new Separator
        {
            [DockPanel.DockProperty] = Dock.Top,
            Height = 1,
            Background = new SolidColorBrush(Color.Parse("#aca3a38e")),
            Margin = new Thickness(0),
            Padding = new Thickness(0)
        });
        footer.Children.Add(new SheetTabs(workbook)
        {
            [DockPanel.DockProperty] = Dock.Top
        });
        footer.Children.Add(new Separator
        {
            [DockPanel.DockProperty] = Dock.Top,
            Height = 1,
            Background = new SolidColorBrush(Color.Parse("#aca3a38e")),
            Margin = new Thickness(0),
            Padding = new Thickness(0)
        });
        footer.Children.Add(statusBar);

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
                toolBar,
                new Separator
                {
                    [DockPanel.DockProperty] = Dock.Top,
                    Height = 1,
                    Background = new SolidColorBrush(Color.Parse("#aca3a38e")),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                formulaBar,
                footer,
                gridView
            }
        };

    }
}
