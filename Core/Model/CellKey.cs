namespace MiniSheets.Core.Model;

/// <summary>
/// Represents grid coordinates.<br/>
/// Column starts from zero, Row starts from one.
/// </summary>
public struct CellKey
{
    public int Column { get; set; }
    public int Row    { get; set; }

    public CellKey(int column, int row)
    {
        Column = column;
        Row = row;
    }
}
