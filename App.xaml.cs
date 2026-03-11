using System;
using System.IO;
using System.Windows;
using BDP_MVVM.Repositories;
using BDP_MVVM.Repositories.Interfaces;
using BDP_MVVM.Services;
using BDP_MVVM.Services.Interfaces;
using BDP_MVVM.ViewModels;
using System.Data.SQLite;

namespace BDP_MVVM
{
    // Главный класс приложения
    // Настраивает подключение к базе данных SQLite и контейнер Dependency Injection
    public partial class App : Application
    {
        #region Fields
        private static SimpleServiceProvider _serviceProvider;
        private string _connectionString;
        #endregion
        #region Properties
        // Глобальный провайдер сервисов для всего приложения
        public static SimpleServiceProvider ServiceProvider => _serviceProvider;
        #endregion
        #region Lifecycle Methods
        // Вызывается при запуске приложения
        // Настраивает базу данных, DI контейнер и показывает окно входа
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                // Настраиваем путь к базе данных и строку подключения
                SetupDatabase();
                // Регистрируем все зависимости (Repositories, Services, ViewModels)
                SetupDependencyInjection();
                // Показываем окно входа
                var loginWindow = new Views.LoginWindow();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка инициализации приложения:\n\n{ex.Message}",
                    "Ошибка запуска",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
        #endregion
        #region Private Methods - Database
        // Настроить путь к базе данных SQLite и строку подключения
        // База данных хранится в AppData пользователя
        // При первом запуске создаёт структуру БД и заполняет тестовыми данными
        private void SetupDatabase()
        {
            // Определяем путь к БД в AppData пользователя
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BDP_MVVM"
            );
            // Создаём папку если не существует
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            string dbPath = Path.Combine(appDataPath, "BDPData.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
            // Если БД не существует - создать структуру и заполнить тестовыми данными
            if (!File.Exists(dbPath))
            {
                CreateDatabase(dbPath);
                DatabaseSeeder.SeedDatabase(_connectionString);
            }
        }
        // Создать структуру базы данных SQLite при первом запуске
        // Создаёт все таблицы, внешние ключи и добавляет начальные роли и администратора
        // "dbPath">Путь к файлу базы данных
        private void CreateDatabase(string dbPath)
        {
            SQLiteConnection.CreateFile(dbPath);
            using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                connection.Open();
                string createTables = @"
                    CREATE TABLE IF NOT EXISTS User_role (
                        Role_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Код_роли TEXT NOT NULL,
                        Название TEXT NOT NULL,
                        Может_редактировать_задачи INTEGER DEFAULT 0,
                        Может_удалять_задачи INTEGER DEFAULT 0,
                        Может_управлять_пользователями INTEGER DEFAULT 0
                    );
                    CREATE TABLE IF NOT EXISTS Users (
                        User_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Логин TEXT UNIQUE NOT NULL,
                        Пароль_hash TEXT NOT NULL,
                        Email TEXT,
                        Описание TEXT,
                        Role_ID INTEGER,
                        Дата_создания TEXT DEFAULT (datetime('now')),
                        FOREIGN KEY (Role_ID) REFERENCES User_role(Role_ID)
                    );
                    CREATE TABLE IF NOT EXISTS Tags (
                        Tag_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Название TEXT UNIQUE NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS Platform (
                        Platform_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Название TEXT UNIQUE NOT NULL,
                        Автопроверка_готовности INTEGER DEFAULT 0
                    );
                    CREATE TABLE IF NOT EXISTS Contest (
                        Contest_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Название TEXT UNIQUE NOT NULL,
                        Год_создания INTEGER
                    );
                    CREATE TABLE IF NOT EXISTS Task (
                        Task_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Название TEXT NOT NULL,
                        Автор_ID INTEGER,
                        Краткое_условие TEXT,
                        Идея_решения TEXT,
                        Примечание TEXT,
                        Сложность INTEGER DEFAULT 5,
                        Дата_создания TEXT DEFAULT (datetime('now')),
                        Ссылка_polygon TEXT,
                        FOREIGN KEY (Автор_ID) REFERENCES Users(User_ID)
                    );
                    CREATE TABLE IF NOT EXISTS Task_Tag (
                        Task_ID INTEGER,
                        Tag_ID INTEGER,
                        PRIMARY KEY (Task_ID, Tag_ID),
                        FOREIGN KEY (Task_ID) REFERENCES Task(Task_ID) ON DELETE CASCADE,
                        FOREIGN KEY (Tag_ID) REFERENCES Tags(Tag_ID) ON DELETE CASCADE
                    );
                    CREATE TABLE IF NOT EXISTS Task_Platform (
                        Task_ID INTEGER,
                        Platform_ID INTEGER,
                        Готовность INTEGER DEFAULT 0,
                        PRIMARY KEY (Task_ID, Platform_ID),
                        FOREIGN KEY (Task_ID) REFERENCES Task(Task_ID) ON DELETE CASCADE,
                        FOREIGN KEY (Platform_ID) REFERENCES Platform(Platform_ID) ON DELETE CASCADE
                    );
                    CREATE TABLE IF NOT EXISTS Task_Contest (
                        Task_ID INTEGER,
                        Contest_ID INTEGER,
                        PRIMARY KEY (Task_ID, Contest_ID),
                        FOREIGN KEY (Task_ID) REFERENCES Task(Task_ID) ON DELETE CASCADE,
                        FOREIGN KEY (Contest_ID) REFERENCES Contest(Contest_ID) ON DELETE CASCADE
                    );
                    INSERT INTO User_role (Код_роли, Название, Может_редактировать_задачи, Может_удалять_задачи, Может_управлять_пользователями)
                    VALUES 
                        ('admin', 'Администратор', 1, 1, 1),
                        ('editor', 'Редактор', 1, 0, 0),
                        ('guest', 'Гость', 0, 0, 0);
                    INSERT INTO Users (Логин, Пароль_hash, Email, Role_ID)
                    VALUES ('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'admin@example.com', 1);
                ";
                using (var command = new SQLiteCommand(createTables, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region Private Methods - Dependency Injection
        // Настроить контейнер Dependency Injection
        // Регистрирует все репозитории, сервисы и ViewModels с соответствующими lifetime (Singleton/Transient)
        private void SetupDependencyInjection()
        {
            var services = new ServiceCollection();
            // Регистрируем строку подключения как Singleton
            services.AddSingleton(_connectionString);
            // === REPOSITORIES (Transient - создаётся новый экземпляр каждый раз) ===
            services.AddTransient<ITaskRepository>(provider =>
                new TaskRepository(_connectionString));
            services.AddTransient<ITagRepository>(provider =>
                new TagRepository(_connectionString));
            services.AddTransient<IContestRepository>(provider =>
                new ContestRepository(_connectionString));
            services.AddTransient<IPlatformRepository>(provider =>
                new PlatformRepository(_connectionString));
            services.AddTransient<IUserRepository>(provider =>
                new UserRepository(_connectionString));
            // === SERVICES (Singleton - один экземпляр на всё приложение) ===
            services.AddSingleton<IAuthenticationService>(provider =>
            {
                var userRepo = provider.GetService(typeof(IUserRepository)) as IUserRepository;
                return new AuthenticationService(userRepo);
            });
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            // === VIEWMODELS (Transient - создаём новый каждый раз) ===
            services.AddTransient<LoginViewModel>(provider =>
            {
                var authService = provider.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
                var dialogService = provider.GetService(typeof(IDialogService)) as IDialogService;
                return new LoginViewModel(authService, dialogService);
            });
            services.AddTransient<MainViewModel>(provider =>
            {
                var authService = provider.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
                return new MainViewModel(authService);
            });
            services.AddTransient<TasksViewModel>(provider =>
            {
                var taskRepo = provider.GetService(typeof(ITaskRepository)) as ITaskRepository;
                var tagRepo = provider.GetService(typeof(ITagRepository)) as ITagRepository;
                var platformRepo = provider.GetService(typeof(IPlatformRepository)) as IPlatformRepository;
                var contestRepo = provider.GetService(typeof(IContestRepository)) as IContestRepository;
                var navService = new NavigationService();
                var authService = provider.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
                var dialogService = provider.GetService(typeof(IDialogService)) as IDialogService;
                return new TasksViewModel(taskRepo, tagRepo, platformRepo, contestRepo, navService, authService, dialogService);
            });
            services.AddTransient<TagsViewModel>(provider =>
            {
                var tagRepo = provider.GetService(typeof(ITagRepository)) as ITagRepository;
                var authService = provider.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
                var dialogService = provider.GetService(typeof(IDialogService)) as IDialogService;
                return new TagsViewModel(tagRepo, authService, dialogService);
            });
            services.AddTransient<ContestsViewModel>(provider =>
            {
                var repo = provider.GetService(typeof(IContestRepository)) as IContestRepository;
                var auth = provider.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
                var dialog = provider.GetService(typeof(IDialogService)) as IDialogService;
                return new ContestsViewModel(repo, auth, dialog);
            });
            services.AddTransient<PlatformsViewModel>(provider =>
            {
                var repo = provider.GetService(typeof(IPlatformRepository)) as IPlatformRepository;
                var auth = provider.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
                var dialog = provider.GetService(typeof(IDialogService)) as IDialogService;
                return new PlatformsViewModel(repo, auth, dialog);
            });
            services.AddTransient<ReportsViewModel>(provider =>
            {
                var taskRepo = provider.GetService(typeof(ITaskRepository)) as ITaskRepository;
                var tagRepo = provider.GetService(typeof(ITagRepository)) as ITagRepository;
                var contestRepo = provider.GetService(typeof(IContestRepository)) as IContestRepository;
                var dialog = provider.GetService(typeof(IDialogService)) as IDialogService;
                return new ReportsViewModel(taskRepo, tagRepo, contestRepo, dialog);
            });
            services.AddTransient<UsersViewModel>(provider =>
            {
                var repo = provider.GetService(typeof(IUserRepository)) as IUserRepository;
                var auth = provider.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
                var dialog = provider.GetService(typeof(IDialogService)) as IDialogService;
                return new UsersViewModel(repo, auth, dialog);
            });
            // Создаём контейнер из зарегистрированных сервисов
            _serviceProvider = services.BuildServiceProvider();
        }
        #endregion
        #region Public Methods
        // Получить сервис по типу из контейнера DI
        // "serviceType">Тип запрашиваемого сервиса
        // Экземпляр сервиса или null
        public static object GetService(Type serviceType)
        {
            return _serviceProvider?.GetService(serviceType);
        }
        // Получить сервис с generic типом из контейнера DI
        // "T">Тип запрашиваемого сервиса
        // Экземпляр сервиса или null
        public static T GetService<T>() where T : class
        {
            return _serviceProvider?.GetService(typeof(T)) as T;
        }
        #endregion
    }
    #region Dependency Injection Container
    // Интерфейс провайдера сервисов для разрешения зависимостей
    public interface IServiceProvider
    {
        // Получить зарегистрированный сервис по типу
        // "serviceType">Тип запрашиваемого сервиса
        // Экземпляр сервиса или null
        object GetService(Type serviceType);
    }
    // Коллекция для регистрации сервисов в DI контейнере
    // Поддерживает регистрацию с Singleton и Transient lifetime
    public class ServiceCollection
    {
        private readonly System.Collections.Generic.Dictionary<Type, Func<IServiceProvider, object>> _services =
            new System.Collections.Generic.Dictionary<Type, Func<IServiceProvider, object>>();
        // Зарегистрировать готовый экземпляр как Singleton
        // Один и тот же экземпляр будет возвращаться при каждом запросе
        public void AddSingleton<T>(T instance)
        {
            _services[typeof(T)] = _ => instance;
        }
        // Зарегистрировать тип как Singleton
        // Создаётся один раз при первом запросе и переиспользуется
        public void AddSingleton<TInterface, TImplementation>()
            where TImplementation : TInterface, new()
        {
            object instance = null;
            _services[typeof(TInterface)] = provider =>
            {
                if (instance == null)
                {
                    instance = new TImplementation();
                }
                return instance;
            };
        }
        // Зарегистрировать сервис через фабрику как Singleton
        // Фабрика вызывается один раз при первом запросе
        public void AddSingleton<TInterface>(Func<IServiceProvider, TInterface> factory)
        {
            object instance = null;
            _services[typeof(TInterface)] = provider =>
            {
                if (instance == null)
                {
                    instance = factory(provider);
                }
                return instance;
            };
        }
        // Зарегистрировать сервис как Transient
        // Создаётся новый экземпляр при каждом запросе
        public void AddTransient<T>(Func<IServiceProvider, T> factory)
        {
            _services[typeof(T)] = provider => factory(provider);
        }
        // Создать провайдер сервисов из зарегистрированных зависимостей
        // Провайдер сервисов для разрешения зависимостей
        public SimpleServiceProvider BuildServiceProvider()
        {
            return new SimpleServiceProvider(_services);
        }
    }
    // Простой провайдер сервисов для разрешения зависимостей
    // Использует зарегистрированные фабрики для создания экземпляров сервисов
    public class SimpleServiceProvider : IServiceProvider
    {
        private readonly System.Collections.Generic.Dictionary<Type, Func<IServiceProvider, object>> _services;
        // Инициализация провайдера с зарегистрированными сервисами
        // "services">Словарь типов и их фабрик
        public SimpleServiceProvider(System.Collections.Generic.Dictionary<Type, Func<IServiceProvider, object>> services)
        {
            _services = services;
        }
        // Получить сервис по типу, используя зарегистрированную фабрику
        // "serviceType">Тип запрашиваемого сервиса
        // Экземпляр сервиса или null
        public object GetService(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out var factory))
            {
                return factory(this);
            }
            return null;
        }
    }
    #endregion
}