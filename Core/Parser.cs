using System.Diagnostics;

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

        return ParseExpr();
        // throw new ParsingException("Not implemented yet");
    }

    private bool IsAtEnd()
    {
        return _tokens.Length <= _current;
    }

    private Token? PeekToken()
    {
        if (IsAtEnd())
        {
            return null;
        }

        return _tokens[_current];
    }

    private void SkipToken()
    {
        _current++;
    }

    private Token ConsumeToken()
    {
        Token? token = PeekToken();
        if (token is null)
        {
            throw new ParsingException("No more tokens left");
        }

        SkipToken();

        return token;
    }

    private bool AcceptToken(Token acceptedToken)
    {
        try
        {
            Token? token = PeekToken();
            if (token is null || token != acceptedToken) // TODO: I don't think this works as I expect it to.
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

    private bool MatchToken(params Type[] tokenTypes)
    {
        Token? token = PeekToken();
        if (token is null)
        {
            return false;
        }

        return tokenTypes.Any(type => type == token.GetType());
    }

    private Token PreviousToken()
    {
        if (_current <= 0)
        {
            throw new ParsingException("There are no previous tokens.");
        }

        return _tokens[_current - 1];
    }

    private Expr ParseExprPrimary()
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
                if (rightParenthesis is not Token.RightParenthesis _)
                {
                    throw new ParsingException("No accompanying right parenthesis found.");
                }
                return innerExpr;
            }

            default:
                throw new ParsingException("Failed to parse primary exception.");
        }
    }

    private Expr ParseExprPostfix()
    {
        Expr expr = ParseExprPrimary();

        while (true)
        {
            Token? token;
            UnaryExprOperator? unaryOperator;

            token = PeekToken();
            if (token is null)
            {
                break;
            }

            unaryOperator = token switch
            {
                Token.Percent => UnaryExprOperator.Percentify,
                _ => null
            };

            if (unaryOperator is null)
            {
                break;
            }

            expr = new Expr.Unary(unaryOperator.Value, expr);
        }

        return expr;
    }

    private Expr ParseExprUnary()
    {
        Token token;
        try
            { token = ConsumeToken();}
        catch (ParsingException)
            { return ParseExprPostfix(); }

        UnaryExprOperator? unaryOperator = token switch
        {
            Token.Minus => UnaryExprOperator.Negate,
            Token.Plus  => UnaryExprOperator.Plus  ,
            _ => null
        };
        if (unaryOperator is null)
        {
            throw new ParsingException("Couldn't recognize unary operator.");
        }

        return new Expr.Unary(unaryOperator.Value, ParseExprUnary());
    }

    private Expr ParseExprRaise()
    {
        Expr expr = ParseExprUnary();

        while (MatchToken(typeof(Token.Caret)))
        {
            BinaryExprOperator binaryOperator = PreviousToken() switch
            {
                Token.Caret => BinaryExprOperator.Raise,
                _ => throw new UnreachableException()
            };
            Expr right = ParseExprUnary();
            expr = new Expr.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private Expr ParseExprFactor()
    {
        Expr expr = ParseExprRaise();

        while (MatchToken(typeof(Token.Star), typeof(Token.Slash)))
        {
            BinaryExprOperator binaryOperator = PreviousToken() switch
            {
                Token.Star  => BinaryExprOperator.Multiply,
                Token.Slash => BinaryExprOperator.Divide  ,
                _ => throw new UnreachableException()
            };
            Expr right = ParseExprRaise();
            expr = new Expr.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private Expr ParseExprTerm()
    {
        Expr expr = ParseExprFactor();

        while (MatchToken(typeof(Token.Plus), typeof(Token.Minus)))
        {
            BinaryExprOperator binaryOperator = PreviousToken() switch
            {
                Token.Plus  => BinaryExprOperator.Add     ,
                Token.Minus => BinaryExprOperator.Subtract,
                _ => throw new UnreachableException()
            };
            Expr right = ParseExprFactor();
            expr = new Expr.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private Expr ParseExprJoin()
    {
        Expr expr = ParseExprTerm();

        while (MatchToken(typeof(Token.Ampersand)))
        {
            BinaryExprOperator binaryOperator = PreviousToken() switch
            {
                Token.Ampersand => BinaryExprOperator.Join,
                _ => throw new UnreachableException()
            };
            Expr right = ParseExprTerm();
            expr = new Expr.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private Expr ParseExprComparison()
    {
        Expr expr = ParseExprJoin();

        while (
            MatchToken (
                typeof(Token.Equal),
                typeof(Token.NotEqual),
                typeof(Token.LessThan),
                typeof(Token.GreaterThan),
                typeof(Token.LessThanEqual),
                typeof(Token.GreaterThanEqual)
                )
            )
        {
            BinaryExprOperator binaryOperator = PreviousToken() switch
            {
                Token.Equal            => BinaryExprOperator.Equal           ,
                Token.NotEqual         => BinaryExprOperator.NotEqual        ,
                Token.LessThan         => BinaryExprOperator.LessThan        ,
                Token.GreaterThan      => BinaryExprOperator.GreaterThan     ,
                Token.LessThanEqual    => BinaryExprOperator.LessThanEqual   ,
                Token.GreaterThanEqual => BinaryExprOperator.GreaterThanEqual,
                _ => throw new UnreachableException()
            };
            Expr right = ParseExprJoin();
            expr = new Expr.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private Expr ParseExpr()
    {
        return ParseExprComparison();
        // throw new ParsingException("Not implemented yet");
    }

    /// <summary>
    /// Same as <c>AcceptChar</c>, but used when expecting a character
    /// </summary>
    private bool ExpectToken(Token expectedToken)
        => AcceptToken(expectedToken);
}
