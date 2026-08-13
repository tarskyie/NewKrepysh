using Microsoft.UI.Xaml.Controls;
using NewKrepysh.WinUI.ViewModels;

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
}
