using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

// TODO: Implement Cell ranges.
public class SpreadsheetEngine
{
    private readonly Workbook _workbook;
    private readonly Recalculator _recalculator = new();

    public SpreadsheetEngine(Workbook workbook)
    {
        _workbook = workbook;

        // Calculate paths for all of that.

        ;
    }

    /// <exception cref="ArgumentOutOfRange">
    /// When accessing a non-existant sheet.
    /// </exception>
    public void SetCellText(int sheetId, CellKey cellKey, string text)
    {
        Sheet sheet = _workbook.sheets[sheetId];
        Cell  cell  = sheet.GetCell(cellKey);
    }

    public string GetCellText(int sheetId, CellKey cellKey)
    {
        return "";
    }
}
