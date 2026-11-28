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
    private readonly StyleManager _styleManager;
    private readonly Evaluator _evaluator;
    private readonly Parser _parser;
    private readonly LocaleSettings _localeSettings;
    private readonly ValueParser _valueParser;
    private readonly ValueFormatter _valueFormatter;

    public SpreadsheetEngine(Workbook workbook)
    {
        _workbook = workbook;
        _recalculator = new Recalculator(_workbook);
        _styleManager = new StyleManager(_workbook);
        _evaluator = new Evaluator(_workbook, _styleManager);
        _parser = new Parser();
        _localeSettings = new LocaleSettings(LocaleType.EN_US);
        _valueParser = new ValueParser(_parser, _localeSettings, _styleManager);
        _valueFormatter = new ValueFormatter(_styleManager, _localeSettings);

        // Calculate paths for all of that.
        foreach (var kpv in _workbook.Sheets)
        {
            Guid sheetId = kpv.Key;
            Sheet sheet = kpv.Value;

            // Parse workbook.
            foreach (var cellKpv in sheet.Cells)
            {
                Cell cell = cellKpv.Value;
                cell.Ast = _valueParser.Parse(cell);
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
        Console.WriteLine($"DEBUG: SetCellText(column: {cellKey.Column}, row: {cellKey.Row})");
        Console.WriteLine($"DEBUG: SetCellText(text: \"{text}\")");
        Sheet sheet = _workbook.Sheets[sheetId];
        Cell  cell  = sheet.GetCell(cellKey);

        cell.Raw = text;
        cell.Ast = _valueParser.Parse(cell);
        // Console.WriteLine($"DEBUG: SetCellText({cell.Raw}, {cell.Ast})");

        // Evaluate every dependency in order.
        List<CellKey> cellKeys = _recalculator.RecalculateCell(sheetId, cellKey);

        Console.WriteLine($"DEBUG: SetCellText.BeforeLoop(dependencyCount: {cellKeys.Count}, raw: {cell.Raw}, ast: {cell.Ast})");
        foreach (var c in cellKeys)
        {
            Console.WriteLine($"DEBUG: SetCellText.KeyInCellKeys(column: {c.Column}, row: {c.Row})");
        }

        foreach (CellKey recalculatedCellKey in cellKeys)
        {
            Console.WriteLine($"DEBUG: SetCellText.RecalculatedKey(column: {recalculatedCellKey.Column}, row: {recalculatedCellKey.Row});");
            Cell recalculatedCell = sheet.GetCell(recalculatedCellKey);
            if (recalculatedCell.Ast is null)
            {
                recalculatedCell.Raw = "";
                recalculatedCell.Ast = _valueParser.Parse(cell);
                // throw new UnreachableException();
            }
            Console.WriteLine($"DEBUG: SetCellText.Ast({recalculatedCell.Ast})");
            recalculatedCell.Value = _evaluator.Evaluate(sheetId, recalculatedCell.Ast);
            Console.WriteLine($"DEBUG: SetCellText.Value({recalculatedCell.Value})");
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

        return _valueFormatter.Format(cell);
    }

    public string GetRawCellText(int sheetIndex, CellKey cellKey)
    {
        Guid sheetId = _workbook.SheetIds[sheetIndex];

        return GetRawCellText(sheetId, cellKey);
    }

    public string GetRawCellText(Guid sheetId, CellKey cellKey)
    {
        Sheet sheet = _workbook.Sheets[sheetId];
        Cell cell = sheet.GetCell(cellKey);

        return cell.Raw;
    }
}
