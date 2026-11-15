using System.Text;
using MiniSheets.Shared;
using MiniSheets.Core.Model;

namespace MiniSheets.Core.Formula;

// NOT_SPECIFICATION: There was a `Parser` in the specification but no `Lexer`.
// TODO: I don't really like the fact that reseting Lexer state involves manual manipulation of _current.
public class Lexer
{
    private string _text = ""; // Stores the string that is being parsed.
    private int _current = 0; // Current position inside the `_text`.

    public Lexer() {}

    public Token[]? TokenizeText(string text)
    {
        _text = text;
        _current = 0;

        List<Token>? tokens = TokenizeTokens();
        if (tokens == null)
        {
            return null;
        }
        else
        {
            return tokens.ToArray();
        }
    }

    private bool IsAtEnd()
    {
        return _text.Length <= _current;
    }

    private char? PeekChar()
    {
        if (IsAtEnd())
        {
            return null;
        }

        return _text[_current];
    }

    private void SkipChar()
    {
        _current++;
    }

    private char? ConsumeChar()
    {
        char? character = PeekChar();

        if (character == null)
        {
            return null;
        }

        SkipChar();

        return character;
    }

    private bool AcceptChar(char accepted_character)
    {
        char? character = PeekChar();

        if (character == null || character != accepted_character)
        {
            return false;
        }

        ConsumeChar();

        return true;
    }

    /// <summary>
    /// Same as <c>AcceptChar</c>, but used when expecting a character
    /// </summary>
    private bool ExpectChar(char expected_character)
        => AcceptChar(expected_character);

    private string? PeekStr(int length)
    {
        try
        {
            return _text.Substring(startIndex: _current, length: length);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private string? ConsumeStr(int length)
    {
        string? str = PeekStr(length);

        if (str == null)
        {
            return null;
        }

        _current += length;
        return str;
    }

    private bool AcceptStr(string accepted_str)
    {
        int length = accepted_str.Length;
        string? str = PeekStr(length);

        if (str == null || !str.Equals(accepted_str))
        {
            return false;
        }

        SkipStr(length);

        return true;
    }

    private void SkipStr(int length)
    {
        _current += length;
    }

    private bool ExpectStr(string expected_str)
        => AcceptStr(expected_str);

    private void ConsumeUntilInclusive(Func<char, bool> predicate)
    {
        do
        {
            char? character = ConsumeChar();
            if (character == null || predicate(character.Value))
            {
                break;
            }
        }
        while (true);
    }

    private void ConsumeUntilExclusive(Func<char, bool> predicate)
    {
        do
        {
            char? character = PeekChar();
            if (character == null || predicate(character.Value))
            {
                break;
            }
            SkipChar();
        }
        while (true);
    }

    private Token.Str? TokenizeStr()
    {
        int start = _current;

        StringBuilder strBuilder = new StringBuilder("");
        char? character = null;
        
        if (!AcceptChar('"'))
        {
            return null;
        }

        do
        {
            character = ConsumeChar();
            if (character == null)
            {
                _current = start;
                return null;
            }
            else if (character == '"')
            {
                SkipChar();
                character = ConsumeChar();
                if (character == null || character != '"')
                {
                    break;
                }

                strBuilder.Append('"');
            }
            else
            {
                strBuilder.Append(character.Value);
            }
        }
        while (true);

        return new Token.Str(strBuilder.ToString());
    }

    private Token.Number? TokenizeNumber()
    {
        int start = _current;
        char? character = null;

        character = ConsumeChar();
        if (character == null || NotDigit(character.Value))
        {
            _current = start;
            return null;
        }
        ConsumeUntilExclusive(NotDigit);

        // Optional fraction part
        character = PeekChar();
        if (character != null && character.Value == '.')
        {
            SkipChar();
            character = ConsumeChar();
            if (character == null)
            {
                _current = start;
                return null;
            }

            if (NotDigit(character.Value))
            {
                _current = start;
                return null;
            }

            ConsumeUntilExclusive(NotDigit);
        }

        // Exponent part
        character = PeekChar();
        if (character != null && character.Value == 'E')
        {
            SkipChar();
            character = ConsumeChar();
            if (character == null)
            {
                _current = start;
                return null;
            }
            else if (character.Value == '-')
            {
                character = ConsumeChar();
            }

            if (character == null)
            {
                _current = start;
                return null;
            }

            if (NotDigit(character.Value))
            {
                _current = start;
                return null;
            }

            ConsumeUntilExclusive(NotDigit);
        }

        string? canonizedText = NumberCanonizer.Canonize
            (_text.Substring
                (startIndex: start, length: _current - start));

        if (canonizedText == null)
        {
            _current = start;
            return null;
        }

        return new Token.Number(canonizedText);
    }

    private int? GetInteger()
    {
        int start = _current;

        char? character = ConsumeChar();
        if (character == null)
        {
            _current = start;
            return null;
        }

        if (NotDigit(character.Value))
        {
            _current = start;
            return null;
        }

        ConsumeUntilExclusive(NotDigit);

        var span = _text.AsSpan(start: start, length: _current - start);
        if (Int32.TryParse(span, out int number))
        {
            return number;
        }
        else
        {
            return null;
        }
    }

    private Token.Identifier? TokenizeIdentifier()
    {
        int start = _current;

        char? character = PeekChar();
        if (character == null || !IsIdentifier(character.Value))
        {
            _current = start;
            return null;
        }

        ConsumeUntilExclusive((char c) => !IsIdentifier(c));

        return (Token.Identifier) new (_text.Substring(startIndex: start, length: _current - start));
    }

    private Token.Cell? TokenizeCell()
    {
        int start = _current;
        char? character = null;
        bool isColumnFixed = false;
        bool isRowFixed = false;
        int? column = null;
        int? row = null;

        // Column fixed
        character = PeekChar();
        if (character == null)
        {
            _current = start;
            return null;
        }
        else if (character.Value == '$')
        {
            isColumnFixed = true;
            ConsumeChar();
        }

        // Scan column
        column = ConsumeColumn();
        if (column == null)
        {
            _current = start;
            return null;
        }

        // Row fixed
        character = PeekChar();
        if (character == null)
        {
            _current = start;
            return null;
        }
        else if (character.Value == '$')
        {
            isRowFixed = true;
            ConsumeChar();
        }

        // Scan row
        row = ConsumeRow();
        if (row == null)
        {
            _current = start;
            return null;
        }

        return (Token.Cell) new (column.Value, isColumnFixed, row.Value, isRowFixed);
    }

    private Token.Cell? TokenizeCellBare()
    {
        int start = _current;
        int? column = null;
        int? row = null;

        // Scan column
        column = ConsumeColumn();
        if (column == null)
        {
            _current = start;
            return null;
        }

        // Scan row
        row = ConsumeRow();
        if (row == null)
        {
            _current = start;
            return null;
        }

        return (Token.Cell) new (column.Value, false, row.Value, false);
    }

    private int? ConsumeColumn()
    {
        int start = _current;
        char? character = null;
        var notColumn = (char c) => !IsAlpha(c) || !char.IsUpper(c);

        character = PeekChar();
        if (character == null || notColumn(character.Value))
        {
            return null;
        }
        ConsumeUntilExclusive(c => notColumn(c));

        return ColumnEncodingConverter.Decode
            (_text.Substring(startIndex: start, length: _current - start));
    }

    private int? ConsumeRow()
    {
        const int rowLimit = 10000;
        int start = _current;

        int? row = GetInteger();
        if (row is null)
        {
            _current = start;
            return null;
        }

        if (1 <= row.Value && row.Value <= rowLimit)
        {
            return row.Value;
        }
        else
        {
            _current = start;
            return null;
        }
    }

    private Token? TokenizeKeyword(string keyword, Token token)
        => AcceptStr(keyword) ? token : null;

    private Token? ConvertIdentifier
        (string identifier, string keywordToMatch, Token convertedToken)
            => identifier.Equals(keywordToMatch) ? convertedToken : null;

    private Token? TokenizeKeywords()
        =>
            // Two or more character symbols
            TokenizeKeyword("<>", new Token.NotEqual         () ) ??
            TokenizeKeyword("<=", new Token.LessThanEqual    () ) ??
            TokenizeKeyword(">=", new Token.GreaterThanEqual () ) ??

            // Single character symbols
            TokenizeKeyword("%", new Token.Percent          () ) ??
            TokenizeKeyword("-", new Token.Minus            () ) ??
            TokenizeKeyword("+", new Token.Plus             () ) ??
            TokenizeKeyword("^", new Token.Caret            () ) ??
            TokenizeKeyword("*", new Token.Star             () ) ??
            TokenizeKeyword("/", new Token.Slash            () ) ??
            TokenizeKeyword("&", new Token.Ampersand        () ) ??
            TokenizeKeyword("=", new Token.Equal            () ) ??
            TokenizeKeyword("<", new Token.LessThan         () ) ??
            TokenizeKeyword(">", new Token.GreaterThan      () ) ??
            TokenizeKeyword(":", new Token.Colon            () ) ??
            TokenizeKeyword("(", new Token.LeftParenthesis  () ) ??
            TokenizeKeyword(")", new Token.RightParenthesis () ) ??
            TokenizeKeyword(",", new Token.Comma            () ) ??

            null;

    // NOTE: Only keywords are capable of being identifiers.
    private Token? ConvertIdentifierToKeywords(Token.Identifier identifierToken)
    {
        string id = identifierToken.Value;
        return
            ConvertIdentifier(id, "TRUE"   , new Token.True  () ) ??
            ConvertIdentifier(id, "FALSE"  , new Token.False () ) ??
            ConvertIdentifier(id, "#ERROR!", new Token.Error ("#ERROR!") ) ??
            ConvertIdentifier(id, "#NAME?" , new Token.Error ("#NAME?" ) ) ??
            ConvertIdentifier(id, "#REF!"  , new Token.Error ("#REF!"  ) ) ??
            ConvertIdentifier(id, "#VALUE!", new Token.Error ("#VALUE!") ) ??
            ConvertIdentifier(id, "#DIV/0!", new Token.Error ("#DIV/0!") ) ??
            ConvertIdentifier(id, "#NUM!"  , new Token.Error ("#NUM!"  ) ) ??
            ConvertIdentifier(id, "#CIRC!" , new Token.Error ("#CIRC!" ) ) ??

            identifierToken;
    }

    private Token? TokenizeToken()
    {
        Token? token;
        Token.Identifier? identifierToken;

        ConsumeUntilExclusive((char c) => !IsWhitespace(c));

        // Console.WriteLine($"DEBUG: -1 {_current}");
        token = TokenizeStr();
        if (token != null) return token;
        // Console.WriteLine($"DEBUG: 0 {_current}");
        token = TokenizeNumber();
        if (token != null) return token;
        // Console.WriteLine($"DEBUG: 1 {_current}");
        token = TokenizeCell();
        if (token != null) return token;
        // Console.WriteLine($"DEBUG: 2 {_current}");
        identifierToken = TokenizeIdentifier();
        if (identifierToken != null)
        {
            // Console.WriteLine($"DEBUG: 3 {_current}");
            return ConvertIdentifierToKeywords(identifierToken) ?? identifierToken;
        }
        // Console.WriteLine($"DEBUG: 4 {_current}");
        token = TokenizeKeywords();
        if (token != null) return token;
        // Console.WriteLine($"DEBUG: 5 {_current}");

        return null;
    }

    private List<Token>? TokenizeTokens()
    {
        int start = _current;
        List<Token> tokens = new List<Token>();
        bool tokensLeft = true;

        do
        {
            Token? token = TokenizeToken();

            if (token == null)
            {
                if (IsAtEnd())
                {
                    // Console.WriteLine($"DEBUG: IsAtEnd(); _current={_current}; length={_text.Length}");
                    tokensLeft = false;
                }
                else
                {
                    _current = start;
                    return null;
                }
            }
            else
            {
                tokens.Add(token);
            }
        }
        while (tokensLeft);

        return tokens;
    }

    private bool IsDigit(char character)
        => character switch
        {
            '0' => true,
            '1' => true,
            '2' => true,
            '3' => true,
            '4' => true,
            '5' => true,
            '6' => true,
            '7' => true,
            '8' => true,
            '9' => true,
            _ => false,
        };

    private bool NotDigit(char character)
        => !IsDigit(character);

    private bool IsIdentifier(char character)
        => character >= 'A' && character <= 'Z';

    private bool IsAlpha(char character)
        => (character >= 'a' && character <= 'z')
        || (character >= 'A' && character <= 'Z');

    private bool IsWhitespace(char character)
        => character == ' ' || character == '\t';
}
