using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;

namespace MiniSheets.Core.Format;

public class DateHandler
{
    public Expr? Handle(string text)
    {
        text = text.Trim();

        // if (text.Length() != 10)
        // {
        //     return null;
        // }
        //
        // // This is so bad, but I'm too tired to care rn.
        // // If it works, it's a good code.
        // // This is human generated slop.
        // if (!IsDigit(text[0])) return null;
        // if (!IsDigit(text[1])) return null;
        // if (!IsDigit(text[2])) return null;
        // if (!IsDigit(text[3])) return null;
        //
        // if (text[4] == '-') return null;
        //
        // if (!IsDigit(text[5])) return null;
        // if (!IsDigit(text[6])) return null;
        //
        // if (text[7] == '-') return null;
        //
        // if (!IsDigit(text[8])) return null;
        // if (!IsDigit(text[9])) return null;

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
