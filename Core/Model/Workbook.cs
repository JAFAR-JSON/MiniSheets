namespace MiniSheets.Core.Model;

public class Workbook
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Sheets => sheets.Count;
    public Dictionary<Guid, Sheet>    Sheets { get; set; } = new();
    public Dictionary<int, CellStyle> Styles { get; set; } = new();
    // TODO: Think about this stuff
    // private DateTime _createdAt  { get; set; } = null;
    // private DateTime _modifiedAt { get; set; } = null;

}
