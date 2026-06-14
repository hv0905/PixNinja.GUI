using System;
using PixNinja.GUI.ViewModels;
using PixNinja.GUI.Views;
using ReactiveUI;

namespace PixNinja.GUI.ViewLocators;

public class HomeViewLocator : IViewLocator
{
    public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract = null)
        where TViewModel : class
    {
        return null;
    }

    public IViewFor? ResolveView(object? instance, string? contract = null) =>
        instance switch
        {
            HomePageViewModel context => new HomePage { ViewModel = context },
            ProgressPageViewModel context => new ProgressPage { ViewModel = context },
            ComparePageViewModel context => new ComparePage { ViewModel = context },
            CompletePageViewModel context => new CompletePage { ViewModel = context },
            _ => throw new ArgumentOutOfRangeException(nameof(instance), instance, null)
        };
}
