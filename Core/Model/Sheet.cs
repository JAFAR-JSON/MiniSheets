namespace MiniSheets.Core.Model;

// I think it is very important to keep the classes defined in the Data Model
// as close to the specification as possible. This part of the project is rather detailed,
// and is presumably going to be used pretty much every where, hence the more it deviates from
// it's definition from the specification, the harder it is to reason about.
// The structure should remain the same, however, some additional helper functions or fields,
// or whatever are not entirely unwelcome.
public class Sheet
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = "";

    private readonly Dictionary<CellKey, Cell> _cells   = new();
    private readonly Dictionary<int, int> _columnWidths = new(); // NOT_SPECIFICATION: The names of these fields.
    private readonly Dictionary<int, int> _rowHeights   = new(); // NOT_SPECIFICATION: The names of these fields.

    public Sheet() { }
    public Sheet(string name) { this.Name = name; }
    // Used when opening a file, the id has to stay the same as in the file.
    public Sheet(Guid id, string name) { this.Id = id; this.Name = name; }

    public IEnumerable<KeyValuePair<CellKey, Cell>> GetCells()
        => _cells;

    public void SetCell(CellKey cellKey, Cell cell)
    {
        _cells[cellKey] = cell;
    }

    public IReadOnlyDictionary<int, int> GetColumnWidths()
        => _columnWidths;

    public IReadOnlyDictionary<int, int> GetRowHeights()
        => _rowHeights;

    public void SetColumnWidth(int column, int width)
    {
        _columnWidths[column] = width;
    }

    public void SetRowHeight(int row, int height)
    {
        _rowHeights[row] = height;
    }

    public Cell GetCell(CellKey cellKey)
    {
        if (_cells.TryGetValue(cellKey, out Cell cell))
        {
            return cell;
        }
        else
        {
            Cell newCell = new Cell();
            _cells[cellKey] = newCell;
            return newCell;
        }
    }

    public int GetFarthestInitializedColumn
    {
        get
        {
            return _cells.Keys
                .Select(key => key.Column)
                .DefaultIfEmpty(0)
                .Max();
        }
    }

    public int GetFarthestInitializedRow
    {
        get
        {
            return _cells.Keys
                .Select(key => key.Row)
                .DefaultIfEmpty(0)
                .Max();
        }
    }
}
