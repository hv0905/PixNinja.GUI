using System;
using PixNinja.GUI.ViewModels;
using PixNinja.GUI.Views;
using ReactiveUI;

namespace PixNinja.GUI.Services;

public class RouteService : ReactiveObject
{
    private int _step = 0;
    
    public HomePageViewModel? HomePageViewModel { get; set; }
    
    public ProgressPageViewModel? ProgressPageViewModel { get; set; }

    public RouteService(IScreen hostWindow)
    {
        HostWindow = hostWindow;
    }

    public IScreen HostWindow { get; }


}
