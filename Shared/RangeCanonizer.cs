using MiniSheets.Core.Formula;

namespace MiniSheets.Shared;

public static class RangeCanonizer
{
    public static CanonRange GetCanonRange(AstNode.Primary.Range range)
    {
        int columnStart = Math.Min(range.Start.Column, range.End.Column);
        int rowStart    = Math.Min(range.Start.Row   , range.End.Row   );
        int columnEnd   = Math.Max(range.Start.Column, range.End.Column);
        int rowEnd      = Math.Max(range.Start.Row   , range.End.Row   );

        return new CanonRange(
            columnStart,
            rowStart,
            columnEnd - columnStart + 1,
            rowEnd    - rowStart    + 1
            );
    }
}
