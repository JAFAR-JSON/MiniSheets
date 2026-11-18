using System.Diagnostics;
using MiniSheets.Core.Formula;

namespace MiniSheets.Core.Format;

public class ValueFormatter
{
    private readonly StyleManager _styleManager;

    public ValueFormatter(StyleManager styleManager)
    {
        _styleManager = styleManager;
    }

    public string Format(CellStyle style, CellValue cellValue)
    {
        throw new UnreachableException();
    }
}
