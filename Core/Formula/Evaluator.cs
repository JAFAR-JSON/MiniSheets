using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

public class Evaluator
{
    // Similiar to Haskell's 'Ord' typeclass.
    private enum Ordering
    {
        LT,
        EQ,
        GT
    };

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

    // TODO: Implement date lexing, parsing and evaluation for dates.
    // Recheck ALL logic according to the specification.
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
            case AstNode.Primary.Number e: return EvaluateNumber(e.Value);
            case AstNode.Primary.Error  e: return new CellValue.Error(e.Value);
            case AstNode.Primary.Cell   e:
                // return _cellValueGetter.GetCellValue(sheetId, new CellKey(e.Column, e.Row));
                return _workbook.Sheets[sheetId].GetCell(new CellKey(e.Column, e.Row)).Value;
        }

        throw new UnreachableException();
    }

    // TODO: Implement function evaluation (and function registry).
    private CellValue EvaluateFunction(Guid sheetId, AstNode.Call astCall)
    {
        throw new UnreachableException();
    }

    private CellValue EvaluateNumber(string canonizedString)
    {
        // We're assuming that the number here is canonized (WHICH IT SHOULD BE).
        try
        {
            return new CellValue.Number
                (Convert.ToDouble(canonizedString));
        }
        catch (FormatException)
        {
            return new CellValue.Error("#VALUE!");
        }
        catch (OverflowException)
        {
            return new CellValue.Error("#NUM!");
        }
    }

    // private CellValue EvaluateDecimal(Guid sheetId, string canonizedString)
    // {
    //     // We're assuming that the number here is canonized (WHICH IT SHOULD BE).
    //     try
    //     {
    //         return new CellValue.Decimal
    //             (Convert.ToDecimal(astPrimary.Value));
    //     }
    //     catch (FormatException e)
    //     {
    //         return new CellValue.Error("#NUM!");
    //     }
    //     catch (OverflowException e)
    //     {
    //         return new CellValue.Error("#NUM!");
    //     }
    // }

    private CellValue EvaluateUnary(Guid sheetId, AstNode.Unary astUnary)
    {
        CellValue body = Evaluate(sheetId, astUnary.Body);
        UnaryAstNodeOperator unaryOperator = astUnary.Op;

        switch (unaryOperator)
        {
            case UnaryAstNodeOperator.Negate:
                switch (body)
                {
                    case CellValue.Empty   v:
                        return new CellValue.Bool(false);

                    case CellValue.Number  v:
                        return new CellValue.Error("#VALUE!");

                    case CellValue.Decimal v:
                        return new CellValue.Error("#VALUE!");

                    case CellValue.Text    v:
                        return new CellValue.Error("#VALUE!");

                    case CellValue.Bool    v:
                        return v with { Value = !v.Value };

                    case CellValue.Date    v:
                        throw new UnreachableException();

                    case CellValue.Error   v:
                        return v;
                }
                break;

            case UnaryAstNodeOperator.Plus:
                switch (body)
                {
                    case CellValue.Empty   v:
                        return new CellValue.Number(0);

                    case CellValue.Number  v:
                        return v;

                    case CellValue.Decimal v:
                        return v;

                    case CellValue.Text    v:
                        return EvaluateNumber(v.Value);

                    case CellValue.Bool    v:
                        return new CellValue.Number(v.Value ? 1 : 0);

                    case CellValue.Date    v:
                        throw new UnreachableException();

                    case CellValue.Error   v:
                        return v;
                }
                break;

            case UnaryAstNodeOperator.Percentify:
                switch (body)
                {
                    case CellValue.Empty   v:
                        return new CellValue.Number(0);

                    case CellValue.Number  v:
                        return v;

                    case CellValue.Decimal v:
                        return v;

                    case CellValue.Text    v:
                        return EvaluateNumber(v.Value);

                    case CellValue.Bool    v:
                        return new CellValue.Number(v.Value ? 1 : 0);

                    case CellValue.Date    v:
                        throw new UnreachableException();

                    case CellValue.Error   v:
                        return v;
                }
                break;
        }

        throw new UnreachableException();
    }

    private CellValue EvaluateBinary(Guid sheetId, AstNode.Binary astBinary)
    {
        CellValue left  = Evaluate(sheetId, astBinary.Left );
        CellValue right = Evaluate(sheetId, astBinary.Right);
        BinaryAstNodeOperator binaryOperator = astBinary.Op;

        switch (binaryOperator)
        {
            case BinaryAstNodeOperator.Add:
                return EvaluateBinaryArithmetic
                    (sheetId, left, right
                    , (l, r) => l + r
                    , (l, r) => l + r
                    );

            case BinaryAstNodeOperator.Subtract:
                return EvaluateBinaryArithmetic
                    (sheetId, left, right
                    , (l, r) => l - r
                    , (l, r) => l - r
                    );

            case BinaryAstNodeOperator.Multiply:
                return EvaluateBinaryArithmetic
                    (sheetId, left, right
                    , ( l,  r) => l * r
                    , (l, r) => l * r
                    );

            case BinaryAstNodeOperator.Divide:
                return EvaluateBinaryArithmetic
                    (sheetId, left, right
                    , (l, r) => l / r
                    , (l, r) => l / r
                    );

            case BinaryAstNodeOperator.Join:
                throw new UnreachableException();

            // TODO: Implement comparison evaluations.
            case BinaryAstNodeOperator.Equal:
                // return EvaluateBinaryComparison
                //     (sheetId, left, right, (l, r) => l == r);

            case BinaryAstNodeOperator.NotEqual:
                // return EvaluateBinaryComparison
                //     (sheetId, left, right, (l, r) => l != r);

            case BinaryAstNodeOperator.LessThan:
                // return EvaluateBinaryComparison
                //     (sheetId, left, right, (l, r) => l < r);

            case BinaryAstNodeOperator.GreaterThan:
                // return EvaluateBinaryComparison
                //     (sheetId, left, right, (l, r) => l > r);

            case BinaryAstNodeOperator.LessThanEqual:
                // return EvaluateBinaryComparison
                //     (sheetId, left, right, (l, r) => l <= r);

            case BinaryAstNodeOperator.GreaterThanEqual:
                // return EvaluateBinaryComparison
                //     (sheetId, left, right, (l, r) => l >= r);

            // TODO: Implement Math.Pow() for decimals,
            // since there are no built-in functions for that that I've found.
            case BinaryAstNodeOperator.Raise:
                return EvaluateBinaryArithmetic
                    (sheetId, left, right
                    , (l, r) => Math.Pow(l, r)
                    , (l, r) => (decimal) Math.Pow((double) l, (double) r)
                    );
        }

        throw new UnreachableException();
    }

    private CellValue EvaluateBinaryAdd(Guid sheetId, CellValue left, CellValue right)
    {
        CellValue result;
        if (ArithmeticIsEmpty(left, out result)) { return result; }
        if (ArithmeticIsText (left, out result)) { return result; }
        if (ArithmeticIsBool (left, out result)) { return result; }
        if (ArithmeticIsError(left, out result)) { return result; }

        var evalNumbers = CellValue (double left, double right) =>
        {
            var result = left + right;
            return !double.IsInfinity(result)
                ? new CellValue.Number(result)
                : new CellValue.Error("#NUM!");
        };

        var evalDecimals = CellValue (decimal left, decimal right) =>
        {
            try
                { return new CellValue.Decimal(checked(left + right)); }
            catch (OverflowException)
                { return new CellValue.Error("#NUM!"); }
        };

        return (left, right) switch
        {
            (CellValue.Number l, CellValue.Number r)
                => evalNumbers(l.Value, r.Value),

            (CellValue.Number l, CellValue.Decimal r)
                => evalDecimals((decimal) l.Value, r.Value),

            (CellValue.Decimal l, CellValue.Number r)
                => evalDecimals(l.Value, (decimal) r.Value),

            (CellValue.Decimal l, CellValue.Decimal r)
                => evalDecimals(l.Value, r.Value),

            _ => new CellValue.Error("#ERROR!")
        };
    }

    private CellValue EvaluateBinaryArithmetic
        (Guid sheetId, CellValue left, CellValue right
        , Func<double , double , double > doubleFunc
        , Func<decimal, decimal, decimal> decimalFunc
        )
    {
        CellValue result;
        if (ArithmeticIsEmpty(left , out result)) { return result; }
        if (ArithmeticIsEmpty(right, out result)) { return result; }
        if (ArithmeticIsText (left , out result)) { return result; }
        if (ArithmeticIsText (right, out result)) { return result; }
        if (ArithmeticIsBool (left , out result)) { return result; }
        if (ArithmeticIsBool (right, out result)) { return result; }
        if (ArithmeticIsError(left , out result)) { return result; }
        if (ArithmeticIsError(right, out result)) { return result; }

        var evalNumbers = CellValue (double left, double right) =>
        {
            var result = doubleFunc(left, right);
            return !double.IsInfinity(result)
                ? new CellValue.Number(result)
                : new CellValue.Error("#NUM!");
        };

        var evalDecimals = CellValue (decimal left, decimal right) =>
        {
            try
                { return new CellValue.Decimal(checked(decimalFunc(left, right))); }
            catch (OverflowException)
                { return new CellValue.Error("#NUM!"); }
        };

        return (left, right) switch
        {
            (CellValue.Number l, CellValue.Number r)
                => evalNumbers(l.Value, r.Value),

            (CellValue.Number l, CellValue.Decimal r)
                => evalDecimals((decimal) l.Value, r.Value),

            (CellValue.Decimal l, CellValue.Number r)
                => evalDecimals(l.Value, (decimal) r.Value),

            (CellValue.Decimal l, CellValue.Decimal r)
                => evalDecimals(l.Value, r.Value),

            _ => new CellValue.Error("#ERROR!")
        };
    }

    private bool ArithmeticIsEmpty(CellValue input, out CellValue output)
    {
        if (input is CellValue.Empty)
        {
            output = new CellValue.Number(0);
            return true;
        }

        output = input;
        return false;
    }

    private bool ArithmeticIsText(CellValue input, out CellValue output)
    {
        if (input is CellValue.Text text)
        {
            output = EvaluateNumber(text.Value);
            return true;
        }

        output = input;
        return false;
    }

    private bool ArithmeticIsBool(CellValue input, out CellValue output)
    {
        if (input is CellValue.Bool)
        {
            output = input;
            return true;
        }

        output = input;
        return false;
    }

    private bool ArithmeticIsDate(CellValue input, out CellValue output)
    {
        if (input is CellValue.Date date)
        {
            throw new UnreachableException();
            return true;
        }

        output = input;
        return false;
    }

    private bool ArithmeticIsError(CellValue input, out CellValue output)
    {
        if (input is CellValue.Error)
        {
            output = input;
            return true;
        }

        output = input;
        return false;
    }

    private Ordering EvaluateBinaryComparison
        (CellValue left, CellValue right, Func<Ordering, bool> compare)
    {
        return (left, right) switch
        {
            (CellValue.Date l, CellValue.Date r)
                => EvaluateBinaryComparisonDate(l.Value, r.Value),

            (CellValue.Text l, CellValue.Text r)
                => EvaluateBinaryComparisonText(l.Value, r.Value),

            (CellValue.Bool l, CellValue.Bool r)
                => EvaluateBinaryComparisonBool(l.Value, r.Value),

            (CellValue.Number l, CellValue.Number r)
                => EvaluateBinaryComparisonNumber(l.Value, r.Value),

            (CellValue.Number l, CellValue.Decimal r)
                => EvaluateBinaryComparisonDecimal((decimal) l.Value, r.Value),

            (CellValue.Decimal l, CellValue.Number r)
                => EvaluateBinaryComparisonDecimal(l.Value, (decimal) r.Value),

            (CellValue.Decimal l, CellValue.Decimal r)
                => EvaluateBinaryComparisonDecimal(l.Value, r.Value),

            _ => throw new UnreachableException()
            // _ => new CellValue.Error("#ERROR!")
        };
    }

    private Ordering EvaluateBinaryComparisonDate
        (DateOnly left, DateOnly right)
    {
        throw new UnreachableException();
    }

    private Ordering EvaluateBinaryComparisonText
        (string leftText, string rightText)
    {
        if (leftText.Length == rightText.Length)
        {
            for (int i = 0; i < leftText.Length; ++i)
            {
                int  leftChar = (int)  leftText[i];
                int rightChar = (int) rightText[i];

                if (leftChar != rightChar)
                {
                    return leftChar < rightChar ? Ordering.LT : Ordering.GT;
                }
            }

            return Ordering.EQ;
        }
        else
        {
            return leftText.Length < rightText.Length ? Ordering.LT : Ordering.GT;
        }
    }

    private Ordering EvaluateBinaryComparisonBool
        (bool leftBool, bool rightBool)
    {
        return leftBool
            ? (rightBool ? Ordering.EQ : Ordering.GT )
            : (rightBool ? Ordering.LT : Ordering.EQ );
    }

    private Ordering EvaluateBinaryComparisonNumber
        (double leftNumber, double rightNumber)
    {
        return leftNumber == rightNumber
            ? Ordering.EQ
            : (leftNumber < rightNumber ? Ordering.LT : Ordering.GT );
    }

    private Ordering EvaluateBinaryComparisonDecimal
        (decimal leftDecimal, decimal rightDecimal)
    {
        return leftDecimal == rightDecimal
            ? Ordering.EQ
            : (leftDecimal < rightDecimal ? Ordering.LT : Ordering.GT );
    }
}
