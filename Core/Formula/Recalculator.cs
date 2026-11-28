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
    public RecalculationInfo RecalculateCell(Guid sheetId, CellKey cellKey)
    {
        if (!SheetDependencies.ContainsKey(sheetId))
        {
            throw new ArgumentException("Sheet does not already exist.");
        }

        Cell cell = _workbook.Sheets[sheetId].GetCell(cellKey);
        if (cell.Ast is null)
        {
            throw new UnreachableException();
        }

        DependencyGraph<CellKey> dependencyGraph = SheetDependencies[sheetId];
        List<CellKey> dependencies = GetDependencies(cell.Ast);
        // Console.WriteLine($"DEBUG: RecalculateCell({dependencies.Count})");
        dependencyGraph.RemoveAllDependencies(cellKey);
        dependencyGraph.Add(cellKey, dependencies);

        var sortedGraph = dependencyGraph.Sort();
        return new RecalculationInfo(sortedGraph.Item1, sortedGraph.Item2);
    }

    private List<CellKey> GetDependencies(AstNode ast)
    {
        // Console.WriteLine($"DEBUG: GetDependencies(ast: {ast})");
        // TODO: Maybe recursion isn't the best thing way to do things here (in terms of performance).
        switch (ast)
        {
            case AstNode.Unary   a:
                return GetDependencies(a.Body);

            case AstNode.Binary  a:
                return GetDependencies(a.Left).Concat(GetDependencies(a.Right)).ToList();

            case AstNode.Call    a:
            {
                List<CellKey> cellKeys = new();
                // Console.WriteLine($"DEBUG: GetDependencies(arguments: {a.Arguments.Length})");
                foreach (var argument in a.Arguments)
                {
                    cellKeys.AddRange(GetDependencies(argument));
                    // Console.WriteLine($"DEBUG: GetDependencies(current_cells: {cellKeys.Count})");
                }
                // Console.WriteLine($"DEBUG: GetDependencies(total_cells: {cellKeys.Count})");
                return cellKeys;
            }

            case AstNode.Primary.Cell a:
                return new List<CellKey>
                    (new[] { new CellKey(a.Column, a.Row) });

            case AstNode.Primary.Range a:
                return GetRangeDependencies(a);

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

        // Console.WriteLine($"DEBUG: GetRangeDependencies(canonRange: {canonRange})");
        for (int i = canonRange.ColumnStart; i < canonRange.ColumnStart + canonRange.ColumnLength; ++i)
        {
            // Console.WriteLine($"DEBUG: GetRangeDependencies({i})");
            for (int j = canonRange.RowStart; j < canonRange.RowStart + canonRange.RowLength; ++j)
            {
                // Console.WriteLine($"DEBUG: GetRangeDependencies({i}, {j})");
                cellKeys.Add(new CellKey(i, j));
            }
        }

        return cellKeys;
    }
}
