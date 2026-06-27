using CommunityToolkit.Mvvm.ComponentModel;
using SimpleExternalUnityConsole.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace SimpleExternalUnityConsole.ModelView
{
    public partial class MainWindowViewModel
    {
        [ObservableProperty]
        private string _applicationTitle = "Simple External Unity Tool - Disconnected";

        private string _selectedGamePath = string.Empty;

        private UnityBuild _currentBuild;

        [ObservableProperty]
        private int _errorCount = 0;
        [ObservableProperty]
        private int _warningCount = 0;
        [ObservableProperty]
        private int _logCount = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectionStatusText))]
        [NotifyCanExecuteChangedFor(nameof(LookForGameCommand))]
        private bool _isListening = false;

        public string ConnectionStatusText =>
            IsListening ? $"Connected to {Path.GetFileNameWithoutExtension(_selectedGamePath)}" : "Disconnected";

        public ObservableCollection<UnityLogEntry> LogMessages { get; } = new ObservableCollection<UnityLogEntry>();

        public ICollectionView FilteredLogs { get; }

        [ObservableProperty]
        public bool _showErrors = true;

        [ObservableProperty]
        public bool _showWarnings = true;

        [ObservableProperty]
        public bool _showLogs = true;

        partial void OnShowLogsChanged(bool value) => FilteredLogs.Refresh();
        partial void OnShowWarningsChanged(bool value) => FilteredLogs.Refresh();
        partial void OnShowErrorsChanged(bool value) => FilteredLogs.Refresh();

        private bool CanLaunchGame() => !IsListening;

        private bool CanClearConsole() => LogMessages.Count > 0;

        [ObservableProperty]
        private string _stackTraceText = string.Empty;

        [ObservableProperty]
        private UnityLogEntry _selectedLogEntry;

        public Settings MainSettings = new Settings();

        private Process _gameProcess;
    }
}
