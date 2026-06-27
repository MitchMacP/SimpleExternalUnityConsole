using SimpleExternalUnityConsole.ModelView;
using System.Collections.Specialized;
using System.Windows;

namespace SimpleExternalUnityConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;
        }
    }
}