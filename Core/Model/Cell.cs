namespace MiniSheets.Core.Model;

public class Cell
{
    public string Raw { get; set; } = "";
    public int StyleId { get; set; } = 0;
    public CellValue Value { get; set; } = new CellValue.Empty();
}
