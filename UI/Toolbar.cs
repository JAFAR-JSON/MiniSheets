using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace MiniSheets;

internal sealed class ToolBar : Border
{
    public ToolBar()
    {
        Padding = new Thickness(8, 8, 10, 8);

        Child = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                CreateToolButton("New"),
                CreateToolButton("Open"),
                CreateToolButton("Save"),
                new Separator
                {
                    Height = 30,
                    Width = 1,
                    Background = new SolidColorBrush(Color.Parse("#aca3a370")),
                    Margin = new Thickness(0),
                    Padding = new Thickness(2, 4)
                },
                CreateToolButton("Undo"),
                CreateToolButton("Redo"),
                new Separator
                {
                    Height = 30,
                    Width = 1,
                    Background = new SolidColorBrush(Color.Parse("#aca3a370")),
                    Margin = new Thickness(0),
                    Padding = new Thickness(2, 4)
                },
                CreateToolButton("Cut"),
                CreateToolButton("Copy"),
                CreateToolButton("Paste"),
                new Separator
                {
                    Height = 30,
                    Width = 1,
                    Background = new SolidColorBrush(Color.Parse("#aca3a370")),
                    Margin = new Thickness(0),
                    Padding = new Thickness(2, 4)
                },
                CreateToolButton("Fill"),
                CreateToolButton("Text"),
                new Separator
                {
                    Height = 30,
                    Width = 1,
                    Background = new SolidColorBrush(Color.Parse("#aca3a370")),
                    Margin = new Thickness(0),
                    Padding = new Thickness(2, 4)
                },
                new ComboBox
                {
                    Width = 120,
                    Height = 30,
                    ItemsSource = new string[] { "Currency", "EUR", "USD" , "GBP"},
                    SelectedIndex = 0,

                    Background = Brushes.Transparent,
                    BorderBrush = new SolidColorBrush(Color.Parse("#6a6c7070")),
                    BorderThickness = new Thickness(1),

                    Foreground = new SolidColorBrush(Color.Parse("#1f2937")),
                    CornerRadius = new CornerRadius(3),

                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,

                    Padding = new Thickness(4, 2)
                },
                CreateToolButton(".00+"),
                CreateToolButton(".00-")
            }
        };
    }

    private Button CreateToolButton(string title)
    {
        return new Button
        {
            Height = 30,
            Width = 54,
            Content = title,
            Background = new SolidColorBrush(Color.Parse("#e5e7eb")),
            BorderBrush = new SolidColorBrush(Color.Parse("#b8bec8")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Foreground = new SolidColorBrush(Color.Parse("#1f2937")),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
    }
}
