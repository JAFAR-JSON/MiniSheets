using MiniSheets.Core.Formula;

namespace MiniSheets.Core.Model;

public class Cell
{
    public string Raw { get; set; } = "";
    public int StyleId { get; set; } = 0;
    public Expr? AST { get; set; } = null;
    public CellValue Value { get; set; } = new CellValue.Empty();
}
