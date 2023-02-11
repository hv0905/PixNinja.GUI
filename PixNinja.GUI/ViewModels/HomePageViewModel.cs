using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private readonly WindowStateService _windowStateService;
        private readonly ImageScanningService _imageScanningService;
        private string _inputPath = "";

        public HomePageViewModel(WindowStateService windowStateService, Interaction<MessageBoxStandardParams, Task<ButtonResult>> showMessageBox, ImageScanningService imageScanningService)
        {
            _windowStateService = windowStateService;
            _imageScanningService = imageScanningService;
            ShowMessageBox = showMessageBox;
        }

        public HomePageViewModel()
        {
        }

        public Interaction<MessageBoxStandardParams, Task<ButtonResult>> ShowMessageBox { get; }

        public string InputPath
        {
            get => _inputPath;
            set => this.RaiseAndSetIfChanged(ref _inputPath, value);
        }

        public ObservableCollection<string> Paths { get; set; } = new();

        public void AddPath()
        {
            if (Directory.Exists(InputPath))
            {
                if (!Paths.Contains(InputPath))
                {
                    Paths.Add(InputPath);
                    InputPath = string.Empty;
                    this.RaisePropertyChanged(nameof(InputPath));
                    this.RaisePropertyChanged(nameof(Paths));
                }
                else
                {
                    ShowMessageBox.Handle(new()
                    {
                        ContentTitle = "Warning",
                        ContentMessage = "The given path is already exists in the queue.",
                        ButtonDefinitions = ButtonEnum.Ok,
                        Icon = Icon.Warning,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    }).Subscribe();
                }
            }
            else
            {
                ShowMessageBox.Handle(new()
                {
                    ContentTitle = "Warning",
                    ContentMessage = "The given path doesn't exist, or cannot access.",
                    ButtonDefinitions = ButtonEnum.Ok,
                    Icon = Icon.Warning,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                }).Subscribe();
            }
        }

        public void Remove(object item)
        {
            Paths.Remove((string)item);
        }

        public void StartScan()
        {
            _windowStateService.Step = 1;
            _imageScanningService.ScanAndAdd(Paths);
            _imageScanningService.ComputeHash();
        }
    }
}
