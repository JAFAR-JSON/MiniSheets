namespace MiniSheets.Core.Model;

public class Workbook
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Sheets
        => _sheets.Count;
    private Dictionary<Guid, Sheet>    _sheets { get; set; } = new();
    private Dictionary<int, CellStyle> _styles { get; set; } = new();
    // TODO: Think about this stuff
    // private DateTime _createdAt  { get; set; } = null;
    // private DateTime _modifiedAt { get; set; } = null;
}
