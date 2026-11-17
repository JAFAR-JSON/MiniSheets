using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;
using MiniSheets.Shared;
using System.Collections.Immutable;

namespace MiniSheets.Core.Format;

// NOT_SPECIFICATION: This is a pretty big deviation from the specification,
// but instead if having TypeHandler classes, we instead have the ValueParser alone.
// This is because the there's just not enough logic inside any of the Handlers to
// warrant having them as seperate classes.

// TODO: Clean this whole class up and make sure it's correct,
// because I'm 99% sure that the here logic is cooked.
public class ValueParser
{
    private Parser _parser;

    public ValueParser(Parser parser)
    {
        _parser = parser;
    }

    public Expr? ParseValue(string text)
    {
        return ParseValue(CellFormat.General, text);
    }

    public Expr? ParseValue(CellFormat format, string text)
    {
        if (text.Length >= 1 && text[0] == '\'')
        {
            return new Expr.Literal(new CellValue.Text(text.Substring(1)));
        }
        else if (text.Length >= 1 && text[0] == '=')
        {
            if (format == CellFormat.Text)
            {
                return new Expr.Literal(new CellValue.Text(text));
            }

            return _parser.ParseText(text.Substring(1));
        }

        // switch (format)
        // {
        //     case CellFormat.General:
        //         return HandleBoolean(text)
        //             ?? HandleNumber(text)
        //             ?? HandleCurrency(text)
        //             ?? HandleDate(text)
        //             ?? new Expr.Literal(new CellValue.Text(text));
        //     // {
        //     //     Expr? expr = null;
        //     //
        //     //     expr = HandleBoolean(text);
        //     //     if (expr is not null) return expr;
        //     //
        //     //     expr = HandleNumber(text);
        //     //     if (expr is not null) return expr;
        //     //
        //     //     expr = HandleCurrency(text); 
        //     //     if (expr is not null) return expr;
        //     //
        //     //     expr = HandleDate(text);
        //     //     if (expr is not null) return expr;
        //     //
        //     //     return new Expr.Literal(new CellValue.Text(text));
        //     // }
        //
        //     case CellFormat.Text:
        //         return new Expr.Literal(new CellValue.Text(text));
        //
        //     case CellFormat.Number   : return HandleNumber(text);
        //
        //     case CellFormat.Currency: return HandleC
        //     case CellFormat.Date:
        //
        //     default:
        //         expr = new Expr.Literal(new CellValue.Empty());
        // }
        //
        // return expr;

        return format switch
        {
            CellFormat.General => HandleBoolean(text)
                ?? HandleNumber(text)
                ?? HandleCurrency(text)
                ?? HandleDate(text)
                ?? new Expr.Literal(new CellValue.Text(text)),

            CellFormat.Text => new Expr.Literal(new CellValue.Text(text)),

            CellFormat.Number   => HandleNumber  (text),
            CellFormat.Currency => HandleCurrency(text),
            CellFormat.Date     => HandleDate    (text),
            _ => null
        };
    }

    public Expr? HandleBoolean(string text)
    {
        string convertedText = text.ToUpper();

        if (convertedText.Equals("TRUE"))
        {
            return new Expr.Literal(new CellValue.Bool(true));
        }
        else if (convertedText.Equals("FALSE"))
        {
            return new Expr.Literal(new CellValue.Bool(false));
        }
        else
        {
            return null;
        }
    }

    public Expr? HandleNumber(string text)
    {
        string? canonizedText = NumberCanonizer.Canonize(text);
        if (canonizedText is null)
        {
            return null;
        }

        if (decimal.TryParse(canonizedText, out decimal convertedDecimal))
        {
            return new Expr.Literal(new CellValue.Number((double) convertedDecimal));
        }
        else
        {
            return null;
        }
    }

    // TODO: Handle the case where if a Cell's style is General,
    // then set it to Currency with 2 decimal places.
    public Expr? HandleCurrency(string text)
    {
        var currencyStrings =
            ImmutableArray.Create( "€", "$", "£", "EUR", "USD", "GBP" );

        CurrencyType? currencyType = null;
        string? currencyString = null;

        text = text.Trim();

        currencyString = currencyStrings.FirstOrDefault(c => text.StartsWith(c));
        if (currencyString is not null)
        {
            text = text.Substring(currencyString.Length, text.Length - 1);
            currencyType = ConvertString(currencyString);
        }

        currencyString = currencyStrings.FirstOrDefault(c => text.EndsWith(c));
        if (currencyString is not null)
        {
            if (currencyType is not null)
            {
                return null;
            }

            text = text.Substring(0, text.Length - currencyString.Length - 1);
            currencyType = ConvertString(currencyString);
        }

        text = text.Trim();

        if (text.Length == 0)
            return null;

        return HandleDecimal(text);
    }

    private static CurrencyType? ConvertString(string currencyStr)
        => currencyStr switch
        {
            "€"   => CurrencyType.EUR,
            "$"   => CurrencyType.USD,
            "£"   => CurrencyType.GBP,
            "EUR" => CurrencyType.EUR,
            "USD" => CurrencyType.USD,
            "GBP" => CurrencyType.GBP,
            _ => null
        };

    private Expr? HandleDecimal(string text)
    {
        string? canonizedText = NumberCanonizer.Canonize(text);
        if (canonizedText is null)
        {
            return null;
        }

        if (decimal.TryParse(canonizedText, out decimal convertedDecimal))
        {
            return new Expr.Literal(new CellValue.Decimal(convertedDecimal));
        }
        else
        {
            return null;
        }
    }

    public Expr? HandleDate(string text)
    {
        text = text.Trim();

        try
        {
            DateOnly firstDate =
                DateOnly.ParseExact
                    (text, "yyyy-MM-dd");
            return new Expr.Literal(new CellValue.Date(firstDate));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private bool IsDigit(char character)
        => character switch
        {
            '0' => true,
            '1' => true,
            '2' => true,
            '3' => true,
            '4' => true,
            '5' => true,
            '6' => true,
            '7' => true,
            '8' => true,
            '9' => true,
            _ => false,
        };
}
