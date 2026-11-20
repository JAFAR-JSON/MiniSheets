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
}
