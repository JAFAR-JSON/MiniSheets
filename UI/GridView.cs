using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace MiniSheets;

internal class GridView : Border
{
    private readonly Grid _grid;
    private const int ColumnCount = 100;
    private const int RowCount = 100;
    private const double CellWidth = 110;
    private const double CellHeight = 30;

    private Vector _offset;

    public GridView()
    {
        Padding = new Thickness(10);

        _grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(
                string.Join(", ",
                    new[] { "40" }.Concat(
                        Enumerable.Repeat(CellWidth.ToString(), ColumnCount)
                    )
                )
            ),
            RowDefinitions = new RowDefinitions(string.Join(", ", Enumerable.Repeat(CellHeight.ToString(), 100))),
            ColumnSpacing = 0,
            RowSpacing = 0
        };

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = _grid
        };
        Child = scrollViewer;

        CreateHeaders();
        CreateRows();
    }

    private void CreateHeaders()
    {
        for (int i = -1; i < ColumnCount; i++)
        {
            var textBlock = new TextBlock
            {
                Text = GetColumnName(i),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#374151")),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var cell = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#f3f4f6")),
                BorderBrush = new SolidColorBrush(Color.Parse("#83878da8")),
                BorderThickness = new Thickness(0.6),
                Child = textBlock
            };

            Grid.SetRow(cell, 0);
            Grid.SetColumn(cell, i + 1);

            _grid.Children.Add(cell);
        }
    }

    private void CreateRows()
    {
        for (int rowIndex = 0; rowIndex < RowCount; rowIndex++)
        {
            var textBlock = new TextBlock
            {
                Text = (rowIndex + 1).ToString(),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#374151")),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var numberedCell = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#f3f4f6")),
                BorderBrush = new SolidColorBrush(Color.Parse("#83878da8")),
                BorderThickness = new Thickness(0.6),
                Child = textBlock
            };

            Grid.SetRow(numberedCell, rowIndex + 1);
            Grid.SetColumn(numberedCell, 0);

            _grid.Children.Add(numberedCell);

            for (int cellIndex = 1; cellIndex <= ColumnCount; cellIndex++)
            {
                var cellContent = new TextBlock
                {
                    Text = "",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.Parse("#374151")),
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                var cellBorder = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.Parse("#83878da8")),
                    BorderThickness = new Thickness(0.6),
                    Child = cellContent
                };

                Grid.SetRow(cellBorder, rowIndex + 1);
                Grid.SetColumn(cellBorder, cellIndex);

                _grid.Children.Add(cellBorder);
            }
        }
    }


    private string GetColumnName(int index)
    {
        string result = "";

        if (index == -1)
        {
            return "";
        }

        while (index >= 0)
        {
            // Convert number into a letter
            result = (char)('A' + index % 26) + result;
            index = index / 26 - 1;
        }

        return result;
    }
}