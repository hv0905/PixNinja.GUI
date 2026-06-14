using System;
using System.IO;
using System.Text.Json;
using PixNinja.GUI.Models;

namespace PixNinja.GUI.Services;

public class ConfigService : ServiceBase
{
    private const string FileName = "config.json";
    private readonly string _configDir;
    private readonly string _filePath;

    private readonly string _legacyFilePath;

    public AppSettings Settings { get; set; }

    public void SaveConfigure()
    {
        if (!Directory.Exists(_configDir)) Directory.CreateDirectory(_configDir);
        using var f = File.Open(_filePath, FileMode.Create);
        JsonSerializer.Serialize(f, Settings, SourceGenerationContext.Default.AppSettings);
    }

    public ConfigService() : this(new AppPathService())
    {
    }

    public ConfigService(AppPathService appPathService)
    {
        _configDir = appPathService.ConfigDirectory;
        _filePath = Path.Combine(_configDir, FileName);
        _legacyFilePath = Path.Combine(appPathService.LegacyConfigDirectory, FileName);

        var loadPath = File.Exists(_filePath) ? _filePath : _legacyFilePath;
        if (File.Exists(loadPath))
        {
            try
            {
                using var f = File.OpenRead(loadPath);
                Settings = JsonSerializer.Deserialize(f, SourceGenerationContext.Default.AppSettings) ??
                           new AppSettings();
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        Settings ??= new AppSettings();
    }
}
