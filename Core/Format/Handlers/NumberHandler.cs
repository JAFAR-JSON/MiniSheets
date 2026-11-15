using MiniSheets.Shared;
using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;

namespace MiniSheets.Core.Format;

public class NumberHandler : TypeHandler
{
    public Expr? Handle(string text)
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
}
