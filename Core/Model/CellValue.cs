namespace MiniSheets.Core.Model;

public abstract record CellValue
{
    public sealed record Empty   () : CellValue;
    // public sealed record Number  () : CellValue;
    // public sealed record Decimal () : CellValue;
    // public sealed record Text    () : CellValue;
    // public sealed record Bool    () : CellValue;
    // public sealed record Date    () : CellValue;
    // public sealed record Error   () : CellValue;
 
}
