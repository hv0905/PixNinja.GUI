using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using MessageBox.Avalonia;
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
                ViewModel!.ShowMessageBox.RegisterHandler(t =>
                {
                    t.SetOutput(MessageBoxManager.GetMessageBoxStandardWindow(t.Input).ShowDialog(this));
                });
                //ViewModel!.Window = this;
            };
        }
    }
}
