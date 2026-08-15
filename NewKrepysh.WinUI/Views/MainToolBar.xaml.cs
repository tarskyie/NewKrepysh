using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NewKrepysh.WinUI.ViewModels;
using NewKrepysh.WinUI.Services;
using System.Collections;
using NewKrepysh.WinUI.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace NewKrepysh.WinUI.Views;

public sealed partial class MainToolBar : UserControl
{
    private EditorViewModel? _viewModel;

    public EditorViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            this.DataContext = _viewModel;  // Update DataContext when ViewModel is set
        }
    }

    public MainToolBar()
    {
        InitializeComponent();
    }

    private void SaveProject_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is EditorViewModel)
        {
            ViewModel.Save();
        }
    }

    private void BuildSite_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is EditorViewModel)
        {
            SiteBuilder.Build(new Project()
            {
                Id = ViewModel.ProjectId,
                Name = ViewModel.ProjectName,
                Pages = ViewModel.Pages,
                Assets = ViewModel.Assets
            }, Path.Combine(ProjectService.AppDataDir, "previews", ViewModel.ProjectId));
        }
    }
}
