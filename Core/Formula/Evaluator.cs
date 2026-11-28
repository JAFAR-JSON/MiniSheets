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

    public Evaluator
        (Workbook workbook, StyleManager styleManager)
    {
        _workbook = workbook;
        _styleManager = styleManager;
    }

    /// <summary>
    /// The "entry-point" (for lack of a better term) in the evaluator class.<br/>
    /// Evaluates every kind of AstNode, with the exception of ranges,<br/>
    /// which have to be handled manually (i.e. during function evaluation).<br/>
    /// </summary>
    public CellValue Evaluate(Guid sheetId, AstNode expr)
        => expr switch
        {
            AstNode.Literal e => e.Value,
            AstNode.Unary   e => EvaluateUnary(sheetId, e),
            AstNode.Binary  e => EvaluateBinary(sheetId, e),

            // AstNode.Primary.Function e => EvaluateFunction(e),
            AstNode.Call e => EvaluateFunction(sheetId, e),
            AstNode.Primary.Text   e => new CellValue.Text(e.Value),
            AstNode.Primary.Date   e => new CellValue.Date(e.Value),
            AstNode.Primary.Number e => EvaluateNumber(e.Value),
            AstNode.Primary.Error  e => new CellValue.Error(e.Value),
            AstNode.Primary.Cell   e => GetCellValue(sheetId, e.Column, e.Row),
            AstNode.Primary.Range  e => new CellValue.Error("#ERROR!"),

            _ => throw new UnreachableException(expr.ToString())
        };
    
    private CellValue GetCellValue(Guid sheetId, int column, int row)
        => GetCellValue(sheetId, new CellKey(column, row));

    private CellValue GetCellValue(Guid sheetId, CellKey key)
        => _workbook.Sheets[sheetId].GetCell(key).Value;

    // TODO: Implement the rest of the function registry functions,
    // and test out the existing ones.
    private CellValue EvaluateFunction(Guid sheetId, AstNode.Call astCall)
        => astCall.Name switch
        {
            "SUM"     => EvaluateFunctionSum     (sheetId, astCall.Arguments),
            "AVERAGE" => EvaluateFunctionAverage (sheetId, astCall.Arguments),
            "MIN"     => EvaluateFunctionMin     (sheetId, astCall.Arguments),
            "MAX"     => EvaluateFunctionMax     (sheetId, astCall.Arguments),
            "COUNT"   => EvaluateFunctionCount   (sheetId, astCall.Arguments),
            "ROUND"   => EvaluateFunctionRound   (sheetId, astCall.Arguments),
            "ABS"     => EvaluateFunctionAbs     (sheetId, astCall.Arguments),
            _ => new CellValue.Error("#NAME?"),
        };

    /// <summary>
    /// Arithmetic rules as defined in section 3.5.3;<br/>
    /// TODO: Should follow LocaleSettings.
    /// </summary>
    private CellValue ConvertTextByArithmeticRules(string text)
    {
        // We're assuming that the number here is canonized (WHICH IT SHOULD BE).
        try
        {
            return new CellValue.Number
                (Convert.ToDouble(text));
        }
        catch (OverflowException)
        {
            return new CellValue.Error("#NUM!");
        }
        catch (FormatException) { }

        try
        {
            DateOnly firstDate =
                DateOnly.ParseExact
                    (text, "yyyy-MM-dd");
            return new CellValue.Date(firstDate);
        }
        catch (FormatException) { }

        return new CellValue.Error("#VALUE!");
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

    private CellValue EvaluateUnary(Guid sheetId, AstNode.Unary astUnary)
    {
        CellValue body = Evaluate(sheetId, astUnary.Body);
        UnaryAstNodeOperator unaryOperator = astUnary.Op;

        return unaryOperator switch
        {
            UnaryAstNodeOperator.Negate
                => AttemptConvertByNegate(body),

            UnaryAstNodeOperator.Plus
                => AttemptConvertByArithmetic(body),

            UnaryAstNodeOperator.Percentify
                => AttemptConvertByArithmetic(body),

            _ => throw new UnreachableException(astUnary.ToString())
        };
    }

    private CellValue AttemptConvertByNegate(CellValue body)
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
                => new CellValue.Error("#VALUE!"),

            CellValue.Error   v => v,

            _ => throw new UnreachableException(body.ToString())
        };

    private CellValue AttemptConvertByArithmetic(CellValue body)
        => body switch
        {
            CellValue.Empty v
                => new CellValue.Number(0),

            CellValue.Number v
                => v,

            CellValue.Decimal v
                => v,

            CellValue.Text v
                => ConvertTextByArithmeticRules(v.Value),

            CellValue.Bool v
                => new CellValue.Number(v.Value ? 1 : 0),

            CellValue.Date v
                => v,

            CellValue.Error v
                => v,

            _ => throw new UnreachableException(body.ToString())
        };

    private CellValue AttemptConvertEmptyByArithmetic(CellValue body)
        => body switch
        {
            CellValue.Empty v
                => new CellValue.Number(0),

            _ => body
        };

    private CellValue AttemptConvertBoolByArithmetic(CellValue body)
        => body switch
        {
            CellValue.Bool v
                => new CellValue.Number(v.Value ? 1 : 0),

            _ => body
        };

    private CellValue AttemptConvertTextByArithmetic(CellValue body)
        => body switch
        {
            CellValue.Text v
                => ConvertTextByArithmeticRules(v.Value),

            _ => body
        };

    private CellValue EvaluateBinary(Guid sheetId, AstNode.Binary astBinary)
    {
        CellValue left  = Evaluate(sheetId, astBinary.Left );
        CellValue right = Evaluate(sheetId, astBinary.Right);
        BinaryAstNodeOperator binaryOperator = astBinary.Op;

        switch (binaryOperator)
        {
            case BinaryAstNodeOperator.Add:
                return EvaluateBinaryArithmeticWithDateHandling
                    (left, right , (l, r) => l + r , (l, r) => l + r);

            case BinaryAstNodeOperator.Subtract:
                return EvaluateBinaryArithmeticWithDateHandling
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
                    (left, right , (o) => o is Ordering.EQ );

            case BinaryAstNodeOperator.NotEqual:
                return EvaluateBinaryComparison
                    (left, right , (o) => o is not Ordering.EQ );

            case BinaryAstNodeOperator.LessThan:
                return EvaluateBinaryComparison
                    (left, right , (o) => o is Ordering.LT);

            case BinaryAstNodeOperator.GreaterThan:
                return EvaluateBinaryComparison
                    (left, right , (o) => o is Ordering.GT);

            case BinaryAstNodeOperator.LessThanEqual:
                return EvaluateBinaryComparison
                    (left, right , (o) => o is Ordering.LT or Ordering.EQ);

            case BinaryAstNodeOperator.GreaterThanEqual:
                return EvaluateBinaryComparison
                    (left, right , (o) => o is Ordering.GT or Ordering.EQ);

            // TODO: Implement Math.Pow() for decimals,
            // since there are no built-in functions for that that I've found.
            case BinaryAstNodeOperator.Raise:
                return EvaluateBinaryArithmetic
                    (left, right , (l, r) => Math.Pow(l, r) , (l, r) => (decimal) Math.Pow((double) l, (double) r));
        }

        throw new UnreachableException(binaryOperator.ToString());
    }

    private CellValue EvaluateBinaryArithmeticWithDateHandling
        (CellValue left, CellValue right
        , Func<double , double , double > doubleFunc
        , Func<decimal, decimal, decimal> decimalFunc
        )
    {
        CellValue result;
        if (ArithmeticIsDate (left , out result)) { left = result; }
        if (ArithmeticIsDate (right, out result)) { right = result; }

        return EvaluateBinaryArithmetic(left, right, doubleFunc, decimalFunc);
    }

    /// <summary>
    /// Since there are two types of numbers in this language,<br/>
    /// Numbers and Decimals, two functions are needed for number evaluation.
    /// </summary>
    private CellValue EvaluateBinaryArithmetic
        (CellValue left, CellValue right
        , Func<double , double , double > doubleFunc
        , Func<decimal, decimal, decimal> decimalFunc
        )
    {
        // This is quite bad in fact.
        CellValue result;
        if (ArithmeticIsEmpty(left , out result)) { left = result; }
        if (ArithmeticIsEmpty(right, out result)) { right = result; }
        if (ArithmeticIsText (left , out result)) { left = result; }
        if (ArithmeticIsText (right, out result)) { right = result; }
        // if (ArithmeticIsDate (left , out result)) { left = result; }
        // if (ArithmeticIsDate (right, out result)) { right = result; }
        if (ArithmeticIsBool (left , out result)) { left = result; }
        if (ArithmeticIsBool (right, out result)) { right = result; }
        if (ArithmeticIsError(left , out result)) { left = result; }
        if (ArithmeticIsError(right, out result)) { right = result; }

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

    private bool ArithmeticIsDate(CellValue input, out CellValue output)
    {
        if (input is CellValue.Date date)
        {
            output = new CellValue.Number(date.Value.DayNumber);
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
            (CellValue.Text l, CellValue.Text r)
                => CompareText(l.Value, r.Value),

            (CellValue.Date l, CellValue.Date r)
                => CompareDate(l.Value, r.Value),

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

    private Ordering CompareDate
        (DateOnly left, DateOnly right)
    {
        if (left.Year < right.Year) return Ordering.LT;
        else if (left.Year > right.Year) return Ordering.GT;

        if (left.Month < right.Month) return Ordering.LT;
        else if (left.Month > right.Month) return Ordering.GT;

        if (left.Day < right.Day) return Ordering.LT;
        else if (left.Day > right.Day) return Ordering.GT;

        return Ordering.EQ;
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

    // ================================================================================
    // NOT_SPECIFICATION: Function Registry has not been implemented as a seperate module.
    // ================================================================================

    private CellValue EvaluateFunctionSum(Guid sheetId, AstNode[] arguments)
    {
        decimal total = 0;
        bool decimalWasFound = false;

        foreach (var argument in arguments)
        {
            if (argument is AstNode.Primary.Range r)
            {
                var cellValues = EvaluateRange(sheetId, r);

                foreach (CellValue cellValue in cellValues)
                {
                    if (cellValue is CellValue.Error)
                    {
                        return cellValue;
                    }

                    AccumulateNumericValue(cellValue, ref total, ref decimalWasFound);
                }
                continue;
            }

            CellValue result = EvaluateArgument(sheetId, argument);

            if (result is CellValue.Error)
            {
                return result;
            }
            else if (result is not CellValue.Number and not CellValue.Decimal)
            {
                return new CellValue.Error("#VALUE!");
            }

            AccumulateNumericValue(result, ref total, ref decimalWasFound);
        }

        return decimalWasFound
            ? new CellValue.Decimal(total)
            : new CellValue.Number((double) total);
    }

    private CellValue EvaluateFunctionAverage(Guid sheetId, AstNode[] arguments)
    {
        int valuesFound = 0;
        decimal total = 0;
        bool decimalWasFound = false;

        foreach (var argument in arguments)
        {
            if (argument is AstNode.Primary.Range r)
            {
                var cellValues = EvaluateRange(sheetId, r);

                foreach (CellValue cellValue in cellValues)
                {
                    if (cellValue is CellValue.Error)
                    {
                        return cellValue;
                    }
                    else if (cellValue is CellValue.Number or CellValue.Decimal)
                    {
                        ++valuesFound;
                    }

                    AccumulateNumericValue(cellValue, ref total, ref decimalWasFound);
                }
                continue;
            }

            CellValue result = EvaluateArgument(sheetId, argument);

            if (result is CellValue.Error)
            {
                return result;
            }
            else if (result is not CellValue.Number and not CellValue.Decimal)
            {
                return new CellValue.Error("#VALUE!");
            }

            AccumulateNumericValue(result, ref total, ref decimalWasFound);

            ++valuesFound;
        }

        valuesFound = valuesFound != 0 ? valuesFound : 0;

        return decimalWasFound
            ? new CellValue.Decimal(total / valuesFound)
            : new CellValue.Number((double) total / valuesFound);
    }

    private void AccumulateNumericValue
        (CellValue value, ref decimal total, ref bool decimalWasFound)
    {
        switch (value)
        {
            case CellValue.Number v: total += (decimal) v.Value; break;
            case CellValue.Decimal v:
                total += v.Value;
                decimalWasFound = true;      
                break;
        }
    }

    private CellValue EvaluateFunctionMin(Guid sheetId, AstNode[] arguments)
        => EvaluateFunctionGetExtreme
            (sheetId, arguments, (o) => o is Ordering.LT);

    private CellValue EvaluateFunctionMax(Guid sheetId, AstNode[] arguments)
        => EvaluateFunctionGetExtreme
            (sheetId, arguments, (o) => o is Ordering.GT );

    private CellValue EvaluateFunctionGetExtreme
        (Guid sheetId, AstNode[] arguments, Func<Ordering, bool> predicate)
    {
        if (arguments.Length == 0) return new CellValue.Number(0);

        CellValue current = new CellValue.Empty();

        foreach (var argument in arguments)
        {
            if (argument is AstNode.Primary.Range r)
            {
                var cellValues = EvaluateRange(sheetId, r);

                foreach (CellValue cellValue in cellValues)
                {
                    if (!IsUsableByExtreme(cellValue))
                    {
                        continue;
                    }
                    else if (current is CellValue.Empty)
                    {
                        current = cellValue;
                        continue;
                    }

                    AssignNewExtremeIfPossible(cellValue, predicate, ref current);
                }

                continue;
            }

            CellValue result = EvaluateArgument(sheetId, argument);

            if (result is CellValue.Error)
            {
                return result;
            }
            else if (!IsUsableByExtreme(result))
            {
                return new CellValue.Error("#VALUE!");
            }
            else if (current is CellValue.Empty)
            {
                current = result;
                continue;
            }

            AssignNewExtremeIfPossible(result, predicate, ref current);
        }

        return current;
    }

    private bool IsUsableByExtreme(CellValue cellValue)
        => cellValue switch
        {
            CellValue.Number  => true,
            CellValue.Decimal => true,
            CellValue.Date    => true,
            _ => false
        };

    private void AssignNewExtremeIfPossible
        (CellValue potential, Func<Ordering, bool> predicate, ref CellValue current)
    {
        if (IsNumericValue(potential) && IsNumericValue(current)
            || potential is CellValue.Date && current is CellValue.Date)
        {
            Ordering? ordering = Compare(potential, current);
            if (ordering is not null && predicate(ordering.Value))
            {
                current = potential;
            }
        }
    }

    private CellValue EvaluateFunctionCount(Guid sheetId, AstNode[] arguments)
    {
        if (arguments.Length == 0) return new CellValue.Error("#ERROR!");

        int count = 0;

        foreach (var argument in arguments)
        {
            if (argument is AstNode.Primary.Range r)
            {
                var cellValues = EvaluateRange(sheetId, r);

                foreach (CellValue cellValue in cellValues)
                {
                    if (cellValue is CellValue.Error) return cellValue;

                    count += cellValue switch
                    {
                        CellValue.Number  => 1,
                        CellValue.Decimal => 1,
                        CellValue.Date    => 1,
                        _ => 0
                    };
                }
                continue;
            }

            count += EvaluateArgument(sheetId, argument) switch
            {
                CellValue.Number  => 1,
                CellValue.Decimal => 1,
                CellValue.Date    => 1,
                _ => 0
            };
        }

        return new CellValue.Number((double) count);
    }

    private CellValue EvaluateFunctionRound(Guid sheetId, AstNode[] arguments)
    {
        if (arguments.Length != 2) return new CellValue.Error("#ERROR!");

        decimal numberToRound = 0;
        int placesToRound = 0;
        bool isDecimalFound = false;

        CellValue number = Evaluate(sheetId, arguments[0]);
        switch (number)
        {
            case CellValue.Number   v: numberToRound = (decimal) v.Value; break;
            case CellValue.Decimal  v:
                numberToRound = v.Value;
                isDecimalFound = true;
                break;

            default: return new CellValue.Error("#VALUE!");
        };

        CellValue places = Evaluate(sheetId, arguments[0]);
        switch (places)
        {
            case CellValue.Number   v: placesToRound = (int)  double.Round(v.Value); break;
            case CellValue.Decimal  v: placesToRound = (int) decimal.Round(v.Value); break;

            default: return new CellValue.Error("#VALUE!");
        };

        return isDecimalFound
            ? new CellValue.Decimal(decimal.Round(numberToRound, placesToRound))
            : new CellValue.Number ((double) decimal.Round(numberToRound, placesToRound));
    }

    private CellValue EvaluateFunctionAbs(Guid sheetId, AstNode[] arguments)
    {
        if (arguments.Length != 1) return new CellValue.Error("#ERROR!");

        CellValue result = Evaluate(sheetId, arguments[0]);
        return result switch
        {
            CellValue.Number  v => v with { Value = v.Value < 0 ? (-1) * v.Value : v.Value },
            CellValue.Decimal v => v with { Value = v.Value < 0 ? (-1) * v.Value : v.Value },
            _ => result
        };
    }

    private CellValue EvaluateArgument(Guid sheetId, AstNode argument)
        => AttemptConvertByArithmetic(Evaluate(sheetId, argument));

    /// <summary>
    /// Yields the CellValue of every Cell in range.<br/>
    /// This function DOES NOT do arithmetic conversions.
    /// </summary>
    private IEnumerable<CellValue> EvaluateRange
        (Guid sheetId, AstNode.Primary.Range range)
    {
        CanonRange canonRange = RangeCanonizer.GetCanonRange(range);

        for (int i = canonRange.ColumnStart; i < canonRange.ColumnStart + canonRange.ColumnLength; ++i)
        {
            for (int j = canonRange.RowStart; j < canonRange.RowStart + canonRange.RowLength; ++j)
            {
                AstNode? node = _workbook.GetCell(sheetId, new CellKey(i, j)).Ast;
                if (node is null)
                {
                    throw new UnreachableException("node is null");
                }

                yield return Evaluate(sheetId, node);
            }
        }
    }

    private bool IsNumericValue(CellValue value)
        => value switch
        {
            CellValue.Number  v => true,
            CellValue.Decimal v => true,
            _ => false
        };

    private decimal? ExtractNumericValue(CellValue value)
        => value switch
        {
            CellValue.Number  v => (decimal) v.Value,
            CellValue.Decimal v => v.Value,
            _ => null
        };
}
