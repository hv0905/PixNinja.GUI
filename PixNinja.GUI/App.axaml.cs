using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PixNinja.GUI.Services;
using PixNinja.GUI.ViewModels;
using PixNinja.GUI.Views;

namespace PixNinja.GUI;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(new ImageScanningService())
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static Window GetCurrentMainWindow()
    {
        if (Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow!;
        }
        throw new NotSupportedException();
    }
}
