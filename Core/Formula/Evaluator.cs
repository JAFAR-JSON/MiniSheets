using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

// public class Evaluator
// {
//     private Sheet _sheet = null;
//
//     public void EvaluateInOrder(Sheet sheet, List<CellKey> cellKeys)
//     {
//         _sheet = sheet;
//
//         for (CellKey cellKey in CellKey)
//         {
//             Evaluate(cellKey);
//         }
//     }
//
//     public CellValue EvaluateCell(CellKey cellKey)
//     {
//         Cell cell = _sheet.GetCell(cellKey);
//         CellValue cellValue;
//
//         if (cell.Value is null)
//         {
//             return new CellValue.Error("#ERROR!");;
//         }
//
//         cell.Value = EvaluateExpr(cellKey, cell.Value);
//     }
//
//     public CellValue EvaluateExpr(CellKey cellKey, Expr expr)
//     {
//         switch (expr)
//         {
//             case Expr.Literal e: return e.Value;
//             case Expr.Unary   e: return EvaluateExpr(cellKey e);
//             case Expr.Binary  e: return EvaluateExprBinary(cellKey, e);
//
//             case Expr.Primary.Identifier e: throw new UnreachableException();
//             case Expr.Primary.Text   e: return new CellValue.Text(e.Value);
//             case Expr.Primary.Number e: return new CellValue.Numeric(e.Value);
//             case Expr.Primary.Error  e: return new CellValue.Error(e.Value);
//             case Expr.Primary.Cell   e:
//                 return EvaluateCell(new CellKey(e.Column, e.Row));
//         }
//     }
//
//     private CellValue EvaluateExprBinary(CellKey cellKey, Expr.Binary exprBinary)
//     {
//         switch (exprBinary)
//         {
//         }
//     }
// }
