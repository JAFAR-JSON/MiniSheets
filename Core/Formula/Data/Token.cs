namespace MiniSheets.Core.Formula;

public abstract record Token
{
    private Token() {}

    // Literals
    public sealed record Identifier(string Value) : Token;
    public sealed record Str(string Value) : Token;
    public sealed record Date(DateOnly Value) : Token;
    public sealed record Number(string Value) : Token;
    public sealed record Cell
        (int Column, bool IsColumnFixed, int Row, bool IsRowFixed) : Token;

    // Keywords
    public sealed record Error (string Value) : Token;
    public sealed record True  () : Token;
    public sealed record False () : Token;
    
    // Single character symbols
    public sealed record Percent          () : Token;
    public sealed record Minus            () : Token;
    public sealed record Plus             () : Token;
    public sealed record Caret            () : Token;
    public sealed record Star             () : Token;
    public sealed record Slash            () : Token;
    public sealed record Ampersand        () : Token;
    public sealed record Equal            () : Token;
    public sealed record LessThan         () : Token;
    public sealed record GreaterThan      () : Token;
    public sealed record Colon            () : Token;
    public sealed record LeftParenthesis  () : Token;
    public sealed record RightParenthesis () : Token;
    public sealed record Comma            () : Token;

    // Two or more character symbols
    public sealed record NotEqual        () : Token;
    public sealed record LessThanEqual   () : Token;
    public sealed record GreaterThanEqual() : Token;
}
