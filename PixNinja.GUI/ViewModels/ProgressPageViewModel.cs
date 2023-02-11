using System;
using System.Linq;
using System.Reactive.Linq;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class ProgressPageViewModel : ViewModelBase
{
    private readonly ImageScanningService _imageScanningService;
    private readonly ObservableAsPropertyHelper<string> _status;
    private readonly ObservableAsPropertyHelper<int> _progress;
    public string Status => _status.Value;
    public int Progress => _progress.Value;

    public ProgressPageViewModel(ImageScanningService imageScanningService)
    {
        _imageScanningService = imageScanningService;

        var statusUpdateOb = _imageScanningService
            .WhenAnyValue(t => t.CompletedCountSync)
            .Where(t => t > 0);
            
            
        _status = statusUpdateOb
            .Select(t => $"({t} / {_imageScanningService.ImageFilePaths.Count}) Calculating {_imageScanningService.LastFileName}...")
            .ToProperty(this, t => t.Status);

        _progress = statusUpdateOb
            .Select(t => t * 100 / _imageScanningService.ImageFilePaths.Count)
            .ToProperty(this, t => t.Progress);

    }
}
