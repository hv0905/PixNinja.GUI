using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class ComparePageViewModel : ViewModelBase, IRoutableViewModel
{
    private ImageScanningService _imageScanningService;
    private readonly RouteService _routeService;

    public ComparePageViewModel(ImageScanningService imageScanningService, RouteService routeService)
    {
        _imageScanningService = imageScanningService;
        _routeService = routeService;
    }


    public string? UrlPathSegment => "compare";
    public IScreen HostScreen => _routeService.HostWindow;
}
