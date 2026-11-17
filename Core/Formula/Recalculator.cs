using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

public class Recalculator
{
    private readonly Evaluator _evaluator;
    private readonly Workbook  _workbook;

    private Dictionary<Guid, DependencyGraph<CellKey>> _sheetDependencies { get; } = new();

    public Recalculator(Evaluator evaluator, Workbook workbook)
    {
        _evaluator = evaluator;
        _workbook  = workbook;
    }

    public void CalculateSheet(Sheet sheet)
    {
        if (SheetDependencies.Contains(sheet.Id))
        {
            throw new ArgumentException("Sheet already exists.");
        }

        DependencyGraph dependencyGraph = new();

        foreach (var kvp in sheet.Cells)
        {
            CellKey cellKey = kvp.Key;
            Cell    cell    = kvp.Value;

            List<CellKey> dependencies = GetDependencies(cell.AST);
            dependencyGraph.Add(cellKey, dependencies);
        }

        SheetDependencies[sheet.Id] = dependencyGraph;

        for (var sheet in _workbook.Sheets)
        {
            Guid sheetId = sheet.Id;

            _sheetDependencies
        }
    }

    public void RecalculateCell(Guid sheetId, CellKey cellKey, Expr cellAST)
    {
        List<CellKey> dependencies = GetDependencies(cellAST);
        dependencyGraph.Add(cellKey, dependencies);
    }

    private List<CellKey> GetDependencies(Expr AST)
    {
        // TODO: Maybe recursion isn't the best thing way to do things here (in terms of performance).
        switch (AST)
        {
            Expr.Unary   ast: return GetDependencies(ast.Body);
            Expr.Binary  ast: return GetDependencies(ast.Left).Concat(GetDependencies(ast.Right));
            Expr.Primary.Cell ast:
                return new[] { new CellKey(ast.Column, ast.Row) };
            default: return Enumerable.Empty<Expr>();
        }
    }
}
