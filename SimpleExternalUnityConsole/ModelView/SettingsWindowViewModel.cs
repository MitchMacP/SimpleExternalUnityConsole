using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SimpleExternalUnityConsole.ModelView
{
    public partial class SettingsWindowViewModel : ObservableObject
    {
        private MainWindowViewModel _mainWindowViewModel;

        [ObservableProperty]
        private int _maxLogs;


        public SettingsWindowViewModel(MainWindowViewModel mainWindowViewModel) 
        {
            _mainWindowViewModel = mainWindowViewModel;
            MaxLogs = _mainWindowViewModel.MainSettings.MaxLogs;
        }

        partial void OnMaxLogsChanged(int value)
        {
            _mainWindowViewModel.MainSettings.MaxLogs = value;
            _ = _mainWindowViewModel.SaveSettings();
        }
    }
}
