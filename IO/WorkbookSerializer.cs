using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.IO;

// Converts the Workbook to and from the .msheet JSON text (8.3).
// Following the specification's package diagram, this never touches the disk,
// the reading and writing of the file itself is done somewhere else.
public class WorkbookSerializer
{
    public const string Format  = "minisheets";
    public const int    Version = 1;
    private const string dateFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    private static readonly MsheetJsonContext _jsonContext = new
    (
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            // The specification wants LF line endings, on Windows the default would be CRLF.
            NewLine = "\n",
            // Otherwise letters like 'ą' or 'š' in sheet names and cells would be written as ą.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }
    );

    public string Serialize(Workbook workbook)
    {
        // Index 0 is always the default style. The rest are added in the order the cells use them,
        // this way the styles that are not used by any cell are dropped, like the specification wants.
        List<MsheetStyle> styles = new() { new MsheetStyle() };
        Dictionary<int, int> newStyleIds = new() { [0] = 0 };
        List<MsheetSheet> sheets = new();

        foreach (Sheet sheet in workbook.GetSheets())
        {
            Dictionary<string, MsheetCell> cells = new();

            // A cell with an empty raw and the default style counts as removed, so it isn't saved.
            IEnumerable<KeyValuePair<CellKey, Cell>> orderedCells = sheet.GetCells()
                .Where(pair => pair.Value.Raw != "" || pair.Value.StyleId != 0)
                .OrderBy(pair => pair.Key.Row)
                .ThenBy(pair => pair.Key.Column);

            foreach (KeyValuePair<CellKey, Cell> pair in orderedCells)
            {
                if (!newStyleIds.TryGetValue(pair.Value.StyleId, out int newStyleId))
                {
                    newStyleId = styles.Count;
                    newStyleIds[pair.Value.StyleId] = newStyleId;
                    styles.Add(ToMsheetStyle(workbook.GetStyle(pair.Value.StyleId)));
                }

                cells[ToAddress(pair.Key)] = new MsheetCell
                (
                    Raw: pair.Value.Raw,
                    Style: newStyleId != 0 ? newStyleId : null
                );
            }

            sheets.Add
            (
                new MsheetSheet
                (
                    Id: sheet.Id.ToString(),
                    Name: sheet.Name,
                    ColWidths: new Dictionary<int, int>(sheet.GetColumnWidths()),
                    RowHeights: new Dictionary<int, int>(sheet.GetRowHeights()),
                    Cells: cells
                )
            );
        }

        // If no sheet was made active yet, the first one is used, since the file needs a valid one.
        Guid activeSheetId = workbook.ActiveSheetId;
        if (workbook.GetSheet(activeSheetId) == null && workbook.Sheets > 0)
        {
            activeSheetId = workbook.GetSheets()[0].Id;
        }

        MsheetFile file = new MsheetFile
        (
            Format: Format,
            Version: Version,
            Id: workbook.Id.ToString(),
            CreatedAt: ToDateText(workbook.CreatedAt),
            ModifiedAt: ToDateText(workbook.ModifiedAt),
            ActiveSheetId: activeSheetId.ToString(),
            Styles: styles,
            Sheets: sheets
        );

        return JsonSerializer.Serialize(file, _jsonContext.MsheetFile);
    }

    public Workbook Deserialize(string json)
    {
        // TODO: The file has to be checked before any of this, right now a bad file will just throw
        // whatever exception the parsing throws.
        MsheetFile file = JsonSerializer.Deserialize(json, _jsonContext.MsheetFile)!;

        Workbook workbook = new Workbook(Guid.Parse(file.Id!));
        workbook.CreatedAt  = ParseDateText(file.CreatedAt!);
        workbook.ModifiedAt = ParseDateText(file.ModifiedAt!);

        List<MsheetStyle> styles = file.Styles ?? new();
        for (int styleId = 0; styleId < styles.Count; styleId++)
        {
            workbook.SetStyle(styleId, ToCellStyle(styles[styleId]));
        }

        foreach (MsheetSheet msheetSheet in file.Sheets ?? new())
        {
            Sheet sheet = new Sheet(Guid.Parse(msheetSheet.Id!), msheetSheet.Name ?? "");

            foreach (KeyValuePair<int, int> pair in msheetSheet.ColWidths ?? new())
            {
                sheet.SetColumnWidth(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<int, int> pair in msheetSheet.RowHeights ?? new())
            {
                sheet.SetRowHeight(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<string, MsheetCell> pair in msheetSheet.Cells ?? new())
            {
                CellKey? cellKey = ToCellKey(pair.Key);
                if (cellKey == null)
                {
                    continue; // TODO: Should reject the whole file, once the validation is added.
                }

                Cell cell = new Cell();
                cell.Raw = pair.Value.Raw ?? "";
                cell.StyleId = pair.Value.Style ?? 0;
                sheet.SetCell(cellKey.Value, cell);
            }

            workbook.AddSheet(sheet);
        }

        workbook.ActiveSheetId = Guid.Parse(file.ActiveSheetId!);

        return workbook;
    }

    // The fields that are the same as in a new CellStyle are left out of the file.
    private static MsheetStyle ToMsheetStyle(CellStyle style)
    {
        CellStyle defaultStyle = new CellStyle();

        return new MsheetStyle
        (
            Format:    style.Format     != defaultStyle.Format     ? style.Format.ToString()   : null,
            Decimals:  style.Decimals   != defaultStyle.Decimals   ? style.Decimals            : null,
            Currency:  style.Currency   != defaultStyle.Currency   ? style.Currency.ToString() : null,
            Fill:      style.Fill       != defaultStyle.Fill       ? style.Fill                : null,
            TextColor: style.TextColor  != defaultStyle.TextColor  ? style.TextColor           : null,
            Borders:   style.HasBorders != defaultStyle.HasBorders ? style.HasBorders          : null
        );
    }

    private static CellStyle ToCellStyle(MsheetStyle msheetStyle)
    {
        CellStyle style = new CellStyle();

        if (msheetStyle.Format != null)
        {
            style.Format = Enum.Parse<CellFormat>(msheetStyle.Format);
        }
        if (msheetStyle.Decimals != null)
        {
            style.Decimals = msheetStyle.Decimals.Value;
        }
        if (msheetStyle.Currency != null)
        {
            style.Currency = Enum.Parse<CurrencyType>(msheetStyle.Currency);
        }
        if (msheetStyle.Fill != null)
        {
            style.Fill = msheetStyle.Fill;
        }
        if (msheetStyle.TextColor != null)
        {
            style.TextColor = msheetStyle.TextColor;
        }
        if (msheetStyle.Borders != null)
        {
            style.HasBorders = msheetStyle.Borders.Value;
        }

        return style;
    }

    // CellKey(0, 1) -> "A1", the column starts from 0 and the row from 1, same as in the Lexer.
    private static string ToAddress(CellKey cellKey)
        => ColumnEncodingConverter.Encode(cellKey.Column) + cellKey.Row.ToString(CultureInfo.InvariantCulture);

    // "A1" -> CellKey(0, 1), null if the address is not valid.
    private static CellKey? ToCellKey(string address)
    {
        string letters = new string(address.TakeWhile(char.IsAsciiLetterUpper).ToArray());
        string digits  = address.Substring(letters.Length);

        int? column = ColumnEncodingConverter.Decode(letters);
        if (column == null || !int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int row))
        {
            return null;
        }

        return new CellKey(column.Value, row);
    }

    private static string ToDateText(DateTime date)
        => date.ToUniversalTime().ToString(dateFormat, CultureInfo.InvariantCulture);

    private static DateTime ParseDateText(string text)
        => DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
}
