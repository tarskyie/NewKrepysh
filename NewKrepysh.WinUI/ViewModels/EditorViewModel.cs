using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewKrepysh.WinUI.Views;
using System.Collections.ObjectModel;
using System.Linq;
using NewKrepysh.WinUI.Models;

namespace NewKrepysh.WinUI.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
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
            Pages.Add(new SitePage() { Title="New Page" });
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
    }
}
