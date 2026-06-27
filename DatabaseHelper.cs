using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace CyberBotGUI
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reminder { get; set; }
        public bool IsCompleted { get; set; }
    }

    public static class DatabaseHelper
    {
        private static string _connectionString = "Data Source=cyberbot.db";

        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            string createTable = @"CREATE TABLE IF NOT EXISTS Tasks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT,
                Reminder TEXT,
                IsCompleted INTEGER DEFAULT 0)";
            using var command = new SqliteCommand(createTable, connection);
            command.ExecuteNonQuery();
        }

        public static void AddTask(string title, string description, string reminder)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            string insert = "INSERT INTO Tasks (Title, Description, Reminder) VALUES (@title, @desc, @reminder)";
            using var command = new SqliteCommand(insert, connection);
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@desc", description);
            command.Parameters.AddWithValue("@reminder", reminder ?? "");
            command.ExecuteNonQuery();
        }

        public static List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            string select = "SELECT * FROM Tasks WHERE IsCompleted = 0";
            using var command = new SqliteCommand(select, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new TaskItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    Reminder = reader.GetString(3),
                    IsCompleted = reader.GetInt32(4) == 1
                });
            }
            return tasks;
        }

        public static void CompleteTask(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            string update = "UPDATE Tasks SET IsCompleted = 1 WHERE Id = @id";
            using var command = new SqliteCommand(update, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public static void DeleteTask(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            string delete = "DELETE FROM Tasks WHERE Id = @id";
            using var command = new SqliteCommand(delete, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }
    }
}