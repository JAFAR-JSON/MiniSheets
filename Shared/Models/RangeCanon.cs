namespace MiniSheets.Shared;

public sealed record CanonRange
    (int ColumnStart, int RowStart, int ColumnLength, int RowLength);
