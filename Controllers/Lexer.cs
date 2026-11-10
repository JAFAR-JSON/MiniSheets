using System;
using System.Text;

namespace MiniSheets;

public class Lexer
{
    private string text = "";
    private int current = 0;
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
        if (!AcceptChar('"'))
        {
            return null;
        }

        StringBuilder str_builder = new StringBuilder();
        bool is_escaping = false;
        char? character = null;
        
        do
        {
            character = PeekChar();
            if (character == null)
            {
                return null;
            }
            str_builder.Append(ConsumeChar().Value);

            if (character == '"')
            {
                is_escaping = !is_escaping;
            }
        }
        while (character != '\"' || is_escaping);

        return (Token.Str) new (str_builder.ToString());
    }

    // TODO: Make it so that Token stores number in the format requested by the specification. (IEEE-754 doubles)
    private Token.Number? TokenizeNumber()
    {
        int start = current;

        char? character = ConsumeChar();
        if (character == null || NotDigit(character.Value))
        {
            return null;
        }
        ConsumeUntilExclusive(NotDigit);

        character = PeekChar();
        if (character == null)
        {
            return null;
        }
        else if (character.Value == '.')
        {
            ConsumeChar();
            character = ConsumeChar();
            if (character == null)
            {
                return null;
            }

            if (NotDigit(character.Value))
            {
                return null;
            }

            ConsumeUntilExclusive(NotDigit);
        }

        return (Token.Number) new (text.Substring(startIndex: start, length: current));
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
}
