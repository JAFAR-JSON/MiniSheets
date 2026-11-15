namespace MiniSheets.Shared;

public static class NumberCanonizer
{
    public static string? Canonize(string text)
    {
        string[] parts = text.Split('E');

        if (parts.Length > 2)
            return null;

        string mantissa = parts[0];

        int exponent = 0;
        if (parts.Length == 2 && Int32.TryParse(parts[1], out int parsedExponent))
        {
            exponent = parsedExponent;
        }

        string[] decimalParts = mantissa.Split('.');
        if (decimalParts.Length == 2)
            return null;

        string integerPart = decimalParts[0];
        string fractionalPart = decimalParts.Length == 2
            ? decimalParts[1]
            : "";

        string digits = integerPart + fractionalPart;
        int decimalPosition = integerPart.Length + exponent;

        int leadingZeros = digits.TakeWhile(c => c == '0').Count();
        digits = digits[leadingZeros..];
        decimalPosition -= leadingZeros;

        if (digits.Length == 0)
            return "0";

        string result;

        if (decimalPosition <= 0)
        {
            result = "0."
                + new string('0', -decimalPosition)
                + digits;
        }
        else if (decimalPosition >= digits.Length)
        {
            result = digits
                + new string('0', decimalPosition - digits.Length);
        }
        else
        {
            result = digits.Insert(decimalPosition, ".");
        }

        if (result.Contains('.'))
            result = result.TrimEnd('0').TrimEnd('.');

        return result;
    }
}
