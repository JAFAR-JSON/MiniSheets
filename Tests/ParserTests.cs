using MiniSheets.Core.Formula;

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
        AstNode parsedTokens = parser.ParseTokens(tokens);
        AstNode testAstNode =
            new AstNode.Binary
            (
                BinaryAstNodeOperator.Add,
                new AstNode.Binary
                (
                    BinaryAstNodeOperator.Multiply,
                    new AstNode.Primary.Number("2"),
                    new AstNode.Primary.Number("3")
                ),
                new AstNode.Binary
                (
                    BinaryAstNodeOperator.Divide,
                    new AstNode.Unary
                    (
                        UnaryAstNodeOperator.Percentify,
                        new AstNode.Primary.Number("5")
                    ),
                    new AstNode.Primary.Number("5")
                )
            );

        Assert.True(parsedTokens.Equals(testAstNode), "String values are not equal.");
    }
}
