using System;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class CompletePageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly RouteService _routeService;
    private readonly ImageScanningService _imageScanningService;

    public string UrlPathSegment => "complete";
    public IScreen HostScreen => _routeService.HostWindow;

    public CompletePageViewModel(RouteService routeService, ImageScanningService imageScanningService)
    {
        _routeService = routeService;
        _imageScanningService = imageScanningService;
    }

    public void Purge()
    {
        
    }

    public void ExportText()
    {
        
    }

    public void ExportJson()
    {
        
    }

    public void MoveAway()
    {
        
    }

    [Obsolete("For design purpose only.")]
#pragma warning disable CS8618
    public CompletePageViewModel()
#pragma warning restore CS8618
    {
        
    }
}
