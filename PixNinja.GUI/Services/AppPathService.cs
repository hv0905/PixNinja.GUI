using System;
using System.IO;

namespace PixNinja.GUI.Services;

public class AppPathService : ServiceBase
{
    private const string AppDirectoryName = "PixNinja";
    private const string LegacyAppDirectoryName = ".PixNinja";

    public string ConfigDirectory { get; } = GetConfigDirectory();

    public string LegacyConfigDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), LegacyAppDirectoryName);

    private static string GetConfigDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, AppDirectoryName);
        }

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", AppDirectoryName);
        }

        var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (!string.IsNullOrWhiteSpace(xdgConfigHome))
        {
            return Path.Combine(xdgConfigHome, AppDirectoryName);
        }

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, ".config", AppDirectoryName);
    }
}
