using System;
using PixNinja.GUI.ViewModels;
using PixNinja.GUI.Views;
using ReactiveUI;

namespace PixNinja.GUI.ViewLocators;

public class HomeViewLocator : IViewLocator
{
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null) =>
        viewModel switch
        {
            HomePageViewModel context => new HomePage() { ViewModel = context},
            ProgressPageViewModel context => new ProgressPage() {ViewModel = context},
            ComparePageViewModel context => new ComparePage() {ViewModel = context},
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel), viewModel, null)
        };
}
