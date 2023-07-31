using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels
{
    public class HomePageViewModel : ViewModelBase, IRoutableViewModel
    {
        private readonly RouteService _routeService;
        private readonly UIInteractiveService _uiInteractiveService;
        private readonly ImageScanningService _imageScanningService;
        private string _inputPath = "";

        public HomePageViewModel(RouteService routeService, UIInteractiveService uiInteractiveService,
            ImageScanningService imageScanningService)
        {
            _routeService = routeService;
            _uiInteractiveService = uiInteractiveService;
            _imageScanningService = imageScanningService;

            AddPath = ReactiveCommand.Create(() =>
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
                        _uiInteractiveService.Warning("The given path is already exists in the queue.")
                            .ConfigureAwait(false);
                    }
                }
                else
                {
                    _uiInteractiveService.Warning("The given path doesn't exist, or cannot access.")
                        .ConfigureAwait(false);
                }
            }, this.WhenAnyValue(t => t.InputPath, t => !string.IsNullOrEmpty(t)));

            StartScan = ReactiveCommand.Create(() =>
            {
                HostScreen.Router.Navigate.Execute(_routeService.ProgressPageViewModel).Subscribe(t =>
                {
                    _imageScanningService.ScanAndAdd(Paths);
                    _imageScanningService.ComputeHash();
                });
                
            }, Paths.WhenAnyValue(t => t.Count, t => t != 0));
        }

        public HomePageViewModel()
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

        public string? UrlPathSegment => "home";
        public IScreen HostScreen => _routeService.HostWindow;
    }
}
