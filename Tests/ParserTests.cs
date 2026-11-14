using MiniSheets.Core;

namespace MiniSheets.Tests;

public class ParserTests
{
    [Fact]
    public void Parser_ComplexArithmeticFormula_Correct()
    {
        Parser parser = new Parser();
        Token[] tokens =
        {
            new Token.Number("2"),
            new Token.Star(),
            new Token.Number("3"),
            new Token.Plus(),
            new Token.Number("5"),
            new Token.Percent(),
            new Token.Slash(),
            new Token.Number("5"),
        };
        Expr parsedTokens = parser.ParseTokens(tokens);
        Expr testExpr =
            new Expr.Binary
            (
                BinaryExprOperator.Add,
                new Expr.Binary
                (
                    BinaryExprOperator.Multiply,
                    new Expr.Primary.Number("2"),
                    new Expr.Primary.Number("3")
                ),
                new Expr.Binary
                (
                    BinaryExprOperator.Divide,
                    new Expr.Unary
                    (
                        UnaryExprOperator.Percentify,
                        new Expr.Primary.Number("5")
                    ),
                    new Expr.Primary.Number("5")
                )
            );

        Assert.True(parsedTokens.Equals(testExpr), "String values are not equal.");
    }
}
