using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace PixNinja.GUI.Services;

public class UIInteractiveService : ReactiveObject
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

    public Task<bool> Question(string text, string title = "Warning")
    {
        throw new NotImplementedException();
    }
}
