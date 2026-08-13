using Microsoft.UI.Xaml.Controls;
using NewKrepysh.WinUI.ViewModels;

namespace NewKrepysh.WinUI.Views;

public sealed partial class EditorPage : Page
{
    public EditorViewModel ViewModel { get; set; }

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

        //ViewModel.NewPageCommand.Execute(null);
    }

    private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (MainSelectionBar.SelectedItem == SelectorBarItemFile) 
        {
            MainSelectionBar.SelectedItem = SelectorBarItemMain;
            if (App.MainWindowInstance == null) return;
            App.MainWindowInstance.Navigate(typeof(FileMenu)); 
        }
    }
}
