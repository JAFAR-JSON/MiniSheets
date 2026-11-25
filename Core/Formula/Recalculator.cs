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

    private List<CellKey> GetDependencies(AstNode Ast)
    {
        // TODO: Maybe recursion isn't the best thing way to do things here (in terms of performance).
        switch (Ast)
        {
            case AstNode.Unary   ast:
                return GetDependencies(ast.Body);

            case AstNode.Binary  ast:
                return GetDependencies(ast.Left).Concat(GetDependencies(ast.Right)).ToList();

            case AstNode.Primary.Cell ast:
                return new List<CellKey>
                    (new[] { new CellKey(ast.Column, ast.Row) });

            default: return new List<CellKey>();
        }
    }
}
