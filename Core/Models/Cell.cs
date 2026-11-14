namespace MiniSheets.Core;

public class Cell
{
    public int Row { get; set; }
    public int Column { get; set; }

    public string CellReference => $"{(char)('A' + Column)}{Row + 1}";

    public bool IsStyled { get; set; } = false;

    public string Value { get; set; } = string.Empty;

    public CellStyle Style { get; set; }

    public Cell(int row, int column)
    {
        Row = row;
        Column = column;
        Style = new CellStyle();
    }
}
