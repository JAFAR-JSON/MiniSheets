using MiniSheets.Core.Model;

namespace MiniSheets.IO;

// NOT_SPECIFICATION: In the specification the validation is a part of `WorkbookSerializer`,
// it is a separate class here so that the serializer doesn't get too long.
// Checks an opened .msheet file before it is turned into a Workbook (8.3).
// Any failure rejects the whole file, so the first problem that is found gets thrown.
// The messages never contain cell contents, since they end up in the log file (3.11.3).
// Unknown fields don't need to be checked, the JSON parsing already ignores them.
public class MsheetValidator
{
    private const int maxSheets      = 32;
    private const int maxNameLength  = 31;
    private const int maxRawLength   = 32767;
    private const int maxColumn      = 701; // "ZZ", the columns start from 0.
    private const int maxRow         = 10000;
    private const int minColumnWidth = 20;
    private const int maxColumnWidth = 1000;
    private const int minRowHeight   = 16;
    private const int maxRowHeight   = 400;

    // The 16 colours of the palette (3.4.5), a style can't use any other colour.
    private static readonly HashSet<string> paletteColours = new(StringComparer.OrdinalIgnoreCase)
    {
        "#000000", "#7F7F7F", "#D9D9D9", "#FFFFFF", "#C00000", "#FF0000", "#FFC000", "#FFFF00",
        "#92D050", "#00B050", "#00B0F0", "#0070C0", "#002060", "#7030A0", "#F4B183", "#BDD7EE"
    };

    private static readonly char[] forbiddenNameChars = { '!', ':', '/', '\\', '?', '*', '[', ']' };

    public void Validate(MsheetFile file)
    {
        if (file.Format != WorkbookSerializer.Format || file.Version < 1)
            throw new WorkbookFileException("not a MiniSheets workbook");
        // A newer version gets its own message (M-06), so it is checked before everything else.
        if (file.Version > WorkbookSerializer.Version)
            throw new WorkbookFileException($"version {file.Version} is newer than {WorkbookSerializer.Version}", isNewerVersion: true);

        if (!IsGuid(file.Id))
            throw new WorkbookFileException("invalid workbook id");
        if (!WorkbookSerializer.IsDateText(file.CreatedAt) || !WorkbookSerializer.IsDateText(file.ModifiedAt))
            throw new WorkbookFileException("invalid date-time");

        // Index 0 is always the default style, so there has to be at least one.
        if (file.Styles == null || file.Styles.Count == 0)
            throw new WorkbookFileException("missing styles");
        for (int styleId = 0; styleId < file.Styles.Count; styleId++)
        {
            if (file.Styles[styleId] == null)
                throw new WorkbookFileException($"style {styleId}: missing");
            ValidateStyle(file.Styles[styleId], $"style {styleId}");
        }

        if (file.Sheets == null || file.Sheets.Count < 1 || file.Sheets.Count > maxSheets)
            throw new WorkbookFileException($"not 1 to {maxSheets} sheets");

        HashSet<string> sheetIds = new();
        HashSet<string> sheetNames = new(StringComparer.OrdinalIgnoreCase);
        for (int sheetIndex = 0; sheetIndex < file.Sheets.Count; sheetIndex++)
        {
            MsheetSheet sheet = file.Sheets[sheetIndex];
            // Counted from 1 in the messages, the same way the user sees the tabs.
            string where = $"sheet {sheetIndex + 1}";

            if (sheet == null)
                throw new WorkbookFileException($"{where}: missing");
            if (!IsGuid(sheet.Id))
                throw new WorkbookFileException($"{where}: invalid id");
            if (!sheetIds.Add(sheet.Id!.ToLowerInvariant()))
                throw new WorkbookFileException($"{where}: duplicate id");
            if (!IsValidSheetName(sheet.Name))
                throw new WorkbookFileException($"{where}: invalid name");
            if (!sheetNames.Add(sheet.Name!.Trim()))
                throw new WorkbookFileException($"{where}: duplicate name");

            ValidateSheet(sheet, where, file.Styles.Count);
        }

        if (!IsGuid(file.ActiveSheetId) || !sheetIds.Contains(file.ActiveSheetId!.ToLowerInvariant()))
            throw new WorkbookFileException("unknown activeSheetId");
    }

    private static void ValidateSheet(MsheetSheet sheet, string where, int styleCount)
    {
        foreach (KeyValuePair<int, int> pair in sheet.ColWidths ?? new())
        {
            if (pair.Key < 0 || pair.Key > maxColumn || pair.Value < minColumnWidth || pair.Value > maxColumnWidth)
                throw new WorkbookFileException($"{where}: invalid width of column {pair.Key}");
        }

        // Row indexes start from 0 here, like the column ones (8.2.2).
        foreach (KeyValuePair<int, int> pair in sheet.RowHeights ?? new())
        {
            if (pair.Key < 0 || pair.Key >= maxRow || pair.Value < minRowHeight || pair.Value > maxRowHeight)
                throw new WorkbookFileException($"{where}: invalid height of row {pair.Key}");
        }

        foreach (KeyValuePair<string, MsheetCell> pair in sheet.Cells ?? new())
        {
            CellKey? cellKey = WorkbookSerializer.ToCellKey(pair.Key);
            if (cellKey == null || cellKey.Value.Column > maxColumn || cellKey.Value.Row < 1 || cellKey.Value.Row > maxRow)
                throw new WorkbookFileException($"{where}: cell key outside A1:ZZ10000");

            // The cell itself can be null too, e.g. "A1": null.
            string? raw = pair.Value?.Raw;
            if (raw == null || raw.Length > maxRawLength || raw.Contains('\n') || raw.Contains('\r'))
                throw new WorkbookFileException($"{where}, cell {pair.Key}: invalid raw");

            int styleId = pair.Value!.Style ?? 0;
            if (styleId < 0 || styleId >= styleCount)
                throw new WorkbookFileException($"{where}, cell {pair.Key}: unknown style index {styleId}");
        }
    }

    private static void ValidateStyle(MsheetStyle style, string where)
    {
        if (style.Format != null && !Enum.GetNames<CellFormat>().Contains(style.Format))
            throw new WorkbookFileException($"{where}: invalid format");
        if (style.Currency != null && !Enum.GetNames<CurrencyType>().Contains(style.Currency))
            throw new WorkbookFileException($"{where}: invalid currency");

        // Currency allows up to 4 decimals, Number (and the rest) up to 10 (5.1).
        int maxDecimals = style.Format == nameof(CellFormat.Currency) ? 4 : 10;
        if (style.Decimals != null && (style.Decimals < 0 || style.Decimals > maxDecimals))
            throw new WorkbookFileException($"{where}: invalid decimals");

        if (style.Fill != null && !paletteColours.Contains(style.Fill))
            throw new WorkbookFileException($"{where}: fill is not a palette colour");
        if (style.TextColor != null && !paletteColours.Contains(style.TextColor))
            throw new WorkbookFileException($"{where}: text colour is not a palette colour");
    }

    // 1 to 31 characters after trimming, none of ! : / \ ? * [ ] (3.1).
    private static bool IsValidSheetName(string? name)
    {
        if (name == null)
            return false;

        string trimmed = name.Trim();
        return trimmed.Length >= 1
            && trimmed.Length <= maxNameLength
            && trimmed.IndexOfAny(forbiddenNameChars) == -1;
    }

    private static bool IsGuid(string? text)
        => Guid.TryParse(text, out _);
}
