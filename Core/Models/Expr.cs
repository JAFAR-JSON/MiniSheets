namespace MiniSheets.Core;

public abstract record Expr
{
    private Expr() {}

    // public sealed record Literal() : Expr;
    public abstract record Primary() : Expr
    {
        public sealed record Identifier(string identifier) : Primary;
        public sealed record Str(string str) : Primary;
        public sealed record Number(string number) : Primary;
        public sealed record Cell
            (int column, bool isColumnFixed, int row, bool isRowFixed) : Primary;
    }

    public sealed record Unary(UnaryExprOperator op, Expr body) : Expr;
    public sealed record Binary(BinaryExprOperator op, Expr left, Expr right) : Expr;

    // // Literals
    // public sealed record Identifier(string identifier) : ASTNode;
    // public sealed record Str(string str) : ASTNode;
    // public sealed record Number(string number) : ASTNode;
    // public sealed record Cell
    //     (int column, bool is_column_fixed, int row, bool is_row_fixed) : ASTNode;
}
