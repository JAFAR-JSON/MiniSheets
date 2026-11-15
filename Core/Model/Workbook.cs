namespace MiniSheets.Core.Model;

public class Workbook
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid ActiveSheetId { get; set; } = Guid.Empty;
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public int Sheets => _sheets.Count;
    private readonly List<Sheet>    _sheets = new();
    private readonly Dictionary<int, CellStyle> _styles = new();

    public Workbook() { }
    // Used when opening a file, the id has to stay the same as in the file.
    public Workbook(Guid id) { this.Id = id; }

    public IReadOnlyList<Sheet> GetSheets()
        => _sheets;

    public Sheet? GetSheet(Guid sheetId)
        => _sheets.FirstOrDefault(sheet => sheet.Id == sheetId);

    public void AddSheet(Sheet sheet)
    {
        _sheets.Add(sheet);
    }

    public IReadOnlyDictionary<int, CellStyle> GetStyles()
        => _styles;

    public CellStyle GetStyle(int styleId)
    {
        if (_styles.TryGetValue(styleId, out CellStyle? style))
        {
            return style;
        }
        else
        {
            return new CellStyle();
        }
    }

    public void SetStyle(int styleId, CellStyle style)
    {
        _styles[styleId] = style;
    }
}
