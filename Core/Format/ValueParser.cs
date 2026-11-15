using MiniSheets.Core.Formula;
using MiniSheets.Core.Model;

namespace MiniSheets.Core.Format;

public class ValueParser
{
    private Parser _parser;
    private BooleanHandler _booleanHandler  = new();
    private BooleanHandler _numberHandler   = new();
    private BooleanHandler _currencyHandler = new();
    private DateHandler    _dateHandler     = new();

    public ValueParser(Parser parser)
    {
        _parser = parser;
    }

    public Expr ParseValue(string text)
    {
        return ParseValue(CellFormat.General, text);
    }

    public Expr ParseValue(CellFormat format, string text)
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
        else
        {
            switch (format)
            {
                case CellFormat.General:
                    break;
                case CellFormat.Text:
                    break;
                case CellFormat.Number:
                    break;
                case CellFormat.Currency:
                    break;
                case CellFormat.Date:
                    break;

                default:
                    return new Expr.Literal(new CellValue.Empty());
            }

            return null;
        }
    }
}
