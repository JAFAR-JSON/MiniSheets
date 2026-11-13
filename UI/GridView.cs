using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Input;

namespace MiniSheets;

internal class GridView : Border
{
    private readonly Grid _grid;
    private const int ColumnCount = 100;
    private const int RowCount = 100;
    private const double CellWidth = 110;
    private const double CellHeight = 30;

    private Border? _selectedCell;
    private TextBox? _activeTextBox;
    private Border? _editingCell;

    private readonly List<Border> _highlightedCells = new();

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
            RowDefinitions = new RowDefinitions(string.Join(", ", Enumerable.Repeat(CellHeight.ToString(), 101))),
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
                int currentRow = rowIndex;
                int currentColumn = cellIndex;

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
                    Child = cellContent,
                    Focusable = true
                };

                // Clicked or highlighted cell once
                cellBorder.PointerPressed += (sender, e) =>
                {
                    CellClicked(cellBorder);
                    cellBorder.Focus();

                    e.Handled = true;
                };
                
                cellBorder.KeyDown += (s, e) =>
                {
                    if (e.Source == cellBorder && IsNavigationKey(e.Key))
                    {
                        EnterOrArrowPressedOnCell(currentRow, currentColumn, e.Key);
                        e.Handled = true;
                    }
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

                    textBox.LostFocus += (_, _) =>
                    {
                        FinishEditing();
                    };

                    textBox.KeyDown += (s, e) =>
                    {
                        if (IsNavigationKey(e.Key))
                        {
                            EnterOrArrowPressedOnCell(currentRow, currentColumn, e.Key);
                            if(_highlightedCells.Contains(cellBorder))
                            {
                                PermenantlyHighlightCell(cellBorder);
                            }
                            e.Handled = true;
                        }
                        else if (e.Key == Key.Escape)
                        {
                            textBox.Text = "";
                            e.Handled = true;
                        }
                    };

                    e.Handled = true;
                };


                Grid.SetRow(cellBorder, rowIndex + 1);
                Grid.SetColumn(cellBorder, cellIndex);

                _grid.Children.Add(cellBorder);
            }
        }
    }

    private static bool IsNavigationKey(Key key)
    {
        return key is Key.Enter or Key.Left or Key.Right or Key.Up or Key.Down;
    }

    private void CellClicked(Border cellBorder)
    {
        // If another cell is being edited, finish editing
        if (_activeTextBox != null && _editingCell != cellBorder)
        {
            FinishEditing();
        }

        // Deselect previous cell
        if (_selectedCell != null && _selectedCell != cellBorder)
        {
            if (!_highlightedCells.Contains(_selectedCell))
            {
                ResetCellBorder(_selectedCell);
                _highlightedCells.Remove(_selectedCell);
            }
            else
            {
                PermenantlyHighlightCell(_selectedCell);
            }
        }

        // Select current cell
        _selectedCell = cellBorder;

        cellBorder.BorderBrush = new SolidColorBrush(Color.Parse("#252627"));
        cellBorder.BorderThickness = new Thickness(2);
    }
    
    private void EnterOrArrowPressedOnCell(int currentRow, int currentColumn, Key key)
    {
        FinishEditing();

        var targetRow = currentRow;
        var targetColumn = currentColumn;

        switch (key)
        {
            case Key.Left when currentColumn > 1:
                targetColumn--;
                break;
            case Key.Right when currentColumn < ColumnCount:
                targetColumn++;
                break;
            case Key.Up when currentRow > 0:
                targetRow--;
                break;
            case Key.Down or Key.Enter when currentRow < RowCount - 1:
                targetRow++;
                break;
            default:
                return;
        }

        var nextCell = GetCell(targetRow, targetColumn);

        if (nextCell != null)
        {
            CellClicked(nextCell);
            nextCell.Focus();
        }
    }

    private Border? GetCell(int row, int column)
    {
        return _grid.Children
            .OfType<Border>()
            .FirstOrDefault(cell =>
                Grid.GetRow(cell) == row + 1 &&
                Grid.GetColumn(cell) == column);
    }

    public void ApplyBorderToSelectedCell()
    {
        if (_selectedCell == null)
            return;

        if (!_highlightedCells.Contains(_selectedCell))
        {
            _highlightedCells.Add(_selectedCell);
            _selectedCell.Focus();
        }
        else
        {
            _highlightedCells.Remove(_selectedCell);
            _selectedCell.BorderBrush = new SolidColorBrush(Color.Parse("#252627"));
            _selectedCell.BorderThickness = new Thickness(2);
             _selectedCell.Focus();
        }
    }

    private static void PermenantlyHighlightCell(Border cell)
    {
        cell.BorderBrush = new SolidColorBrush(Color.Parse("#565b5f"));
        cell.BorderThickness = new Thickness(2);
    }

    private static void ResetCellBorder(Border cell)
    {
        cell.BorderBrush = new SolidColorBrush(Color.Parse("#83878da8"));
        cell.BorderThickness = new Thickness(0.6);
    }

    private void FinishEditing()
    {
        if (_activeTextBox == null || _editingCell == null)
            return;

        var textBox = _activeTextBox;
        var editingCell = _editingCell;

        if (editingCell.Child is TextBox)
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

            editingCell.Child = cellContent;
        }

        CellClicked(editingCell);
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