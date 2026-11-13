namespace MiniSheets;

sealed class Program
{
    private static void PrintLexerOutput(Token[]? tokens)
    {
        if (tokens == null)
        {
            Console.WriteLine("Tokens is null;");
            return;
        }

        Console.WriteLine("Tokens:");
        foreach (var t in tokens)
        {
            Console.WriteLine($"    {t}");
        }
    }

    public static void Main(string[] args)
    {
        Lexer lexer = new Lexer();

        PrintLexerOutput(lexer.TokenizeText("=2+2"));
        PrintLexerOutput(lexer.TokenizeText("=SUM(ZC$1, ) + 2"));
    }
}
