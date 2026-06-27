using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SimpleExternalUnityConsole.Model;
using SimpleExternalUnityConsole.View;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Text;
using System.Windows;

namespace SimpleExternalUnityConsole.ModelView
{
    public partial class MainWindowViewModel
    {
        /// <summary>
        /// An auto-generated partial method which links to the xaml and variable that are binded together.
        /// </summary>
        /// <param name="value">Selected Unity log from list box.</param>
        partial void OnSelectedLogEntryChanged(UnityLogEntry value)
        {
            UpdateStackTrace(value);
        }
        
        /// <summary>
        /// Updates the UI stack trace element with the new selected log.
        /// </summary>
        /// <param name="selectedLog">Selected Unity log from list box.</param>
        [RelayCommand]
        public void UpdateStackTrace(UnityLogEntry selectedLog)
        {
            if (selectedLog != null)
            {
                StackTraceText = selectedLog.StackTrace;
            }
        }

        /// <summary>
        /// Opens file dialog for the user to locate a Unity build executable.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanLaunchGame))]
        public void LookForGame()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Executable Files (*.exe)|*.exe";
            openFileDialog.Title = "Select Unity Game Build";

            bool? result = openFileDialog.ShowDialog();

            if (result == true) 
            {
                _selectedGamePath = openFileDialog.FileName;
                LaunchGame();
            }
        }

        /// <summary>
        /// Clears log entries from UI elements and resets variables.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanClearConsole))]
        public void ClearConsole()
        {
            LogMessages.Clear();
            StackTraceText = string.Empty;
            
            ClearConsoleCommand.NotifyCanExecuteChanged();
            ExportLogsCommand.NotifyCanExecuteChanged();

            LogCount = 0;
            ErrorCount = 0;
            WarningCount = 0;
        }

        [RelayCommand]
        public void OpenSettings()
        {
            try
            {
                var settingsWindow = new SettingsWindow(this);
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        [RelayCommand(CanExecute = nameof(CanClearConsole))]
        public void ExportLogs()
        {
            // Makes deep copy so if any more logs appear, the logs from when the user pressed export aren't modified.
            string[] logs = LogMessages
            .Select(log => log.Condition)
            .ToArray();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var log in logs)
            {
                stringBuilder.AppendLine(log);
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
            saveFileDialog.Title = "Save Export To...";

            if (saveFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, stringBuilder.ToString());
                    Debug.WriteLine($"Exporting log saved to {saveFileDialog.FileName}");
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"Error writing logs: {ex.Message}");
                }
            }
        }

    }
}
