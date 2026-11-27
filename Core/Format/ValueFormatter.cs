using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Core.Formula;

namespace MiniSheets.Core.Format;

public class ValueFormatter
{
    private readonly StyleManager _styleManager;

    public ValueFormatter(StyleManager styleManager)
    {
        _styleManager = styleManager;
    }

    public string Format(CellValue cellValue)
        => cellValue switch
        {
            CellValue.Empty   v => "",
            CellValue.Number  v => v.Value.ToString(),
            CellValue.Decimal v => v.Value.ToString(),
            CellValue.Text    v => v.Value.ToString(),
            CellValue.Bool    v => v.Value.ToString(),
            CellValue.Date    v => v.Value.ToString(),
            CellValue.Error   v => v.Value.ToString(),
            _ => throw new UnreachableException()
        };
}
