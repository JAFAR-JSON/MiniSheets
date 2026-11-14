namespace MiniSheets;

public abstract record Token
{
    private Token() {}

    // Literals
    public sealed record Identifier(string identifier) : Token;
    public sealed record Str(string str) : Token;
    public sealed record Number(string number) : Token;
    public sealed record Cell
        (int column, bool isColumnFixed, int row, bool isRowFixed) : Token;

    // Keywords
    public sealed record True       () : Token;
    public sealed record False      () : Token;
    public sealed record ErrorError () : Token;
    public sealed record ErrorName  () : Token;
    public sealed record ErrorRef   () : Token;
    public sealed record ErrorValue () : Token;
    public sealed record ErrorDiv0  () : Token;
    public sealed record ErrorNum   () : Token;
    public sealed record ErrorCirc  () : Token;
    
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
    public sealed record LessEqualThan   () : Token;
    public sealed record GreaterEqualThan() : Token;
}
