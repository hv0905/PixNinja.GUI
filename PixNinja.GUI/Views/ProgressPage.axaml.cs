using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PixNinja.GUI.ViewModels;

namespace PixNinja.GUI.Views;

public partial class ProgressPage : ReactiveUserControl<ProgressPageViewModel>
{
    public ProgressPage()
    {
        InitializeComponent();
    }
}
