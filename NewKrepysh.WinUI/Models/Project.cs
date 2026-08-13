using System;
using System.Collections.ObjectModel;

namespace NewKrepysh.WinUI.Models
{
    public class Project
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Untitled Project";
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public ObservableCollection<SitePage> Pages { get; set; } = new();
    }
}
