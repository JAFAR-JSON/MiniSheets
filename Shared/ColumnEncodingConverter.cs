namespace MiniSheets.Shared;

static class ColumnEncodingConverter
{
    // 'Z' is the 26th letter in the alphabet.
    private const int decodedZ  = 26;
    private const int decodedZZ = 702;

    public static int? Decode(string encoded)
    {
        if (encoded.Length == 1)
        {
            char firstChar = encoded[0];
            int convertedFirstChar = GetCharColumnPosition(firstChar);

            if (!IsColumn(firstChar))
            {
                return null;
            }

            return convertedFirstChar;
        }
        else if (encoded.Length == 2)
        {
            char firstChar = encoded[0];
            char lastChar  = encoded[1];
            int convertedFirstChar = GetCharColumnPosition(firstChar);
            int convertedLastChar  = GetCharColumnPosition(lastChar);

            if (!IsColumn(firstChar) || !IsColumn(lastChar))
            {
                return null;
            }

            // 'A' is 0, so the first letter needs a +1, otherwise "AA" would be the same as "A".
            return (convertedFirstChar + 1) * decodedZ + convertedLastChar;
        }
        else
        {
            return null;
        }
    }

    public static string? Encode(int decoded)
    {
        if (0 <= decoded && decoded < decodedZ)
        {
            return GetColumnPositionChar(decoded).ToString();
        }
        else if (decodedZ <= decoded && decoded < decodedZZ)
        {
            return new string
            (
               new char[]
               {
                   GetColumnPositionChar(decoded / decodedZ - 1),
                   GetColumnPositionChar(decoded % decodedZ)
               }
            );
        }
        else
        {
            return null;
        }
    }

    private static bool IsColumn(char character)
        => character >= 'A' && character <= 'Z';

    private static int GetCharColumnPosition(char character)
        => character - 'A';

    private static char GetColumnPositionChar(int columnPosition)
        => (char) (columnPosition + 'A');
}
