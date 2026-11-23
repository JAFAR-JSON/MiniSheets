using System.Diagnostics;
using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

public class Evaluator
{
    // Similiar to Haskell's 'Ord' typeclass.
    private enum Ordering
    {
        LT, // Less Than
        EQ, // Equal
        GT  // Greater Than
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
        => expr switch
        {
            AstNode.Literal e => e.Value,
            AstNode.Unary   e => EvaluateUnary(sheetId, e),
            AstNode.Binary  e => EvaluateBinary(sheetId, e),

            // AstNode.Primary.Function e => EvaluateFunction(e),
            AstNode.Call e => EvaluateFunction(sheetId, e),
            AstNode.Primary.Text   e => new CellValue.Text(e.Value),
            AstNode.Primary.Number e => EvaluateNumber(e.Value),
            AstNode.Primary.Error  e => new CellValue.Error(e.Value),
            AstNode.Primary.Cell   e
                => _workbook.Sheets[sheetId].GetCell(new CellKey(e.Column, e.Row)).Value,

            _ => throw new UnreachableException()
        };

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

        return unaryOperator switch
        {
            UnaryAstNodeOperator.Negate
                => EvaluateUnaryNegate(body),

            UnaryAstNodeOperator.Plus
                => EvaluateUnaryArithmetic(body),

            UnaryAstNodeOperator.Percentify
                => EvaluateUnaryArithmetic(body),

            _ => throw new UnreachableException()
        };
    }

    private CellValue EvaluateUnaryNegate(CellValue body)
        => body switch
        {
            CellValue.Empty v
                => new CellValue.Bool(false),

            CellValue.Number v
                => new CellValue.Error("#VALUE!"),

            CellValue.Decimal v
                => new CellValue.Error("#VALUE!"),

            CellValue.Text v
                => new CellValue.Error("#VALUE!"),

            CellValue.Bool    v
                => v with { Value = !v.Value },

            CellValue.Date    v
                => throw new UnreachableException(),

            CellValue.Error   v => v
        };

    private CellValue EvaluateUnaryArithmetic(CellValue body)
        => body switch
        {
            CellValue.Empty v
                => new CellValue.Number(0),

            CellValue.Number v
                => v,

            CellValue.Decimal v
                => v,

            CellValue.Text v
                => EvaluateNumber(v.Value),

            CellValue.Bool v
                => new CellValue.Number(v.Value ? 1 : 0),

            CellValue.Date v
                => throw new UnreachableException(),

            CellValue.Error v
                => v
        };

    private CellValue EvaluateBinary(Guid sheetId, AstNode.Binary astBinary)
    {
        CellValue left  = Evaluate(sheetId, astBinary.Left );
        CellValue right = Evaluate(sheetId, astBinary.Right);
        BinaryAstNodeOperator binaryOperator = astBinary.Op;

        switch (binaryOperator)
        {
            case BinaryAstNodeOperator.Add:
                return EvaluateBinaryArithmetic
                    (left, right , (l, r) => l + r , (l, r) => l + r);

            case BinaryAstNodeOperator.Subtract:
                return EvaluateBinaryArithmetic
                    (left, right , (l, r) => l - r , (l, r) => l - r);

            case BinaryAstNodeOperator.Multiply:
                return EvaluateBinaryArithmetic
                    (left, right , ( l,  r) => l * r , (l, r) => l * r);

            case BinaryAstNodeOperator.Divide:
                return EvaluateBinaryArithmetic
                    (left, right , (l, r) => l / r , (l, r) => l / r);

            case BinaryAstNodeOperator.Join:
                return EvaluateBinaryJoin(left, right);

            case BinaryAstNodeOperator.Equal:
                return EvaluateBinaryComparison
                    (left, right , (o) => o switch { Ordering.EQ => true, _  => false });

            case BinaryAstNodeOperator.NotEqual:
                return EvaluateBinaryComparison
                    (left, right , (o) => o switch { Ordering.EQ => false, _  => true });

            case BinaryAstNodeOperator.LessThan:
                return EvaluateBinaryComparison
                    (left, right , (o) => o switch { Ordering.LT => true, _  => false });

            case BinaryAstNodeOperator.GreaterThan:
                return EvaluateBinaryComparison
                    (left, right , (o) => o switch { Ordering.GT => true, _  => false });

            case BinaryAstNodeOperator.LessThanEqual:
                return EvaluateBinaryComparison
                    (left, right , (o) => o switch { Ordering.LT => true, Ordering.EQ => true, _  => false });

            case BinaryAstNodeOperator.GreaterThanEqual:
                return EvaluateBinaryComparison
                    (left, right , (o) => o switch { Ordering.GT => true, Ordering.EQ => true, _  => false });

            // TODO: Implement Math.Pow() for decimals,
            // since there are no built-in functions for that that I've found.
            case BinaryAstNodeOperator.Raise:
                return EvaluateBinaryArithmetic
                    (left, right , (l, r) => Math.Pow(l, r) , (l, r) => (decimal) Math.Pow((double) l, (double) r));
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Since there are two types of numbers in this language,
    /// Numbers and Decimals, two functions are needed for number evaluation.
    /// </summary>
    private CellValue EvaluateBinaryArithmetic
        (CellValue left, CellValue right
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

    private CellValue EvaluateBinaryComparison
        (CellValue left, CellValue right, Func<Ordering, bool> orderingToBool)
        => Compare(left, right) switch
        {
            Ordering ordering => new CellValue.Bool(orderingToBool(ordering)),
            _ => new CellValue.Error("#ERROR!")
        };

    private Ordering? Compare(CellValue left, CellValue right)
        => (left, right) switch
        {
            (CellValue.Date l, CellValue.Date r)
                => CompareDate(l.Value, r.Value),

            (CellValue.Text l, CellValue.Text r)
                => CompareText(l.Value, r.Value),

            (CellValue.Bool l, CellValue.Bool r)
                => CompareBool(l.Value, r.Value),

            (CellValue.Number l, CellValue.Number r)
                => CompareNumber(l.Value, r.Value),

            (CellValue.Number l, CellValue.Decimal r)
                => CompareDecimal((decimal) l.Value, r.Value),

            (CellValue.Decimal l, CellValue.Number r)
                => CompareDecimal(l.Value, (decimal) r.Value),

            (CellValue.Decimal l, CellValue.Decimal r)
                => CompareDecimal(l.Value, r.Value),

            _ => null
        };

    private Ordering CompareDate
        (DateOnly left, DateOnly right)
    {
        throw new UnreachableException();
    }

    private Ordering CompareText
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

    private Ordering CompareBool
        (bool leftBool, bool rightBool)
    {
        return leftBool
            ? (rightBool ? Ordering.EQ : Ordering.GT )
            : (rightBool ? Ordering.LT : Ordering.EQ );
    }

    private Ordering CompareNumber
        (double leftNumber, double rightNumber)
    {
        return leftNumber == rightNumber
            ? Ordering.EQ
            : (leftNumber < rightNumber ? Ordering.LT : Ordering.GT );
    }

    private Ordering CompareDecimal
        (decimal leftDecimal, decimal rightDecimal)
    {
        return leftDecimal == rightDecimal
            ? Ordering.EQ
            : (leftDecimal < rightDecimal ? Ordering.LT : Ordering.GT );
    }

    private CellValue EvaluateBinaryJoin(CellValue left, CellValue right)
        => (left, right) switch
        {
            (CellValue.Text leftText, CellValue.Text rightText)
                => new CellValue.Text(string.Join("", leftText.Value, rightText.Value)),
            _ => new CellValue.Error("#ERROR!")
        };
}
