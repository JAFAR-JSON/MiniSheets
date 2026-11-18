using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

public class Recalculator
{
    private readonly Workbook  _workbook;
    public readonly Dictionary<Guid, DependencyGraph<CellKey>> SheetDependencies { get; } = new();

    public Recalculator(Workbook workbook)
    {
        _workbook  = workbook;
    }

    public List<CellKey> CalculateSheet(Guid sheetId)
    {
        if (SheetDependencies.Contains(sheetId))
        {
            throw new ArgumentException("Sheet already has a graph.");
        }

        DependencyGraph dependencyGraph = new();
        Sheet sheet = _workbook.Sheets[sheetId];

        foreach (var kvp in sheet.Cells)
        {
            CellKey cellKey = kvp.Key;
            Cell    cell    = kvp.Value;

            List<CellKey> dependencies = GetDependencies(cell.Ast);
            dependencyGraph.Add(cellKey, dependencies);
        }

        SheetDependencies[sheet.Id] = dependencyGraph;

        return dependencyGraph.Sort();
    }

    // TODO: This is quite inefficient, fix later.
    public List<CellKey> RecalculateCell(Guid sheetId, CellKey cellKey)
    {
        if (!SheetDependencies.Contains(sheetId))
        {
            throw new ArgumentException("Sheet does not already exist.");
        }

        DependencyGraph dependencyGraph = dependencyGraph[sheetId];
        List<CellKey> dependencies = GetDependencies(_workbook.Sheets[sheetId].Cells[cellKey]);
        dependencyGraph.RemoveAllDependencies(cellKey);
        dependencyGraph.Add(cellKey, dependencies);

        return dependencyGraph.Sort();
    }

    private List<CellKey> GetDependencies(Expr Ast)
    {
        // TODO: Maybe recursion isn't the best thing way to do things here (in terms of performance).
        switch (Ast)
        {
            Expr.Unary   ast: return GetDependencies(ast.Body);
            Expr.Binary  ast: return GetDependencies(ast.Left).Concat(GetDependencies(ast.Right));
            Expr.Primary.Cell ast:
                return new[] { new CellKey(ast.Column, ast.Row) };
            default: return Enumerable.Empty<Expr>();
        }
    }
}
