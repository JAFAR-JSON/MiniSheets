using System;

namespace MiniSheets;

public abstract record Token
{
    private Token() {}

    public sealed record Identifer(string identifer) : Token;
}
