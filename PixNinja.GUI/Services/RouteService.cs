using PixNinja.GUI.ViewModels;
using ReactiveUI;

namespace PixNinja.GUI.Services;

public class RouteService : ReactiveObject
{
    public HomePageViewModel? HomePageViewModel { get; set; }
    
    public ProgressPageViewModel? ProgressPageViewModel { get; set; }
    
    public ComparePageViewModel? ComparePageViewModel { get; set; }

    public CompletePageViewModel? CompletePageViewModel { get; set; }

    public RouteService(IScreen hostWindow)
    {
        HostWindow = hostWindow;
    }

    public IScreen HostWindow { get; }


}
