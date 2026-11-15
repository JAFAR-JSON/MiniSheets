namespace MiniSheets.Core.Model;

public abstract record CellValue
{
    public sealed record Empty   () : CellValue;
    public sealed record Number  (double Value) : CellValue;
    public sealed record Decimal (decimal Value) : CellValue;
    // public sealed record Decimal () : CellValue; // We need seperate permission for this.
    public sealed record Text    (string Value) : CellValue;
    public sealed record Bool    (bool Value) : CellValue;
    public sealed record Date    (DateOnly Value) : CellValue;
    public sealed record Error   (string Value) : CellValue;
}
