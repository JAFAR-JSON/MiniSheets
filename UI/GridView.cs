using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace MiniSheets;

internal class GridView : Border
{
    private readonly Grid _grid;
    private const int ColumnCount = 10;
    private const int RowCount = 10;
    private const double CellWidth = 110;
    private const double CellHeight = 30;

    private Border? _selectedCell;
    private TextBox? _activeTextBox;
    private Border? _editingCell;

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
                Padding = new Thickness(4, 0, 0, 0),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var numberedCell = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#f3f4f6")),
                BorderBrush = new SolidColorBrush(Color.Parse("#83878da8")),
                BorderThickness = new Thickness(0.6),
                Child = textBlock,
                
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
                    Padding = new Thickness(4, 0, 0, 0),
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

                cellBorder.PointerPressed += (sender, e) =>
                {
                    // If another cell is being edited, finish editing
                    if (_activeTextBox != null && _editingCell != cellBorder)
                    {
                        FinishEditing();
                    }

                    // Deselect previous cell
                    if (_selectedCell != null && _selectedCell != cellBorder)
                    {
                        _selectedCell.BorderBrush =
                            new SolidColorBrush(Color.Parse("#83878da8"));

                        _selectedCell.BorderThickness = new Thickness(0.6);
                    }

                    // Select current cell
                    _selectedCell = cellBorder;

                    cellBorder.BorderBrush =
                        new SolidColorBrush(Color.Parse("#252627"));

                    cellBorder.BorderThickness = new Thickness(2);

                    e.Handled = true;
                };

                cellBorder.DoubleTapped += (sender, e) =>
                {
                    if (_activeTextBox != null)
                        return;

                    var existingText = (cellBorder.Child as TextBlock)?.Text ?? "";
                    var textBox = new TextBox
                    {
                        Text = existingText,
                        FontSize = 14,
                        Foreground = new SolidColorBrush(Color.Parse("#374151")),
                        BorderThickness = new Thickness(0),
                        CornerRadius = new CornerRadius(0),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Styles =
                        {
                            CreateTextBoxFocusStyle()
                        }
                    };

                    cellBorder.Child = textBox;

                    cellBorder.BorderBrush = new SolidColorBrush(Color.Parse("#83878da8"));
                    cellBorder.BorderThickness = new Thickness(0.6);

                    _activeTextBox = textBox;
                    _editingCell = cellBorder;

                    textBox.Focus();
                    textBox.CaretIndex = textBox.Text?.Length ?? 0;

                    e.Handled = true;
                };


                Grid.SetRow(cellBorder, rowIndex + 1);
                Grid.SetColumn(cellBorder, cellIndex);

                _grid.Children.Add(cellBorder);
            }
        }
    }
    
    private void FinishEditing()
    {
        if (_activeTextBox == null || _editingCell == null)
            return;

        var textBox = _activeTextBox;

        if (_editingCell.Child is TextBox)
        {
            var cellContent = new TextBlock
            {
                Text = textBox.Text,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#374151")),
                TextAlignment = TextAlignment.Left,
                Padding = new Thickness(4, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            _editingCell.Child = cellContent;
        }
        _activeTextBox = null;
        _editingCell = null;
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

    private Style CreateTextBoxFocusStyle()
    {
        return new Style(selector => selector
            .OfType<TextBox>()
            .Class(":focus")
            .Descendant()
            .OfType<Border>()
            .Name("PART_BorderElement"))
        {
            Setters =
            {
                new Setter(Border.BorderBrushProperty,
                    new SolidColorBrush(Color.Parse("#252627"))),

                new Setter(Border.BorderThicknessProperty,
                    new Thickness(2)),
            }
        };
    }
}
