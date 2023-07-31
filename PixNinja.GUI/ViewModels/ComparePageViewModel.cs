using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PixNinja.GUI.Models;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class ComparePageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly ImageScanningService _imageScanningService;
    private readonly RouteService _routeService;
    private readonly UIInteractiveService _uiInteractiveService;
    private readonly ObservableAsPropertyHelper<string> _statusbarText;

    private int _currentGroupId = 0;
    private int _currentSelected = 0;

    public string? UrlPathSegment => "compare";
    public IScreen HostScreen => _routeService.HostWindow;

    public int CurrentGroupId
    {
        get => _currentGroupId;
        set => this.RaiseAndSetIfChanged(ref _currentGroupId, value);
    }

    public string StatusbarText => _statusbarText.Value;

    public ObservableCollection<ImageCompareElementModel> ListContents { get; } = new();

    public List<ImgFile> CurrentGroup => _imageScanningService.ImgGroups![CurrentGroupId];
    
    public int CurrentSelected
    {
        get => _currentSelected;
        set => this.RaiseAndSetIfChanged(ref _currentSelected, value);
    }

    public ComparePageViewModel(ImageScanningService imageScanningService, RouteService routeService,
        UIInteractiveService uiInteractiveService)
    {
        _imageScanningService = imageScanningService;
        _routeService = routeService;
        _uiInteractiveService = uiInteractiveService;
        var statusUpdateOb = this.WhenAnyValue(t => t.CurrentGroupId, t => t._imageScanningService.ImgGroups);
        _statusbarText = statusUpdateOb
            .Select(t => t.Item2 != null ? $"Group: {t.Item1} / {t.Item2.Count}" : string.Empty)
            .ToProperty(this, t => t.StatusbarText);

        Next = ReactiveCommand.Create(() =>
        {
            ++CurrentGroupId;
            PrepareContent();
        }, statusUpdateOb.Select(t => t.Item1 < t.Item2.Count - 1));

        Previous = ReactiveCommand.Create(() =>
        {
            --CurrentGroupId;
            PrepareContent();
        }, statusUpdateOb.Select(t => t.Item1 > 0));
    }


    #region Actions

    public ICommand Next { get; }
    public ICommand Previous { get; }

    public void Complete()
    {
    }

    public void Init()
    {
        if (_imageScanningService.ImgGroups!.Count == 0)
        {
            // nothing to do
            _uiInteractiveService.Warning("Scan complete. No similarity images found.").ConfigureAwait(false);
        }

        CurrentGroupId = 0;
        PrepareContent();
    }

    public void PrepareContent()
    {
        ListContents.Clear();
        var bestSize = CurrentGroup.MaxBy(t => t.FileSize);
        var bestRes = CurrentGroup.MaxBy(t => (long)t.Width * t.Height);


        ListContents.AddRange(CurrentGroup.Select((t, i) =>
        {
            return new ImageCompareElementModel(t.FilePath, (ulong)new FileInfo(t.FilePath).Length,
                $"{t.Width} x {t.Height}", t == bestRes, t == bestSize);
        }).ToList());
    }

    public void UpdateSimilarities()
    {
    }

    #endregion

    #region Design

#pragma warning disable CS8618
    [Obsolete("For design purpose only.")]
    public ComparePageViewModel()
    {
    }
#pragma warning restore CS8618

    #endregion
}
