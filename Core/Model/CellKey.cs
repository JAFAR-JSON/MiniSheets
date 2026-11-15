namespace MiniSheets.Core.Model;

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
