using Avalonia.Controls;
using Avalonia.ReactiveUI;

using PixNinja.GUI.ViewModels;

using ReactiveUI;

namespace PixNinja.GUI.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            Activated += (e, args) =>
            {
                ViewModel!.Window = this;
            };
        }
    }
}
