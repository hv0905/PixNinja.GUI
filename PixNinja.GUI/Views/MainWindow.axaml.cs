using PixNinja.GUI.ViewModels;
using ReactiveUI.Avalonia;

namespace PixNinja.GUI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
