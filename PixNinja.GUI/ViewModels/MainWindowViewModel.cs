using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private ImageScanningService _imageScanningService; 
    public HomePageViewModel HomePageViewModel { get; }
    public ProgressPageViewModel ProgressPageViewModel { get; }
    
    public RouteService RouteService { get; }

    public RoutingState Router { get; } = new();

    public UIInteractiveService UiInteractiveService { get; } = new();

    private ObservableAsPropertyHelper<bool> _displayHomePage;
    private ObservableAsPropertyHelper<bool> _displayProgressPage;
    public bool DisplayHomePage => _displayHomePage.Value;
    public bool DisplayProgressPage => _displayProgressPage.Value;
    


    public MainWindowViewModel(ImageScanningService imageScanningService)
    {
        RouteService = new(this);
        _imageScanningService = imageScanningService;
        ProgressPageViewModel = new(_imageScanningService, RouteService);
        HomePageViewModel = new(RouteService, UiInteractiveService, _imageScanningService);
        _displayHomePage = RouteService.WhenAnyValue(t => t.Step, t => t == 0).ToProperty(this, t => t.DisplayHomePage);
        _displayProgressPage = RouteService.WhenAnyValue(t => t.Step, t => t == 1).ToProperty(this, t => t.DisplayProgressPage);

    }
}
