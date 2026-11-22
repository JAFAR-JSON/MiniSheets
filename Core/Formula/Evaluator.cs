using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

public class Evaluator
{
    private readonly Workbook _workbook;
    private readonly StyleManager _styleManager;
    private readonly FunctionRegistry _functionRegistry;

    public Evaluator
        (Workbook workbook, StyleManager styleManager, FunctionRegistry functionRegistry)
    {
        _workbook = workbook;
        _styleManager = styleManager;
        _functionRegistry = functionRegistry;
    }

    public CellValue Evaluate(Guid sheetId, AstNode expr)
    {
        switch (expr)
        {
            case AstNode.Literal e: return e.Value;
            case AstNode.Unary   e: return EvaluateUnary(sheetId, e);
            case AstNode.Binary  e: return EvaluateBinary(sheetId, e);

            // case AstNode.Primary.Function e: return EvaluateFunction(e);
            case AstNode.Call e: return EvaluateFunction(sheetId, e);
            case AstNode.Primary.Text   e: return new CellValue.Text(e.Value);
            case AstNode.Primary.Number e: return EvaluateNumber(sheetId, e);
            case AstNode.Primary.Error  e: return new CellValue.Error(e.Value);
            case AstNode.Primary.Cell   e:
                // return _cellValueGetter.GetCellValue(sheetId, new CellKey(e.Column, e.Row));
                return _workbook.Sheets[sheetId].GetCell(new CellKey(e.Column, e.Row)).Value;
        }

        throw new UnreachableException();
    }

    private CellValue EvaluateFunction(Guid sheetId, AstNode.Call astCall)
    {
        throw new UnreachableException();
    }

    private CellValue EvaluateNumber(Guid sheetId, AstNode.Primary.Number astPrimary)
    {
        // return new CellValue.Number(e.Value);
        throw new UnreachableException();
    }

    private CellValue EvaluateUnary(Guid sheetId, AstNode.Unary astUnary)
    {
        return Evaluate(sheetId, astUnary.Body);
    }

    private CellValue EvaluateBinary(Guid sheetId, AstNode.Binary astBinary)
    {
        AstNode  left = astBinary.Left;
        AstNode right = astBinary.Right;

        throw new UnreachableException();
    }
}
