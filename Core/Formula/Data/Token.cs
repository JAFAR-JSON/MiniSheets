namespace MiniSheets.Core.Formula;

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
    public abstract record Error : Token
    {
        public sealed record Generic () : Error;
        public sealed record Name    () : Error;
        public sealed record Ref     () : Error;
        public sealed record Value   () : Error;
        public sealed record Div0    () : Error;
        public sealed record Num     () : Error;
        public sealed record Circ    () : Error;
    }
    public sealed record True       () : Token;
    public sealed record False      () : Token;
    
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
