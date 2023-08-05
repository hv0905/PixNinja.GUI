using System;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private ImageScanningService _imageScanningService;
    public RouteService RouteService { get; }

    public RoutingState Router { get; } = new();

    public UiInteractiveService UiInteractiveService { get; } = new();
    
    public MainWindowViewModel(ImageScanningService imageScanningService)
    {
        RouteService = new(this);
        _imageScanningService = imageScanningService;
        RouteService.ProgressPageViewModel = new(_imageScanningService, RouteService);
        RouteService.HomePageViewModel = new(RouteService, UiInteractiveService, _imageScanningService);
        RouteService.ComparePageViewModel = new(_imageScanningService, RouteService, UiInteractiveService);
        RouteService.CompletePageViewModel = new(RouteService, _imageScanningService);
        Router.Navigate.Execute(RouteService.HomePageViewModel);
    }

    #region Design
    [Obsolete("For design purpose only.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainWindowViewModel() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion
}
