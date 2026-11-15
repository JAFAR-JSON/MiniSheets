namespace MiniSheets.Core.Model;

public class CellStyle
{
    public CellFormat Format { get; set; } = CellFormat.General;
    public int Decimals { get; set; } = 2;
    public CurrencyType Currency { get; set; } = CurrencyType.EUR;
    public string? Fill { get; set; } = "#FFFFFF";
    public string? TextColor { get; set; } = "#000000";
    public bool HasBorders { get; set; } = false;
}
