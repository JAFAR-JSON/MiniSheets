using MiniSheets.Core;

namespace MiniSheets.Tests;

public class LexerTests
{
    [Fact]
    public void Lexer_InputIsEmptyString_ReturnsEmptyTokenList()
    {
        Lexer lexer = new Lexer();
        Token[] tokenizedText = lexer.TokenizeText("");
        Token[] testTokens = { };

        Assert.True(tokenizedText.SequenceEqual(testTokens), "Empty string should return empty token list.");
    }

    [Fact]
    public void Lexer_ComparingTwoIdenticalStrings_Equals()
    {
        Lexer lexer = new Lexer();
        Token[] tokenizedText = lexer.TokenizeText("\"Hello\"");
        Token[] testTokens = { new Token.Str("Hello") };

        Assert.True(tokenizedText.SequenceEqual(testTokens), "String values are not equal.");
    }

    [Fact]
    public void Lexer_ComplexArithmeticOperation_ReturnsCorrectList()
    {
        Lexer lexer = new Lexer();
        Token[] tokenizedText = lexer.TokenizeText("2*3+(5%/5)");
        Token[] testTokens =
        {
            new Token.Number("2"),
            new Token.Star(),
            new Token.Number("3"),
            new Token.Plus(),
            new Token.LeftParenthesis(),
            new Token.Number("5"),
            new Token.Percent(),
            new Token.Slash(),
            new Token.Number("5"),
            new Token.RightParenthesis()
        };

        Assert.True(tokenizedText.SequenceEqual(testTokens), "Tokenizing is incorrect.");
    }
}
