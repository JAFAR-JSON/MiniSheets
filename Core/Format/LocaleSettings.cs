using System.Diagnostics;
using MiniSheets.Core.Model;

namespace MiniSheets.Core.Format;

// seperators, date, currency
public class LocaleSettings
{
    public LocaleType CurrentLocale { get; set; }

    public LocaleSettings(LocaleType currentLocale)
    {
        CurrentLocale = currentLocale;
    }

    public string DecimalSeperator
    {
        get => CurrentLocale switch
        {
            LocaleType.EN_US => ".",
            LocaleType.LT_LT => ",",
            _ => throw new UnreachableException()
        };
    }

    // TODO: Figure this out, since we can have only one seperator
    // (we also need '\u00A0' for Lithuanian).
    public string ThousandsSeperator
    {
        get => CurrentLocale switch
        {
            LocaleType.EN_US => ",",
            LocaleType.LT_LT => " ",
            _ => throw new UnreachableException()
        };
    }

    public string ArgumentSeperator
    {
        get => CurrentLocale switch
        {
            LocaleType.EN_US => ",",
            LocaleType.LT_LT => ";",
            _ => throw new UnreachableException()
        };
    }

    public string ShortDate
    {
        get => CurrentLocale switch
        {
            LocaleType.EN_US => "M/d/yyyy",
            LocaleType.LT_LT => "yyyy-MM-dd",
            _ => throw new UnreachableException()
        };
    }

    public CurrencyType DefaultCurrency
    {
        get => CurrentLocale switch
        {
            LocaleType.EN_US => CurrencyType.USD,
            LocaleType.LT_LT => CurrencyType.EUR,
            _ => throw new UnreachableException()
        };
    }
}
