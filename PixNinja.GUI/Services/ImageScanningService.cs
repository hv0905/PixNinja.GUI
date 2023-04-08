using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using PixNinja.GUI.Models;
using PixNinja.GUI.Util;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PixNinja.GUI.Services;

public class ImageScanningService : ReactiveObject
{
    public List<string> ImageFilePaths { get; set; } = new();
    public List<ImgFile> ImgFiles { get; } = new();
    public IImageHash HashAlgo = new DifferenceHash();
    public int Similarity { get; set; } = 2;
    
    private int _completedCount = 0;
    private int _completedCountSync = 0;
    private VpTree<ImgFile>? _imgTree = null;
    private List<List<ImgFile>>? _imgGroups = null;
    private object _lockHackCal = new();

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
                lock (_lockHackCal)
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
            lock(_lockHackCal)
            {
                CompletedCountSync = _completedCount;
                LastFileName = ImgFiles.LastOrDefault()?.FilePath ?? string.Empty;
            }
            
        }
        CompletedCountSync = _completedCount;

        for (var i = 0; i < ImgFiles.Count; i++) ImgFiles[i].Id = i;

        // Building Data Hashes
        _imgTree = new VpTree<ImgFile>(ImgFiles.ToArray(), (x, y) => x == y ? 0 : (int)x.ImageDiff(y));
        
        // Summarize groups
        SummarizeGroupsFromTree();
        
        
    }

    public void SummarizeGroupsFromTree()
    {
        Dsu dsu = new(ImgFiles.Count);
        foreach (var item in ImgFiles)
        {
            var result = _imgTree!.SearchByMaxDist(item, Similarity);
            result.RemoveAll(t => t.Item1.Id == item.Id);
            result.ForEach(t => dsu.Union(item.Id, t.Item1.Id));
        }

        Dictionary<int, List<ImgFile>> groupsDict = new();
        foreach (var item in ImgFiles)
        {
            var g = dsu.Find(item.Id);
            if (groupsDict.ContainsKey(g))
            {
                groupsDict[g].Add(item);
            }
            else
            {
                groupsDict.Add(g, new List<ImgFile> { item });
            }
        }
        
        _imgGroups = new();
        foreach (var (_, value) in groupsDict)
        {
            if (value.Count > 1)
            {
                _imgGroups.Add(value);
            }
        }
        Debug.WriteLine(_imgGroups.Count);
    }
    
    
}
