using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NewKrepysh.WinUI.Models;
using NewKrepysh.WinUI.Services;
using NewKrepysh.WinUI.ViewModels;

namespace NewKrepysh.WinUI.Views
{
    public sealed partial class FileMenu : Page
    {
        public FileMenuViewModel ViewModel { get; set; }

        public FileMenu()
        {
            InitializeComponent();
            ViewModel = ViewModel ?? new();
            this.DataContext = ViewModel;
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is Project project)
            {
                Project? loadedProject = ProjectService.LoadProject(project.Id);
                if (App.MainWindowInstance == null) return;

                if (loadedProject == null) {
                    project.Pages = new EditorViewModel().Pages;
                    App.MainWindowInstance.OpenProjectInEditor(project);
                    return;
                }

                App.MainWindowInstance.OpenProjectInEditor(loadedProject);
            }
        }
    }
}
