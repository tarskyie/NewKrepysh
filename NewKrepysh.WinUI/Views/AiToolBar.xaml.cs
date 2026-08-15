using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NewKrepysh.WinUI.Services;
using NewKrepysh.WinUI.ViewModels;

namespace NewKrepysh.WinUI.Views
{
    public sealed partial class AiToolBar : UserControl
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

        public AiToolBar()
        {
            InitializeComponent();
        }

        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            if (ViewModel.SelectedSitePage == null) return;
            string markup = await AiAssistanceService.GenerateBody(ViewModel.SelectedSitePage, UrlTextBox.Text, model: ModelTextBox.Text, apiKey: KeyPasswordBox.Password, prompt: PromptTextBox.Text,
                assets: ViewModel.Assets);
            ViewModel.SelectedSitePage.HtmlContent = markup;
        }
    }
}
