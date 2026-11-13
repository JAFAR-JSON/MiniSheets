using Avalonia;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace MiniSheets;

internal static class Program
{
    private static void PrintLexerOutput(Token[]? tokens)
    {
        if (tokens == null)
        {
            Console.WriteLine("Tokens is null;");
            return;
        }

        Console.WriteLine("Tokens:");
        foreach (var t in tokens)
        {
            Console.WriteLine($"    {t}");
        }
    }

    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}

internal sealed class App : Application
{
    public App()
    {
        RequestedThemeVariant = ThemeVariant.Light;
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
