using System.Windows;
using System.Windows.Controls;
using BDP_MVVM.ViewModels;

namespace BDP_MVVM.Views
{
    // Окно авторизации - первое окно при запуске приложения
    // Поддерживает вход с логином/паролем или как гость
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel;
        public LoginWindow()
        {
            InitializeComponent();
            // Получаем ViewModel из DI контейнера
            _viewModel = App.GetService<LoginViewModel>();
            DataContext = _viewModel;
            // Подписываемся на событие успешного входа
            _viewModel.LoginSuccessful += OnLoginSuccessful;
            // Фокус на поле логина при открытии окна
            this.Loaded += (s, e) => txtLogin.Focus();
        }
        // Успешный вход - открываем главное окно и закрываем окно входа
        private void OnLoginSuccessful(bool success)
        {
            if (success)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }
        // PasswordBox не поддерживает биндинг Password - передаём значение вручную
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && _viewModel != null)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }
        // Отписываемся от событий при закрытии (предотвращаем утечки памяти)
        protected override void OnClosed(System.EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.LoginSuccessful -= OnLoginSuccessful;
            }
            base.OnClosed(e);
        }
    }
}