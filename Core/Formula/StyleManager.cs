using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

public class StyleManager
{
    private readonly Workbook _workbook;
    
    public StyleManager(Workbook workbook)
    {
        _workbook = workbook;
    }

    public CellStyle? GetStyle(int styleIndex)
    {
        var Styles = _workbook.Styles;
        if (Styles.TryGetValue(styleIndex, out CellStyle? style))
        {
            return style;
        }

        return null;
    }

    public bool SetStyle(int styleIndex, CellStyle style)
    {
        if (styleIndex == 0) return false;
        if (_workbook.Styles.ContainsKey(styleIndex)) return false;

        _workbook.Styles[styleIndex] = style;

        return true;
    }

    public int AddStyle(CellStyle style)
    {
        for (int i = 1; true; ++i)
        {
            if (!_workbook.Styles.ContainsKey(i))
            {
                return i;
            }
        }

        throw new UnreachableException();
    }
}
