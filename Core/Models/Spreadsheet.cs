namespace MiniSheets.Core;

public class Spreadsheet
{
    public int RowCount { get; }
    public int ColumnCount { get; }

    public List<Cell> Cells { get; } = new();

    public Spreadsheet(int rowCount, int columnCount)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;

        InitializeCells();
    }

    private void InitializeCells()
    {
        for (int row = 1; row <= RowCount; row++)
        {
            for (int column = 1; column <= ColumnCount; column++)
            {
                Cells.Add(new Cell(row, column));
            }
        }
    }

    public Cell GetCell(int row, int column)
    {
        if (row < 0 || row >= RowCount)
            throw new ArgumentOutOfRangeException(nameof(row));

        if (column < 1 || column > ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(column));

        return Cells[row * ColumnCount + (column - 1)];
    }
}
