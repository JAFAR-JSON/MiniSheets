using MiniSheets.Core.Model;

namespace MiniSheets.Core.Formula;

public abstract record AstNode
{
    private AstNode() {}

    public sealed record Literal(CellValue Value) : AstNode;
    public abstract record Primary() : AstNode
    {
        // public sealed record Identifier(string Value) : Primary;

        public sealed record Text(string Value) : Primary;
        public sealed record Date(DateOnly Value) : Primary;
        public sealed record Number(string Value) : Primary;
        public sealed record Error(string Value) : Primary;
        public sealed record Cell
            (int Column, bool isColumnFixed, int Row, bool isRowFixed) : Primary;
    }

    public sealed record Call(string Name, AstNode[] Arguments) : AstNode;
    public sealed record Unary(UnaryAstNodeOperator Op, AstNode Body) : AstNode;
    public sealed record Binary(BinaryAstNodeOperator Op, AstNode Left, AstNode Right) : AstNode;

    // // Literals
    // public sealed record Identifier(string identifier) : ASTNode;
    // public sealed record Str(string str) : ASTNode;
    // public sealed record Number(string number) : ASTNode;
    // public sealed record Cell
    //     (int column, bool is_column_fixed, int row, bool is_row_fixed) : ASTNode;
}
