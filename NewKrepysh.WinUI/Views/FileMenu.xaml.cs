using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace NewKrepysh.WinUI.Views
{
    public sealed partial class FileMenu : Page
    {
        public FileMenu()
        {
            InitializeComponent();
        }

        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            App.MainWindowInstance.Navigate(typeof(EditorPage));
        }
    }
}
