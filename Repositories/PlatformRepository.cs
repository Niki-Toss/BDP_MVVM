using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;

namespace BDP_MVVM.Repositories
{
    // Репозиторий для работы с платформами автопроверки задач
    // Платформы: Codeforces, AtCoder, Yandex Contest и другие
    public class PlatformRepository : IPlatformRepository
    {
        private readonly string _connectionString;
        public PlatformRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        #region Public Methods (CRUD Operations)
        // Получить все платформы с подсчётом количества задач на каждой
        public async Task<List<Platform>> GetAllAsync()
        {
            var platforms = new List<Platform>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT p.Platform_ID, p.Название, p.Автопроверка_готовности,
                               COUNT(tp.Task_ID) as КоличествоЗадач
                        FROM Platform p
                        LEFT JOIN Task_Platform tp ON p.Platform_ID = tp.Platform_ID
                        GROUP BY p.Platform_ID, p.Название, p.Автопроверка_готовности
                        ORDER BY p.Название";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            platforms.Add(new Platform
                            {
                                Platform_ID = reader.GetInt32(0),
                                Название = reader.GetString(1),
                                Автопроверка_готовности = !reader.IsDBNull(2) && reader.GetBoolean(2),
                                КоличествоЗадач = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки платформ: {ex.Message}");
            }
            return platforms;
        }
        // Получить платформу по ID
        public async Task<Platform> GetByIdAsync(int platformId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT Platform_ID, Название, Автопроверка_готовности 
                        FROM Platform 
                        WHERE Platform_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", platformId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Platform
                                {
                                    Platform_ID = reader.GetInt32(0),
                                    Название = reader.GetString(1),
                                    Автопроверка_готовности = !reader.IsDBNull(2) && reader.GetBoolean(2)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения платформы: {ex.Message}");
            }
            return null;
        }
        // Создать новую платформу
        public async Task<int> CreateAsync(Platform platform)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        INSERT INTO Platform (Название, Автопроверка_готовности)
                        VALUES (@Название, @Авто);
                        SELECT last_insert_rowid();";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Название", platform.Название);
                        command.Parameters.AddWithValue("@Авто", platform.Автопроверка_готовности);
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания платформы: {ex.Message}");
                return -1;
            }
        }
        // Обновить существующую платформу
        public async Task<bool> UpdateAsync(Platform platform)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        UPDATE Platform 
                        SET Название = @Название, Автопроверка_готовности = @Авто 
                        WHERE Platform_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Название", platform.Название);
                        command.Parameters.AddWithValue("@Авто", platform.Автопроверка_готовности);
                        command.Parameters.AddWithValue("@Id", platform.Platform_ID);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления платформы: {ex.Message}");
                return false;
            }
        }
        // Удалить платформу по ID
        public async Task<bool> DeleteAsync(int platformId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM Platform WHERE Platform_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", platformId);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления платформы: {ex.Message}");
                return false;
            }
        }
        #endregion
        #region Related Data Methods
        // Проверить, используется ли платформа в задачах
        // Используется для защиты от удаления платформ с задачами
        public async Task<bool> IsUsedInTasksAsync(int platformId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT COUNT(1) FROM Task_Platform WHERE Platform_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", platformId);
                        return (int)await command.ExecuteScalarAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки использования: {ex.Message}");
                return false;
            }
        }
        // Получить платформы для конкретной задачи с флагами готовности
        public async Task<List<TaskPlatformItem>> GetPlatformsByTaskAsync(int taskId)
        {
            var items = new List<TaskPlatformItem>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT Platform_ID, Готовность 
                        FROM Task_Platform 
                        WHERE Task_ID = @TaskId";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", taskId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(new TaskPlatformItem
                                {
                                    PlatformId = reader.GetInt32(0),
                                    Готовность = !reader.IsDBNull(1) && reader.GetBoolean(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения платформ задачи: {ex.Message}");
            }
            return items;
        }
        // Сохранить связи между задачей и платформами с флагами готовности
        // Старые связи удаляются, новые создаются
        public async Task<bool> SaveTaskPlatformsAsync(int taskId, IEnumerable<TaskPlatformItem> items)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Удаляем все существующие связи
                    string delete = "DELETE FROM Task_Platform WHERE Task_ID = @TaskId";
                    using (var cmd = new SQLiteCommand(delete, connection))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", taskId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    // Добавляем новые связи с флагами готовности
                    foreach (var item in items)
                    {
                        string insert = @"
                            INSERT INTO Task_Platform (Task_ID, Platform_ID, Готовность)
                            VALUES (@TaskId, @PlatformId, @Готовность)";

                        using (var cmd = new SQLiteCommand(insert, connection))
                        {
                            cmd.Parameters.AddWithValue("@TaskId", taskId);
                            cmd.Parameters.AddWithValue("@PlatformId", item.PlatformId);
                            cmd.Parameters.AddWithValue("@Готовность", item.Готовность);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                return true;
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