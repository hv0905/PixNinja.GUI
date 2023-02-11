using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using PixNinja.GUI.ViewModels;

namespace PixNinja.GUI.Views;

public partial class HomePage : ReactiveUserControl<HomePageViewModel>
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var result = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Open the directory you want to scan.."
        });
        if (result.Count > 0)
        {
            ViewModel!.InputPath = result[0].Path.LocalPath;
        }
    }
}
