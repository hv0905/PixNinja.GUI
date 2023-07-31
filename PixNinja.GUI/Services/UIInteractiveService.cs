using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace PixNinja.GUI.Services;

public class UIInteractiveService : ReactiveObject
{
    public Interaction<MessageBoxStandardParams, Task<ButtonResult>> ShowMessageBox { get; } = new();

    public async Task Warning(string text, string title = "Warning")
    {
        await await ShowMessageBox.Handle(new()
        {
            ContentTitle = title,
            ContentMessage = text,
            ButtonDefinitions = ButtonEnum.Ok,
            Icon = Icon.Warning,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        }).ToTask();
    }

    public async Task<bool> Question(string text, string title = "Warning")
    {
        throw new NotImplementedException();
    }
}
