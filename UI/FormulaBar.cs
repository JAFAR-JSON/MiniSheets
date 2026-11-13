using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Styling;

namespace MiniSheets;

internal sealed class FormulaBar : Border
{
    public FormulaBar()
    {
        Padding = new Thickness(10, 10, 30, 10);

        var functionLabel = CreateFunctionLabel();
        var formulaTextBox = CreateFormulaTextBox();
        var nameBox = CreateNameBox();

        Grid.SetColumn(nameBox, 0);
        Grid.SetColumn(functionLabel, 2);
        Grid.SetColumn(formulaTextBox, 4);

        Child = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("110, 30, 16, 1, *"),
            ColumnSpacing = 0,
            Children =
            {
                nameBox,
                functionLabel,
                formulaTextBox
            }
        };
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