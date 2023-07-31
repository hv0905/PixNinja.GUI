using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;

namespace PixNinja.GUI.Models;

public class ImageCompareElementModel : ReactiveObject
{
    public string FilePath { get; }
    public string FileName { get; }
    public ulong Size { get; }
    public string Resolution { get; }
    public bool TagBestResolution { get; }
    public bool TagBestSize { get; }
    public Bitmap Image { get; }

    private bool _shouldRemove;
    private int _similarity;
    private ObservableAsPropertyHelper<string> _similarityText;

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

    public ImageCompareElementModel(string filePath, ulong size, string resolution, bool tagBestResolution, bool tagBestSize)
    {
        FilePath = filePath;
        Size = size;
        Resolution = resolution;
        TagBestResolution = tagBestResolution;
        TagBestSize = tagBestSize;
        Image = new Bitmap(filePath);
        FileName = Path.GetFileName(FilePath);
        

        _similarityText = this.WhenAnyValue(t => t.Similarity)
            .Select(t => t == -1 ? "Exact Copy" : $"{t} %")
            .ToProperty(this, t => t.SimilarityText);
    }

    #region Actions

    public void OpenExternal()
    {
        // TODO add linux & osx support
        Process.Start(FilePath);
    }

    public void OpenPath()
    {
        // TODO add linux & osx support
        Process.Start("explorer",  $"/select,\"{FilePath}\"");
    }
    
    #endregion

}
