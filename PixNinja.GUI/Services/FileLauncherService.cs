using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PixNinja.GUI.Services;

public class FileLauncherService : ServiceBase
{
    public Task OpenFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The file no longer exists.", path);
        }

        if (OperatingSystem.IsWindows())
        {
            StartDetached(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
            return Task.CompletedTask;
        }

        if (OperatingSystem.IsMacOS())
        {
            return RunCommand("open", path);
        }

        return RunFirstAvailable(
            ("xdg-open", new[] { path }),
            ("gio", new[] { "open", path }));
    }

    public Task ShowInFolder(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            throw new FileNotFoundException("The file no longer exists.", path);
        }

        if (OperatingSystem.IsWindows())
        {
            StartDetached(new ProcessStartInfo("explorer.exe")
            {
                UseShellExecute = false
            }, "/select," + path);
            return Task.CompletedTask;
        }

        if (OperatingSystem.IsMacOS())
        {
            return RunCommand("open", "-R", path);
        }

        var directory = Directory.Exists(path)
            ? path
            : Path.GetDirectoryName(path) ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return RunFirstAvailable(
            ("xdg-open", new[] { directory }),
            ("gio", new[] { "open", directory }));
    }

    private static void StartDetached(ProcessStartInfo startInfo, params string[] arguments)
    {
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        Process.Start(startInfo)?.Dispose();
    }

    private static async Task RunCommand(string fileName, params string[] arguments)
    {
        var result = await TryRunCommand(fileName, arguments);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }
    }

    private static async Task RunFirstAvailable(params (string FileName, string[] Arguments)[] commands)
    {
        string? lastError = null;
        foreach (var command in commands)
        {
            var result = await TryRunCommand(command.FileName, command.Arguments);
            if (result.Success)
            {
                return;
            }

            lastError = result.ErrorMessage;
        }

        throw new InvalidOperationException(lastError ?? "No supported file opener was found.");
    }

    private static async Task<CommandResult> TryRunCommand(string fileName, params string[] arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            if (!process.Start())
            {
                return CommandResult.Failed($"Could not start {fileName}.");
            }

            await process.WaitForExitAsync();
            return process.ExitCode == 0
                ? CommandResult.Succeeded()
                : CommandResult.Failed($"{fileName} exited with code {process.ExitCode}.");
        }
        catch (Win32Exception ex)
        {
            return CommandResult.Failed($"{fileName} is not available: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return CommandResult.Failed($"{fileName} could not be started: {ex.Message}");
        }
    }

    private readonly record struct CommandResult(bool Success, string? ErrorMessage)
    {
        public static CommandResult Succeeded() => new(true, null);

        public static CommandResult Failed(string message) => new(false, message);
    }
}
