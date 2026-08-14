using Microsoft.UI;
using Microsoft.UI.Windowing;
using NewKrepysh.WinUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Windows.System;
using WinRT.Interop;
using Microsoft.Windows.Storage.Pickers;

namespace NewKrepysh.WinUI.Services
{
    public static class PublishingService
    {
        private static string PackageProject(Project project)
        {
            string zipFilePath = Path.Combine(ProjectService.AppDataDir, "build", $"{SiteBuilder.SanitizeFilename(project.Name)}.zip");
            string folderPath = Path.Combine(ProjectService.AppDataDir, "build", "site");

            if (Directory.Exists(Path.Combine(ProjectService.AppDataDir, "build")))
            {
                Directory.Delete(Path.Combine(ProjectService.AppDataDir, "build"), true );
            }

            SiteBuilder.Build(project.Pages, folderPath);
            
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(folderPath, zipFilePath);

            return zipFilePath;
        }
        public static async void Export(Project project)
        {
            var location = PackageProject(project);

            var windowId = Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(App.MainWindowInstance));
            var savePicker = new FileSavePicker(windowId);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.SuggestedFileName = $"{SiteBuilder.SanitizeFilename(project.Name)}.zip";

            savePicker.FileTypeChoices.Add("Zip Folder", new List<string>() { ".zip" });

            PickFileResult result = await savePicker.PickSaveFileAsync();

            if (result != null && !string.IsNullOrEmpty(result.Path))
            {
                File.Copy(location, result.Path, true);
            }
        }
    }
}
