using Avalonia.ReactiveUI;
using PixNinja.GUI.ViewModels;

namespace PixNinja.GUI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
