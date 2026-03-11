using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;

namespace BDP_MVVM.Repositories
{
    // Репозиторий для работы с тегами задач (темы и категории алгоритмов)
    public class TagRepository : ITagRepository
    {
        private readonly string _connectionString;
        public TagRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        #region Public Methods (CRUD Operations)
        // Получить все теги с подсчётом количества задач в каждом
        public async Task<List<Tag>> GetAllAsync()
        {
            var tags = new List<Tag>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT t.Tag_ID, t.Название, COUNT(tt.Task_ID) as КоличествоЗадач
                        FROM Tags t
                        LEFT JOIN Task_Tag tt ON t.Tag_ID = tt.Tag_ID
                        GROUP BY t.Tag_ID, t.Название
                        ORDER BY t.Название";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tags.Add(new Tag
                            {
                                Tag_ID = reader.GetInt32(0),
                                Название = reader.GetString(1),
                                КоличествоЗадач = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки тегов: {ex.Message}");
            }
            return tags;
        }
        // Получить тег по ID
        public async Task<Tag> GetByIdAsync(int tagId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Tag_ID, Название FROM Tags WHERE Tag_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", tagId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Tag
                                {
                                    Tag_ID = reader.GetInt32(0),
                                    Название = reader.GetString(1)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения тега: {ex.Message}");
            }
            return null;
        }
        // Создать новый тег
        public async Task<int> CreateAsync(Tag tag)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        INSERT INTO Tags (Название) 
                        VALUES (@Название);
                        SELECT last_insert_rowid();";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Название", tag.Название);
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания тега: {ex.Message}");
                return -1;
            }
        }
        // Обновить существующий тег
        public async Task<bool> UpdateAsync(Tag tag)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "UPDATE Tags SET Название = @Название WHERE Tag_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Название", tag.Название);
                        command.Parameters.AddWithValue("@Id", tag.Tag_ID);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления тега: {ex.Message}");
                return false;
            }
        }
        // Удалить тег по ID
        // Также удаляются все связи с задачами
        public async Task<bool> DeleteAsync(int tagId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Удаляем связи с задачами
                    string deleteLinks = "DELETE FROM Task_Tag WHERE Tag_ID = @Id";
                    using (var cmd = new SQLiteCommand(deleteLinks, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", tagId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    // Удаляем сам тег
                    string deleteTag = "DELETE FROM Tags WHERE Tag_ID = @Id";
                    using (var cmd = new SQLiteCommand(deleteTag, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", tagId);
                        return await cmd.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления тега: {ex.Message}");
                return false;
            }
        }
        #endregion
        #region Related Data Methods
        // Проверить, используется ли тег в задачах
        // Используется для защиты от удаления тегов с задачами
        public async Task<bool> IsUsedInTasksAsync(int tagId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT COUNT(1) FROM Task_Tag WHERE Tag_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", tagId);
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки использования: {ex.Message}");
                return false;
            }
        }
        // Получить ID тегов, связанных с конкретной задачей
        public async Task<List<int>> GetTagIdsByTaskAsync(int taskId)
        {
            var ids = new List<int>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Tag_ID FROM Task_Tag WHERE Task_ID = @TaskId";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", taskId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ids.Add(reader.GetInt32(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения тегов задачи: {ex.Message}");
            }
            return ids;
        }
        // Сохранить связи между задачей и тегами
        // Старые связи удаляются, новые создаются
        public async Task<bool> SaveTaskTagsAsync(int taskId, IEnumerable<int> tagIds)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Удаляем старые связи
                    string delete = "DELETE FROM Task_Tag WHERE Task_ID = @TaskId";
                    using (var cmd = new SQLiteCommand(delete, connection))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", taskId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    // Добавляем новые связи
                    foreach (var tagId in tagIds)
                    {
                        string insert = "INSERT INTO Task_Tag (Task_ID, Tag_ID) VALUES (@TaskId, @TagId)";
                        using (var cmd = new SQLiteCommand(insert, connection))
                        {
                            cmd.Parameters.AddWithValue("@TaskId", taskId);
                            cmd.Parameters.AddWithValue("@TagId", tagId);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения связей: {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}