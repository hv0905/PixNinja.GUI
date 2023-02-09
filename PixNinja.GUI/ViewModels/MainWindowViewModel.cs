using System;
using System.Threading.Tasks;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public HomePageViewModel HomePageViewModel { get; }
    public ProgressPageViewModel ProgressPageViewModel { get; }
    
    public WindowStateService WindowStateService { get; }

    public Interaction<MessageBoxStandardParams, Task<ButtonResult>> ShowMessageBox { get; } = new();
    
    public IObservable<bool> DisplayHomePage { get; }
    public IObservable<bool> DisplayProgressPage { get; }
    


    public MainWindowViewModel()
    {
        ProgressPageViewModel = new();
        WindowStateService = new();
        HomePageViewModel = new(WindowStateService, ShowMessageBox);


    }
}
