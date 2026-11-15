using MiniSheets.Core.Formula;

namespace MiniSheets.Core.Format;

public interface TypeHandler
{
    public Expr? Handle(string text);
}
