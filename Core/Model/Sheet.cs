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

    private Dictionary<CellKey, Cell> _cells   { get; set; } = new();
    private Dictionary<int, int> _columnWidths { get; set; } = new(); // NOT_SPECIFICATION: The names of these fields.
    private Dictionary<int, int> _rowHeights   { get; set; } = new(); // NOT_SPECIFICATION: The names of these fields.

    public Sheet() { }
    public Sheet(string name) { this.Name = name; }

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
