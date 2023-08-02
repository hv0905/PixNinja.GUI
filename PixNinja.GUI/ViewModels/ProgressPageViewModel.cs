using System;
using System.Reactive.Linq;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class ProgressPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly ImageScanningService _imageScanningService;
    private readonly RouteService _routeService;
    private readonly ObservableAsPropertyHelper<string> _status;
    private readonly ObservableAsPropertyHelper<int> _progress;
    public string Status => _status.Value;
    public int Progress => _progress.Value;

    public ProgressPageViewModel(ImageScanningService imageScanningService, RouteService routeService)
    {
        _imageScanningService = imageScanningService;
        _routeService = routeService;

        var statusUpdateOb = _imageScanningService
            .WhenAnyValue(t => t.CompletedCountSync)
            .Where(t => t > 0);


        _status = statusUpdateOb
            .Select(t =>
                $"({t} / {_imageScanningService.ImageFilePaths.Count}) Calculating {_imageScanningService.LastFileName ?? string.Empty}...")
            .ToProperty(this, t => t.Status);

        _progress = statusUpdateOb
            .Select(t => t * 100 / _imageScanningService.ImageFilePaths.Count)
            .ToProperty(this, t => t.Progress);
    }

    public string UrlPathSegment => "progress";
    public IScreen HostScreen => _routeService.HostWindow;

    #region Design

#pragma warning disable CS8618
    [Obsolete("For design purpose only.")]
    public ProgressPageViewModel()
    {
    }
#pragma warning restore CS8618

    #endregion
}
