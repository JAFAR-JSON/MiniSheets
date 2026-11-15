using MiniSheets.Shared;
using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;

namespace MiniSheets.Core.Format;

public class CurrencyHandler
{
    private static string[] _currencyStrings = new string[]
        { "€", "$", "£", "EUR", "USD", "GBP" };

    public Expr? Handle(string text)
    {
        CurrencyType? currencyType = null;
        string? currencyString = null;

        text = text.Trim();

        currencyString = _currencyStrings.FirstOrDefault(c => text.StartsWith(c));
        if (currencyString is not null)
        {
            text = text.Substring(currencyString.Length, text.Length - 1);
            currencyType = ConvertString(currencyString);
        }

        currencyString = _currencyStrings.FirstOrDefault(c => text.EndsWith(c));
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
}
