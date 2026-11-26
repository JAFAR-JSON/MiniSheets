using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

public class Recalculator
{
    private readonly Workbook  _workbook;
    public Dictionary<Guid, DependencyGraph<CellKey>> SheetDependencies { get; } = new();

    public Recalculator(Workbook workbook)
    {
        _workbook  = workbook;
    }

    public List<CellKey> CalculateSheet(Guid sheetId)
    {
        if (SheetDependencies.ContainsKey(sheetId))
        {
            throw new ArgumentException("Sheet already has a graph.");
        }

        DependencyGraph<CellKey> dependencyGraph = new();
        Sheet sheet = _workbook.Sheets[sheetId];

        foreach (var kvp in sheet.Cells)
        {
            CellKey cellKey = kvp.Key;
            Cell    cell    = kvp.Value;

            if (cell.Ast is null)
            {
                throw new UnreachableException();
            }

            List<CellKey> dependencies = GetDependencies(cell.Ast);
            dependencyGraph.Add(cellKey, dependencies);
        }

        SheetDependencies[sheet.Id] = dependencyGraph;

        return dependencyGraph.Sort().Item1;
    }

    // TODO: This is quite inefficient, fix later.
    public List<CellKey> RecalculateCell(Guid sheetId, CellKey cellKey)
    {
        if (!SheetDependencies.ContainsKey(sheetId))
        {
            throw new ArgumentException("Sheet does not already exist.");
        }

        Cell cell = _workbook.Sheets[sheetId].Cells[cellKey];
        if (cell.Ast is null)
        {
            throw new UnreachableException();
        }

        DependencyGraph<CellKey> dependencyGraph = SheetDependencies[sheetId];
        List<CellKey> dependencies = GetDependencies(cell.Ast);
        dependencyGraph.RemoveAllDependencies(cellKey);
        dependencyGraph.Add(cellKey, dependencies);

        var first = dependencyGraph.Sort().Item1;
        // first.Add(cellKey);
        return first;
    }

    private List<CellKey> GetDependencies(AstNode ast)
    {
        // TODO: Maybe recursion isn't the best thing way to do things here (in terms of performance).
        switch (ast)
        {
            case AstNode.Unary   a:
                return GetDependencies(a.Body);

            case AstNode.Binary  a:
                return GetDependencies(a.Left).Concat(GetDependencies(a.Right)).ToList();

            case AstNode.Primary.Cell a:
                return new List<CellKey>
                    (new[] { new CellKey(a.Column, a.Row) });

            case AstNode.Primary.Range a:

            default: return new List<CellKey>();
        }
    }

    // TODO: I think it might be a good idea to refactor this in the future,
    // since this is not very performant, though I feel like that would require
    // modifying DependencyGraph, which might take a while.
    private List<CellKey> GetRangeDependencies(AstNode.Primary.Range range)
    {
        CanonRange canonRange = RangeCanonizer.GetCanonRange(range);
        List<CellKey> cellKeys = new();

        for (int i = canonRange.ColumnStart; i < canonRange.ColumnLength; ++i)
        {
            for (int j = canonRange.RowStart; j < canonRange.RowLength; ++j)
            {
                cellKeys.Add(new CellKey(i, j));
            }
        }

        return cellKeys;
    }
}
