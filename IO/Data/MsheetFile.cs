using System.Text.Json.Serialization;

namespace MiniSheets.IO;

// These records are the JSON form of the .msheet file, as written in the specification (8.3).
// They are kept seperate from the Core.Model classes, so that the model doesn't need to know
// anything about how it is saved.
// Most of the fields are strings or nullable, since a file that is being opened can contain anything,
// and the checking of it is done after it has been read.
public sealed record MsheetFile
(
    string? Format,
    int Version,
    string? Id,
    string? CreatedAt,
    string? ModifiedAt,
    string? ActiveSheetId,
    List<MsheetStyle>? Styles,
    List<MsheetSheet>? Sheets
);

public sealed record MsheetSheet
(
    string? Id,
    string? Name,
    Dictionary<int, int>? ColWidths,
    Dictionary<int, int>? RowHeights,
    Dictionary<string, MsheetCell>? Cells
);

public sealed record MsheetCell
(
    string? Raw,
    int? Style
);

// Fields that are equal to their default are null, so that they are not written to the file.
public sealed record MsheetStyle
(
    string? Format     = null,
    int?    Decimals   = null,
    string? Currency   = null,
    string? Fill       = null,
    string? TextColor  = null,
    bool?   Borders    = null
);

// Needed because of <PublishAot>, the normal JsonSerializer uses reflection, which doesn't work with AOT.
// This generates the serialization code at compile time instead.
// The options (camelCase, indenting, etc.) are set in WorkbookSerializer.
[JsonSerializable(typeof(MsheetFile))]
public partial class MsheetJsonContext : JsonSerializerContext
{
}
