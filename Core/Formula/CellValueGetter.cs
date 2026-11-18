using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

public class CellValueGetter
{
    private readonly Workbook _workbook;

    public CellValueGetter(Workbook workbook)
    {
        _workbook = workbook;
    }

    public CellKey GetCellValue(Guid sheetId, CellKey cellKey)
    {
        return _workbook.Sheets[sheetId].GetCell(cellKey).Value;
    }
}
