using MiniSheets.Core.Model;

namespace MiniSheets.Core.Formula;

public abstract record Expr
{
    private Expr() {}

    public sealed record Literal(CellValue Value) : Expr;
    public abstract record Primary() : Expr
    {
        public sealed record Identifier(string Value) : Primary;
        public sealed record Text(string Value) : Primary;
        public sealed record Number(string Value) : Primary;
        public sealed record Error(string Value) : Primary;
        public sealed record Cell
            (int Column, bool isColumnFixed, int Row, bool isRowFixed) : Primary;
    }

    public sealed record Unary(UnaryExprOperator Op, Expr Body) : Expr;
    public sealed record Binary(BinaryExprOperator Op, Expr Left, Expr Right) : Expr;

    // // Literals
    // public sealed record Identifier(string identifier) : ASTNode;
    // public sealed record Str(string str) : ASTNode;
    // public sealed record Number(string number) : ASTNode;
    // public sealed record Cell
    //     (int column, bool is_column_fixed, int row, bool is_row_fixed) : ASTNode;
}
