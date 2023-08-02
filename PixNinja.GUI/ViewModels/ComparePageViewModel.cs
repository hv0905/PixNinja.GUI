using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
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

    private int _currentGroupId;
    private int _currentSelected;

    public string UrlPathSegment => "compare";
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
            .Select(t => t.Item2 != null ? $"Group: {t.Item1 + 1} / {t.Item2.Count}" : string.Empty)
            .ToProperty(this, t => t.StatusbarText);

        Next = ReactiveCommand.Create(() =>
        {
            ++CurrentGroupId;
            PrepareContent();
        }, statusUpdateOb.Select(t => t.Item1 < (t.Item2?.Count ?? 0) - 1));

        Previous = ReactiveCommand.Create(() =>
        {
            --CurrentGroupId;
            PrepareContent();
        }, statusUpdateOb.Select(t => t.Item1 > 0));

        this.WhenAnyValue(t => t.CurrentSelected)
            .Where(t => t > 0 && t < ListContents.Count)
            .Subscribe(_ => UpdateSimilarities());
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
            _routeService.HostWindow.Router.Navigate.Execute(_routeService.HomePageViewModel);
            return;
        }

        CurrentGroupId = 0;
        PrepareContent();
    }

    public async void PrepareContent()
    {
        ListContents.Clear();
        var bestSize = CurrentGroup.MaxBy(t => t.FileSize)!;
        var bestRes = CurrentGroup.MaxBy(t => (long)t.Width * t.Height)!;
        
        var converted = await Task.WhenAll(CurrentGroup.Select(async t =>
        {
            if (t.FileHash is null)
            {
                await t.ComputeFileHash();
            }
            return new ImageCompareElementModel(t, t.Width * t.Height == bestRes.Width * bestRes.Height, t.FileSize == bestSize.FileSize);
        }).ToList());
        ListContents.AddRange(converted);
        
        UpdateSimilarities();
    }

    public void UpdateSimilarities()
    {
        if (ListContents.Count == 0)
        {
            return;
        }
        if (CurrentSelected < 0 || CurrentSelected > ListContents.Count)
        {
            CurrentSelected = 0;
        }

        for (var i = 0; i < ListContents.Count; i++)
        {
            if (i == CurrentSelected)
            {
                ListContents[i].Similarity = -2;
                continue;
            }
            if (ListContents[i].Img.FileHash!.SequenceEqual(ListContents[CurrentSelected].Img.FileHash!))
            {
                ListContents[i].Similarity = -1;
                continue;
            }

            ListContents[i].Similarity =
                (int)(ListContents[i].Img.ImageSimilarityRatio(ListContents[CurrentSelected].Img) * 100);
        }
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
