using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NewKrepysh.WinUI.Services;
using NewKrepysh.WinUI.ViewModels;

namespace NewKrepysh.WinUI.Views
{
    public sealed partial class PublishMenu : UserControl
    {
        private EditorViewModel? _viewModel;

        public EditorViewModel? ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;
                this.DataContext = _viewModel;  
            }
        }


        public PublishMenu()
        {
            InitializeComponent();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            PublishingService.Export(new Models.Project() { Id = ViewModel.ProjectId, Name = ViewModel.ProjectName, Pages = ViewModel.Pages });
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            PublishingService.Publish(EmailTextBox.Text, PasswordPasswordBox.Password, new Models.Project() { Id = ViewModel.ProjectId, Name = ViewModel.ProjectName, Pages = ViewModel.Pages });
        }
    }
}
