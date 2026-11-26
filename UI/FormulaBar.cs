using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Input;
using Avalonia.Interactivity;
using MiniSheets.State;

namespace MiniSheets.UI;

public class FormulaBar : Border
{

    private readonly TextBox _formulaTextBox;
    private readonly TextBox _nameBox;
    private bool _updatingFromSelection;

    public event EventHandler<string>? ChangeGridCellContent;
    public event EventHandler? EnterPressed;
    public event EventHandler<(string firstChar, string restOfCellName)>? GoToCell;

    public FormulaBar()
    {
        Padding = new Thickness(10, 10, 30, 10); 

        var functionLabel = CreateFunctionLabel();
        _formulaTextBox = CreateFormulaTextBox();
        _nameBox = CreateNameBox();

        _nameBox.AddHandler(KeyDownEvent, (_, e) =>
        {
            if (HandleClipboardShortcut(_nameBox, e))
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                var cellName = (_nameBox.Text ?? string.Empty).Trim();
                var separator = 0;
                while (separator < cellName.Length && char.IsLetter(cellName[separator]))
                    separator++;

                var columnName = cellName[..separator];
                var rowName = cellName[separator..];
                if (columnName.Length > 0 &&
                    int.TryParse(rowName, out var rowNumber) &&
                    rowNumber > 0 &&
                    rowNumber <= 10000)
                {
                    GoToCell?.Invoke(this, (columnName, rowName));
                }

                e.Handled = true;
            }
        }, RoutingStrategies.Bubble, handledEventsToo: true);

        _formulaTextBox.TextChanged += (_, _) =>
        {
            if (_updatingFromSelection)
                return;

            if (_formulaTextBox is TextBox formulaTextBox)
            {
                var text = formulaTextBox.Text ?? string.Empty;
                // Event gets triggered in SpreadSheetUiHandler
                ChangeGridCellContent?.Invoke(this, text);
            }
        };

        _formulaTextBox.AddHandler(KeyDownEvent, (_, e) =>
        {
            if (HandleClipboardShortcut(_formulaTextBox, e))
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                EnterPressed?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }, RoutingStrategies.Bubble, handledEventsToo: true);

        Grid.SetColumn(_nameBox, 0);
        Grid.SetColumn(functionLabel, 2);
        Grid.SetColumn(_formulaTextBox, 4);

        Child = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("110, 30, 16, 1, *"),
            ColumnSpacing = 0,
            Children =
            {
                _nameBox,
                functionLabel,
                _formulaTextBox
            }
        };
    }

    private static bool HandleClipboardShortcut(TextBox textBox, KeyEventArgs keyEvent)
    {
        if ((keyEvent.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) == 0)
            return false;

        if (keyEvent.Key == Key.C && textBox.SelectedText != null)
        {
            _ = Clipboard.CopyAsync(textBox.SelectedText);
            keyEvent.Handled = true;
            return true;
        }
        else if (keyEvent.Key == Key.C)
        {
            _ = Clipboard.CopyAsync(textBox.Text ?? string.Empty);
            keyEvent.Handled = true;
            return true;
        }

        if (keyEvent.Key == Key.V)
        {
            _ = PasteTextAsync(textBox);
            keyEvent.Handled = true;
            return true;
        }

        return false;
    }

    private static async Task PasteTextAsync(TextBox textBox)
    {
        textBox.Text += await Clipboard.PasteAsync();
    }

    private TextBox CreateNameBox()
    {
        return new TextBox
        {
            Height = 20,
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#b8bec8")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(0),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            Padding = new Thickness(4, 0),
            Margin = new Thickness(0, 0, 12, 0),
            Styles =
            {
                CreateTextBoxFocusStyle()
            }
        };
    }

    public void SetSelectedCell(string cellName, string text)
    {
        _updatingFromSelection = true;
        try
        {
            if (_nameBox.Text != cellName)
                _nameBox.Text = cellName;

            if (_formulaTextBox.Text != text)
                _formulaTextBox.Text = text;
        }
        finally
        {
            _updatingFromSelection = false;
        }
    }

    private TextBlock CreateFunctionLabel()
    {
        return new TextBlock
        {
            Text = "fx",
            FontStyle = FontStyle.Italic,
            Foreground = new SolidColorBrush(Color.Parse("#4b5563")),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0)
        };
    }

    private TextBox CreateFormulaTextBox()
    {
        return new TextBox
        {
            Height = 20,
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#b8bec8")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(0),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            Padding = new Thickness(6, 0),
            Margin = new Thickness(2, 0, 0, 0),
            Styles =
            {
                CreateTextBoxFocusStyle()
            }
        };
    }

    private Style CreateTextBoxFocusStyle()
    {
        return new Style(selector => selector
            .OfType<TextBox>()
            .Class(":focus")
            .Descendant()
            .OfType<Border>()
            .Name("PART_BorderElement"))
        {
            Setters =
            {
                new Setter(Border.BorderBrushProperty,
                    new SolidColorBrush(Color.Parse("#b8bec8"))),

                new Setter(Border.BorderThicknessProperty,
                    new Thickness(1)),

                new Setter(Border.BackgroundProperty, Brushes.White)
            }
        };
    }
}
