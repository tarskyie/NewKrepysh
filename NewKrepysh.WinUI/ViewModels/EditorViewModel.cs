using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewKrepysh.WinUI.Models;
using NewKrepysh.WinUI.Services;

namespace NewKrepysh.WinUI.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string projectId = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string projectName = "Untitled Project";

        [ObservableProperty]
        private ObservableCollection<SitePage> pages = new()
        {
            new SitePage
            {
                Title = "Home",
                Children =
                {
                    new SitePage { Title = "About" },
                    new SitePage { Title = "Contact" }
                }
            }
        };

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveSelectedPageCommand))]
        private SitePage? selectedSitePage;

        [RelayCommand]
        private void NewPage()
        {
            var newPage = new SitePage() { Title = "New Page" };
            Pages.Add(newPage);
            SelectedSitePage = newPage;
        }

        [RelayCommand]
        private void NewSubPage()
        {
            if (SelectedSitePage == null)
            {
                NewPage();
                return;
            }
            var newSub = new SitePage() { Title = "New Sub Page" };
            SelectedSitePage.Children.Add(newSub);
            // Optional: select the newly created subpage
            SelectedSitePage = newSub;
        }

        [RelayCommand(CanExecute = nameof(CanRemoveSelectedPage))]
        private void RemoveSelectedPage()
        {
            if (SelectedSitePage == null) return;

            // Try remove from root pages
            if (Pages.Remove(SelectedSitePage))
            {
                SelectedSitePage = null;
                return;
            }

            // Otherwise search recursively in children
            foreach (var root in Pages.ToList())
            {
                if (RemoveFromChildren(root, SelectedSitePage))
                {
                    SelectedSitePage = null;
                    return;
                }
            }
        }

        private bool RemoveFromChildren(SitePage parent, SitePage target)
        {
            if (parent.Children.Remove(target)) return true;

            // iterate over a snapshot to avoid collection-modification issues
            foreach (var child in parent.Children.ToList())
            {
                if (RemoveFromChildren(child, target)) return true;
            }

            return false;
        }

        private bool CanRemoveSelectedPage() => SelectedSitePage != null;

        public void LoadProject(Project project)
        {
            ProjectId = project.Id;
            ProjectName = project.Name;
            Pages = project.Pages;
            SelectedSitePage = Pages.FirstOrDefault();
        }

        public void Save()
        {
            var project = new Project
            {
                Id = ProjectId,
                Name = ProjectName,
                Pages = Pages
            };
            ProjectService.SaveProject(project);
        }
    }
}
