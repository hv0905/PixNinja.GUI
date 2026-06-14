using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using PixNinja.GUI.Services;
using PixNinja.GUI.Views;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class HomePageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly RouteService _routeService;
    private readonly UiInteractiveService _uiInteractiveService;
    private readonly ImageScanningService _imageScanningService;
    private string _inputPath = "";

    public HomePageViewModel(RouteService routeService, UiInteractiveService uiInteractiveService,
        ImageScanningService imageScanningService)
    {
        _routeService = routeService;
        _uiInteractiveService = uiInteractiveService;
        _imageScanningService = imageScanningService;

        AddPath = ReactiveCommand.CreateFromTask(async () =>
        {
            if (Directory.Exists(InputPath))
            {
                if (!Paths.Contains(InputPath))
                {
                    Paths.Add(InputPath);
                    InputPath = string.Empty;
                }
                else
                {
                    await _uiInteractiveService.Warning("The given path is already exists in the queue.");
                }
            }
            else
            {
                await _uiInteractiveService.Warning("The given path doesn't exist, or cannot access.");
            }
        }, this.WhenAnyValue(t => t.InputPath, t => !string.IsNullOrEmpty(t)));

        StartScan = ReactiveCommand.CreateFromTask(async () =>
        {
            await HostScreen.Router.Navigate.Execute(_routeService.ProgressPageViewModel!).ToTask();
            _imageScanningService.Reset();
            _imageScanningService.ScanAndAdd(Paths);
            await _imageScanningService.ComputeHash();
            await _routeService.ComparePageViewModel!.Init();
            await HostScreen.Router.Navigate.Execute(_routeService.ComparePageViewModel!).ToTask();
        }, Paths.WhenAnyValue(t => t.Count, t => t != 0));
    }

#pragma warning disable CS8618
    [Obsolete("For design purpose only.")]
    public HomePageViewModel()
#pragma warning restore CS8618
    {
    }

    public string InputPath
    {
        get => _inputPath;
        set => this.RaiseAndSetIfChanged(ref _inputPath, value);
    }

    public ObservableCollection<string> Paths { get; set; } = new();

    public ICommand StartScan { get; }

    public ICommand AddPath { get; }

    public void Remove(object item)
    {
        Paths.Remove((string)item);
    }

    public void GoAbout()
    {
        new AboutWindow().Show();
    }

    public string UrlPathSegment => "home";
    public IScreen HostScreen => _routeService.HostWindow;
}
