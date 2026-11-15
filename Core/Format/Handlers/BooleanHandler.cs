using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;

namespace MiniSheets.Core.Format;

public class BooleanHandler : TypeHandler
{
    public Expr? Handle(string text)
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
}
