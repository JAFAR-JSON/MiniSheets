namespace MiniSheets.Core;

// https://craftinginterpreters.com/parsing-expressions.html
public class Parser
{
    private Token[] tokens = {};
    private int current = 0;

    public Parser() { }
    
    public Expr ParseTokens(Token[] tokens)
    {
        tokens = tokens;
        current = 0;

        return null;
    }

    private bool IsAtEnd()
    {
        return tokens.Length <= current;
    }

    private Token? PeekToken()
    {
        if (IsAtEnd())
        {
            return null;
        }

        return tokens[current];
    }

    private void SkipToken()
    {
        current++;
    }

    private Token? ConsumeToken()
    {
        Token? token = PeekToken();

        if (token == null)
        {
            return null;
        }

        SkipToken();

        return token;
    }

    private bool AcceptToken(Token acceptedToken)
    {
        Token? token = PeekToken();

        if (token == null || token != acceptedToken)
        {
            return false;
        }

        ConsumeToken();

        return true;
    }

    private Expr? ParseExprPrimary()
    {
        int start = current;
        Token? token = null;

        token = ConsumeToken();
        if (token == null)
        {
            current = start;
            return null;
        }

        return token switch
        {
            Token.Identifier identifier => new Expr.Literal.Identifier(identifier.identifier),
            Token.Str str               => new Expr.Literal.Str(str.str),
            Token.Number number         => new Expr.Literal.Number(number.number),
            Token.Cell cell             => new Expr.Literal.Cell
                (cell.column, cell.isColumnFixed, cell.row, cell.isRowFixed),
            // Token.LeftParenthesis _     => { return null }, 
            _ => null
        };
    }

    private Expr? ParseExpr()
    {
        return null;
    }

    /// <summary>
    /// Same as <c>AcceptChar</c>, but used when expecting a character
    /// </summary>
    private bool ExpectToken(Token expectedToken)
        => AcceptToken(expectedToken);
}
