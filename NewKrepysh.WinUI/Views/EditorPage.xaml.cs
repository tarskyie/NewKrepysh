using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using NewKrepysh.WinUI.Models;
using NewKrepysh.WinUI.Services;
using NewKrepysh.WinUI.ViewModels;
using System;
using System.IO;

namespace NewKrepysh.WinUI.Views;

public sealed partial class EditorPage : Page
{
    public EditorViewModel ViewModel { get; set; }
    private PreviewServer previewServer = new();
    public EditorPage()
    {
        InitializeComponent();
        MainSelectionBar.SelectedItem = SelectorBarItemMain;
        ToolFrame.Content = new MainToolBar();
        ViewModel = ViewModel ?? new EditorViewModel();
        this.DataContext = ViewModel;

        if (ToolFrame.Content is MainToolBar mainToolBar)
        {
            mainToolBar.ViewModel = ViewModel;
        }

        //StartPreview();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Project project)
        {
            ViewModel.LoadProject(project);
            StartPreview();
        }
    }

    public void StartPreview()
    {
        if (ViewModel.Pages == null)
            return;

        string outputDir = Path.Combine(
            ProjectService.AppDataDir,
            "previews",
            ViewModel.ProjectId
        );

        SiteBuilder.Build(new Project() { Id = ViewModel.ProjectId,
        Name= ViewModel.ProjectName,
        Pages = ViewModel.Pages,
        Assets = ViewModel.Assets }, outputDir);

        string? url = previewServer.Start(outputDir);

        if (url != null)
        {
            PreviewWebView2.Source = new Uri(url);
        }
    }

    private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (MainSelectionBar.SelectedItem == SelectorBarItemFile) 
        {
            MainSelectionBar.SelectedItem = SelectorBarItemMain;
            if (App.MainWindowInstance == null) return;
            App.MainWindowInstance.Navigate(typeof(FileMenu));
            return;
        }
        if (MainSelectionBar.SelectedItem == SelectorBarItemPublish)
            ToolFrame.Content = new PublishMenu() { ViewModel = this.ViewModel };
        if (MainSelectionBar.SelectedItem == SelectorBarItemMain)
            ToolFrame.Content = new MainToolBar() { ViewModel = this.ViewModel };
        if (MainSelectionBar.SelectedItem == SelectorBarItemAi)
            ToolFrame.Content = new AiToolBar() { ViewModel = this.ViewModel }; 
        if (MainSelectionBar.SelectedItem == SelectorBarItemAssets)
            ToolFrame.Content = new AssetsToolBar() { ViewModel = this.ViewModel };
    }
}
