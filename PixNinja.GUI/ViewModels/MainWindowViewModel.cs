using System.Threading.Tasks;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ImageScanningService _imageScanningService; 
    public HomePageViewModel HomePageViewModel { get; }
    public ProgressPageViewModel ProgressPageViewModel { get; }
    
    public WindowStateService WindowStateService { get; }

    public Interaction<MessageBoxStandardParams, Task<ButtonResult>> ShowMessageBox { get; } = new();

    private ObservableAsPropertyHelper<bool> _displayHomePage;
    private ObservableAsPropertyHelper<bool> _displayProgressPage;
    public bool DisplayHomePage => _displayHomePage.Value;
    public bool DisplayProgressPage => _displayProgressPage.Value;
    


    public MainWindowViewModel(ImageScanningService imageScanningService)
    {
        _imageScanningService = imageScanningService;
        ProgressPageViewModel = new(_imageScanningService);
        WindowStateService = new();
        HomePageViewModel = new(WindowStateService, ShowMessageBox, _imageScanningService);
        _displayHomePage = WindowStateService.WhenAnyValue(t => t.Step, t => t == 0).ToProperty(this, t => t.DisplayHomePage);
        _displayProgressPage = WindowStateService.WhenAnyValue(t => t.Step, t => t == 1).ToProperty(this, t => t.DisplayProgressPage);

    }
}
