using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.Windows.Storage.Pickers;
using NewKrepysh.WinUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WinRT.Interop;

namespace NewKrepysh.WinUI.Services
{
    public static class PublishingService
    {
        private static HttpClient _httpClient = new();
        private static string _apiEndpoint = "https://ztcjx076-7270.asse.devtunnels.ms";

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

        public static async void Publish(string email, string password, Project project)
        {
            var location = PackageProject(project);

            var payload = new { email, password };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"{_apiEndpoint}/login", payload);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);
                if (tokenData is not TokenResponse) return;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.Token);

                using var form = new MultipartFormDataContent();
                using var fileStream = new FileStream(location, FileMode.Open, FileAccess.Read);
                using var streamContent = new StreamContent(fileStream);

                string formFieldName = "file";
                string fileName = Path.GetFileName(location);

                form.Add(streamContent, formFieldName, fileName);

                HttpResponseMessage uploadResponse = await _httpClient.PostAsync($"{_apiEndpoint}/upload", form);
            }
        }
    }
    public record TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
