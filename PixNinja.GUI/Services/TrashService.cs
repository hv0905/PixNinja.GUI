using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace PixNinja.GUI.Services;

public class TrashService : ServiceBase
{
    public async Task<TrashResult> MoveFileToTrash(string path)
    {
        if (!File.Exists(path))
        {
            return TrashResult.Skipped();
        }

        if (OperatingSystem.IsWindows())
        {
            FileSystem.DeleteFile(
                path,
                UIOption.OnlyErrorDialogs,
                RecycleOption.SendToRecycleBin,
                UICancelOption.ThrowException);
            return TrashResult.Succeeded();
        }

        if (OperatingSystem.IsMacOS())
        {
            return await TryRunCommand(
                "osascript",
                "-e", "on run argv",
                "-e", "tell application \"Finder\" to delete POSIX file (item 1 of argv)",
                "-e", "end run",
                path);
        }

        var result = await TryRunCommand("gio", "trash", path);
        if (result.Success)
        {
            return result;
        }

        result = await TryRunCommand("trash-put", path);
        if (result.Success)
        {
            return result;
        }

        result = await TryRunCommand("kioclient6", "move", path, "trash:/");
        if (result.Success)
        {
            return result;
        }

        result = await TryRunCommand("kioclient5", "move", path, "trash:/");
        return result.Success
            ? result
            : TrashResult.Failed("No supported Linux trash command was found. Install gio, trash-cli, or KDE kioclient.");
    }

    private static async Task<TrashResult> TryRunCommand(string fileName, params string[] arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                }
            };

            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            if (!process.Start())
            {
                return TrashResult.Failed($"Could not start {fileName}.");
            }

            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
            {
                return TrashResult.Succeeded();
            }

            return TrashResult.Failed(string.IsNullOrWhiteSpace(error)
                ? $"{fileName} exited with code {process.ExitCode}."
                : error.Trim());
        }
        catch (Win32Exception)
        {
            return TrashResult.Failed($"{fileName} is not available.");
        }
        catch (InvalidOperationException ex)
        {
            return TrashResult.Failed($"{fileName} could not be started: {ex.Message}");
        }
    }
}

public readonly record struct TrashResult(bool Success, bool WasSkipped, string? ErrorMessage)
{
    public static TrashResult Succeeded() => new(true, false, null);

    public static TrashResult Skipped() => new(false, true, null);

    public static TrashResult Failed(string message) => new(false, false, message);
}
