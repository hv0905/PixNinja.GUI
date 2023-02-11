using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using PixNinja.GUI.Models;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PixNinja.GUI.Services;

public class ImageScanningService : ReactiveObject
{
    public List<string> ImageFilePaths { get; set; } = new();
    public List<ImgFile> ImgFiles { get; set; } = new();
    public IImageHash HashAlgo = new DifferenceHash();
    private int _completedCount = 0;
    private int _completedCountSync = 0;

    public int CompletedCountSync
    {
        get => _completedCountSync;
        set => this.RaiseAndSetIfChanged(ref _completedCountSync, value);
    }
    
    public string LastFileName { get; set; }

    public void ScanAndAdd(ICollection<string> paths)
    {
        foreach (var path in paths)
        {
            ImageFilePaths.AddRange(Directory.GetFiles(path, "*", new EnumerationOptions()
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true
                })
                .Where(t => t.EndsWith(".jpg") || t.EndsWith(".png") || t.EndsWith(".jpeg") || t.EndsWith(".jfif")));
        }
        Trace.WriteLine($"Added {ImageFilePaths.Count} files by scanning.");
    }

    public async void ComputeHash()
    {
        var tsk = Task.Run(() => Parallel.ForEach(ImageFilePaths, t =>
        {
            Debug.WriteLine($"Calculating {t}");
            try
            {
                using var img = Image.Load<Rgba32>(t);
                var hash = HashAlgo.Hash(img);
                lock (ImgFiles)
                {
                    ImgFiles.Add(new ImgFile(t, img.Width, img.Height, hash));
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("..." + e.Message);
            }

            lock (this)
            {
                ++_completedCount;
            }
        }));

        while (!tsk.IsCompleted)
        {
            await Task.Delay(200);
            lock(ImgFiles)
            {
                CompletedCountSync = _completedCount;
                LastFileName = ImgFiles?.LastOrDefault()?.FilePath ?? string.Empty;
            }
            
        }
        CompletedCountSync = _completedCount;

    }
    
    
}
