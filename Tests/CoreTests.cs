using Xunit.Abstractions;
using MiniSheets.Core.Formula;
using MiniSheets.Core.Format;
using MiniSheets.Core.Model;

namespace MiniSheets.Tests;

public class CoreTests
{
    private readonly ITestOutputHelper _output;

    public CoreTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SpreadsheetEngine_EmptyWorkbook_NoExceptions()
    {
        Workbook workbook = new();

        try
        {
            SpreadsheetEngine spreadsheetEngine = new(workbook);
        }
        catch (Exception e)
        {
            Assert.Fail(e.ToString());
        }
    }

    [Fact]
    public void SpreadsheetEngine_SetCellText_NoExceptions()
    {
        Workbook workbook = new();
        workbook.AddSheet(new Sheet());

        try
        {
            SpreadsheetEngine spreadsheetEngine = new(workbook);
            spreadsheetEngine.SetCellText(0, new CellKey(0, 0), "2");
            spreadsheetEngine.SetCellText(0, new CellKey(0, 1), "3");
            var cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 2), "=A1+A2");

            // Assert.True(cellKeys.Count == 3, cellKeys.ToString());
        }
        catch (Exception e)
        {
            Assert.Fail(e.ToString());
        }
    }

    [Fact]
    public void Lexer_InputIsEmptyString_ReturnsEmptyTokenList()
    {
        Lexer lexer = new Lexer();
        Token[] tokenizedText = lexer.TokenizeText("");
        Token[] testTokens = { };

        Assert.True(tokenizedText.SequenceEqual(testTokens), "Empty string should return empty token list.");
    }
}
