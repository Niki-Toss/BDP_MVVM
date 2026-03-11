using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;
using BDP_MVVM.Services.Interfaces;

namespace BDP_MVVM.Services
{
    // Сервис аутентификации - проверка логина и пароля и управление текущим пользователем
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        public User CurrentUser { get; private set; }
        public bool IsAdmin => CurrentUser?.Роль?.Код_роли == "admin";
        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        // Вход в систему - проверяем хеш пароля из БД
        public async Task<User> LoginAsync(string login, string password)
        {
            try
            {
                var user = await _userRepository.GetByLoginAsync(login);
                if (user != null)
                {
                    string storedHash = user.Пароль_hash;
                    string inputHash = HashPassword(password);
                    // Сравниваем хеши (пароль в БД хранится как SHA-256 хеш)
                    if (storedHash == inputHash)
                    {
                        CurrentUser = user;
                        return CurrentUser;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка входа: {ex.Message}");
            }
            return null;
        }
        // SHA-256 хеширование пароля (такое же как в UserRepository)
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
        // Вход без пароля, одно из требований задания, для просмотра данных
        public User LoginAsGuest()
        {
            CurrentUser = new User
            {
                User_ID = 0,
                Логин = "Гость",
                Роль = new Role
                {
                    Role_ID = 0,
                    Код_роли = "guest",
                    Название = "Гость",
                    Может_редактировать_задачи = false,
                    Может_удалять_задачи = false,
                    Может_управлять_пользователями = false
                }
            };
            return CurrentUser;
        }
        public void Logout() => CurrentUser = null;
        // Проверка конкретных прав через роль текущего пользователя
        public bool CanEditTasks() => CurrentUser?.Роль?.Может_редактировать_задачи ?? false;
        public bool CanDeleteTasks() => CurrentUser?.Роль?.Может_удалять_задачи ?? false;
        public bool CanManageUsers() => CurrentUser?.Роль?.Может_управлять_пользователями ?? false;
    }
}