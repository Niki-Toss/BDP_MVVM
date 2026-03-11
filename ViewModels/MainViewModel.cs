using System;
using System.Windows.Input;
using BDP_MVVM.Common;
using BDP_MVVM.Services.Interfaces;

namespace BDP_MVVM.ViewModels
{
    // ViewModel главного окна приложения
    // Управляет навигацией между разделами и отображением информации о текущем пользователе
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly IAuthenticationService _authService;
        private string _currentPage;
        private string _currentPageTitle;
        private string _statusText;
        #endregion
        #region Properties
        // Идентификатор текущей открытой страницы (Tasks, Tags, Contests и т.д.)
        public string CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }
        // Заголовок текущей страницы для отображения в шапке окна
        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            set => SetProperty(ref _currentPageTitle, value);
        }
        // Текст в статус-баре (название страницы и текущее время)
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }
        // Имя текущего пользователя для отображения в шапке
        public string CurrentUserName => _authService?.CurrentUser?.Логин ?? "Гость";
        // Роль текущего пользователя для отображения в шапке
        public string CurrentUserRole => _authService?.CurrentUser?.Роль?.Название ?? "Гость";
        // Может ли текущий пользователь управлять другими пользователями
        // Используется для показа/скрытия пункта меню "Пользователи"
        public bool CanManageUsers => _authService?.CanManageUsers() ?? false;
        #endregion
        #region Commands
        // Команда навигации к списку задач
        public ICommand NavigateToTasksCommand { get; private set; }
        // Команда навигации к справочнику тегов
        public ICommand NavigateToTagsCommand { get; private set; }
        // Команда навигации к справочнику контестов
        public ICommand NavigateToContestsCommand { get; private set; }
        // Команда навигации к справочнику платформ
        public ICommand NavigateToPlatformsCommand { get; private set; }
        // Команда навигации к управлению пользователями (требует права администратора)
        public ICommand NavigateToUsersCommand { get; private set; }
        // Команда навигации к разделу отчётов
        public ICommand NavigateToReportsCommand { get; private set; }
        // Команда выхода из системы
        public ICommand LogoutCommand { get; private set; }
        #endregion
        #region Events
        // Событие запроса выхода из системы
        // Используется в MainWindow для возврата к окну авторизации
        public event Action LogoutRequested;
        // Событие запроса навигации к другой странице
        // Используется в MainWindow для смены контента
        public event Action<string> NavigationRequested;
        #endregion
        #region Constructor
        // Инициализация ViewModel главного окна
        public MainViewModel(IAuthenticationService authService)
        {
            _authService = authService;
            InitializeCommands();
            UpdateStatus();
            // Стартовая страница при открытии приложения
            NavigateTo("Tasks", "Список задач");
        }
        #endregion
        #region Private Methods
        // Инициализировать все команды навигации с проверкой прав доступа
        private void InitializeCommands()
        {
            NavigateToTasksCommand = new RelayCommand(_ =>
                NavigateTo("Tasks", "Список задач"));
            NavigateToTagsCommand = new RelayCommand(_ =>
                NavigateTo("Tags", "Теги"));
            NavigateToContestsCommand = new RelayCommand(_ =>
                NavigateTo("Contests", "Контесты"));
            NavigateToPlatformsCommand = new RelayCommand(_ =>
                NavigateTo("Platforms", "Платформы"));
            // Пользователи - только для тех у кого есть права
            NavigateToUsersCommand = new RelayCommand(
                _ => NavigateTo("Users", "Пользователи"),
                _ => CanManageUsers);
            NavigateToReportsCommand = new RelayCommand(_ =>
                NavigateTo("Reports", "Отчёты"));
            LogoutCommand = new RelayCommand(_ => Logout());
        }
        // Переключиться на другую страницу
        // Вызывает событие NavigationRequested для смены контента в MainWindow
        private void NavigateTo(string page, string title)
        {
            CurrentPage = page;
            CurrentPageTitle = title;
            UpdateStatus();
            NavigationRequested?.Invoke(page);
        }
        // Обновить текст в статус-баре (название страницы и текущее время)
        private void UpdateStatus()
        {
            StatusText = $"{CurrentPageTitle}  |  {DateTime.Now:dd.MM.yyyy HH:mm}";
        }
        // Выполнить выход из системы
        // Возвращает пользователя к окну авторизации
        private void Logout()
        {
            _authService?.Logout();
            LogoutRequested?.Invoke();
        }
        #endregion
    }
}