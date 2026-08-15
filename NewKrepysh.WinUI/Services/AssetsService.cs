using Microsoft.Windows.Storage.Pickers;
using NewKrepysh.WinUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewKrepysh.WinUI.Services
{
    public static class AssetsService
    {
        public static async Task<string> SelectAndSaveAnAsset(string project)
        {
            string output = string.Empty;

            if (App.MainWindowInstance == null) return output;
            var openPicker = new FileOpenPicker(App.MainWindowInstance.AppWindow.Id);

            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".webp");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".txt");
            //openPicker.FileTypeFilter.Add(".*");

            var result = await openPicker.PickSingleFileAsync();

            if (result != null)
            {
                output = Path.GetFileName(result.Path);
                var dir = Path.Combine(ProjectService.AppDataDir, "assets", project);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Copy(result.Path, Path.Combine(dir, output), true);
            }

            return output;
        }
    }
}
