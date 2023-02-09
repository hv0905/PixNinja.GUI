using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace PixNinja.GUI.Services;

public class WindowStateService : ReactiveObject
{
    private int _step = 0;

    public int Step
    {
        get => _step;
        set => this.RaiseAndSetIfChanged(ref _step, value);
    }
}
