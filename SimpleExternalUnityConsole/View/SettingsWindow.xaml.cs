using SimpleExternalUnityConsole.ModelView;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SimpleExternalUnityConsole.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        SettingsWindowViewModel viewModel;

        public SettingsWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();

            viewModel = new SettingsWindowViewModel(mainWindowViewModel);
            this.DataContext = viewModel;
        }
    }
}
