namespace MiniSheets.Core.Model;

public struct CellKeyVertice
{
    public CellKey From { get; set; }
    public CellKey To   { get; set; }

    public CellKeyVertice(CellKey fromKey, CellKey toKey)
    {
        From = fromKey;
        To   =   toKey;
    }
}
