using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace NewKrepysh.WinUI.Models;

public partial class SitePage : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    public string HtmlContent { get; set; } = string.Empty;

    public ObservableCollection<SitePage> Children { get; } = new();
}
