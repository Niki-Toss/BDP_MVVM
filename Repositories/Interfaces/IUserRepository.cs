using System.Collections.Generic;
using System.Threading.Tasks;
using BDP_MVVM.Models;

namespace BDP_MVVM.Repositories.Interfaces
{
    // Репозиторий для работы с пользователями и их ролями
    public interface IUserRepository
    {
        // Получение данных
        Task<List<User>> GetAllAsync();
        Task<List<Role>> GetAllRolesAsync();
        Task<User> GetByIdAsync(int userId);
        // Для аутентификации - ищем пользователя по логину вместе с ролью
        Task<User> GetByLoginAsync(string login);
        // CRUD операции (пароль хешируется автоматически при создании)
        Task<int> CreateAsync(User user, string password);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);
        // Отдельное обновление пароля (хешируется внутри метода)
        Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
        // Проверка уникальности логина (excludeUserId нужен при редактировании)
        Task<bool> LoginExistsAsync(string login, int excludeUserId = 0);
    }
}