using System.Threading.Tasks;
using BDP_MVVM.Models;

namespace BDP_MVVM.Services.Interfaces
{
    // Сервис аутентификации и управления правами доступа пользователей
    public interface IAuthenticationService
    {
        // Текущий авторизованный пользователь (null если не авторизован)
        User CurrentUser { get; }
        // Быстрая проверка - админ или текущий пользователь
        bool IsAdmin { get; }
        // Вход в систему (возвращает пользователя или null при ошибке)
        Task<User> LoginAsync(string login, string password);
        // Вход без авторизации (только просмотр)
        User LoginAsGuest();
        // Выход из системы (сбрасывает CurrentUser)
        void Logout();
        // Проверка конкретных прав текущего пользователя
        bool CanEditTasks();
        bool CanDeleteTasks();
        bool CanManageUsers();
    }
}