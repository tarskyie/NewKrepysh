using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewKrepysh.WinUI.Models;
using NewKrepysh.WinUI.Services;

namespace NewKrepysh.WinUI.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        #region PublishMenu
        [ObservableProperty]
        private string email;
        [ObservableProperty]
        private string password;
        #endregion

        #region AiToolBar
        [ObservableProperty]
        private string aiUrl;
        [ObservableProperty]
        private string aiKey;
        [ObservableProperty]
        private string aiModel;
        [ObservableProperty]
        private string aiPrompt;
        #endregion


        [ObservableProperty]
        private string projectId = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string projectName = "Untitled Project";

        [ObservableProperty]
        private ObservableCollection<string> assets = new(); 

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

        [ObservableProperty]
        private string? selectedAsset;

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

        [RelayCommand]
        private async Task AddAsset()
        {
            string newAssetName = await AssetsService.SelectAndSaveAnAsset(ProjectId);
            if (string.IsNullOrWhiteSpace(newAssetName)) return;
            Assets.Add(newAssetName);
            Save();
        }
        [RelayCommand(CanExecute = nameof(CanRemoveSelectedAsset))]
        private void RemoveAsset()
        {
            if (SelectedAsset == null) return;
            var filePath = Path.Combine(ProjectService.AppDataDir, "assets", ProjectId, SelectedAsset);
            if (File.Exists(filePath))
                File.Delete(filePath);
            Assets.Remove(SelectedAsset);
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
        private bool CanRemoveSelectedAsset() => SelectedAsset != null;

        public void LoadProject(Project project)
        {
            ProjectId = project.Id;
            ProjectName = project.Name;
            Pages = project.Pages;
            Assets = project.Assets;
            SelectedSitePage = Pages.FirstOrDefault();
        }

        public void Save()
        {
            var project = new Project
            {
                Id = ProjectId,
                Name = ProjectName,
                Pages = Pages,
                Assets = Assets
            };
            ProjectService.SaveProject(project);
        }
    }
}
