using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleExternalUnityConsole.Model;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;

namespace SimpleExternalUnityConsole.ModelView
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            FilteredLogs = CollectionViewSource.GetDefaultView(LogMessages);
            FilteredLogs.Filter = FilterLogEntries;
            _ = ImportSettings();
        }

        /// <summary>
        /// Filters a Unity log based on current log toggle settings.
        /// </summary>
        /// <param name="obj">A Unity log entry.</param>
        /// <returns>Returns a boolean based on if the log should be enabled/disabled.</returns>
        private bool FilterLogEntries(object obj)
        {
            if (obj is not UnityLogEntry log) return false;

            switch (log.LogType)
            {
                case "Log":
                    return ShowLogs; 

                case "Warning":
                    return ShowWarnings;

                case "Error":
                case "Exception":
                    return ShowErrors;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Launches the current selected Unity game build.
        /// </summary>
        public void LaunchGame()
        {
            if (ExecutableIsValid() == false) return;
            
            ClearConsole();
            _currentBuild = new UnityBuild(_selectedGamePath);

            // Install bepinex if it doesn't exist
            if (!Directory.Exists(_currentBuild.BepInExDirectory))
            {
                string zipSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInExTemplate.zip");

                try
                {
                    ZipFile.ExtractToDirectory(zipSource, _currentBuild.GameDirectory);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to extract BepInEx. Reason: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("BepInEx already exists. Skipping extraction.");
            }

            // Create plugin folder if it doesn't exist
            if (!Directory.Exists(_currentBuild.PluginsDirectory))
            {
                Directory.CreateDirectory(_currentBuild.PluginsDirectory);
            }

            try
            {
                string sourceDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PipelineTemplateMod.dll");
                File.Copy(sourceDllPath, _currentBuild.TargetDLLPath, true);
            }
            catch
            {
                Debug.WriteLine("Failed to copy mod file to BepInEx location");
                return;
            }

            // Start Named Pipe Server
            IsListening = true;
            Task.Run(() => StartPipeServer());

            // Try Launching Target Game
            try
            {
                _currentBuild.Process = new Process();
                _currentBuild.Process.StartInfo = new ProcessStartInfo
                {
                    FileName = _selectedGamePath,
                    WorkingDirectory = _currentBuild.GameDirectory
                };

                _currentBuild.Process.EnableRaisingEvents = true;
                _currentBuild.Process.Exited += (s, args) => CleanUpBepInExFiles();
                _currentBuild.Process.Start();
                ApplicationTitle = $"Simple External Unity Tool - Connected to {Path.GetFileNameWithoutExtension(_selectedGamePath)}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to launch Unity build");
                return;
            }
        }

        private bool ExecutableIsValid()
        {
            string directoryPath = Path.GetDirectoryName(_selectedGamePath);
            string executableWithoutExtension = Path.GetFileNameWithoutExtension(_selectedGamePath);

            string expectedDataFolder = Path.Combine(directoryPath, $"{executableWithoutExtension}_Data");

            if (Directory.Exists(expectedDataFolder)) return true;

            if (executableWithoutExtension.Equals("UnityCrashHandler64", StringComparison.OrdinalIgnoreCase) ||
                    executableWithoutExtension.Equals("UnityCrashHandler32", StringComparison.OrdinalIgnoreCase))
            {
                ShowErrorMessageDialog("This executable isn't a Unity build. Please only select Unity build executables");
                return false;
            }

            ShowErrorMessageDialog("This executable isn't a Unity build. Please only select Unity build executables");

            return false;
        }

        private void ShowErrorMessageDialog(string message)
        {
            string messageBoxText = message;
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBoxResult result = MessageBox.Show(messageBoxText, null, button, icon, MessageBoxResult.Yes);
        }

        /// <summary>
        /// Removes all BepInEx mod files from the current selected game build.
        /// </summary>
        private async Task CleanUpBepInExFiles()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsListening = false;
                ApplicationTitle = "Simple External Unity Tool - Disconnected";
            });

            try
            {
                string winhttpPath = Path.Combine(_currentBuild.GameDirectory, "winhttp.dll");
                if (File.Exists(winhttpPath)) File.Delete(winhttpPath);

                string doorstopPath = Path.Combine(_currentBuild.GameDirectory, "doorstop_config.ini");
                if (File.Exists(doorstopPath)) File.Delete(doorstopPath);

                string changeLogPath = Path.Combine(_currentBuild.GameDirectory, "changelog.txt");
                if (File.Exists(changeLogPath)) File.Delete(changeLogPath);

                string doorstopVersionPath = Path.Combine(_currentBuild.GameDirectory, ".doorstop_version");
                if (File.Exists(doorstopVersionPath)) File.Delete(doorstopVersionPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BepInEx single files cleanup error: {ex.Message}");
            }

            try
            {
                if (Directory.Exists(_currentBuild.BepInExDirectory))
                {
                    string tempTrashPath = _currentBuild.BepInExDirectory + "_To_Delete_" + Guid.NewGuid().ToString().Substring(0, 8);

                    Directory.Move(_currentBuild.BepInExDirectory, tempTrashPath);

                    var di = new DirectoryInfo(tempTrashPath);
                    foreach (var file in di.GetFiles("*", SearchOption.AllDirectories)) file.Attributes = FileAttributes.Normal;
                    foreach (var dir in di.GetDirectories("*", SearchOption.AllDirectories)) dir.Attributes = FileAttributes.Normal;
                    di.Attributes = FileAttributes.Normal;

                    Directory.Delete(tempTrashPath, true);
                }

                Debug.WriteLine("BepInEx cleaned up successfully");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cleanup Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates and starts server-side named pipe.
        /// </summary>
        private void StartPipeServer()
        {
            while (IsListening)
            {
                try
                {
                    using (var pipeServer = new NamedPipeServerStream("UnityLogPipe", PipeDirection.In))
                    {
                        pipeServer.WaitForConnection();

                        using (var reader = new StreamReader(pipeServer))
                        {
                            string logLine;
                            while ((logLine = reader.ReadLine()) != null)
                            {
                                try
                                {
                                    Debug.WriteLine($"New log: {logLine}");
                                    UnityLogEntry newLogEntry = JsonSerializer.Deserialize<UnityLogEntry>(logLine);
                                    AddNewLog(newLogEntry);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                }
                            }
                        }
                    }
                } 
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    break;
                }
            }
        }

        /// <summary>
        /// Adds a new log to 'LogMessages', while also removing the oldest log if over the log limit.
        /// </summary>
        /// <param name="newLogEntry">New Unity log entry.</param>
        private void AddNewLog(UnityLogEntry newLogEntry)
        {
            if (newLogEntry == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessages.Insert(0, newLogEntry);

                ClearConsoleCommand.NotifyCanExecuteChanged();
                ExportLogsCommand.NotifyCanExecuteChanged();

                if (LogMessages.Count > MainSettings.MaxLogs)
                {
                    UnityLogEntry oldLog = LogMessages[LogMessages.Count - 1];
                    switch (oldLog.LogType)
                    {
                        case "Log":
                            LogCount--;
                            break;

                        case "Warning":
                            WarningCount--;
                            break;

                        case "Error":
                        case "Exception":
                            ErrorCount--;
                            break;
                    }

                    LogMessages.RemoveAt(LogMessages.Count - 1); 
                }

                switch (newLogEntry.LogType)
                {
                    case "Log":
                        LogCount++;
                        break;

                    case "Warning":
                        WarningCount++;
                        break;

                    case "Error":
                    case "Exception":
                        ErrorCount++;
                        break;
                }
            });
        }

        /// <summary>
        /// Gives the user warning conditions if it is still connected to a Unity build.
        /// </summary>
        /// <param name="e">Window closing event.</param>
        [RelayCommand]
        private void WindowClosing(CancelEventArgs e)
        {
            if (IsListening)
            {
                string messageBoxText = "You are still connected to a Unity build. Do you wish to close the build before closing?";
                string caption = "Warning!";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    CloseCleanup(e);
                }
                else if (result == MessageBoxResult.No)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private async void CloseCleanup(CancelEventArgs e)
        {
            IsListening = false;

            try
            {
                if (_currentBuild.Process != null && !_currentBuild.Process.HasExited)
                {
                    ApplicationTitle = "CLOSING!";
                    _currentBuild.Process.Kill();
                    _currentBuild.Process.WaitForExit();
                }

                CleanUpBepInExFiles().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Close Cleanup Error: {ex}");
            }

            e.Cancel = false;
        }

        public async Task SaveSettings()
        {
            try
            {
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                string fileName = "SavedSettings.json";
                string fullPath = Path.Combine(baseFolder, fileName);

                string jsonString = JsonSerializer.Serialize(MainSettings);
                
                await File.WriteAllTextAsync(fullPath, jsonString);
                
                Debug.WriteLine("Settings Saved");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save Settings Failed: {ex}");
            }
        }

        public async Task ImportSettings()
        {
            try
            {
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                string fileName = "SavedSettings.json";
                string fullPath = Path.Combine(baseFolder, fileName);

                if (File.Exists(fullPath))
                {
                    string jsonString = File.ReadAllText(fullPath);
                    MainSettings = JsonSerializer.Deserialize<Settings>(jsonString);
                    Debug.WriteLine("Settings successfully imported");
                    return;
                }

                await SaveSettings();
                Debug.WriteLine("No Previous Settings. Creating new Settings File!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Importing Saved Settings: {ex}");
            }
        }
    }
}
