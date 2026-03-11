using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;

namespace BDP_MVVM.Repositories
{
    // Репозиторий для работы с задачами по программированию
    // Основная сущность системы - содержит задачи с их свойствами и связями
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;
        public TaskRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        #region Public Methods (CRUD Operations)
        // Получить все задачи с полными данными о тегах, платформах и контестах
        public async Task<List<ProgrammingTask>> GetAllAsync()
        {
            var tasks = new List<ProgrammingTask>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Загружаем основные данные задач
                    string query = @"
                        SELECT t.Task_ID, t.Название, t.Сложность, t.Краткое_условие,
                               t.Идея_решения, t.Ссылка_polygon, t.Примечание,
                               t.Дата_создания, t.Автор_ID,
                               u.Логин as АвторЛогин
                        FROM Task t
                        LEFT JOIN Users u ON t.Автор_ID = u.User_ID
                        ORDER BY t.Дата_создания DESC";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tasks.Add(new ProgrammingTask
                            {
                                Task_ID = reader.GetInt32(0),
                                Название = reader.GetString(1),
                                Сложность = reader.GetInt32(2),
                                Краткое_условие = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Идея_решения = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Ссылка_polygon = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Примечание = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Дата_создания = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                                Автор_ID = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                                Автор = reader.IsDBNull(9) ? null : new User { Логин = reader.GetString(9) }
                            });
                        }
                    }
                    // Загружаем связанные данные для каждой задачи
                    foreach (var task in tasks)
                    {
                        await LoadTaskTagsAsync(connection, task);
                        await LoadTaskPlatformsAsync(connection, task);
                        await LoadTaskContestsAsync(connection, task);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки задач: {ex.Message}");
            }
            return tasks;
        }
        // Фильтрация задач по сложности и тегам
        public async Task<List<ProgrammingTask>> FilterAsync(int? minDifficulty, int? maxDifficulty, List<int> tagIds = null)
        {
            var tasks = new List<ProgrammingTask>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT DISTINCT
                        t.Task_ID, t.Название, t.Сложность, t.Краткое_условие, 
                        t.Идея_решения, t.Ссылка_polygon, t.Примечание, 
                        t.Автор_ID, t.Дата_создания,
                        u.User_ID, u.Логин, u.Email
                    FROM Task t
                    LEFT JOIN Users u ON t.Автор_ID = u.User_ID
                    LEFT JOIN Task_Tag tt ON t.Task_ID = tt.Task_ID
                    WHERE 1=1";
                // Динамически добавляем условия фильтрации
                if (minDifficulty.HasValue)
                    query += " AND t.Сложность >= @MinDifficulty";
                if (maxDifficulty.HasValue)
                    query += " AND t.Сложность <= @MaxDifficulty";
                // Фильтр по тегам (задача должна содержать ВСЕ указанные теги)
                if (tagIds != null && tagIds.Count > 0)
                {
                    query += @" 
                        AND t.Task_ID IN (
                            SELECT Task_ID 
                            FROM Task_Tag 
                            WHERE Tag_ID IN (" + string.Join(",", tagIds) + @")
                            GROUP BY Task_ID 
                            HAVING COUNT(DISTINCT Tag_ID) = @TagCount
                        )";
                }
                query += " ORDER BY t.Task_ID DESC";
                using (var command = new SQLiteCommand(query, connection))
                {
                    if (minDifficulty.HasValue)
                        command.Parameters.AddWithValue("@MinDifficulty", minDifficulty.Value);
                    if (maxDifficulty.HasValue)
                        command.Parameters.AddWithValue("@MaxDifficulty", maxDifficulty.Value);
                    if (tagIds != null && tagIds.Count > 0)
                        command.Parameters.AddWithValue("@TagCount", tagIds.Count);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            tasks.Add(MapFromReader(reader));
                    }
                }
            }
            return tasks;
        }
        // Получить задачу по ID
        public async Task<ProgrammingTask> GetByIdAsync(int taskId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT 
                        t.Task_ID, t.Название, t.Сложность, t.Краткое_условие, 
                        t.Идея_решения, t.Ссылка_polygon, t.Примечание, 
                        t.Автор_ID, t.Дата_создания,
                        u.User_ID, u.Логин, u.Email
                    FROM Task t
                    LEFT JOIN Users u ON t.Автор_ID = u.User_ID
                    WHERE t.Task_ID = @TaskId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapFromReader(reader);
                    }
                }
            }
            return null;
        }
        // Создать новую задачу
        public async Task<int> CreateAsync(ProgrammingTask task, List<int> tagIds = null, List<int> contestIds = null)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    INSERT INTO Task (Название, Сложность, Краткое_условие, Идея_решения, 
                                     Ссылка_polygon, Примечание, Автор_ID)
                    VALUES (@Название, @Сложность, @Краткое_условие, @Идея_решения, 
                            @Ссылка_polygon, @Примечание, @Автор_ID);
                    SELECT last_insert_rowid();";
                using (var command = new SQLiteCommand(query, connection))
                {
                    AddTaskParameters(command, task);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        // Обновить существующую задачу
        public async Task<bool> UpdateAsync(ProgrammingTask task, List<int> tagIds = null, List<int> contestIds = null)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    UPDATE Task 
                    SET Название = @Название, 
                        Сложность = @Сложность, 
                        Краткое_условие = @Краткое_условие, 
                        Идея_решения = @Идея_решения, 
                        Ссылка_polygon = @Ссылка_polygon, 
                        Примечание = @Примечание
                    WHERE Task_ID = @Task_ID";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Task_ID", task.Task_ID);
                    AddTaskParameters(command, task);
                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }
        }
        // Удалить задачу по ID
        public async Task<bool> DeleteAsync(int taskId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Task WHERE Task_ID = @TaskId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }
        }
        #endregion
        #region Related Data Methods
        // Получить список тегов для конкретной задачи
        public async Task<List<Tag>> GetTagsForTaskAsync(int taskId)
        {
            var tags = new List<Tag>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT t.Tag_ID, t.Название
                    FROM Tags t
                    INNER JOIN Task_Tag tt ON t.Tag_ID = tt.Tag_ID
                    WHERE tt.Task_ID = @TaskId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tags.Add(new Tag
                            {
                                Tag_ID = reader.GetInt32(0),
                                Название = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return tags;
        }
        // Получить список контестов для конкретной задачи
        public async Task<List<Contest>> GetContestsForTaskAsync(int taskId)
        {
            var contests = new List<Contest>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT c.Contest_ID, c.Название, c.Год_создания
                    FROM Contest c
                    INNER JOIN Task_Contest tc ON c.Contest_ID = tc.Contest_ID
                    WHERE tc.Task_ID = @TaskId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            contests.Add(new Contest
                            {
                                Contest_ID = reader.GetInt32(0),
                                Название = reader.GetString(1),
                            });
                        }
                    }
                }
            }
            return contests;
        }
        #endregion
        #region Private Helper Methods
        // Загрузить теги для задачи (внутренний метод)
        private async Task LoadTaskTagsAsync(SQLiteConnection connection, ProgrammingTask task)
        {
            string query = @"
                SELECT t.Tag_ID, t.Название
                FROM Tags t
                INNER JOIN Task_Tag tt ON t.Tag_ID = tt.Tag_ID
                WHERE tt.Task_ID = @TaskId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TaskId", task.Task_ID);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int tagId = reader.GetInt32(0);
                        string tagName = reader.GetString(1);
                        task.TagIds.Add(tagId);
                        task.Теги.Add(new Tag
                        {
                            Tag_ID = tagId,
                            Название = tagName
                        });
                    }
                }
            }
        }
        // Загрузить платформы для задачи (внутренний метод)
        private async Task LoadTaskPlatformsAsync(SQLiteConnection connection, ProgrammingTask task)
        {
            string query = "SELECT Platform_ID FROM Task_Platform WHERE Task_ID = @TaskId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TaskId", task.Task_ID);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        task.PlatformIds.Add(reader.GetInt32(0));
                    }
                }
            }
        }
        // Загрузить контесты для задачи (внутренний метод)
        private async Task LoadTaskContestsAsync(SQLiteConnection connection, ProgrammingTask task)
        {
            string query = "SELECT Contest_ID FROM Task_Contest WHERE Task_ID = @TaskId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TaskId", task.Task_ID);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        task.ContestIds.Add(reader.GetInt32(0));
                    }
                }
            }
        }
        // Маппинг данных из DbDataReader в объект ProgrammingTask
        private ProgrammingTask MapFromReader(System.Data.Common.DbDataReader reader)
        {
            var task = new ProgrammingTask
            {
                Task_ID = reader.GetInt32(reader.GetOrdinal("Task_ID")),
                Название = reader.GetString(reader.GetOrdinal("Название")),
                Сложность = reader.GetInt32(reader.GetOrdinal("Сложность")),
                Краткое_условие = reader.IsDBNull(reader.GetOrdinal("Краткое_условие"))
                    ? null : reader.GetString(reader.GetOrdinal("Краткое_условие")),
                Идея_решения = reader.IsDBNull(reader.GetOrdinal("Идея_решения"))
                    ? null : reader.GetString(reader.GetOrdinal("Идея_решения")),
                Ссылка_polygon = reader.IsDBNull(reader.GetOrdinal("Ссылка_polygon"))
                    ? null : reader.GetString(reader.GetOrdinal("Ссылка_polygon")),
                Примечание = reader.IsDBNull(reader.GetOrdinal("Примечание"))
                    ? null : reader.GetString(reader.GetOrdinal("Примечание")),
                Автор_ID = reader.IsDBNull(reader.GetOrdinal("Автор_ID"))
                    ? (int?)null : reader.GetInt32(reader.GetOrdinal("Автор_ID")),
                Дата_создания = reader.GetDateTime(reader.GetOrdinal("Дата_создания"))
            };
            // Добавляем автора если присутствует в результате
            if (!reader.IsDBNull(reader.GetOrdinal("User_ID")))
            {
                task.Автор = new User
                {
                    User_ID = reader.GetInt32(reader.GetOrdinal("User_ID")),
                    Логин = reader.GetString(reader.GetOrdinal("Логин")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                        ? null : reader.GetString(reader.GetOrdinal("Email"))
                };
            }
            return task;
        }
        // Добавить параметры задачи в SQL команду
        private void AddTaskParameters(SQLiteCommand command, ProgrammingTask task)
        {
            command.Parameters.AddWithValue("@Название", task.Название ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Сложность", task.Сложность);
            command.Parameters.AddWithValue("@Краткое_условие", task.Краткое_условие ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Идея_решения", task.Идея_решения ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Ссылка_polygon", task.Ссылка_polygon ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Примечание", task.Примечание ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Автор_ID", task.Автор_ID ?? (object)DBNull.Value);
        }
        #endregion
    }
}