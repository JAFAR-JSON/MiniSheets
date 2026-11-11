using System;

namespace MiniSheets;

public abstract record Token
{
    private Token() {}

    public sealed record Identifier(string identifer) : Token;
    public sealed record Str(string str) : Token;
    public sealed record Number(string number) : Token;
    public sealed record Cell
        (string column, bool is_column_fixed, int row, bool is_row_fixed) : Token;
}
