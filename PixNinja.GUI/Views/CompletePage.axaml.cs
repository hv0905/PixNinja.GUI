using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PixNinja.GUI.ViewModels;

namespace PixNinja.GUI.Views;

public partial class CompletePage : ReactiveUserControl<CompletePageViewModel>
{
    public CompletePage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
