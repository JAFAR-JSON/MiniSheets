using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;

// Class that connects the GUI with the SpeadsheetEngine logic
class SpreadsheetProviderService
{
    private readonly SpreadsheetEngine _spreadsheetEngine;

    public SpreadsheetProviderService(SpreadsheetEngine spreadsheetEngine)
    {
        _spreadsheetEngine = spreadsheetEngine;
    }

    public void GetFormattedCellValue(int sheetIndex, CellKey cellValue)
    {
        _spreadsheetEngine.GetFormattedCellText(sheetIndex, cellValue);
    }

    public void SetCellValue(int sheetIndex, CellKey cellKey, string value)
    {
        _spreadsheetEngine.SetCellText(sheetIndex, cellKey, value);
    }
}