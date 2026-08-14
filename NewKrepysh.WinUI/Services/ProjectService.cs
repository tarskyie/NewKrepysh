using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NewKrepysh.WinUI.Models;

namespace NewKrepysh.WinUI.Services
{
    public static class ProjectService
    {
        public static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NewKrepysh"
        );

        public static readonly string ProjectsDir = Path.Combine(AppDataDir, "projects");

        static ProjectService()
        {
            Directory.CreateDirectory(ProjectsDir);
        }

        public static void SaveProject(Project project)
        {
            project.LastModified = DateTime.UtcNow;
            string filePath = Path.Combine(ProjectsDir, $"{project.Id}.json");
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            string json = JsonSerializer.Serialize(project, options);
            File.WriteAllText(filePath, json);

            DataService.UpdateProjectEntry(project);
        }

        public static List<Project> GetProjects()
        {
            var list = new List<Project>();
            if (!Directory.Exists(ProjectsDir)) return list;

            foreach (var file in Directory.GetFiles(ProjectsDir, "*.json"))
            {
                try
                 {
                    string json = File.ReadAllText(file);
                    var project = JsonSerializer.Deserialize<Project>(json);
                    if (project != null)
                    {
                        list.Add(project);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading project file {file}: {ex.Message}");
                }
            }

            // Sort by last modified descending
            list.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));
            return list;
        }

        public static Project? LoadProject(string id)
        {
            string filePath = Path.Combine(ProjectsDir, $"{id}.json");
            if (!File.Exists(filePath)) return null;

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<Project>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading project {id}: {ex.Message}");
                return null;
            }
        }

        public static void DeleteProject(string id)
        {
            string filePath = Path.Combine(ProjectsDir, $"{id}.json");
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting project {id}: {ex.Message}");
                }
            }

            // Also delete its preview directory if it exists
            string previewDir = Path.Combine(AppDataDir, "previews", id);
            if (Directory.Exists(previewDir))
            {
                try
                {
                    Directory.Delete(previewDir, true);
                }
                catch { }
            }
        }
    }
}
