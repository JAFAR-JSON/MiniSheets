using MiniSheets.Core.Formula;

namespace MiniSheets.Shared;

public static class RangeCanonizer
{
    public static CanonRange GetCanonRange(AstNode.Primary.Range range)
    {
        int columnStart = range.Start.Column;
        int rowStart    = range.Start.Row;
        int columnEnd   = range.End.Column;
        int rowEnd      = range.End.Row;

        int canonColumnStart = columnStart < columnEnd
            ? columnStart
            : columnEnd;

        int canonRowStart = rowStart < rowEnd
            ? rowStart
            : rowEnd;

        int canonColumnLength = (columnStart > columnEnd
            ? columnStart
            : columnEnd) - canonColumnStart;

        int canonRowLength = (columnStart > columnEnd
            ? columnStart
            : columnEnd) - canonRowStart;

        return new CanonRange
            (canonColumnStart, canonRowStart, canonColumnLength, canonRowLength);
    }
}
