using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MiniSheets.Core.Model;
using MiniSheets.State;
using MiniSheets.Shared;

namespace MiniSheets.UI;

internal class GridView : Border
{
    private readonly Grid _grid;

    private readonly Sheet _sheet;
    private readonly ScrollViewer _scrollViewer;
    private readonly SpreadsheetProviderService _spreadsheetProviderService;

    private readonly Dictionary<CellKey, Border> _visibleCells = new();

    private const double CellWidth = 110;
    private const double CellHeight = 30;
    private const int InitialRows = 20;
    private const int InitialColumns = 20;
    private const int ScrollBuffer = 5;

    private const int MaxRows = 10000;
    private const int MaxColumns = 702;

    private int _renderedRows = InitialRows;
    private int _renderedColumns = InitialColumns;
    private bool _isUpdatingScroll;
    private double _lastOffsetX;
    private double _lastOffsetY;

    private const int BatchSize = 20;

    private Border? _selectedCell;
    private TextBox? _activeTextBox;
    private Border? _editingCell;
    private bool _isHandlingNavigation;

    private readonly List<Border> _highlightedCells = new();

    public event EventHandler<(string cellName, string text)>? SelectedCellChanged;
    public event EventHandler<string>? StatusTextChanged;

    public GridView(Sheet sheet, SpreadsheetProviderService spreadsheetProviderService)
    {
        _sheet = sheet;
        _spreadsheetProviderService = spreadsheetProviderService;
        Padding = new Thickness(10);

        _grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(
                string.Join(", ",
                    new[] { "40" }.Concat(
                        Enumerable.Repeat(CellWidth.ToString(), InitialColumns)
                    )
                )
            ),
            RowDefinitions = CreateInitialRowDefinitions(),
            ColumnSpacing = 0,
            RowSpacing = 0
        };

        _scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = _grid
        };
        Child = _scrollViewer;

        _scrollViewer.ScrollChanged += (_, _) => OnScrollChanged();
        AttachedToVisualTree += (_, _) => VirtualizeVisibleCells();

        CreateHeaders();
        CreateInitialRows();
    }

    private static RowDefinitions CreateInitialRowDefinitions()
    {
        var rowDefinitions = new RowDefinitions
        {
            new RowDefinition(new GridLength(CellHeight))
        };

        for (var row = 0; row < InitialRows; row++)
            rowDefinitions.Add(new RowDefinition(new GridLength(CellHeight)));

        return rowDefinitions;
    }

    // Add a cell to the grid if it is not already visible
    private void AddVisibleCell(int row, int column)
    {
        var key = new CellKey(row, column);

        if (_visibleCells.ContainsKey(key))
            return;

        var cell = CreateDynamicCell(row, column);

        _grid.Children.Add(cell);
        _visibleCells.Add(key, cell);
    }

    // Remove a cell from the grid if it not visible anymore
    private void RemoveVisibleCell(CellKey key)
    {
        if (!_visibleCells.TryGetValue(key, out var cell))
            return;

        if (cell == _selectedCell || cell == _editingCell)
            return;

        _grid.Children.Remove(cell);
        _visibleCells.Remove(key);
    }

    private void AddRows(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int rowIndex = _renderedRows;

            _grid.RowDefinitions.Add(
                new RowDefinition(new GridLength(CellHeight))
            );

            _renderedRows++;
        }
    }

    private void AddColumns(int count)
    {
        for (int i = 0; i < count; i++)
        {    
            int columnIndex = _renderedColumns + 1;

            _grid.ColumnDefinitions.Add(
                new ColumnDefinition(new GridLength(CellWidth))
            );

            CreateColumnHeader(columnIndex);

            _renderedColumns++;
        }
    }

    // Removes columns that are not longer visible completely
    private void RemoveColumnsTo(int columnCount)
    {
        columnCount = Math.Max(InitialColumns, columnCount);
        if (columnCount >= _renderedColumns)
            return;

        if (_editingCell != null && Grid.GetColumn(_editingCell) > columnCount)
            FinishEditing();

        foreach (var key in _visibleCells.Keys
                     .Where(key => key.Column > columnCount)
                     .ToList())
        {
            RemoveVisibleCell(key);
        }

        while (_grid.ColumnDefinitions.Count > columnCount + 1)
            _grid.ColumnDefinitions.RemoveAt(_grid.ColumnDefinitions.Count - 1);

        _renderedColumns = columnCount;
    }

    private void RemoveDownwardRows(int rowCount)
    {
        rowCount = Math.Max(InitialRows, rowCount);
        if (rowCount >= _renderedRows)
            return;

        if (_editingCell != null && Grid.GetRow(_editingCell) > rowCount)
            FinishEditing();

        foreach (var key in _visibleCells.Keys
                     .Where(key => key.Row >= rowCount)
                     .ToList())
        {
            RemoveVisibleCell(key);
        }

        while (_grid.RowDefinitions.Count > rowCount + 1)
            _grid.RowDefinitions.RemoveAt(_grid.RowDefinitions.Count - 1);

        _renderedRows = rowCount;
    }

    private void CreateColumnHeader(int columnIndex)
    {
        var headerText = new TextBlock
        {
            Text = GetColumnName(columnIndex - 1),
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#374151")),
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var header = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#f3f4f6")),
            BorderBrush = new SolidColorBrush(Color.Parse("#83878da8")),
            BorderThickness = new Thickness(0.6),
            Child = headerText
        };

        Grid.SetRow(header, 0);
        Grid.SetColumn(header, columnIndex);
        _grid.Children.Add(header);
    }

    private Border CreateDynamicCell(int rowIndex, int columnIndex)
    {
        var cell = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#83878da8")),
            BorderThickness = new Thickness(0.6),
            Child = new TextBlock
            {
                Text = _spreadsheetProviderService.GetFormattedCellValue(0, new CellKey(rowIndex, columnIndex)),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#374151")),
                Padding = new Thickness(4, 0, 0, 0),
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            },
            Focusable = true
        };

        cell.PointerPressed += (_, e) =>
        {
            CellClicked(cell);
            cell.Focus();
            e.Handled = true;
        };

        cell.AddHandler(KeyDownEvent, (_, e) =>
        {
            if (e.Source == cell && IsNavigationKey(e.Key))
            {
                e.Handled = HandleNavigationKey(rowIndex, columnIndex, e);
            }
            else if (e.Source == cell && e.Key == Key.F2)
            {
                EditMode(cell, rowIndex, columnIndex);
                e.Handled = true;
            }
            else if (e.Source == cell && HandleCellClipboardCommand(cell, e))
            {
                e.Handled = true;
            }
        }, RoutingStrategies.Bubble, handledEventsToo: true);

        cell.DoubleTapped += (_, e) =>
        {
            EditMode(cell, rowIndex, columnIndex);
            e.Handled = true;
        };

        Grid.SetRow(cell, rowIndex + 1);
        Grid.SetColumn(cell, columnIndex);
        return cell;
    }

    private void EditMode(Border cell, int rowIndex, int columnIndex)
    {
        if (_activeTextBox != null)
            return;

        var cellKey = new CellKey(rowIndex, columnIndex);

        var textBox = new TextBox
        {
            Text = _spreadsheetProviderService.GetRawCellValue(0, cellKey),//(cell.Child as TextBlock)?.Text ?? "",//_spreadsheetProviderService.GetRawCellValue(0, cellKey),
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#374151")),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Styles = { CreateTextBoxFocusStyle() }
        };

        cell.Child = textBox;
        _activeTextBox = textBox;
        _editingCell = cell;
        textBox.Focus();
        StatusTextChanged?.Invoke(this, "Enter");
        textBox.CaretIndex = textBox.Text?.Length ?? 0;
        textBox.LostFocus += (_, _) => FinishEditing();
        textBox.TextChanged += (_, _) => NotifySelectedCellChanged(cell);
        textBox.AddHandler(KeyDownEvent, (_, keyEvent) =>
        {
            if (IsNavigationKey(keyEvent.Key))
            {
                keyEvent.Handled = HandleNavigationKey(rowIndex, columnIndex, keyEvent);
            }
            else if (keyEvent.Key == Key.Escape)
            {
                textBox.Text = "";
                FinishEditing();
                keyEvent.Handled = true;
            }
        }, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void OnScrollChanged()
    {
        if (_isUpdatingScroll)
            return;

        if (_scrollViewer.Viewport.Height <= 0 || _scrollViewer.Viewport.Width <= 0)
        {
            return;
        }

        var requiredRows = (int)Math.Ceiling(
            (_scrollViewer.Offset.Y + _scrollViewer.Viewport.Height) / CellHeight)
            + ScrollBuffer;

        var requiredColumns = (int)Math.Ceiling(
            (_scrollViewer.Offset.X + _scrollViewer.Viewport.Width - 40) / CellWidth)
            + ScrollBuffer;

        if (requiredRows > MaxRows)
            requiredRows = MaxRows;

        if (requiredColumns > MaxColumns)
            requiredColumns = MaxColumns;

        _isUpdatingScroll = true;
        try
        {
            while (_renderedRows < requiredRows)
                AddRows(Math.Min(BatchSize, requiredRows - _renderedRows));

            while (_renderedColumns < requiredColumns)
                AddColumns(Math.Min(BatchSize, requiredColumns - _renderedColumns));

            if (_scrollViewer.Offset.X < _lastOffsetX)
            {
                RemoveColumnsTo(requiredColumns);
            }

            if (_scrollViewer.Offset.Y < _lastOffsetY)
                RemoveDownwardRows(requiredRows);
        }
        finally
        {
            _isUpdatingScroll = false;
        }

        VirtualizeVisibleCells();
        _lastOffsetX = _scrollViewer.Offset.X;
        _lastOffsetY = _scrollViewer.Offset.Y;
    }

    // Remove or create cells when they become visible
    private void VirtualizeVisibleCells()
    {
        var firstRow = Math.Max(0,
            (int)Math.Floor(_scrollViewer.Offset.Y / CellHeight) - ScrollBuffer);
        var lastRow = Math.Min(_renderedRows,
            (int)Math.Ceiling((_scrollViewer.Offset.Y + _scrollViewer.Viewport.Height) / CellHeight)
            + ScrollBuffer);
        var firstColumn = Math.Max(1,
            (int)Math.Floor((_scrollViewer.Offset.X - 40) / CellWidth) - ScrollBuffer + 1);
        var lastColumn = Math.Min(_renderedColumns,
            (int)Math.Ceiling((_scrollViewer.Offset.X + _scrollViewer.Viewport.Width - 40) / CellWidth)
            + ScrollBuffer);

        // Remove offscreen cells while retaining their data in the sheet.
        foreach (var cell in _grid.Children.OfType<Border>().ToList())
        {
            var row = Grid.GetRow(cell) - 1;
            var column = Grid.GetColumn(cell);
            if (row >= 0 && column > 0 &&
                (row < firstRow || row >= lastRow || column < firstColumn || column > lastColumn) &&
                cell != _selectedCell && cell != _editingCell)
            {
                RemoveVisibleCell(new CellKey(row, column));
            }
            // Remove offscreen row headers
            else if (row >= 0 && column == 0 &&
                     (row < firstRow || row >= lastRow))
            {
                _grid.Children.Remove(cell);
            }
        }

        for (var row = firstRow; row < lastRow; row++)
        {
            // Create row headers for visible rows
            if (!_grid.Children.OfType<Border>().Any(cell =>
                    Grid.GetRow(cell) == row + 1 && Grid.GetColumn(cell) == 0))
            {
                CreateRow(row);
            }

            for (var column = firstColumn; column <= lastColumn; column++)
            {
                AddVisibleCell(row, column);
            }
        }
    }

    private void CreateHeaders()
    {
        for (int i = -1; i < _renderedColumns; i++)
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

    private void CreateRow(int rowIndex)
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
        if (rowIndex < InitialRows)
        {
            for (int cellIndex = 1; cellIndex <= _renderedColumns; cellIndex++)
            {
                AddVisibleCell(rowIndex, cellIndex);
            }
        }

    }

    private void CreateInitialRows()
    {
        for (int rowIndex = 0; rowIndex < InitialRows; rowIndex++)
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

            for (int cellIndex = 1; cellIndex <= InitialColumns; cellIndex++)
            {
                AddVisibleCell(rowIndex, cellIndex);
            }
        }
    }
    
    public void UpdateSelectedCellContent(string text)
    {
        if (_selectedCell == null)
            return;

        // Update the raw value of the selected cell
        var cellKey = new CellKey(Grid.GetColumn(_selectedCell) - 1, Grid.GetRow(_selectedCell));
        _spreadsheetProviderService.SetCellValue(0, cellKey, text);

        Console.WriteLine($"DEBUG: UpdateSelectedCellContent({cellKey}, {text})");

        if (_selectedCell.Child is TextBlock textBlock)
        {
            textBlock.Text = _spreadsheetProviderService.GetFormattedCellValue(0, cellKey);
        }
        else if (_selectedCell.Child is TextBox textBox)
        {
            textBox.Text = _spreadsheetProviderService.GetRawCellValue(0, cellKey);
        }

        NotifySelectedCellChanged(_selectedCell);
    }

    private static bool IsNavigationKey(Key key)
    {
        return key is Key.Enter or Key.Left or Key.Right or Key.Up or Key.Down or Key.Tab;
    }

    private static bool IsClipboardModifierPressed(KeyEventArgs keyEvent)
    {
        return (keyEvent.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0;
    }

    private bool HandleCellClipboardCommand(Border cell, KeyEventArgs keyEvent)
    {
        if (!IsClipboardModifierPressed(keyEvent))
            return false;

        switch (keyEvent.Key)
        {
            case Key.C:
                _ = CopyToClipboard(GetFormattedCellText(cell));
                return true;
            case Key.V:
                _ = PasteIntoSelectedCellAsync();
                return true;
            case Key.X:
                _ = CutSelectedCellAsync();
                return true;
            default:
                return false;
        }
    }

    private static string GetFormattedCellText(Border cell)
    {
        return (cell.Child as TextBlock)?.Text
               ?? (cell.Child as TextBox)?.Text
               ?? string.Empty;
    }

    private bool HandleNavigationKey(int currentRow, int currentColumn, KeyEventArgs keyEvent)
    {
        if (_isHandlingNavigation)
            return true;

        _isHandlingNavigation = true;
        try
        {
            EnterOrArrowPressedOnCell(
                currentRow,
                currentColumn,
                keyEvent.Key,
                (keyEvent.KeyModifiers & KeyModifiers.Shift) != 0);
            return true;
        }
        finally
        {
            _isHandlingNavigation = false;
        }
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
        NotifySelectedCellChanged(cellBorder);
        StatusTextChanged?.Invoke(this, "Ready");
    }

   private void NotifySelectedCellChanged(Border cell)
    {
        var row = Grid.GetRow(cell);
        var column = Grid.GetColumn(cell);

        var cellKey = new CellKey(row - 1, column - 1);
        var cellName = $"{GetColumnName(column - 1)}{row}";

        var rawText = _spreadsheetProviderService.GetRawCellValue(0, cellKey);

        SelectedCellChanged?.Invoke(this, (cellName, rawText));
    }

    public void MoveDownToNextCell()
    {
        if (_selectedCell == null)
            return;

        var currentRow = Grid.GetRow(_selectedCell) - 1;
        var currentColumn = Grid.GetColumn(_selectedCell);

        EnterOrArrowPressedOnCell(currentRow, currentColumn, Key.Enter, false);
    }

    public async Task CopySelectedCellToClipboardAsync()
    {
        if (_selectedCell == null)
            return;

        await CopyToClipboard(GetFormattedCellText(_selectedCell));
    }

    private async Task CopyToClipboard(string text)
    {
        await Clipboard.CopyAsync(text);
    }

    public async Task PasteIntoSelectedCellAsync()
    {
        if (_selectedCell == null)
            return;

        var existingText = GetFormattedCellText(_selectedCell);
        var pastedText = await Clipboard.PasteAsync();
        UpdateSelectedCellContent(existingText + pastedText);
    }

    public async Task CutSelectedCellAsync()
    {
        if (_selectedCell == null)
            return;

        await CopyToClipboard(GetFormattedCellText(_selectedCell));
        UpdateSelectedCellContent(string.Empty);
    }

    private void EnterOrArrowPressedOnCell(
        int currentRow,
        int currentColumn,
        Key key,
        bool shiftPressed)
    {
        var targetRow = currentRow;
        var targetColumn = currentColumn;

        switch (key)
        {
            case Key.Tab:
                if (!shiftPressed && currentColumn < _renderedColumns)
                {
                    targetColumn++;
                }
                else if (shiftPressed && currentColumn > 1)
                {
                    targetColumn--;
                }
                else if (!shiftPressed)
                {
                    targetColumn = 1;
                    targetRow++;
                }
                else
                {
                    targetColumn = _renderedColumns;
                    targetRow--;
                }
                break;
            case Key.Left when currentColumn > 1:
                targetColumn--;
                break;
            case Key.Right when currentColumn < _renderedColumns:
                targetColumn++;
                break;
            case Key.Up when currentRow > 0:
                targetRow--;
                break;
            case Key.Down when currentRow < _renderedRows - 1:
                targetRow++;
                break;
            case Key.Enter when !shiftPressed && currentRow < _renderedRows - 1:
                targetRow++;
                break;
            case Key.Enter when shiftPressed && currentRow > 0:
                targetRow--;
                break;
            default:
                return;
        }

        FinishEditing();

        var nextCell = GetCell(targetRow, targetColumn);

        if (nextCell != null)
        {
            CellClicked(nextCell);
            nextCell.Focus();
        }
    }

    private void SelectCellAt(int row, int column)
    {
        if (row < 0 || row >= MaxRows || column < 1 || column > MaxColumns)
            return;

        EnsureRenderedThrough(row, column);

        var targetOffsetX = Math.Max(0, (column - 1) * CellWidth - 40);
        var targetOffsetY = Math.Max(0, row * CellHeight);

        Dispatcher.UIThread.Post(() =>
        {
            _scrollViewer.Offset = new Vector(targetOffsetX, targetOffsetY);

            Dispatcher.UIThread.Post(() =>
            {
                VirtualizeVisibleCells();

                var cell = GetCell(row, column);

                if (cell == null)
                {
                    CreateRow(row);
                    AddVisibleCell(row, column);
                    cell = GetCell(row, column);
                }

                if (cell == null)
                    return;

                CellClicked(cell);
                cell.Focus();

            }, DispatcherPriority.Render);
        }, DispatcherPriority.Render);
    }

    private void EnsureRenderedThrough(int row, int column)
    {
        _isUpdatingScroll = true;

        try
        {
            if (_renderedRows <= row)
                AddRows(row + 1 - _renderedRows);

            if (_renderedColumns < column)
                AddColumns(column - _renderedColumns);
        }
        finally
        {
            _isUpdatingScroll = false;
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
            // I'm not sure how to make this fit in with the refactor of MiniSheets.Core.Model;
            // _sheet.GetCell
            //     (new CellKey(Grid.GetRow(_selectedCell) - 1, Grid.GetColumn(_selectedCell)))
            //     .IsStyled = true;

            // Right now make hasBorders true, will have to figure out what to do with the multiple style types
            // _sheet.GetCell(new CellKey(Grid.GetRow(_selectedCell) - 1, Grid.GetColumn(_selectedCell))).StyleId.HasBorders = true;

            _highlightedCells.Add(_selectedCell);
            _selectedCell.Focus();
        }
        else
        {
            // _sheet.GetCell(new CellKey(Grid.GetRow(_selectedCell) - 1, Grid.GetColumn(_selectedCell))).StyleId.HasBorders = false;

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
            var cellKey = new CellKey(Grid.GetColumn(editingCell) - 1, Grid.GetRow(editingCell));
            _spreadsheetProviderService.SetCellValue(0, cellKey, textBox.Text);

         

            string formattedText =
            _spreadsheetProviderService.GetFormattedCellValue(0, cellKey);

            var cellContent = new TextBlock
            {
                Text = formattedText,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#374151")),
                TextAlignment = TextAlignment.Left,
                Padding = new Thickness(4, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            editingCell.Child = cellContent;
        }
        StatusTextChanged?.Invoke(this, "Ready");

        _activeTextBox = null;
        _editingCell = null;
    }


    private string GetColumnName(int index)
        => ColumnEncodingConverter.Encode(index) ?? "";
    // {
    //     string result = "";
    //
    //     if (index == -1)
    //     {
    //         return "";
    //     }
    //
    //     while (index >= 0)
    //     {
    //         // Convert number into a letter
    //         result = (char)('A' + index % 26) + result;
    //         index = index / 26 - 1;
    //     }
    //
    //     return result;
    // }

    public void GoToCell(string columnName, string rowName)
    {
        if (!int.TryParse(rowName, out int row) || row < 1 || row > MaxRows)
            return;

        int column = ColumnNameToIndex(columnName);
        if (column < 1 || column > MaxColumns)
            return;
        SelectCellAt(row - 1, column);
    }

    private int ColumnNameToIndex(string columnName)
        => ColumnEncodingConverter.Decode(columnName) ?? 0;
    // {
    //     int result = 0;
    //     foreach (char c in columnName)
    //     {
    //         if (char.IsLetter(c))
    //         {
    //             result = result * 26 + (char.ToUpper(c) - 'A' + 1);
    //         }
    //     }
    //     return result;
    // }

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
