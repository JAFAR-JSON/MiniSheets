using System;
using System.Text;

namespace MiniSheets;

public class Lexer
{
    private string text = ""; // Stores the string that is being parsed.
    private int current = 0; // Current position inside the `text`.
    private List<Token> tokens = [];

    Lexer()
    {
    }

    public Token[] Tokenize(string text)
    {
        this.text = text;
        this.current = 0;
        this.tokens = [];

        return [];
    }

    private void ResetLexer()
    {
        current = 0;
    }

    private bool IsAtEnd()
    {
        return current < text.Length;
    }

    private char? PeekChar()
    {
        if (IsAtEnd())
        {
            return null;
        }

        return text[current];
    }

    private void SkipChar()
    {
        current++;
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
            return text.Substring(startIndex: current, length: length);
        }
        catch (ArgumentOutOfRangeException _)
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

        current += length;
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
        current += length;
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

    private void AddToken(Token token)
    {
        tokens.Append(token);
    }

    private Token.Str? TokenizeStr()
    {
        int start = current;

        StringBuilder strBuilder = new StringBuilder();
        bool isEscaping = false;
        char? character = null;
        
        if (!AcceptChar('"'))
        {
            return null;
        }

        do
        {
            character = PeekChar();
            if (character == null)
            {
                current = start;
                return null;
            }
            SkipChar();
            strBuilder.Append(character.Value);

            if (character == '"')
            {
                isEscaping = !isEscaping;
            }
        }
        while (character != '\"' || isEscaping);

        return (Token.Str) new (strBuilder.ToString());
    }

    // TODO: Make it so that Token stores number in the format requested by the specification. (IEEE-754 doubles)
    private Token.Number? TokenizeNumber()
    {
        int start = current;
        char? character = null;

        character = ConsumeChar();
        if (character == null || NotDigit(character.Value))
        {
            current = start;
            return null;
        }
        ConsumeUntilExclusive(NotDigit);

        character = PeekChar();
        if (character == null)
        {
            current = start;
            return null;
        }
        else if (character.Value == '.')
        {
            ConsumeChar();
            character = ConsumeChar();
            if (character == null)
            {
                current = start;
                return null;
            }

            if (NotDigit(character.Value))
            {
                current = start;
                return null;
            }

            ConsumeUntilExclusive(NotDigit);
        }

        return (Token.Number) new (text.Substring(startIndex: start, length: current - start));
    }

    private Token.Identifier? TokenizeIdentifier()
    {
        int start = current;

        char? character = PeekChar();
        if (character == null || IsIdentifier(character.Value))
        {
            return null;
        }

        ConsumeUntilExclusive(NotIdentifier);

        return (Token.Identifier) new (text.Substring(startIndex: start, length: current - start));
    }

    private Token.Cell? TokenizeCell()
    {
        int start = current;
        char? character = null;
        bool isColumnFixed = false;
        bool isRowFixed = false;
        string? column = null;
        int? row = null;

        // Column fixed
        character = PeekChar();
        if (character == null)
        {
            current = start;
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
            current = start;
            return null;
        }

        // Row fixed
        character = PeekChar();
        if (character == null)
        {
            current = start;
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
            current = start;
            return null;
        }

        return (Token.Cell) new (column, isColumnFixed, row.Value, isRowFixed);
    }

    private Token.Cell? TokenizeCellBare()
    {
        int start = current;
        string? column = null;
        int? row = null;

        // Scan column
        column = ConsumeColumn();
        if (column == null)
        {
            current = start;
            return null;
        }

        // Scan row
        row = ConsumeRow();
        if (row == null)
        {
            current = start;
            return null;
        }

        return (Token.Cell) new (column, false, row.Value, false);
    }

    private string? ConsumeColumn()
    {
        int start = current;
        char? character = null;
        var notColumn = (char c) => !IsAlpha(c) || !char.IsUpper(c);

        character = PeekChar();
        if (character == null || notColumn(character.Value))
        {
            return null;
        }
        ConsumeUntilExclusive(c => notColumn(c));

        return text.Substring(startIndex: start, length: current - start);
    }

    private int? ConsumeRow()
    {
        const int rowLimit = 10000;
        int start = current;
        char? character = null;
        var notRow = (char c) => !IsDigit(c);

        character = PeekChar();
        if (character == null || notRow(character.Value))
        {
            return null;
        }
        ConsumeUntilExclusive(c => notRow(c));

        var span = text.AsSpan(start: start, length: current - start);
        if (Int32.TryParse(span, out int row) && 1 <= row && row <= rowLimit)
        {
            return row;
        }
        else
        {
            current = start;
            return null;
        }
    }

    private Token? TokenizeKeyword(string keyword, Token token)
        => AcceptStr(keyword) ? token : null;

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
        => char.IsLetter(character);

    private bool NotIdentifier(char character)
        => !IsIdentifier(character);

    private bool IsAlpha(char character)
        => (character >= 'a' && character <= 'z')
        || (character >= 'A' && character <= 'Z');
}
