using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

using Avalonia.Controls;

using MessageBox.Avalonia;

using PixNinja.GUI.Views;

using ReactiveUI;

namespace PixNinja.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindow? Window { get; set; }
        public string InputPath { get; set; } = "";
        public ObservableCollection<string> Paths { get; set; } = new();

        public void AddPath()
        {
            if (Directory.Exists(InputPath))
            {
                if (!Paths.Contains(InputPath))
                {
                    Paths.Add(InputPath);
                    InputPath = string.Empty;
                    this.RaisePropertyChanged(nameof(InputPath));
                    this.RaisePropertyChanged(nameof(Paths));
                }
                else
                {
                    MessageBoxManager.GetMessageBoxStandardWindow("Warning",
                        "The given path is already exists in the queue.",
                        MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        MessageBox.Avalonia.Enums.Icon.Warning
                    ).ShowDialog(Window);
                }
            }
            else
            {
                MessageBoxManager.GetMessageBoxStandardWindow("Warning","The given path doesn't exist, or cannot access.",MessageBox.Avalonia.Enums.ButtonEnum.Ok,MessageBox.Avalonia.Enums.Icon.Warning).ShowDialog(Window);
            }
        }

        public async void Browse()
        {
            OpenFolderDialog ofd = new();
            var result = await ofd.ShowAsync(Window!);
            if (result != null)
            {
                InputPath = result;
                this.RaisePropertyChanged(nameof(InputPath));
            }
        }

        public void Remove()
        {
            
        }
    }
}
