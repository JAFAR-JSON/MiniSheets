using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

public class Evaluator
{
    private readonly CellValueGetter _cellValueGetter;
    private readonly StyleManager _styleManager;

    public Evaluator(CellValueGetter cellValueGetter, StyleManager styleManager)
    {
        _cellValueGetter = cellValueGetter;
        _styleManager = styleManager;
    }

    public CellValue Evaluate(Guid sheetId, Expr expr)
    {
        switch (expr)
        {
            case Expr.Literal e: return e.Value;
            case Expr.Unary   e: return EvaluateUnary(sheetId, e);
            case Expr.Binary  e: return EvaluateBinary(sheetId, e);

            // case Expr.Primary.Function e: return EvaluateFunction(e);
            case Expr.Primary.Identifier e: return EvaluateFunction(sheetId, e);
            case Expr.Primary.Text   e: return new CellValue.Text(e.Value);
            case Expr.Primary.Number e: return EvaluateNumber(sheetId, e);
            case Expr.Primary.Error  e: return new CellValue.Error(e.Value);
            case Expr.Primary.Cell   e:
                return _cellValueGetter.GetCellValue(sheetId, new CellKey(e.Column, e.Row));
        }

        throw new UnreachableException();
    }

    private CellValue EvaluateNumber(Guid sheetId, Expr.Primary.Number expr)
    {
        // return new CellValue.Number(e.Value);
        throw new UnreachableException();
    }

    private CellValue EvaluateUnary(Guid sheetId, Expr.Unary expr)
    {
        return Evaluate(sheetId, expr.Body);
    }

    private CellValue EvaluateBinary(Guid sheetId, Expr.Binary expr)
    {
        Expr  left = expr.Left;
        Expr right = expr.Right;

        throw new UnreachableException();
    }

    private CellValue EvaluateFunction(Guid sheetId, Expr expr)
    {
        throw new UnreachableException();
    }
}
