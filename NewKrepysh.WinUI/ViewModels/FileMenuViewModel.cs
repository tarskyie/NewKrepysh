using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewKrepysh.WinUI.Models;
using NewKrepysh.WinUI.Services;

namespace NewKrepysh.WinUI.ViewModels
{
    public partial class FileMenuViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Project> projects = new ObservableCollection<Project>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteProjectCommand))]
        private Project? selectedProject; 

        public FileMenuViewModel()
        {
            projects = new ObservableCollection<Project>(DataService.GetProjects());
        }

        [RelayCommand]
        private void NewProject()
        {
            Projects.Add(new Project());
            DataService.SaveData(Projects);
        }

        [RelayCommand(CanExecute = nameof(SelectedIsNotNull))]
        private void DeleteProject()
        {
            if (SelectedProject == null) return;
            ProjectService.DeleteProject(SelectedProject.Id);
            Projects.Remove(SelectedProject);
            DataService.SaveData(Projects);
        }

        private bool SelectedIsNotNull()
        {
            if (SelectedProject != null) return true;
            return false;
        }
    }
}
