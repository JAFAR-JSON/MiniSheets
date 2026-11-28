using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;
using MiniSheets.Shared;
using System.Collections.Immutable;
using System.Globalization;

namespace MiniSheets.Core.Format;

// NOT_SPECIFICATION: This is a pretty big deviation from the specification,
// as instead of having TypeHandler classes, we instead have the ValueParser alone.
// This is because the there's just not enough logic inside any of the Handlers to
// warrant having them as seperate classes.

// TODO: Clean this whole class up and make sure it's correct,
// because I'm 99% sure that the here logic is cooked.
public class ValueParser
{
    private readonly Parser         _parser        ;
    private readonly LocaleSettings _localeSettings;
    private readonly StyleManager   _styleManager  ;

    public ValueParser
        (Parser parser, LocaleSettings localeSettings, StyleManager styleManager)
    {
        _parser = parser;
        _localeSettings = localeSettings;
        _styleManager = styleManager;
    }

    public AstNode Parse(Cell cell)
    {
        return Parse(CellFormat.General, cell);
    }

    public AstNode Parse(CellFormat format, Cell cell)
    {
        string text = cell.Raw;

        if (text.Length >= 1 && text[0] == '\'')
        {
            return new AstNode.Literal(new CellValue.Text(text.Substring(1)));
        }
        else if (text.Length >= 1 && text[0] == '=')
        {
            if (format == CellFormat.Text)
            {
                return new AstNode.Literal(new CellValue.Text(text));
            }

            return _parser.ParseText(text.Substring(1));
        }

        return format switch
        {
            CellFormat.General => HandleBoolean(cell)
                ?? HandleNumber(cell)
                ?? HandleCurrency(cell)
                ?? HandleDate(cell)
                ?? new AstNode.Literal(new CellValue.Text(text)),

            CellFormat.Text => new AstNode.Literal(new CellValue.Text(text)),

            CellFormat.Number   => HandleNumber  (cell) ?? ReturnError(),
            CellFormat.Currency => HandleCurrency(cell) ?? ReturnError(),
            CellFormat.Date     => HandleDate    (cell) ?? ReturnError(),
            _ => ReturnError()
        };
    }

    private AstNode ReturnError()
        => new AstNode.Literal(new CellValue.Error("#VALUE!"));

    public AstNode? HandleBoolean(Cell cell)
    {
        string text = cell.Raw;
        string convertedText = text.ToUpper();

        if (convertedText.Equals("TRUE"))
        {
            return new AstNode.Literal(new CellValue.Bool(true));
        }
        else if (convertedText.Equals("FALSE"))
        {
            return new AstNode.Literal(new CellValue.Bool(false));
        }
        else
        {
            return null;
        }
    }

    public AstNode? HandleNumber(Cell cell)
    {
        string text = cell.Raw;
        decimal? decimalOrNull = AttemptParseDecimal(text);
        return decimalOrNull switch
        {
            null => null,
            decimal d =>
                new AstNode.Literal(new CellValue.Number((double) d))
        };
    }

    // TODO: Handle the case where if a Cell's style is General,
    // then set it to Currency with 2 decimal places.
    public AstNode? HandleCurrency(Cell cell)
    {
        string text = cell.Raw;
        var currencyStrings =
            ImmutableArray.Create( "€", "$", "£", "EUR", "USD", "GBP" );

        CurrencyType? currencyType = null;
        string? currencyString = null;

        text = text.Trim();

        currencyString = currencyStrings.FirstOrDefault(c => text.StartsWith(c));
        if (currencyString is not null)
        {
            text = text.Substring(currencyString.Length, text.Length - 1);
            currencyType = ConvertStringToCurrency(currencyString);
        }

        currencyString = currencyStrings.FirstOrDefault(c => text.EndsWith(c));
        if (currencyString is not null)
        {
            if (currencyType is not null)
            {
                return null;
            }

            text = text.Substring(0, text.Length - currencyString.Length - 1);
            currencyType = ConvertStringToCurrency(currencyString);
        }

        text = text.Trim();

        if (text.Length == 0)
            return null;

        decimal? decimalOrNull = AttemptParseDecimal(text);
        return decimalOrNull switch
        {
            null => null,
            decimal d =>
                new AstNode.Literal(new CellValue.Decimal(d))
        };
    }

    private static CurrencyType? ConvertStringToCurrency(string currencyStr)
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

    private decimal? AttemptParseDecimal(string text)
    {
        string? canonizedText = NumberCanonizer.Canonize(text);
        if (canonizedText is null)
        {
            return null;
        }

        // I don't like this thing :/
        var nfi = new NumberFormatInfo();
        nfi.NumberDecimalSeparator = _localeSettings.DecimalSeperator  ;
        nfi.NumberGroupSeparator   = _localeSettings.ThousandsSeperator;

        if (decimal.TryParse(canonizedText, nfi, out decimal convertedDecimal))
        {
            return convertedDecimal;
        }
        else
        {
            return null;
        }
    }

    public AstNode? HandleDate(Cell cell)
    {
        string text = cell.Raw.Trim();

        try
        {
            string dateFormat = _localeSettings.ShortDate;

            DateOnly firstDate =
                DateOnly.ParseExact(text, dateFormat);
            return new AstNode.Literal(new CellValue.Date(firstDate));
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
