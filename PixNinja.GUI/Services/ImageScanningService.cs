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

public class ImageScanningService : ServiceBase
{
    public List<string> ImageFilePaths { get; private set; } = new();
    public List<ImgFile> ImgFiles { get; } = new();
    public IImageHash HashAlgo = new PerceptualHash();
    public int Similarity { get; set; } = 2;

    private int _completedCount;
    private int _completedCountSync;
    private VpTree<ImgFile>? _imgTree;
    private List<List<ImgFile>>? _imgGroups;
    private object _lockHackCal = new();

    public int CompletedCountSync
    {
        get => _completedCountSync;
        set => this.RaiseAndSetIfChanged(ref _completedCountSync, value);
    }

    public List<List<ImgFile>>? ImgGroups
    {
        get => _imgGroups;
        set => this.RaiseAndSetIfChanged(ref _imgGroups, value);
    }

    public string? LastFileName { get; set; }

    public void ScanAndAdd(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            ImageFilePaths.AddRange(Directory.GetFiles(path, "*", new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true
                })
                .Where(t => t.EndsWith(".jpg") || t.EndsWith(".png") || t.EndsWith(".jpeg") || t.EndsWith(".jfif")));
        }

        ImageFilePaths = ImageFilePaths.Distinct().ToList();
        Trace.WriteLine($"Added {ImageFilePaths.Count} files by scanning.");
    }

    public async Task ComputeHash()
    {
        var tsk = Task.Run(() => Parallel.ForEach(ImageFilePaths, new ParallelOptions
        {
            MaxDegreeOfParallelism =
                Math.Max(Environment.ProcessorCount - 4,
                    Environment.ProcessorCount / 2) // Use default settings will stuck the window
        }, t =>
        {
            Debug.WriteLine($"Calculating {t}");
            try
            {
                using var img = Image.Load<Rgba32>(t);
                var width = img.Width;
                var height = img.Height;
                var hash = HashAlgo.Hash(img);
                lock (_lockHackCal)
                {
                    ImgFiles.Add(new ImgFile(t, width, height, hash, new FileInfo(t).Length));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("..." + e.Message);
            }

            lock (_lockHackCal)
            {
                ++_completedCount;
            }
        }));

        while (!tsk.IsCompleted)
        {
            await Task.Delay(200);
            lock (_lockHackCal)
            {
                CompletedCountSync = _completedCount;
                LastFileName = ImgFiles.LastOrDefault()?.FilePath ?? string.Empty;
            }
        }
        GC.Collect();

        CompletedCountSync = _completedCount;

        for (var i = 0; i < ImgFiles.Count; i++) ImgFiles[i].Id = i;

        // Summarize groups
        var result = await Task.Run(SummarizeGroupsFromTree);
        ImgGroups = result;
    }

    public List<List<ImgFile>> SummarizeGroupsFromTree()
    {
        // Building Data Hashes
        Debug.WriteLine("Indexing hashes...");
        _imgTree = new VpTree<ImgFile>(ImgFiles.ToArray(), (x, y) => (int)x.ImageDiff(y));
        Debug.WriteLine("VPTree built.");

        Dsu dsu = new(ImgFiles.Count);
        foreach (var item in ImgFiles)
        {
            var result = _imgTree.SearchByMaxDist(item, Similarity);
            result.ForEach(t => dsu.Union(item.Id, t.Item1.Id));
        }

        Debug.WriteLine("Dsu built.");
        Dictionary<int, List<ImgFile>> groupsDict = new();
        foreach (var item in ImgFiles)
        {
            var g = dsu.Find(item.Id);
            // If the root is the item itself, don't add it to the dict for now.
            // This can reduce the number of List<> needs to be created.
            if (g == item.Id) continue;
            if (groupsDict.TryGetValue(g, out var value))
            {
                value.Add(item);
            }
            else
            {
                groupsDict.Add(g, new List<ImgFile> { item });
            }
        }

        Debug.WriteLine("GroupDict built.");
        List<List<ImgFile>> resultGroups = new();
        foreach (var (id, value) in groupsDict)
        {
            // Add the root element to the list since we've ignored it just now.
            value.Add(ImgFiles[id]);
            resultGroups.Add(value);
        }

        Debug.WriteLine("Exiting...");
        return resultGroups;
    }
}