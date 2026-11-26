using MiniSheets.UI;

namespace MiniSheets.State;

// Class to handle ui events like clicking cells, changing content, chaging formula content, applying borders
internal class SpreadSheetUiEventHandler
{
    private readonly GridView _gridView;
    private readonly FormulaBar _formulaBar;
    private readonly ToolBar _toolBar;

    internal SpreadSheetUiEventHandler(GridView gridView, FormulaBar formulaBar, MenuBar menuBar, ToolBar toolBar)
    {
        _gridView = gridView;
        _formulaBar = formulaBar;
        _toolBar = toolBar;

        // Subscribe to events
        // When the formula bar content changes, update the selected cell content in the grid view
        _formulaBar.ChangeGridCellContent += UpdateSelectedCellContent;
        // When the selected cell changes, update both formula-bar fields
        _gridView.SelectedCellChanged += UpdateFormulaBar;
        // When the "Borders" menu item is clicked, apply a border to the selected cell in the grid view
        menuBar.BordersClicked += ApplyBorderToSelectedCell;

        _formulaBar.EnterPressed += MoveDownToNextCell;

        _formulaBar.GoToCell += SendGridToCell;

        _toolBar.CopyButtonClicked += CopySelectedCellContent;
        _toolBar.PasteButtonClicked += PasteIntoSelectedCell;
        _toolBar.CutButtonClicked += CutSelectedCellContent;
    }

    private void UpdateSelectedCellContent(object? sender, string text)
    {
        _gridView.UpdateSelectedCellContent(text);
    }

    private void UpdateFormulaBar(object? sender, (string cellName, string text) e)
    {
        _formulaBar.SetSelectedCell(e.cellName, e.text);
    }

    private void ApplyBorderToSelectedCell(object? sender, EventArgs e)
    {
        _gridView.ApplyBorderToSelectedCell();
    }

    private void MoveDownToNextCell(object? sender, EventArgs e)
    {
        _gridView.MoveDownToNextCell();
    }

    private void SendGridToCell(object? sender, (string columnName, string rowName) e)
    {
        _gridView.GoToCell(e.columnName, e.rowName);
    }

    private void CopySelectedCellContent(object? sender, EventArgs e)
    {
        _ = _gridView.CopySelectedCellToClipboardAsync();
    }

    private void PasteIntoSelectedCell(object? sender, EventArgs e)
    {
        _ = _gridView.PasteIntoSelectedCellAsync();
    }

    private void CutSelectedCellContent(object? sender, EventArgs e)
    {
        _ = _gridView.CutSelectedCellAsync();
    }
}
