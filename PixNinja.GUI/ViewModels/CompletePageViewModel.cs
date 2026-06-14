using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Microsoft.VisualBasic.FileIO;
using PixNinja.GUI.Models;
using PixNinja.GUI.Services;
using ReactiveUI;

namespace PixNinja.GUI.ViewModels;

public class CompletePageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly RouteService _routeService;
    private readonly ImageScanningService _imageScanningService;
    private readonly UiInteractiveService _uiInteractiveService;
    private bool _isBusy;
    private bool _purgeConfirmed;
    private int _markedCount;
    private long _markedTotalSize;
    private string _statusText = string.Empty;

    public string UrlPathSegment => "complete";
    public IScreen HostScreen => _routeService.HostWindow;

    public int MarkedCount
    {
        get => _markedCount;
        private set
        {
            this.RaiseAndSetIfChanged(ref _markedCount, value);
            RaiseProcessingStateChanged();
        }
    }

    public long MarkedTotalSize
    {
        get => _markedTotalSize;
        private set
        {
            this.RaiseAndSetIfChanged(ref _markedTotalSize, value);
            this.RaisePropertyChanged(nameof(MarkedTotalSizeText));
        }
    }

    public string MarkedTotalSizeText => FormatFileSize(MarkedTotalSize);
    public bool HasMarkedFiles => MarkedCount > 0;
    public bool CanProcess => HasMarkedFiles && !IsBusy;
    public bool CanPurge => CanProcess && PurgeConfirmed;
    public bool IsNotBusy => !IsBusy;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isBusy, value);
            RaiseProcessingStateChanged();
        }
    }

    public bool PurgeConfirmed
    {
        get => _purgeConfirmed;
        set
        {
            this.RaiseAndSetIfChanged(ref _purgeConfirmed, value);
            this.RaisePropertyChanged(nameof(CanPurge));
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public CompletePageViewModel(RouteService routeService, ImageScanningService imageScanningService,
        UiInteractiveService uiInteractiveService)
    {
        _routeService = routeService;
        _imageScanningService = imageScanningService;
        _uiInteractiveService = uiInteractiveService;
    }

    public void Init()
    {
        PurgeConfirmed = false;
        RefreshSummary();
    }

    public async void Purge()
    {
        if (!CanPurge) return;

        if (!OperatingSystem.IsWindows())
        {
            await _uiInteractiveService.Warning("Purge uses the Windows Recycle Bin and is only available on Windows.");
            return;
        }

        var confirmed = await _uiInteractiveService.Question(
            $"Move {MarkedCount} marked image(s) to the Recycle Bin?",
            "Confirm purge");
        if (!confirmed) return;

        await RunFinalizingOperation("Moving files to the Recycle Bin...", "Purge complete", img =>
        {
            if (!File.Exists(img.FilePath)) return OperationResult.Skipped();

            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                img.FilePath,
                UIOption.OnlyErrorDialogs,
                RecycleOption.SendToRecycleBin,
                UICancelOption.ThrowException);
            return OperationResult.Succeeded();
        });
    }

    public async void ExportText()
    {
        var markedFiles = _imageScanningService.GetMarkedFiles();
        if (!await EnsureMarkedFiles(markedFiles)) return;

        var path = await _uiInteractiveService.SelectSaveFile(
            "Export removal list as text",
            "pixninja-removal-list.txt",
            new[] { new FilePickerFileType("Text file") { Patterns = new[] { "*.txt" } } });
        if (string.IsNullOrWhiteSpace(path)) return;

        await RunBusyOnly("Exporting text list...", async () =>
        {
            await File.WriteAllLinesAsync(path, markedFiles.Select(t => t.FilePath));
        });
        await _uiInteractiveService.Info($"Exported {markedFiles.Count} path(s) to:\n{path}", "Export complete");
    }

    public async void ExportJson()
    {
        var markedFiles = _imageScanningService.GetMarkedFiles();
        if (!await EnsureMarkedFiles(markedFiles)) return;

        var path = await _uiInteractiveService.SelectSaveFile(
            "Export removal list as JSON",
            "pixninja-removal-list.json",
            new[] { new FilePickerFileType("JSON file") { Patterns = new[] { "*.json" } } });
        if (string.IsNullOrWhiteSpace(path)) return;

        var export = new RemovalListExport(
            DateTimeOffset.Now,
            markedFiles.Count,
            markedFiles.Select(t => new RemovalFileExport(
                t.FilePath,
                Path.GetFileName(t.FilePath),
                t.FileSize,
                t.Width,
                t.Height,
                t.Hash.ToString("X16"))).ToList());

        await RunBusyOnly("Exporting JSON list...", async () =>
        {
            await using var file = File.Create(path);
            await JsonSerializer.SerializeAsync(file, export, SourceGenerationContext.Default.RemovalListExport);
        });
        await _uiInteractiveService.Info($"Exported {markedFiles.Count} item(s) to:\n{path}", "Export complete");
    }

    public async void MoveAway()
    {
        if (!CanProcess) return;

        var targetDirectory = await _uiInteractiveService.SelectFolder("Choose a directory for marked files");
        if (string.IsNullOrWhiteSpace(targetDirectory)) return;

        await RunFinalizingOperation("Moving marked files...", "Move complete", img =>
        {
            if (!File.Exists(img.FilePath)) return OperationResult.Skipped();

            var destinationPath = GetUniquePath(BuildMoveDestination(targetDirectory, img.FilePath));
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Move(img.FilePath, destinationPath);
            return OperationResult.Succeeded();
        });
    }

    public void GoHome()
    {
        _imageScanningService.Reset();
        _routeService.HostWindow.Router.Navigate.Execute(_routeService.HomePageViewModel!);
    }

    public void Exit()
    {
        App.GetCurrentMainWindow().Close();
    }

    private void RefreshSummary()
    {
        var markedFiles = _imageScanningService.GetMarkedFiles();
        MarkedCount = markedFiles.Count;
        MarkedTotalSize = markedFiles.Sum(t => t.FileSize);
        StatusText = HasMarkedFiles
            ? $"{MarkedCount} image(s) marked for removal."
            : "No images marked for removal.";
    }

    private async Task<bool> EnsureMarkedFiles(IReadOnlyCollection<ImgFile> markedFiles)
    {
        if (markedFiles.Count > 0) return true;

        RefreshSummary();
        await _uiInteractiveService.Warning("No images are marked for removal.");
        return false;
    }

    private async Task RunFinalizingOperation(string busyText, string title, Func<ImgFile, OperationResult> action)
    {
        var markedFiles = _imageScanningService.GetMarkedFiles();
        if (!await EnsureMarkedFiles(markedFiles)) return;

        OperationSummary summary;
        IsBusy = true;
        StatusText = busyText;
        try
        {
            summary = await Task.Run(() => ProcessFiles(markedFiles, action));
        }
        finally
        {
            IsBusy = false;
        }

        await _uiInteractiveService.Info(BuildOperationSummary(summary), title);
        _imageScanningService.Reset();
        _routeService.HostWindow.Router.Navigate.Execute(_routeService.HomePageViewModel!);
    }

    private async Task RunBusyOnly(string busyText, Func<Task> action)
    {
        IsBusy = true;
        StatusText = busyText;
        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
            RefreshSummary();
        }
    }

    private static OperationSummary ProcessFiles(IEnumerable<ImgFile> files, Func<ImgFile, OperationResult> action)
    {
        var summary = new OperationSummary();
        foreach (var file in files)
        {
            try
            {
                var result = action(file);
                if (result.Kind == OperationResultKind.Succeeded)
                {
                    summary.Succeeded++;
                }
                else
                {
                    summary.Skipped++;
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OperationCanceledException)
            {
                summary.Failed++;
                summary.Failures.Add($"{file.FilePath}: {ex.Message}");
            }
        }

        return summary;
    }

    private string BuildMoveDestination(string targetDirectory, string sourceFilePath)
    {
        var root = FindScanRoot(sourceFilePath);
        if (root is null)
        {
            return Path.Combine(targetDirectory, "outside-scan-root", Path.GetFileName(sourceFilePath));
        }

        var rootFolder = SanitizePathFragment(root);
        var relativePath = Path.GetRelativePath(root, sourceFilePath);
        return Path.Combine(targetDirectory, rootFolder, relativePath);
    }

    private string? FindScanRoot(string filePath)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return _imageScanningService.ScanRoots
            .Where(root => filePath.StartsWith(root + Path.DirectorySeparatorChar, comparison) ||
                           filePath.StartsWith(root + Path.AltDirectorySeparatorChar, comparison) ||
                           string.Equals(filePath, root, comparison))
            .OrderByDescending(t => t.Length)
            .FirstOrDefault();
    }

    private static string GetUniquePath(string path)
    {
        if (!File.Exists(path)) return path;

        var directory = Path.GetDirectoryName(path) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);
        var index = 1;
        string candidate;
        do
        {
            candidate = Path.Combine(directory, $"{fileName} ({index}){extension}");
            index++;
        } while (File.Exists(candidate));

        return candidate;
    }

    private static string SanitizePathFragment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = value
            .Replace(Path.DirectorySeparatorChar, '_')
            .Replace(Path.AltDirectorySeparatorChar, '_');

        foreach (var character in invalid)
        {
            sanitized = sanitized.Replace(character, '_');
        }

        sanitized = sanitized.Trim('_', ' ');
        return string.IsNullOrWhiteSpace(sanitized) ? "scan-root" : sanitized;
    }

    private static string BuildOperationSummary(OperationSummary summary)
    {
        var message =
            $"Succeeded: {summary.Succeeded}\nSkipped: {summary.Skipped}\nFailed: {summary.Failed}";

        if (summary.Failures.Count == 0) return message;

        var failures = string.Join("\n", summary.Failures.Take(5));
        var remaining = summary.Failures.Count > 5 ? $"\n...and {summary.Failures.Count - 5} more failure(s)." : string.Empty;
        return $"{message}\n\nFailures:\n{failures}{remaining}";
    }

    private static string FormatFileSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} {units[unit]}" : $"{size:0.##} {units[unit]}";
    }

    private void RaiseProcessingStateChanged()
    {
        this.RaisePropertyChanged(nameof(HasMarkedFiles));
        this.RaisePropertyChanged(nameof(CanProcess));
        this.RaisePropertyChanged(nameof(CanPurge));
        this.RaisePropertyChanged(nameof(IsNotBusy));
    }

    [Obsolete("For design purpose only.")]
#pragma warning disable CS8618
    public CompletePageViewModel()
#pragma warning restore CS8618
    {
    }

    private enum OperationResultKind
    {
        Succeeded,
        Skipped
    }

    private sealed record OperationResult(OperationResultKind Kind)
    {
        public static OperationResult Succeeded() => new(OperationResultKind.Succeeded);
        public static OperationResult Skipped() => new(OperationResultKind.Skipped);
    }

    private sealed class OperationSummary
    {
        public int Succeeded { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public List<string> Failures { get; } = new();
    }
}
