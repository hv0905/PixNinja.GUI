using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private ImageScanningService _imageScanningService;
    public RouteService RouteService { get; }

    public RoutingState Router { get; } = new();

    public UIInteractiveService UiInteractiveService { get; } = new();
    
    public MainWindowViewModel(ImageScanningService imageScanningService)
    {
        RouteService = new(this);
        _imageScanningService = imageScanningService;
        RouteService.ProgressPageViewModel = new(_imageScanningService, RouteService);
        RouteService.HomePageViewModel = new(RouteService, UiInteractiveService, _imageScanningService);
        RouteService.ComparePageViewModel = new(_imageScanningService, RouteService, UiInteractiveService);
        Router.Navigate.Execute(RouteService.HomePageViewModel);
    }
}
