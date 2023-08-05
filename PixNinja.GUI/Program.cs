using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace PixNinja.GUI;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        FontManagerOptions fontOptions = new();
        if (OperatingSystem.IsLinux())
        {
            fontOptions.DefaultFamilyName = "Noto Sans, Ubuntu, sans-serif";
        }
        
        IconProvider.Current.Register<FontAwesomeIconProvider>();
        
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI()
            .With(fontOptions);
    }
}
