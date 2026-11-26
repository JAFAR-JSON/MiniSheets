using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MiniSheets.Core.Model;

namespace MiniSheets.UI;

class SheetTabs : Border
{
    private readonly Workbook _workbook;
    private int _selectedSheetIndex = 0;
    private Button? _selectedButton;
    
    public SheetTabs(Workbook workbook)
    {
        _workbook = workbook;
        Height = 40;
        MinHeight = 40;
        Padding = new Thickness(8);
        Background = new SolidColorBrush(Color.Parse("#f3f4f6"));
        BorderBrush = new SolidColorBrush(Color.Parse("#aca3a370"));
        BorderThickness = new Thickness(0, 1, 0, 0);
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        var stackPanel = GenerateSheetButtons();

        Child = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                stackPanel,
                CreatePlusButton(),
            }
        };
    }

    private StackPanel GenerateSheetButtons()
    {
        var sheetButtonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        for (int i = 0; i < _workbook.SheetCount; i++)
        {
            var sheetId = _workbook.SheetIds[i];
            var sheet = _workbook.Sheets[sheetId];

            var sheetButton = CreateSheetButton(
                sheet.Name ?? $"Sheet{i + 1}"
            );

            sheetButtonsPanel.Children.Add(sheetButton);
        }

        if (_workbook.SheetCount == 0)
        {
            var sheetButton = CreateSheetButton("Sheet1");
            // change later
            _workbook.AddSheet(new Sheet { Name = "Sheet1" });
            sheetButtonsPanel.Children.Add(sheetButton);
        }

        return sheetButtonsPanel;
    }

    private void AddSheetButton()
    {
        // CHANGE LATER TO STATE FILES
        // Add a new sheet to the workbook
        var newSheet = new Sheet();
        _workbook.AddSheet(newSheet);

        var newSheetButton = CreateSheetButton($"Sheet{_workbook.SheetIds.Count}");

        if (Child is StackPanel stackPanel && stackPanel.Children[0] is StackPanel sheetButtonsPanel)
        {
            sheetButtonsPanel.Children.Add(newSheetButton);
        }
        _selectedSheetIndex = _workbook.SheetCount - 1;
        newSheetButton.RaiseEvent(
            new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent)
        );
    }

    private Button CreateSheetButton(string title)
    {
        var sheetButton = new Button
        {
            Height = 30,
            Content = title,
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#b8bec8")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Foreground = new SolidColorBrush(Color.Parse("#1f2937")),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        _selectedButton ??= sheetButton;

        sheetButton.Click += (sender, e) =>
        {
            // Return previously selected button to normal
            if (_selectedButton != null)
            {
                _selectedButton.Background = new SolidColorBrush(Color.Parse("#e5e7eb"));
                _selectedButton.BorderBrush = new SolidColorBrush(Color.Parse("#b8bec8"));
            }

            // Highlight newly selected button
            _selectedButton = sheetButton;

            _selectedButton.Background = Brushes.White;
            _selectedButton.BorderBrush = new SolidColorBrush(Color.Parse("#9ca3af"));

            // _selectedSheetIndex = _workbook.Sheets.FindIndex(s => s.Name == title);
            for (int i = 0; i < _workbook.SheetCount; ++i)
            {
                var sheet = _workbook.Sheets[_workbook.SheetIds[i]];
                if (sheet.Name == title)
                {
                    _selectedSheetIndex = i;
                    break;
                }
            }
        };

        return sheetButton;
    }

    private Button CreatePlusButton()
    {
        var plusButton = new Button
        {
            Height = 30,
            Width = 30,
            Content = "+",
            Background = new SolidColorBrush(Color.Parse("#e5e7eb")),
            BorderBrush = new SolidColorBrush(Color.Parse("#b8bec8")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Foreground = new SolidColorBrush(Color.Parse("#1f2937")),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        plusButton.Click += (sender, e) =>
        {
            AddSheetButton();
        };
        return plusButton;
    }
}
