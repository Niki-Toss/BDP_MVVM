using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;

namespace BDP_MVVM.Repositories
{
    // Репозиторий для работы с пользователями и их ролями
    // Содержит методы для аутентификации и управления пользователями
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        #region Public Methods (CRUD Operations)
        // Получить всех пользователей с информацией о их ролях
        public async Task<List<User>> GetAllAsync()
        {
            var users = new List<User>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT u.User_ID, u.Логин, u.Email, u.Описание,
                               u.Role_ID, u.Дата_создания,
                               r.Название as РольНазвание, r.Код_роли,
                               r.Может_редактировать_задачи,
                               r.Может_удалять_задачи,
                               r.Может_управлять_пользователями
                        FROM Users u
                        LEFT JOIN User_role r ON u.Role_ID = r.Role_ID
                        ORDER BY u.Логин";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                User_ID = reader.GetInt32(0),
                                Логин = reader.GetString(1),
                                Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Описание = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Role_ID = reader.GetInt32(4),
                                Дата_создания = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                Роль = new Role
                                {
                                    Role_ID = reader.GetInt32(4),
                                    Название = reader.IsDBNull(6) ? "—" : reader.GetString(6),
                                    Код_роли = reader.IsDBNull(7) ? "" : reader.GetString(7),
                                    Может_редактировать_задачи = !reader.IsDBNull(8) && reader.GetBoolean(8),
                                    Может_удалять_задачи = !reader.IsDBNull(9) && reader.GetBoolean(9),
                                    Может_управлять_пользователями = !reader.IsDBNull(10) && reader.GetBoolean(10)
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки пользователей: {ex.Message}");
            }
            return users;
        }
        // Получить пользователя по ID
        public async Task<User> GetByIdAsync(int userId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT u.User_ID, u.Логин, u.Email, u.Описание,
                               u.Role_ID, u.Дата_создания,
                               r.Название, r.Код_роли
                        FROM Users u
                        LEFT JOIN User_role r ON u.Role_ID = r.Role_ID
                        WHERE u.User_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", userId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    User_ID = reader.GetInt32(0),
                                    Логин = reader.GetString(1),
                                    Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Описание = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    Role_ID = reader.GetInt32(4),
                                    Дата_создания = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                    Роль = new Role
                                    {
                                        Role_ID = reader.GetInt32(4),
                                        Название = reader.IsDBNull(6) ? "—" : reader.GetString(6),
                                        Код_роли = reader.IsDBNull(7) ? "" : reader.GetString(7)
                                    }
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения пользователя: {ex.Message}");
            }
            return null;
        }
        // Создать нового пользователя с автоматическим хешированием пароля
        public async Task<int> CreateAsync(User user, string password)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string hash = HashPassword(password);
                    string query = @"
                        INSERT INTO Users (Логин, Пароль_hash, Email, Описание, Role_ID, Дата_создания)
                        VALUES (@Логин, @Хеш, @Email, @Описание, @RoleId, datetime('now'));
                        SELECT last_insert_rowid();";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Логин", user.Логин);
                        command.Parameters.AddWithValue("@Хеш", hash);
                        command.Parameters.AddWithValue("@Email",
                            string.IsNullOrEmpty(user.Email) ? (object)DBNull.Value : user.Email);
                        command.Parameters.AddWithValue("@Описание",
                            string.IsNullOrEmpty(user.Описание) ? (object)DBNull.Value : user.Описание);
                        command.Parameters.AddWithValue("@RoleId", user.Role_ID);
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания пользователя: {ex.Message}");
                return -1;
            }
        }
        // Обновить данные пользователя (без изменения пароля)
        public async Task<bool> UpdateAsync(User user)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        UPDATE Users
                        SET Логин = @Логин,
                            Email = @Email,
                            Описание = @Описание,
                            Role_ID = @RoleId
                        WHERE User_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Логин", user.Логин);
                        command.Parameters.AddWithValue("@Email",
                            string.IsNullOrEmpty(user.Email) ? (object)DBNull.Value : user.Email);
                        command.Parameters.AddWithValue("@Описание",
                            string.IsNullOrEmpty(user.Описание) ? (object)DBNull.Value : user.Описание);
                        command.Parameters.AddWithValue("@RoleId", user.Role_ID);
                        command.Parameters.AddWithValue("@Id", user.User_ID);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления пользователя: {ex.Message}");
                return false;
            }
        }
        // Удалить пользователя по ID
        public async Task<bool> DeleteAsync(int userId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM Users WHERE User_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", userId);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления пользователя: {ex.Message}");
                return false;
            }
        }
        #endregion
        #region Authentication Methods
        // Получить пользователя по логину для аутентификации
        // Включает хеш пароля и полные данные роли с правами доступа
        public async Task<User> GetByLoginAsync(string login)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT u.User_ID, u.Логин, u.Email, u.Описание, u.Role_ID, 
                               u.Пароль_hash, u.Дата_создания,
                               r.Role_ID, r.Код_роли, r.Название, 
                               r.Может_редактировать_задачи, r.Может_удалять_задачи, 
                               r.Может_управлять_пользователями
                        FROM Users u
                        INNER JOIN User_Role r ON u.Role_ID = r.Role_ID
                        WHERE u.Логин = @Login";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", login);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    User_ID = reader.GetInt32(0),
                                    Логин = reader.GetString(1),
                                    Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Описание = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    Role_ID = reader.GetInt32(4),
                                    Пароль_hash = reader.GetString(5),
                                    Дата_создания = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                    Роль = new Role
                                    {
                                        Role_ID = reader.GetInt32(7),
                                        Код_роли = reader.GetString(8),
                                        Название = reader.GetString(9),
                                        Может_редактировать_задачи = reader.GetBoolean(10),
                                        Может_удалять_задачи = reader.GetBoolean(11),
                                        Может_управлять_пользователями = reader.GetBoolean(12)
                                    }
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка поиска пользователя по логину: {ex.Message}");
            }
            return null;
        }
        // Обновить пароль пользователя с автоматическим хешированием
        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "UPDATE Users SET Пароль_hash = @Hash WHERE User_ID = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Hash", HashPassword(newPassword));
                        command.Parameters.AddWithValue("@Id", userId);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления пароля: {ex.Message}");
                return false;
            }
        }
        // Проверить существование логина в базе
        // Используется для валидации уникальности логина при создании/редактировании
        // "login">Логин для проверки
        // "excludeUserId">ID пользователя, который нужно исключить из проверки (при редактировании)
        public async Task<bool> LoginExistsAsync(string login, int excludeUserId = 0)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT COUNT(1) FROM Users
                        WHERE Логин = @Логин AND User_ID != @ExcludeId";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Логин", login);
                        command.Parameters.AddWithValue("@ExcludeId", excludeUserId);
                        return (int)await command.ExecuteScalarAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки логина: {ex.Message}");
                return false;
            }
        }
        #endregion
        #region Role Methods
        // Получить список всех ролей для выбора при создании/редактировании пользователя
        public async Task<List<Role>> GetAllRolesAsync()
        {
            var roles = new List<Role>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT Role_ID, Код_роли, Название,
                               Может_редактировать_задачи,
                               Может_удалять_задачи,
                               Может_управлять_пользователями
                        FROM User_role
                        ORDER BY Название";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            roles.Add(new Role
                            {
                                Role_ID = reader.GetInt32(0),
                                Код_роли = reader.GetString(1),
                                Название = reader.GetString(2),
                                Может_редактировать_задачи = !reader.IsDBNull(3) && reader.GetBoolean(3),
                                Может_удалять_задачи = !reader.IsDBNull(4) && reader.GetBoolean(4),
                                Может_управлять_пользователями = !reader.IsDBNull(5) && reader.GetBoolean(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки ролей: {ex.Message}");
            }
            return roles;
        }
        #endregion
        #region Private Helper Methods
        // Хеширование пароля с использованием SHA-256
        // Используется для безопасного хранения паролей в базе данных
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
        #endregion
    }
}