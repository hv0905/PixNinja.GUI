﻿using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PixNinja.GUI.ViewModels;

namespace PixNinja.GUI.Views;

public partial class CompletePage : ReactiveUserControl<CompletePageViewModel>
{
    public CompletePage()
    {
        InitializeComponent();
    }
}
