using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BDP_MVVM.Common;
using BDP_MVVM.Services.Interfaces;

namespace BDP_MVVM.ViewModels
{
    // ViewModel для окна авторизации пользователя
    // Поддерживает вход по логину/паролю и гостевой режим
    public class LoginViewModel : ViewModelBase
    {
        #region Fields
        private readonly IAuthenticationService _authService;
        private readonly IDialogService _dialogService;
        private string _login;
        private string _password;
        #endregion
        #region Properties
        // Логин пользователя для входа в систему
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }
        // Пароль пользователя для входа в систему
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        #endregion
        #region Commands
        // Команда входа в систему по логину и паролю
        public ICommand LoginCommand { get; }
        // Команда входа в гостевом режиме (только просмотр)
        public ICommand LoginAsGuestCommand { get; }
        #endregion
        #region Events
        // Событие успешного входа в систему
        // Используется в LoginWindow для закрытия окна авторизации
        public event Action<bool> LoginSuccessful;
        #endregion
        #region Constructor
        // Инициализация ViewModel для окна авторизации
        public LoginViewModel(IAuthenticationService authService, IDialogService dialogService)
        {
            _authService = authService;
            _dialogService = dialogService;
            LoginCommand = new AsyncRelayCommand(
                async _ => await DoLoginAsync(),
                _ => CanLogin());
            LoginAsGuestCommand = new RelayCommand(_ => DoLoginAsGuest());
        }
        #endregion
        #region Private Methods
        // Проверить заполнение обязательных полей для активации кнопки входа
        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }
        // Выполнить вход в систему по логину и паролю
        // Проверяет хеш пароля в базе данных
        private async Task DoLoginAsync()
        {
            await ExecuteAsync(async () =>
            {
                var user = await _authService.LoginAsync(Login, Password);
                if (user != null)
                {
                    // Успешный вход - закрываем окно авторизации
                    LoginSuccessful?.Invoke(true);
                }
                else
                {
                    ErrorMessage = "Неверный логин или пароль";
                }
            }, "Ошибка при входе в систему");
        }
        // Выполнить вход в гостевом режиме без авторизации
        // Доступен только просмотр данных без возможности редактирования
        private void DoLoginAsGuest()
        {
            _authService.LoginAsGuest();
            LoginSuccessful?.Invoke(true);
        }
        #endregion
    }
}