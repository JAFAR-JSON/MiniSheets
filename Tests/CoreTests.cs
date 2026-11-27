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
    public void SpreadsheetEngine_SetSimpleArithmeticExpression_ReturnsFive()
    {
        Workbook workbook = new();
        workbook.AddSheet(new Sheet());

        try
        {
            SpreadsheetEngine spreadsheetEngine = new(workbook);

            List<CellKey> cellKeys;
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 1), "2");
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 2), "3");
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 3), "=A1+A2");

            Cell cell = workbook.GetCell(0, new CellKey(0, 3));
            Assert.True(cell.Value is CellValue.Number, "Cell value is not a number: " + cell.Value);
            if (cell.Value is CellValue.Number number)
            {
                Assert.True(number.Value == 5, "number.Value == " + number.Value.ToString());
            }
        }
        catch (Exception e)
        {
            Assert.Fail(e.ToString());
        }
    }

    [Fact]
    public void SpreadsheetEngine_SetJoinText_ReturnsJoined()
    {
        Workbook workbook = new();
        workbook.AddSheet(new Sheet());

        try
        {
            SpreadsheetEngine spreadsheetEngine = new(workbook);

            List<CellKey> cellKeys;
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 1), "Hello");
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 2), ", World!");
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 3), "=A1&A2");

            Cell cell = workbook.GetCell(0, new CellKey(0, 3));
            Assert.True(cell.Value is CellValue.Text, "Cell value is not a number: " + cell.Value);
            if (cell.Value is CellValue.Text text)
            {
                Assert.True(text.Value.Equals("Hello, World!"), "text.Value == " + text.Value);
            }
        }
        catch (Exception e)
        {
            Assert.Fail(e.ToString());
        }
    }

    [Fact]
    public void SpreadsheetEngine_SumRangeAndArgument_Returns10()
    {
        Workbook workbook = new();
        workbook.AddSheet(new Sheet());

        try
        {
            SpreadsheetEngine spreadsheetEngine = new(workbook);

            List<CellKey> cellKeys;
            // Console.WriteLine($"DEBUG: CoreTests: {true}");
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 1), "1");
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 2), "2");
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 3), "7");
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(1, 1), "7");
            cellKeys = spreadsheetEngine.SetCellText(0, new CellKey(0, 4), "=SUM(A1:A3, B1)");
            // Console.WriteLine($"DEBUG: CoreTests: {cellKeys.Count}");

            Cell cell = workbook.GetCell(0, new CellKey(0, 4));
            // Console.WriteLine($"DEBUG: CoreTests: {cell.Ast}");
            if (cell.Ast is AstNode.Call nodeCall)
            {
                // Console.WriteLine($"DEBUG: CoreTests: {nodeCall.Arguments.Length}");
                // Console.WriteLine($"DEBUG: CoreTests: {nodeCall.Arguments[0]}");
            }
            Assert.True(cell.Value is CellValue.Number, "Cell value is not a number: " + cell.Value);
            if (cell.Value is CellValue.Number number)
            {
                Assert.True(number.Value == 17, "text.Value == " + number);
            }
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
