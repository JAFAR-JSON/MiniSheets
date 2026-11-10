using System;

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
        var character = PeekChar();

        if (character == null)
        {
            return null;
        }

        SkipChar();

        return character;
    }

    private bool AcceptChar(char accepted_character)
    {
        var character = PeekChar();

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
        var str = PeekStr(length);

        if (str == null)
        {
            return null;
        }

        current += length;
        return str;
    }

    private bool AcceptStr(string accepted_str)
    {
        var length = accepted_str.Length;
        var str = PeekStr(length);

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
            var character = ConsumeChar();
            if (character == null || !predicate(character.Value))
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
            var character = PeekChar();
            if (character == null || !predicate(character.Value))
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
}
