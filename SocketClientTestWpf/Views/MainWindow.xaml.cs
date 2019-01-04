using System.Windows;
using SocketClientTestWpf.ViewModels;

namespace SocketClientTestWpf.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            base.Loaded += MainWindow_Loaded;
            base.Closing += MainWindow_Closing;
            InitializeComponent();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            base.Loaded -= MainWindow_Loaded;

            GetViewModel(sender)?.Init(base.Dispatcher);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            base.Closing -= MainWindow_Closing;

            GetViewModel(sender)?.Close();
        }

        MainWindowViewModel GetViewModel(object sender)
        {
            if (sender is MainWindow view) {
                if (view.DataContext is MainWindowViewModel vm)
                    return vm;
            }
            return null;
        }
    }
}
