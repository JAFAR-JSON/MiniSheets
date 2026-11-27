using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

public class ImportCSVScreen : Window
{
    private ComboBox? delimiterComboBox;
    private CheckBox? hasHeadersCheckBox;

    private const double PreviewCellHeight = 25;

    public ImportCSVScreen()
    {
        Title = "Import CSV";
        Width = 560;
        Height = 430;

        MinWidth = 560;
        MaxWidth = 600;
        MinHeight = 400;
        MaxHeight = 600;

        Icon = null;
        CanMinimize = false;
        CanMaximize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var mainGrid = new Grid
        {
            Margin = new Thickness(15, 12, 15, 10),
            RowDefinitions = new RowDefinitions("Auto, Auto, Auto, Auto, Auto")
        };

        var delimiterPanel = CreateDelimiterPanel();

        // Preview heading
        var infoTextBlock = new TextBlock
        {
            Text = "Preview (first 10 rows)",
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 6),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Preview grid
        var previewGrid = CreatePreviewGrid();

        // Information about the imported sheet
        var secondInfoTextBlock = new TextBlock
        {
            Text = "Will create sheet \"prices\" with 250 rows and 3 columns.",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12),
            VerticalAlignment = VerticalAlignment.Center
        };

        var buttonPanel = CreateButtonsPanel();

        // Assemble layout
        Grid.SetRow(delimiterPanel, 0);
        Grid.SetRow(infoTextBlock, 1);
        Grid.SetRow(previewGrid, 2);
        Grid.SetRow(secondInfoTextBlock, 3);
        Grid.SetRow(buttonPanel, 4);

        mainGrid.Children.Add(delimiterPanel);
        mainGrid.Children.Add(infoTextBlock);
        mainGrid.Children.Add(previewGrid);
        mainGrid.Children.Add(secondInfoTextBlock);
        mainGrid.Children.Add(buttonPanel);

        Content = mainGrid;
    }

    private StackPanel CreateDelimiterPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 12),
            VerticalAlignment = VerticalAlignment.Center
        };

        var label = new TextBlock
        {
            Text = "Delimiter",
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };

        delimiterComboBox = new ComboBox
        {
            Width = 140,
            Height = 24,
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 0),
            ItemsSource = new[]
            {
                "Auto (semicolon)",
                "Comma",
                "Semicolon",
                "Tab"
            },
            SelectedIndex = 0
        };

        hasHeadersCheckBox = new CheckBox
        {
            Content = new TextBlock
            {
                Text = "First row contains headers",
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 11
            },
            Padding = new Thickness(0),
            Margin = new Thickness(18, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        panel.Children.Add(label);
        panel.Children.Add(delimiterComboBox);
        panel.Children.Add(hasHeadersCheckBox);

        return panel;
    }

    private Grid CreatePreviewGrid()
    {
        var grid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 12),
            ColumnDefinitions = new ColumnDefinitions("40, *, *, *"),
            RowDefinitions = new RowDefinitions(
                string.Join(", ", Enumerable.Repeat(
                    PreviewCellHeight.ToString(),
                    11)))
        };

        char[] headers = { 'A', 'B', 'C' };

        for (int column = 0; column < 4; column++)
        {
            string header = column == 0 ? "" : headers[column - 1].ToString();

            var cell = CreatePreviewCell(header, isHeader: true);
            Grid.SetRow(cell, 0);
            Grid.SetColumn(cell, column);

            grid.Children.Add(cell);
        }

        for (int row = 1; row <= 10; row++)
        {
            for (int column = 0; column < 4; column++)
            {
                // First column contains row numbers.
                string text = column == 0 ? row.ToString() : "";

                // If last row, show multiple dots, the rest cells are empty
                if (row == 10 && column == 0)
                {
                    text = "…";
                }
                else if (row == 10 && column > 0)
                {
                    text = "";
                }

                var cell = CreatePreviewCell(text, isHeader: column == 0);

                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, column);

                grid.Children.Add(cell);
            }
        }

        return grid;
    }

    private static Border CreatePreviewCell(string text, bool isHeader)
    {
        return new Border
        {
            Background = isHeader
                ? new SolidColorBrush(Color.Parse("#f3f4f6"))
                : Brushes.White,

            BorderBrush = new SolidColorBrush(Color.Parse("#83878da8")),
            BorderThickness = new Thickness(0.6),

            Child = new TextBlock
            {
                Text = text,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.Parse("#374151")),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
    }

    public StackPanel CreateButtonsPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };

        var cancelButton = new Button
        {
            Content = "Cancel",
            MinWidth = 48,
            Height = 25,
            FontSize = 12,
            Padding = new Thickness(10, 2)
        };

        cancelButton.Click += (_, _) => Close();

        var importButton = new Button
        {
            Content = "Import",
            MinWidth = 48,
            Height = 25,
            FontSize = 12,
            Padding = new Thickness(10, 2)
        };

        importButton.Click += (_, _) =>
        {
            char delimiter = delimiterComboBox?.SelectedIndex switch
            {
                0 => ';',
                1 => ',',
                2 => ';',
                3 => '\t',
                _ => ';'
            };

            bool firstRowContainsHeaders = hasHeadersCheckBox?.IsChecked == true;
            // TODO: make this functional
            Close();
        };

        panel.Children.Add(cancelButton);
        panel.Children.Add(importButton);

        return panel;
    }
}