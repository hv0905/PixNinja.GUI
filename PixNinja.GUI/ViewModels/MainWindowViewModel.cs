using System;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private readonly ImageScanningService _imageScanningService;
    public RouteService RouteService { get; }

    public RoutingState Router { get; } = new();

    public UiInteractiveService UiInteractiveService { get; } = new();
    public FileLauncherService FileLauncherService { get; } = new();
    public TrashService TrashService { get; } = new();

    public MainWindowViewModel(ImageScanningService imageScanningService)
    {
        RouteService = new(this);
        _imageScanningService = imageScanningService;
        RouteService.ProgressPageViewModel = new(_imageScanningService, RouteService);
        RouteService.HomePageViewModel = new(RouteService, UiInteractiveService, _imageScanningService);
        RouteService.ComparePageViewModel = new(_imageScanningService, RouteService, UiInteractiveService, FileLauncherService);
        RouteService.CompletePageViewModel = new(RouteService, _imageScanningService, UiInteractiveService, TrashService);
        Router.Navigate.Execute(RouteService.HomePageViewModel).Subscribe();
    }

    #region Design
    [Obsolete("For design purpose only.")]
#pragma warning disable CS8618
    public MainWindowViewModel() { }
#pragma warning restore CS8618
    #endregion
}
