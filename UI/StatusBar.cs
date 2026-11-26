using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;

namespace MiniSheets.UI;

public class StatusBar : Border
{
    public StatusBar()
    {
        Height = 30;
        MinHeight = 30;
        Padding = new Thickness(8);
        Background = new SolidColorBrush(Color.Parse("#f3f4f6"));
        BorderBrush = new SolidColorBrush(Color.Parse("#aca3a370"));
        BorderThickness = new Thickness(0, 1, 0, 0);
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        Child = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children =
            {
                CreateStatusLabel("Ready", 0),
                CreateStatusLabel("en-US", 1)
            }
        };
    }

    private TextBlock CreateStatusLabel(string text, int column)
    {
        var label = new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(Color.Parse("#1f2937")),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = column == 0
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Right
        };
        Grid.SetColumn(label, column);
        return label;
    }

    public void SetStatusText(string text)
    {
        if (Child is Grid grid && grid.Children[0] is TextBlock statusLabel)
        {
            statusLabel.Text = text;
        }
    }
}