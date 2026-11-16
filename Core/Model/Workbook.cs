namespace MiniSheets.Core.Model;

public class Workbook
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Sheets => _sheets.Count;
    private readonly List<Sheet>    _sheets = new();
    private readonly Dictionary<int, CellStyle> _styles = new();
    // TODO: Think about this stuff
    // private DateTime _createdAt  { get; set; } = null;
    // private DateTime _modifiedAt { get; set; } = null;
}
