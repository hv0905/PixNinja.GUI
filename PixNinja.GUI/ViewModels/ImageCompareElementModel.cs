using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using PixNinja.GUI.Models;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class ImageCompareElementModel : ReactiveObject
{
    public string FileName => Path.GetFileName(Img.FilePath);
    public bool TagBestResolution { get; }
    public bool TagBestSize { get; }
    public Bitmap Image { get; }
    public ImgFile Img { get; }

    private bool _shouldRemove;
    private int _similarity;
    private readonly ObservableAsPropertyHelper<string> _similarityText;

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

    public ImageCompareElementModel(ImgFile img, bool tagBestResolution, bool tagBestSize)
    {
        Img = img;
        TagBestResolution = tagBestResolution;
        TagBestSize = tagBestSize;
        Image = new Bitmap(Img.FilePath);

        _similarityText = this.WhenAnyValue(t => t.Similarity)
            .Select(t => t switch
            {
                -2 => "Base",
                -1 => "Exact Copy",
                _ => $"{t} %"
            })
            .ToProperty(this, t => t.SimilarityText);
    }

    #region Actions

    public void OpenExternal()
    {
        // TODO add linux & osx support
        Process.Start("explorer.exe", Img.FilePath);
    }

    public void OpenPath()
    {
        // TODO add linux & osx support
        Process.Start("explorer.exe",  $"/select,\"{Img.FilePath}\"");
    }
    
    #endregion
}
