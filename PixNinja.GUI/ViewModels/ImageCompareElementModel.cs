using System;
using System.IO;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using PixNinja.GUI.Models;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class ImageCompareElementModel : ReactiveObject
{
    public string FileName => Path.GetFileName(Img.FilePath);
    public bool TagBestResolution { get; }
    public bool TagBestSize { get; }
    public Bitmap? Image { get; }
    public ImgFile Img { get; }

    private bool _shouldRemove;
    private int _similarity;
    private readonly ObservableAsPropertyHelper<string> _similarityText;
    private readonly FileLauncherService _fileLauncherService;
    private readonly UiInteractiveService _uiInteractiveService;

    public bool ShouldRemove
    {
        get => _shouldRemove;
        set => this.RaiseAndSetIfChanged(ref _shouldRemove, value);
    }

    public int Similarity
    {
        get => _similarity;
        set => this.RaiseAndSetIfChanged(ref _similarity, value);
    }

    public string SimilarityText => _similarityText.Value;

    public ImageCompareElementModel(ImgFile img, bool tagBestResolution, bool tagBestSize,
        FileLauncherService fileLauncherService, UiInteractiveService uiInteractiveService)
    {
        Img = img;
        TagBestResolution = tagBestResolution;
        TagBestSize = tagBestSize;
        _fileLauncherService = fileLauncherService;
        _uiInteractiveService = uiInteractiveService;
        Image = File.Exists(Img.FilePath) ? new Bitmap(Img.FilePath) : null;

        _similarityText = this.WhenAnyValue(t => t.Similarity)
            .Select(t => t switch
            {
                -3 => "Not Found",
                -2 => "Base",
                -1 => "Exact Copy",
                _ => $"{t} %"
            })
            .ToProperty(this, t => t.SimilarityText);
    }

    #region Actions

    public async void OpenExternal()
    {
        try
        {
            await _fileLauncherService.OpenFile(Img.FilePath);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            await _uiInteractiveService.Warning($"Could not open the file:\n{ex.Message}");
        }
    }

    public async void OpenPath()
    {
        try
        {
            await _fileLauncherService.ShowInFolder(Img.FilePath);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            await _uiInteractiveService.Warning($"Could not show the file in its folder:\n{ex.Message}");
        }
    }

    #endregion
}
