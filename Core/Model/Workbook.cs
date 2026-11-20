namespace MiniSheets.Core.Model;

public class Workbook
{
    public Guid Id { get; } = Guid.NewGuid();
    public int SheetCount => Sheets.Count;
    public List<Guid> SheetIds { get; } = new();
    public Dictionary<Guid, Sheet>    Sheets { get; } = new();
    public Dictionary<int, CellStyle> Styles { get; } = new();

    // TODO: Think about this stuff
    // private DateTime _createdAt  { get; set; } = null;
    // private DateTime _modifiedAt { get; set; } = null;

    public Guid AddSheet(Sheet sheet)
    {
        Sheets[sheet.Id] = sheet;
        SheetIds.Add(sheet.Id);
        return sheet.Id;
    }
}
