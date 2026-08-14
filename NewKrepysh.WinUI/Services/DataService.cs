using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NewKrepysh.WinUI.Models;

namespace NewKrepysh.WinUI.Services
{
    public static class DataService
    {
        private static string databaseName = "projects.db";
        private static string connectionString = $"Data Source={ProjectService.AppDataDir}\\{databaseName}";

        private static void CreateDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Projects (
                        Id VARCHAR(36) NOT NULL PRIMARY KEY,
                        Name NVARCHAR(255) NOT NULL DEFAULT 'Untitled Project',
                        LastModified DATETIME NOT NULL DEFAULT (GETUTCDATE())
                    );
                ";
                command.ExecuteNonQuery();
            }
        }

        public static List<Project> GetProjects()
        {
            if (!File.Exists(Path.Combine(ProjectService.AppDataDir, databaseName)))
            {
                CreateDatabase();
            }

            List<Project> projects = new List<Project>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, LastModified FROM Projects";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var project = new Project
                        {
                            Id = reader.GetString(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            LastModified = reader.GetDateTime(reader.GetOrdinal("LastModified"))
                            // Pages is excluded since it's not in the table
                        };

                        projects.Add(project);
                    }
                }
            }

            return projects;
        }

        public static void SaveData(IList<Project> projects)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Projects";
                command.ExecuteNonQuery();
                foreach (var project in projects)
                {
                    command.CommandText = $"INSERT INTO Projects (Id, Name, LastModified) VALUES ('{project.Id}', '{project.Name}', '{project.LastModified}')";
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateProjectEntry(Project project)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string query = @"UPDATE Projects 
                         SET Name = @Name, LastModified = @LastModified 
                         WHERE Id = @Id";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", project.Id);
                    command.Parameters.AddWithValue("@Name", project.Name);
                    command.Parameters.AddWithValue("@LastModified", project.LastModified);

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.WriteLine($"{rowsAffected} row(s) updated.");
                }
            }
        }
    }
}
