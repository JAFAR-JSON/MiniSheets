using System.Diagnostics;
using MiniSheets.Core.Model;

namespace MiniSheets.Core.Formula;

// https://craftinginterpreters.com/parsing-expressions.html
public class Parser
{
    private Token[] _tokens = {};
    private int _current = 0;
    private Lexer _lexer = new Lexer();

    public Parser() { }

    public AstNode ParseText(string text)
    {
        Token[]? tokens = _lexer.TokenizeText(text);
        if (tokens is null)
        {
            return ParseTokens(new Token[]{});
        }
        else
        {
            return ParseTokens(tokens);
        }
    }
    
    public AstNode ParseTokens(Token[] tokens)
    {
        _tokens = tokens;
        _current = 0;

        try
        {
            return ParseAstNode();
        }
        catch (ParsingException)
        {
            return new AstNode.Primary.Error("#ERROR!");
        }
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
        Token? token = PeekToken();
        if (token is null || token != acceptedToken) // TODO: I don't think this works as I expect it to.
        {
            return false;
        }

        SkipToken();

        return true;
    }

    private bool MatchToken(params Type[] tokenTypes)
    {
        Token? token = PeekToken();
        if (token is null)
        {
            return false;
        }

        bool tokenMatches = tokenTypes.Any(type => type == token.GetType());
        if (tokenMatches)
        {
            SkipToken();
        }

        return tokenMatches;
    }

    private Token PreviousToken()
    {
        if (_current <= 0)
        {
            throw new ParsingException("There are no previous tokens.");
        }

        return _tokens[_current - 1];
    }

    private AstNode ParseAstNodePrimary()
    {
        Token token;

        token = ConsumeToken();
        switch (token)
        {
            case Token.Identifier t:
                return ParseAstNodeCall(t.Value);

            case Token.Str t:
                return new AstNode.Primary.Text(t.Value);

            case Token.Date t:
                return new AstNode.Primary.Date(t.Value);

            case Token.Number t:
                return new AstNode.Primary.Number(t.Value);

            case Token.Cell t:
                return ParseCellOrRange(t);
                return new AstNode.Primary.Cell
                    (t.Column, t.IsColumnFixed, t.Row, t.IsRowFixed);

            case Token.LeftParenthesis _:
            {
                AstNode innerAstNode = ParseAstNode();

                Token rightParenthesis = ConsumeToken();
                if (rightParenthesis is not Token.RightParenthesis _)
                {
                    throw new ParsingException("No accompanying right parenthesis found.");
                }
                return innerAstNode;
            }

            default:
                throw new ParsingException("Failed to parse primary exception.");
        }
    }

    private AstNode.Primary ParseCellOrRange(Token.Cell cell)
    {
        // int start = _current;
        AstNode.Primary.Cell startCell =
            new AstNode.Primary.Cell
                (cell.Column, cell.IsColumnFixed, cell.Row, cell.IsRowFixed);

        if (PeekToken() is not Token.Colon)
        {
            return startCell;
        }
        SkipToken();

        Token token = ConsumeToken();
        if (token is Token.Cell t)
        {
            AstNode.Primary.Cell endCell =
                new AstNode.Primary.Cell
                    (t.Column, t.IsColumnFixed, t.Row, t.IsRowFixed);
            return new AstNode.Primary.Range(startCell, endCell);
        }
        else
        {
            throw new ParsingException("Expected second range indice.");
        }
    }

    private AstNode ParseAstNodeCall(string name)
    {
        List<AstNode> arguments = new();

        Token token = ConsumeToken();
        if (token is not Token.LeftParenthesis)
        {
            throw new ParsingException("No left parenthesis found.");
        }

        while (true)
        {
            arguments.Add(ParseAstNode());
            token = ConsumeToken();
            if (token is Token.RightParenthesis)
            {
                break;
            }
            else if (token is Token.Comma)
            {
                continue;
            }
            else
            {
                throw new ParsingException("No accompanying right parenthesis found.");
            }
        }

        return new AstNode.Call(name, arguments.ToArray());
    }

    private AstNode ParseAstNodePostfix()
    {
        AstNode expr = ParseAstNodePrimary();

        while (true)
        {
            Token? token;
            UnaryAstNodeOperator? unaryOperator;

            token = PeekToken();
            if (token is null)
            {
                break;
            }

            unaryOperator = token switch
            {
                Token.Percent => UnaryAstNodeOperator.Percentify,
                _ => null
            };

            if (unaryOperator is null)
            {
                break;
            }

            SkipToken();

            expr = new AstNode.Unary(unaryOperator.Value, expr);
        }

        return expr;
    }

    private AstNode ParseAstNodeUnary()
    {
        Token? token = PeekToken();
        if (token is null)
        {
            ConsumeToken();
        }

        UnaryAstNodeOperator? unaryOperator = token switch
        {
            Token.Minus => UnaryAstNodeOperator.Negate,
            Token.Plus  => UnaryAstNodeOperator.Plus  ,
            _ => null
        };

        if (unaryOperator is null)
        {
            return ParseAstNodePostfix();
        }

        return new AstNode.Unary(unaryOperator.Value, ParseAstNodeUnary());
    }

    private AstNode ParseAstNodeRaise()
    {
        AstNode expr = ParseAstNodeUnary();

        while (MatchToken(typeof(Token.Caret)))
        {
            BinaryAstNodeOperator binaryOperator = PreviousToken() switch
            {
                Token.Caret => BinaryAstNodeOperator.Raise,
                _ => throw new UnreachableException()
            };
            AstNode right = ParseAstNodeUnary();
            expr = new AstNode.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private AstNode ParseAstNodeFactor()
    {
        AstNode expr = ParseAstNodeRaise();

        while (MatchToken(typeof(Token.Star), typeof(Token.Slash)))
        {
            BinaryAstNodeOperator binaryOperator = PreviousToken() switch
            {
                Token.Star  => BinaryAstNodeOperator.Multiply,
                Token.Slash => BinaryAstNodeOperator.Divide  ,
                _ => throw new UnreachableException()
            };
            AstNode right = ParseAstNodeRaise();
            expr = new AstNode.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private AstNode ParseAstNodeTerm()
    {
        AstNode expr = ParseAstNodeFactor();

        while (MatchToken(typeof(Token.Plus), typeof(Token.Minus)))
        {
            BinaryAstNodeOperator binaryOperator = PreviousToken() switch
            {
                Token.Plus  => BinaryAstNodeOperator.Add     ,
                Token.Minus => BinaryAstNodeOperator.Subtract,
                _ => throw new UnreachableException()
            };
            AstNode right = ParseAstNodeFactor();
            expr = new AstNode.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private AstNode ParseAstNodeJoin()
    {
        AstNode expr = ParseAstNodeTerm();

        while (MatchToken(typeof(Token.Ampersand)))
        {
            BinaryAstNodeOperator binaryOperator = PreviousToken() switch
            {
                Token.Ampersand => BinaryAstNodeOperator.Join,
                _ => throw new UnreachableException()
            };
            AstNode right = ParseAstNodeTerm();
            expr = new AstNode.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private AstNode ParseAstNodeComparison()
    {
        AstNode expr = ParseAstNodeJoin();

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
            BinaryAstNodeOperator binaryOperator = PreviousToken() switch
            {
                Token.Equal            => BinaryAstNodeOperator.Equal           ,
                Token.NotEqual         => BinaryAstNodeOperator.NotEqual        ,
                Token.LessThan         => BinaryAstNodeOperator.LessThan        ,
                Token.GreaterThan      => BinaryAstNodeOperator.GreaterThan     ,
                Token.LessThanEqual    => BinaryAstNodeOperator.LessThanEqual   ,
                Token.GreaterThanEqual => BinaryAstNodeOperator.GreaterThanEqual,
                _ => throw new UnreachableException()
            };
            AstNode right = ParseAstNodeJoin();
            expr = new AstNode.Binary(binaryOperator, expr, right);
        }

        return expr;
    }

    private AstNode ParseAstNode()
    {
        return ParseAstNodeComparison();
        // throw new ParsingException("Not implemented yet");
    }

    /// <summary>
    /// Same as <c>AcceptChar</c>, but used when expecting a character
    /// </summary>
    private bool ExpectToken(Token expectedToken)
        => AcceptToken(expectedToken);
}
