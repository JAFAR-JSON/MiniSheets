using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

public class ExportCSVScreen : Window
{
    private readonly ComboBox _delimiterComboBox;

    public ExportCSVScreen()
    {
        Title = "Export CSV";
        Width = 400;
        Height = 175;

        MinWidth = 400;
        MaxWidth = 400;
        MinHeight = 175;
        MaxHeight = 175;

        Icon = null;
        CanMinimize = false;
        CanMaximize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var mainGrid = new Grid
        {
            Margin = new Thickness(15, 12, 15, 10),
            RowDefinitions = new RowDefinitions("Auto, Auto, Auto, Auto"),
        };

        var sheetInfoPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 12),
            VerticalAlignment = VerticalAlignment.Center
        };

        var sheetInfoText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12
        };

        sheetInfoText.Inlines?.Add(new Run("Sheet: "));

        //TODO: Replace with actual sheet name and range
        sheetInfoText.Inlines?.Add(new Run("Costs")
        {
            FontWeight = FontWeight.Bold
        });
        sheetInfoText.Inlines?.Add(new Run(", range A1:B7"));

        sheetInfoPanel.Children.Add(sheetInfoText);

        var delimiterPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 12),
            VerticalAlignment = VerticalAlignment.Center
        };

        var delimiterLabel = new TextBlock
        {
            Text = "Delimiter",
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };

        _delimiterComboBox = new ComboBox
        {
            Width = 115,
            Height = 24,
            FontSize = 12,
            ItemsSource = new[]
            {
                "Comma",
                "Semicolon",
                "Tab"
            },
            SelectedIndex = 0
        };

        delimiterPanel.Children.Add(delimiterLabel);
        delimiterPanel.Children.Add(_delimiterComboBox);

        var infoTextBlock = new TextBlock
        {
            Text = "Values are exported as displayed. Formulas and formatting are not exported.",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#888888")),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };

        var buttonPanel = new StackPanel
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

        var exportButton = new Button
        {
            Content = "Export",
            MinWidth = 48,
            Height = 25,
            FontSize = 12,
            Padding = new Thickness(10, 2)
        };

        exportButton.Click += (_, _) =>
        {
            char delimiter = _delimiterComboBox.SelectedIndex switch
            {
                0 => ',',
                1 => ';',
                2 => '\t',
                _ => ','
            };

            // TODO: Trigger CSV export using the selected delimiter.
            Close();
        };

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(exportButton);

        // Add everything to the grid
        Grid.SetRow(sheetInfoPanel, 0);
        Grid.SetRow(delimiterPanel, 1);
        Grid.SetRow(infoTextBlock, 2);
        Grid.SetRow(buttonPanel, 3);

        mainGrid.Children.Add(sheetInfoPanel);
        mainGrid.Children.Add(delimiterPanel);
        mainGrid.Children.Add(infoTextBlock);
        mainGrid.Children.Add(buttonPanel);

        Content = mainGrid;
    }
}