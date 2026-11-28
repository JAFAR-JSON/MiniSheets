using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Core.Formula;

namespace MiniSheets.Core.Format;

public class ValueFormatter
{
    private readonly StyleManager _styleManager;
    private readonly LocaleSettings _localeSettings;

    public ValueFormatter(StyleManager styleManager, LocaleSettings localeSettings)
    {
        _styleManager = styleManager;
        _localeSettings = localeSettings;
    }

    public string Format(Cell cell)
        => cell.Value switch
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

    // public string Format(Cell cell)
    //     => cell.Value switch
    //     {
    //         CellValue.Empty   v => "",
    //         CellValue.Number  v => FormatNumber (v, cell.StyleId),
    //         // CellValue.Decimal v => FormatDecimal(v, cell.StyleId),
    //         CellValue.Text    v => v.Value.ToString(),
    //         CellValue.Bool    v => v.Value.ToString(),
    //         CellValue.Date    v => FormatDate(v, cell.StyleId),
    //         CellValue.Error   v => v.Value.ToString(),
    //         _ => throw new UnreachableException()
    //     };

    // private string FormatNumber(CellValue.Number numberValue, int styleId)
    // {
    //     if (_styleManager.GetStyle(styleId) is not { } style)
    //     {
    //         throw new UnreachableException();
    //     }
    //
    //     switch (style)
    //     {
    //     }
    //
    //     return "";
    // }

    // private string FormatNumber(CellValue.Number numberValue, int styleId)
    // {
    // }
    
    // private string FormatDate(CellValue.Date dateValue, int styleId)
    // {
    // }
}
