using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

// TODO: Implement Cell ranges.
public class SpreadsheetEngine
{
    private readonly Workbook _workbook;
    private readonly Recalculator _recalculator;
    // private readonly FunctionRegistry _functionRegistry;
    private readonly CellValueGetter _cellValueGetter;
    private readonly StyleManager _styleManager;
    private readonly Evaluator _evaluator;
    private readonly Parser _parser;
    private readonly ValueParser _valueParser;
    private readonly ValueFormatter _valueFormatter;

    public SpreadsheetEngine(Workbook workbook)
    {
        _workbook = workbook;
        _recalculator = new Recalculator(_workbook);
        _cellValueGetter = new CellValueGetter(_workbook);
        _styleManager = new StyleManager(_workbook);
        _evaluator = new Evaluator(_cellValueGetter, _styleManager);
        _parser = new Parser();
        _valueParser = new ValueParser(_parser);
        _valueFormatter = new ValueFormatter(_styleManager);

        // Calculate paths for all of that.
        foreach (var kpv in _workbook.Sheets)
        {
            Guid sheetId = kpv.Key;
            Sheet sheet = kpv.Value;

            // Parse workbook.
            foreach (CellKey cellKey in sheet.Cells)
            {
                Cell cell = sheet[cellKey];
                cell.Ast = _valueParser.ParseValue(cell.Raw);
            }

            var dependencyGraph =
                _recalculator.SheetDependencies[sheetId];

            // Evaluate every dependency in order.
            List<CellKey> cellKeys = _recalculator.CalculateSheet(sheetId);
            for (CellKey cellKey in cellKeys)
            {
                _evaluator.Evaluate(sheetId, sheet.GetCell(cellKey));
            }
        }
    }

    /// <exception cref="ArgumentOutOfRange">
    /// When accessing a non-existant sheet.
    /// </exception>
    public void SetCellText(int sheetIndex, CellKey cellKey, string text)
    {
        Guid sheetId = _workbook.SheetIds[sheetIndex];

        SetCellText(sheetId, cellKey, text);
    }

    public void SetCellText(Guid sheetId, CellKey cellKey, string text)
    {
        Sheet sheet = _workbook.sheets[sheetId];
        Cell  cell  = sheet.GetCell(cellKey);

        cell.Raw = text;
        cell.Ast = _parser.ParseText(cell.Raw);

        // Evaluate every dependency in order.
        List<CellKey> cellKeys = _recalculator.RecalculateCell(sheetId, cellKey);
        for (CellKey cellKey in cellKeys)
        {
            _evaluator.Evaluate(sheetId, sheet.GetCell(cellKey));
        }
    }

    public string GetCellText(int sheetIndex, CellKey cellKey)
    {
        Guid sheetId = _workbook.SheetIds[sheetIndex];

        return GetCellText(sheetId, cellKey);
    }

    public string GetCellText(Guid sheetId, CellKey cellKey)
    {
        Sheet sheet = _workbook.Sheets[sheetId];
        Cell cell = sheet.GetCell(cellKey);

        return _valueFormatter.Format(cell.Value);
    }
}
