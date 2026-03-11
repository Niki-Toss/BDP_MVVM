using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;

namespace BDP_MVVM.Repositories
{
    // Репозиторий для работы с контестами (соревнованиями по программированию)
    public class ContestRepository : IContestRepository
    {
        private readonly string _connectionString;
        public ContestRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        #region Public Methods (CRUD Operations)
        // Получить все контесты с подсчётом количества задач в каждом
        public async Task<List<Contest>> GetAllAsync()
        {
            var contests = new List<Contest>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT c.Contest_ID, c.Название, c.Год_создания,
                               COUNT(tc.Task_ID) as КоличествоЗадач
                        FROM Contest c
                        LEFT JOIN Task_Contest tc ON c.Contest_ID = tc.Contest_ID
                        GROUP BY c.Contest_ID, c.Название, c.Год_создания
                        ORDER BY c.Год_создания DESC, c.Название";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            contests.Add(new Contest
                            {
                                Contest_ID = reader.GetInt32(0),
                                Название = reader.GetString(1),
                                Год_создания = reader.GetInt32(2),
                                КоличествоЗадач = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки контестов: {ex.Message}");
            }
            return contests;
        }
        // Получить контест по ID
        public async Task<Contest> GetByIdAsync(int contestId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT Contest_ID, Название, Год_создания 
                        FROM Contest 
                        WHERE Contest_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", contestId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Contest
                                {
                                    Contest_ID = reader.GetInt32(0),
                                    Название = reader.GetString(1),
                                    Год_создания = reader.GetInt32(2)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения контеста: {ex.Message}");
            }
            return null;
        }
        // Создать новый контест
        public async Task<int> CreateAsync(Contest contest)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        INSERT INTO Contest (Название, Год_создания)
                        VALUES (@Название, @Год);
                        SELECT last_insert_rowid();";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Название", contest.Название);
                        command.Parameters.AddWithValue("@Год", contest.Год_создания);
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания контеста: {ex.Message}");
                return -1;
            }
        }
        // Обновить существующий контест
        public async Task<bool> UpdateAsync(Contest contest)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        UPDATE Contest 
                        SET Название = @Название, Год_создания = @Год
                        WHERE Contest_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Название", contest.Название);
                        command.Parameters.AddWithValue("@Год", contest.Год_создания);
                        command.Parameters.AddWithValue("@Id", contest.Contest_ID);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления контеста: {ex.Message}");
                return false;
            }
        }
        // Удалить контест по ID
        // Также удаляются все связи с задачами
        public async Task<bool> DeleteAsync(int contestId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Удаляем связи с задачами
                    string deleteLinks = "DELETE FROM Task_Contest WHERE Contest_ID = @Id";
                    using (var cmd = new SQLiteCommand(deleteLinks, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", contestId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    // Удаляем сам контест
                    string deleteContest = "DELETE FROM Contest WHERE Contest_ID = @Id";
                    using (var cmd = new SQLiteCommand(deleteContest, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", contestId);
                        return await cmd.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления контеста: {ex.Message}");
                return false;
            }
        }
        #endregion
        #region Related Data Methods
        // Получить ID контестов, связанных с конкретной задачей
        public async Task<List<int>> GetContestIdsByTaskAsync(int taskId)
        {
            var ids = new List<int>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Contest_ID FROM Task_Contest WHERE Task_ID = @TaskId";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", taskId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                ids.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения контестов задачи: {ex.Message}");
            }
            return ids;
        }
        // Сохранить связи между задачей и контестами
        // Старые связи удаляются, новые создаются
        public async Task<bool> SaveTaskContestsAsync(int taskId, IEnumerable<int> contestIds)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Удаляем старые связи
                    string delete = "DELETE FROM Task_Contest WHERE Task_ID = @TaskId";
                    using (var cmd = new SQLiteCommand(delete, connection))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", taskId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    // Добавляем новые связи
                    foreach (var contestId in contestIds)
                    {
                        string insert = "INSERT INTO Task_Contest (Task_ID, Contest_ID) VALUES (@TaskId, @ContestId)";
                        using (var cmd = new SQLiteCommand(insert, connection))
                        {
                            cmd.Parameters.AddWithValue("@TaskId", taskId);
                            cmd.Parameters.AddWithValue("@ContestId", contestId);
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