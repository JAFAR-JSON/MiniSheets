using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;

namespace MiniSheets.State;

// Class that connects the GUI with the SpeadsheetEngine logic
class SpreadsheetProviderService
{
    private readonly SpreadsheetEngine _spreadsheetEngine;

    public SpreadsheetProviderService(SpreadsheetEngine spreadsheetEngine)
    {
        _spreadsheetEngine = spreadsheetEngine;
    }

    public string GetFormattedCellValue(int sheetIndex, CellKey cellValue)
    {
        return _spreadsheetEngine.GetFormattedCellText(sheetIndex, cellValue);
    }

    public string GetRawCellValue(int sheetIndex, CellKey cellValue)
    {
        return _spreadsheetEngine.GetRawCellText(sheetIndex, cellValue);
    }

    public void SetCellValue(int sheetIndex, CellKey cellKey, string value)
    {
        _spreadsheetEngine.SetCellText(sheetIndex, cellKey, value);
    }
}
