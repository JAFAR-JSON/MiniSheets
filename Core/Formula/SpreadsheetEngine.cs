using System.Diagnostics;
using MiniSheets.Shared;
using MiniSheets.Core.Model;
using MiniSheets.Core.Format;

namespace MiniSheets.Core.Formula;

// TODO: Implement Cell ranges.
public class SpreadsheetEngine
{
    private readonly Workbook _workbook;
    private readonly Recalculator _recalculator;
    private readonly FunctionRegistry _functionRegistry;
    private readonly StyleManager _styleManager;
    private readonly Evaluator _evaluator;
    private readonly Parser _parser;
    private readonly ValueParser _valueParser;
    private readonly ValueFormatter _valueFormatter;

    public SpreadsheetEngine(Workbook workbook)
    {
        _workbook = workbook;
        _recalculator = new Recalculator(_workbook);
        _functionRegistry = new FunctionRegistry();
        _styleManager = new StyleManager(_workbook);
        _evaluator = new Evaluator(_workbook, _styleManager, _functionRegistry);
        _parser = new Parser();
        _valueParser = new ValueParser(_parser);
        _valueFormatter = new ValueFormatter(_styleManager);

        // Calculate paths for all of that.
        foreach (var kpv in _workbook.Sheets)
        {
            Guid sheetId = kpv.Key;
            Sheet sheet = kpv.Value;

            // Parse workbook.
            foreach (var cellKpv in sheet.Cells)
            {
                Cell cell = cellKpv.Value;
                cell.Ast = _valueParser.ParseValue(cell.Raw);
            }

            // Evaluate every dependency in order.
            List<CellKey> cellKeys = _recalculator.CalculateSheet(sheetId);
            foreach (CellKey cellKey in cellKeys)
            {
                Cell cell  = sheet.GetCell(cellKey);
                if (cell.Ast is null)
                {
                    throw new UnreachableException();
                }
                cell.Value = _evaluator.Evaluate(sheetId, cell.Ast);
            }
        }
    }

    /// <exception cref="ArgumentOutOfRange">
    /// When accessing a non-existant sheet.
    /// </exception>
    public List<CellKey> SetCellText(int sheetIndex, CellKey cellKey, string text)
    {
        Guid sheetId = _workbook.SheetIds[sheetIndex];

        return SetCellText(sheetId, cellKey, text);
    }

    /// <summary>
    /// TODO: List<CellKey> shoould return a list of all changed CellKey.
    /// Currently, it returns EVERY cell.
    /// </summary>
    public List<CellKey> SetCellText(Guid sheetId, CellKey cellKey, string text)
    {
        Sheet sheet = _workbook.Sheets[sheetId];
        Cell  cell  = sheet.GetCell(cellKey);

        cell.Raw = text;
        cell.Ast = _parser.ParseText(cell.Raw);

        // Evaluate every dependency in order.
        List<CellKey> cellKeys = _recalculator.RecalculateCell(sheetId, cellKey);
        foreach (CellKey recalculatedCellKey in cellKeys)
        {
            Cell recalculatedCell = sheet.GetCell(recalculatedCellKey);
            if (recalculatedCell.Ast is null)
            {
                throw new UnreachableException();
            }
            _evaluator.Evaluate(sheetId, recalculatedCell.Ast);
        }

        return cellKeys;
    }

    public string GetFormattedCellText(int sheetIndex, CellKey cellKey)
    {
        Guid sheetId = _workbook.SheetIds[sheetIndex];

        return GetFormattedCellText(sheetId, cellKey);
    }

    public string GetFormattedCellText(Guid sheetId, CellKey cellKey)
    {
        Sheet sheet = _workbook.Sheets[sheetId];
        Cell cell = sheet.GetCell(cellKey);

        return _valueFormatter.Format(cell.Value);
    }
}
