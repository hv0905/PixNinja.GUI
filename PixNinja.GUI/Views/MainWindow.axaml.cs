using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using MsBox.Avalonia;
using PixNinja.GUI.ViewModels;
using ReactiveUI;

namespace PixNinja.GUI.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                d(ViewModel!.UiInteractiveService.ShowMessageBox.RegisterHandler(t =>
                {
                    t.SetOutput(MessageBoxManager.GetMessageBoxStandard(t.Input).ShowAsync());
                }));
            });
        }
    }
}
