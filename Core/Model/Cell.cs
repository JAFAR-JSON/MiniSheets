namespace MiniSheets.Core.Model;

public class Cell
{
    public string Raw { get; set; } = "";
    public CellStyle StyleId { get; set; } = new CellStyle();
    public CellValue Value { get; set; } = new CellValue.Empty();
}
