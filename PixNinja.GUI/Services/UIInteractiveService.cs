using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace PixNinja.GUI.Services;

public class UiInteractiveService : ServiceBase
{
    public async Task Warning(string text, string title = "Warning")
    {
        await MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ContentTitle = title,
            ContentMessage = text,
            ButtonDefinitions = ButtonEnum.Ok,
            Icon = Icon.Warning,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        }).ShowAsync();
    }

    public async Task Info(string text, string title = "PixNinja")
    {
        await MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ContentTitle = title,
            ContentMessage = text,
            ButtonDefinitions = ButtonEnum.Ok,
            Icon = Icon.Info,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        }).ShowAsync();
    }

    public async Task<bool> Question(string text, string title = "Warning")
    {
        var result = await MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ContentTitle = title,
            ContentMessage = text,
            ButtonDefinitions = ButtonEnum.YesNo,
            Icon = Icon.Question,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        }).ShowAsync();
        return result == ButtonResult.Yes;
    }

    public async Task<string?> SelectFolder(string title)
    {
        var result = await App.GetCurrentMainWindow().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = title
        });
        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    public async Task<string?> SelectSaveFile(string title, string suggestedFileName,
        IReadOnlyList<FilePickerFileType> fileTypes)
    {
        var result = await App.GetCurrentMainWindow().StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            FileTypeChoices = fileTypes
        });
        return result?.Path.LocalPath;
    }
}
