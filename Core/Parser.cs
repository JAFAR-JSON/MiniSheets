namespace MiniSheets.Core;

// https://craftinginterpreters.com/parsing-expressions.html
public class Parser
{
    private Token[] _tokens = {};
    private int _current = 0;

    public Parser() { }
    
    public Expr ParseTokens(Token[] tokens)
    {
        _tokens = tokens;
        _current = 0;

        throw new ParsingException("Not implemented yet");
    }

    private bool IsAtEnd()
    {
        return _tokens.Length <= _current;
    }

    private Token PeekToken()
    {
        if (IsAtEnd())
        {
            throw new ParsingException("No more tokens left");
        }

        return _tokens[_current];
    }

    private void SkipToken()
    {
        _current++;
    }

    private Token ConsumeToken()
    {
        Token token = PeekToken();

        SkipToken();

        return token;
    }

    private bool AcceptToken(Token acceptedToken)
    {
        try
        {
            Token token = PeekToken();
            if (token != acceptedToken) // TODO: I don't think this works as I expect it to.
            {
                return false;
            }

            ConsumeToken();

            return true;
        }
        catch (ParsingException)
        {
            return false;
        }
    }

    private Expr ParseExprPrimary()
    {
        int start = _current;
        try
        {
            Token token;

            token = ConsumeToken();
            switch (token)
            {
                case Token.Identifier t:
                    return new Expr.Literal.Identifier(t.identifier);

                case Token.Str t:
                    return new Expr.Literal.Str(t.str);

                case Token.Number t:
                    return new Expr.Literal.Number(t.number);

                case Token.Cell t:
                    return new Expr.Literal.Cell
                        (t.column, t.isColumnFixed, t.row, t.isRowFixed);

                case Token.LeftParenthesis _:
                {
                    Expr innerExpr = ParseExpr();

                    Token rightParenthesis = ConsumeToken();
                    if (!(rightParenthesis is Token.RightParenthesis _))
                    {
                        throw new ParsingException("No accompanying right parenthesis found.");
                    }
                    return innerExpr;
                }

                default:
                    throw new ParsingException("Failed to parse primary exception.");
            }
        }
        catch (ParsingException)
        {
            _current = start;
            throw;
        }
    }

    private Expr ParseExpr()
    {
        throw new ParsingException("Not implemented yet");
    }

    /// <summary>
    /// Same as <c>AcceptChar</c>, but used when expecting a character
    /// </summary>
    private bool ExpectToken(Token expectedToken)
        => AcceptToken(expectedToken);
}
