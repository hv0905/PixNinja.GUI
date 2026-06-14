using System;
using System.IO;
using System.Text.Json;
using PixNinja.GUI.Models;

namespace PixNinja.GUI.Services;

public class ConfigService : ServiceBase
{
    private static readonly string configDir =
        string.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixNinja")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".PixNinja");
    private static readonly string fileName = "config.json";

    private static readonly string filePath =Path.Combine(configDir, fileName);

    public AppSettings Settings { get; set; }

    public void SaveConfigure()
    {
        if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
        using var f = File.Open(filePath, FileMode.Create);
        JsonSerializer.Serialize(f, Settings, SourceGenerationContext.Default.AppSettings);
    }

    public ConfigService()
    {
        if (File.Exists(filePath))
        {
            try
            {
                using var f = File.OpenRead(filePath);
                Settings = JsonSerializer.Deserialize(f, SourceGenerationContext.Default.AppSettings) ??
                           new AppSettings();
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        Settings ??= new AppSettings();
    }
}